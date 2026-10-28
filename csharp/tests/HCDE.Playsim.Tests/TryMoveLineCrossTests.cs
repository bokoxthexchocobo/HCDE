using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TryMoveLineCrossTests
{
    [Fact]
    public void TryMoveCrossingActivatesPlayerCrossLineSpecial()
    {
        var sim = CrossRoom(special: 140, arg1: 3);
        var player = sim.Players.Single();
        player.X = Fixed.FromInt(-32);
        Assert.True(ActorPhysics.TryMove(sim, player, 32, 0, out _));
        Assert.Equal(3, sim.Level.Sectors[0].SoundSequence);
    }

    [Fact]
    public void TryMoveCrossingHonorsMonsterCrossFlag()
    {
        var sim = CrossRoom(special: LineSpecials.FloorLower, monsterCross: true);
        var monster = sim.AddBot(-32, 0);
        monster.MonsterCross = true;
        Assert.True(ActorPhysics.TryMove(sim, monster, 32, 0, out _));
        for (var i = 0; i < 6; i++) sim.Tick();
        Assert.True(sim.Floors[0] < 0);
    }

    [Fact]
    public void TryMoveCrossingSkipsProjectileWithoutMissileCross()
    {
        var sim = CrossRoom(special: LineSpecials.FloorLower);
        var player = sim.Players.Single();
        var projectile = sim.SpawnProjectile(player, ProjectileKind.Plasma);
        projectile.X = Fixed.FromInt(-32);
        projectile.ProjectileCross = false;
        Assert.True(ActorPhysics.TryMove(sim, projectile, 32, 0, out _));
        Assert.Equal(0, sim.Floors[0]);
    }

    private static AuthoritySimulation CrossRoom(int special, int arg1 = 0, bool monsterCross = false)
    {
        var line = new LevelLine
        {
            Index = 0,
            X1 = 0,
            Y1 = -128,
            X2 = 0,
            Y2 = 128,
            SideFront = 0,
            SideBack = 1,
            Special = special,
            Tag = 7,
            Arg0 = 7,
            Arg1 = arg1,
            PlayerCross = true,
            MonsterCross = monsterCross,
            PlayerUse = true,
            Repeat = true,
        };
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors =
            [
                new LevelSector { Tag = 7, CeilingHeight = 128 },
                new LevelSector { CeilingHeight = 128 },
            ],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [line],
            Things = [new LevelThing { Type = 1, X = -32 }],
        });
    }
}
