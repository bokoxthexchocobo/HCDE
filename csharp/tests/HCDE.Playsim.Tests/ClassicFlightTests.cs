using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ClassicFlightTests
{
    [Theory]
    [InlineData(30)]
    [InlineData(-30)]
    public void ClassicForwardFlightStaysHorizontal(double pitch)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.NoGravity = true; player.ClassicFlight = true; player.PitchDegrees = pitch;
        player.Z = Fixed.FromInt(64); player.OnGround = false;
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        Assert.Equal(1, player.X.ToDouble()); Assert.Equal(64, player.Z.ToDouble());
    }

    [Fact]
    public void ClassicFlightDoesNotSuppressJumpAscent()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.NoGravity = true; player.ClassicFlight = true; player.PitchDegrees = 30;
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192, Jump = true }); sim.Tick();
        Assert.Equal(1, player.X.ToDouble()); Assert.Equal(3, player.Z.ToDouble());
    }

    [Fact]
    public void PreferenceSurvivesArchiveAndLegacySaveResetsIt()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim);
        sim.Players.Single().ClassicFlight = true;
        var saved = SimSavegame.Write(sim);
        Assert.Equal(93, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var loaded = Room(); SimSavegame.Apply(loaded, saved);
        Assert.True(loaded.Players.Single().ClassicFlight);
        Assert.Equal(saved, SimSavegame.Write(loaded));
        SimSavegame.Apply(loaded, legacy); Assert.False(loaded.Players.Single().ClassicFlight);
    }

    [Fact]
    public void PreferenceParticipatesInChecksum()
    {
        var first = Room(); var second = Room(); second.Players.Single().ClassicFlight = true;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Theory]
    [InlineData(0, 93)]
    [InlineData(4, -1)]
    [InlineData(8, 2)]
    public void MalformedPreferenceArchiveIsRejected(int offset, int value)
    {
        var sim = Room(); sim.Players.Single().ClassicFlight = true;
        var saved = SimSavegame.Write(sim);
        var start = saved.Length - BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
