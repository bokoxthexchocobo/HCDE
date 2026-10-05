using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialGeometryHealthTests
{
    [Theory]
    [InlineData(false, 35)]
    [InlineData(true, 35)]
    [InlineData(false, -5)]
    [InlineData(true, -5)]
    public void LineHealthUpdatesTargetAndSharedGroup(bool death, int health)
    {
        var sim = Room(); var actor = sim.Actors[0]; Set(actor, 150, 7, health, 0); Run(sim, actor, death);
        var expected = Math.Max(0, health);
        Assert.Equal(expected, sim.Level.Lines[0].Health);
        Assert.Equal(expected, sim.HealthGroups[4]);
        Assert.Equal(70, sim.Level.Lines[1].Health);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    public void SectorHealthChangesOnlySelectedPart(bool death, int part)
    {
        var sim = Room(); var actor = sim.Actors[0]; Set(actor, 151, 9, part, 25); Run(sim, actor, death);
        var sector = sim.Level.Sectors[0];
        Assert.Equal(part == 0 ? 25 : 100, sector.HealthFloor);
        Assert.Equal(part == 1 ? 25 : 80, sector.HealthCeiling);
        Assert.Equal(part == 2 ? 25 : 60, sector.Health3D);
    }

    [Theory]
    [InlineData(99, 0)]
    [InlineData(9, 99)]
    public void UnmatchedSectorOrPartReturnsSuccessWithoutMutation(int tag, int part)
    {
        var sim = Room(); var actor = sim.Actors[0]; Set(actor, 151, tag, part, 25); actor.ActivationType = 32;
        Assert.True(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(0, actor.Special); Assert.Equal(100, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(80, sim.Level.Sectors[0].HealthCeiling); Assert.Equal(60, sim.Level.Sectors[0].Health3D);
    }

    private static void Set(Actor actor, int special, int id, int arg1, int arg2)
    {
        actor.Special = special; actor.SpecialArgs[0] = id; actor.SpecialArgs[1] = arg1; actor.SpecialArgs[2] = arg2;
    }

    private static void Run(AuthoritySimulation sim, Actor actor, bool death)
    {
        var special = actor.Special;
        if (death) ActorDamage.Apply(actor, 1000, null);
        else Assert.True(ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(death ? 0 : special, actor.Special);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText, Namespace = "ZDoom",
        Lines = [new LevelLine { Tag = 7, Health = 100, HealthGroup = 4 }, new LevelLine { Tag = 8, Health = 70 }],
        Sectors = [new LevelSector { Tag = 9, CeilingHeight = 128, HealthFloor = 100, HealthFloorGroup = 4,
            HealthCeiling = 80, Health3D = 60 }],
        Things = [new LevelThing { Type = 3004, X = 20 }],
    });
}
