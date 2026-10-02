using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GeometryHealthActionTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Lines = [new LevelLine { Tag = 7, AdditionalIds = new[] { 8 }, Health = 100, HealthGroup = 4 },
            new LevelLine { Tag = 9, Health = 100, HealthGroup = 4 },
            new LevelLine { Tag = 7, Health = 50 }],
        Sectors = [new LevelSector { Tag = 10, AdditionalTags = new[] { 11 }, CeilingHeight = 128,
            HealthFloor = 100, HealthFloorGroup = 4, HealthCeiling = 100, HealthCeilingGroup = 4,
            Health3D = 100, Health3DGroup = 4 },
            new LevelSector { Index = 1, Tag = 12, CeilingHeight = 128, HealthCeiling = 100, HealthCeilingGroup = 4 },
            new LevelSector { Index = 2, CeilingHeight = 128, HealthFloor = 50 }],
    });

    private static void AssertGroup(AuthoritySimulation sim, int health)
    {
        Assert.Equal(health, sim.HealthGroups[4]);
        Assert.Equal(health, sim.Level.Lines[0].Health);
        Assert.Equal(health, sim.Level.Lines[1].Health);
        Assert.Equal(health, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(health, sim.Level.Sectors[0].HealthCeiling);
        Assert.Equal(health, sim.Level.Sectors[0].Health3D);
        Assert.Equal(health, sim.Level.Sectors[1].HealthCeiling);
    }

    [Theory]
    [InlineData(7, 40)]
    [InlineData(8, 40)]
    [InlineData(8, -20)]
    public void LineActionUpdatesAllMatchesAndCrossTypeGroup(int id, int health)
    {
        var sim = Room();
        var action = new LevelLine { Special = 150, Arg0 = id, Arg1 = health, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), action, true));
        AssertGroup(sim, Math.Max(0, health));
        Assert.Equal(id == 7 ? Math.Max(0, health) : 50, sim.Level.Lines[2].Health);
        Assert.Equal(0, action.Special);
        Assert.Equal(Math.Max(0, health), AcsDestructibleHealth.GetLineHealth(sim, 8));
        Assert.Equal(Math.Max(0, health), AcsDestructibleHealth.GetSectorHealth(sim, 11, 1));
    }

    [Theory]
    [InlineData(0, 40)]
    [InlineData(1, 40)]
    [InlineData(2, 40)]
    [InlineData(0, -20)]
    public void SectorActionUpdatesSelectedPartAndEntireGroup(int part, int health)
    {
        var sim = Room();
        var action = new LevelLine { Special = 151, Arg0 = 11, Arg1 = part, Arg2 = health, PlayerCross = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), action, false));
        AssertGroup(sim, Math.Max(0, health));
        Assert.Equal(50, sim.Level.Sectors[2].HealthFloor);
    }

    [Theory]
    [InlineData(150, -1, 40)]
    [InlineData(150, 999, 40)]
    [InlineData(151, 999, 0)]
    [InlineData(151, 11, 99)]
    public void MissingTargetsAndUnknownPartsReturnSuccessWithoutChangingHealth(int special, int id, int arg1)
    {
        var sim = Room();
        Assert.True(GeometryHealthActions.Execute(sim, special, id, arg1, 40));
        AssertGroup(sim, 100);
    }

    [Fact]
    public void ZeroTagSelectsOnlyUntaggedSectors()
    {
        var sim = Room();
        Assert.True(GeometryHealthActions.Execute(sim, 151, 0, 0, 25));
        Assert.Equal(25, sim.Level.Sectors[2].HealthFloor);
        AssertGroup(sim, 100);
    }

    [Theory]
    [InlineData(false, 150)]
    [InlineData(true, 150)]
    [InlineData(false, 151)]
    [InlineData(true, 151)]
    public void AcsDirectAndStackActionsContinueAfterSettingHealth(bool stack, int special)
    {
        var sim = Room();
        var id = special == 150 ? 8 : 11;
        var arg1 = special == 150 ? 40 : 2;
        int[] words = stack ? [3, id, 3, arg1, 3, 40, 6, special, 11, 151, 0, 0, 25, 1]
            : [11, special, id, arg1, 40, 11, 151, 0, 0, 25, 1];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Tick(sim);
        AssertGroup(sim, 40);
        Assert.Equal(25, sim.Level.Sectors[2].HealthFloor);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DamageSynchronizesAllKindsOfGroupMembers(bool sector)
    {
        var sim = Room();
        if (sector) LevelDestructibleDamage.DamageSectorPart(sim, sim.Level.Sectors[0], 0, 30);
        else LevelDestructibleDamage.DamageLine(sim, sim.Level.Lines[0], 30);
        AssertGroup(sim, 70);
    }
}
