using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GeometryHealthSaveTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Lines = [new LevelLine { Tag = 7, Health = 100, HealthGroup = 4 }, new LevelLine { Tag = 8, Health = 70 }],
        Sectors = [new LevelSector { Tag = 9, CeilingHeight = 128, HealthFloor = 100, HealthFloorGroup = 4,
            HealthCeiling = 80, Health3D = 60 }],
    });

    [Fact]
    public void RoundTripRestoresLocalAndPooledValuesAndDamageContinuation()
    {
        var original = Room();
        GeometryHealthActions.Execute(original, 150, 7, 35, 0);
        GeometryHealthActions.Execute(original, 151, 9, 1, 25);
        GeometryHealthActions.Execute(original, 151, 9, 2, 0);
        original.Tick();
        var bytes = SimSavegame.Write(original);
        Assert.Equal(15, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var restored = Room();
        SimSavegame.Apply(restored, bytes);
        Assert.Equal(original.Checksum, restored.Checksum);
        Assert.Equal(35, AcsDestructibleHealth.GetLineHealth(restored, 7));
        Assert.Equal(25, AcsDestructibleHealth.GetSectorHealth(restored, 9, 1));
        Assert.Equal(0, restored.Level.Sectors[0].Health3D);
        Assert.Equal(70, restored.Level.Lines[1].Health);
        foreach (var sim in new[] { original, restored })
        {
            LevelDestructibleDamage.DamageLine(sim, sim.Level.Lines[0], 10);
            sim.Tick();
        }
        Assert.Equal(25, restored.HealthGroups[4]);
        Assert.Equal(original.Checksum, restored.Checksum);
    }

    [Fact]
    public void LegacyVersionFourteenPreservesCurrentHealth()
    {
        var sim = Room();
        var state = sim.CaptureState();
        state.GeometryHealth = null;
        var bytes = SimSavegame.Write(state);
        Assert.Equal(14, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        GeometryHealthActions.Execute(sim, 150, 7, 12, 0);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(12, sim.Level.Lines[0].Health);
        Assert.Equal(12, sim.HealthGroups[4]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void MapMismatchIsRejectedBeforeClockOrHealthMutation(int kind)
    {
        var sim = Room();
        var state = new SimSaveState { Tic = 99, GeometryHealth = sim.CaptureState().GeometryHealth };
        if (kind == 0) state.GeometryHealth!.Lines.Clear();
        if (kind == 1) state.GeometryHealth!.Sectors.Clear();
        if (kind == 2) { state.GeometryHealth!.Groups.Clear(); state.GeometryHealth.Groups.Add(5, 1); }
        if (kind != 0) state.GeometryHealth!.Lines[0] = 1;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, sim.Thinkers.Clock.Tic);
        Assert.Equal(100, sim.Level.Lines[0].Health);
        Assert.Equal(100, sim.HealthGroups[4]);
    }

    [Fact]
    public void TruncatedArchivesAndInvalidGroupIdsAreRejected()
    {
        var bytes = SimSavegame.Write(Room());
        for (var i = 0; i < bytes.Length; i++)
            Assert.False(SimSavegame.TryRead(bytes.AsSpan(0, i), out _, out _));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 12), 0);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-geometry-health", error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void EveryHealthSurfaceContributesToChecksum(int kind)
    {
        var first = Room(); var second = Room();
        switch (kind)
        {
            case 0: second.Level.Lines[1].Health--; break;
            case 1: second.Level.Sectors[0].HealthFloor--; break;
            case 2: second.Level.Sectors[0].HealthCeiling--; break;
            case 3: second.Level.Sectors[0].Health3D--; break;
            case 4: second.HealthGroups[4]--; break;
        }
        first.Tick(); second.Tick();
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void InvalidTrailerLengthsAreRejected(int length)
    {
        var bytes = SimSavegame.Write(Room());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), length);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-geometry-health", error);
    }

    [Fact]
    public void DuplicateGroupIdsAreRejected()
    {
        var sim = Room(); sim.HealthGroups.Add(5, 50);
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 12), 4);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-geometry-health", error);
    }
}
