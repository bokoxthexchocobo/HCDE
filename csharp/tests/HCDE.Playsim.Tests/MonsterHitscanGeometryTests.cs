using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterHitscanGeometryTests
{
    private static AuthoritySimulation Room(int type = 3004, int health = 10000, int special = 0) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 100, HealthCeiling = 100 }],
        Sides = [new LevelSide { Sector = 0 }],
        Lines = [new LevelLine { X1 = 200, Y1 = 1000, X2 = 200, Y2 = -1000,
            SideFront = 0, SideBack = 0, Flags = LevelLine.BlockHitscanFlag, Health = health, Special = special }],
        Things = [new LevelThing { Type = type }, new LevelThing { Type = 1, X = 800 }],
    }, rngSeed: 42);

    [Theory]
    [InlineData(3004)]
    [InlineData(9)]
    [InlineData(65)]
    public void NativeMonsterProfilesDamageBlockingWallWithoutExtraRandomRolls(int type)
    {
        var sim = Room(type); var control = Room(type, health: 0);
        for (var tic = 0; tic < 45; tic++) { sim.Tick(); control.Tick(); }
        Assert.True(sim.Level.Lines[0].Health < 10000);
        Assert.Equal(100, sim.Players.Single().Health);
        Assert.Equal(control.CombatRandomState, sim.CombatRandomState);
        Assert.Equal(0, (10000 - sim.Level.Lines[0].Health) % 3);
    }

    [Theory]
    [InlineData(90, 0)]
    [InlineData(-90, 1)]
    public void HitscanDamagesFlatPlane(int pitch, int part)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 100, HealthCeiling = 100 }],
            Things = [new LevelThing { Type = 3004 }],
        });
        var source = Assert.Single(sim.Actors);
        var rng = sim.CombatRandomState;
        MonsterBrain.FireHitscan(sim, source, 256, 0, pitch, 7);
        Assert.Equal(part == 0 ? 93 : 100, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(part == 1 ? 93 : 100, sim.Level.Sectors[0].HealthCeiling);
        Assert.Equal(rng, sim.CombatRandomState);
    }

    [Fact]
    public void CloserActorPreventsWallDamage()
    {
        var sim = Room(); var target = sim.Players.Single(); target.X = Fixed.FromInt(100); var health = target.Health;
        var source = sim.Actors.First(actor => actor.DoomEdNum == 3004);
        MonsterBrain.FireHitscan(sim, source, 2048, 0, 0, 7);
        Assert.Equal(health - 7, target.Health);
        Assert.Equal(10000, sim.Level.Lines[0].Health);
    }

    [Fact]
    public void HorizonWallPreventsGeometryDamage()
    {
        var sim = Room(special: 9); var source = sim.Actors.Single(actor => actor.DoomEdNum == 3004);
        MonsterBrain.FireHitscan(sim, source, 2048, 0, 0, 7);
        Assert.Equal(10000, sim.Level.Lines[0].Health);
    }
}
