using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PEnemyActionsLoadedMapTests
{
    [Fact]
    public void Look2WakesOnLastHeardHostile()
    {
        var sim = Room();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var player = sim.Players.Single();
        imp.LastHeardTargetId = player.Id;
        ActorLook2Actions.Look2(imp);
        Assert.Equal(player.Id, imp.TargetId);
        Assert.Equal(10, imp.Brain!.Threshold);
    }

    [Fact]
    public void PainPlaysMonsterSound()
    {
        var sim = Room();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        imp.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Pain] = "imp/pain" };
        ActorSoundActions.Pain(imp);
        Assert.Contains(sim.DrainSoundRequests(), s => s.Sound.Contains("imp/pain", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BossDeathExitsWhenLastBossDies()
    {
        var sim = Room(exitOnBossDeath: true);
        var baron = sim.Actors.Single(a => a.DoomEdNum == 3003);
        BossDeathActions.BossDeath(baron);
        Assert.True(sim.Exited);
    }

    [Fact]
    public void ClassBossHealthScalesOnceInCoop()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = geometry.Sides,
            Lines = geometry.Lines,
            Things =
            [
                new LevelThing { Type = 1, X = 0, Y = 0 },
                new LevelThing { Type = 2, X = 32, Y = 0 },
                new LevelThing { Type = 3003, X = 64, Y = 0 },
            ],
        });
        var baron = sim.Actors.Single(a => a.DoomEdNum == 3003);
        var before = baron.Health;
        ActorClassBossHealthActions.ClassBossHealth(baron);
        ActorClassBossHealthActions.ClassBossHealth(baron);
        Assert.Equal(before * 5, baron.Health);
    }

    [Fact]
    public void ChaseNativeHonorsDontMoveFlag()
    {
        var sim = Room(nativeMovement: true);
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var player = sim.Players.Single();
        imp.Brain!.SetSpecialTarget(player);
        player.X = Fixed.FromDouble(256);
        ActorChaseActions.ChaseNative(imp, 0, 0, 16);
        Assert.Equal(0, imp.VelocityX.Raw);
    }

    [Fact]
    public void ClearLastHeardWipesLastHeardTarget()
    {
        var sim = Room();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        imp.LastHeardTargetId = sim.Players.Single().Id;
        ActorClearLastHeardActions.ClearLastHeard(imp);
        Assert.Null(imp.LastHeardTargetId);
    }

    [Fact]
    public void DispatchInvokesLook2AndPain()
    {
        var sim = Room();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        imp.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Pain] = "imp/pain" };
        Assert.True(ActorActionDispatch.TryInvoke("A_Look2", imp));
        Assert.True(ActorActionDispatch.TryInvoke("A_Pain", imp));
    }

    private static AuthoritySimulation Room(bool exitOnBossDeath = false, bool nativeMovement = false)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            ExitOnAllBossesDead = exitOnBossDeath,
            Sectors = [new LevelSector { CeilingHeight = 128, FloorHeight = 0 }],
            Sides = geometry.Sides,
            Lines = geometry.Lines,
            Things =
            [
                new LevelThing { Type = 1, X = 0, Y = 0 },
                new LevelThing { Type = 3001, X = 64, Y = 0 },
                new LevelThing { Type = 3003, X = 96, Y = 0 },
            ],
        }, spawnOptions: nativeMovement ? new SpawnOptions(NativeMonsterMovement: true) : null);
    }
}
