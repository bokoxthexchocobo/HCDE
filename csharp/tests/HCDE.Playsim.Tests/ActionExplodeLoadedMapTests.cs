using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActionExplodeLoadedMapTests
{
    [Fact]
    public void ExplodeDamagesNearbyActor()
    {
        var sim = CombatRoom();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var player = sim.Players.Single();
        player.X = Fixed.FromDouble(imp.X.ToDouble() + 48);
        player.Invulnerable = false;
        var health = player.Health;
        ActorExplodeActions.Explode(imp, 64, 128);
        Assert.True(player.Health < health);
    }

    [Fact]
    public void RadiusThrustPushesVictimAway()
    {
        var sim = CombatRoom();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var player = sim.Players.Single();
        player.X = Fixed.FromDouble(imp.X.ToDouble() + 32);
        var before = player.VelocityX.ToDouble();
        ActorExplodeActions.RadiusThrust(imp, force: 256, distance: 128);
        Assert.True(Math.Abs(player.VelocityX.ToDouble() - before) > 0.01);
    }

    [Fact]
    public void SplashGroupBlocksRadiusDamage()
    {
        var sim = CombatRoom();
        var source = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var victim = sim.Actors.Single(a => a.DoomEdNum == 3004);
        source.SplashGroup = 3;
        victim.SplashGroup = 3;
        victim.X = Fixed.FromDouble(source.X.ToDouble() + 32);
        var health = victim.Health;
        ActorExplodeActions.Explode(source, 80, 160);
        Assert.Equal(health, victim.Health);
    }

    [Fact]
    public void RadiusGiveAddsHealthPickupToPlayer()
    {
        var sim = CombatRoom();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        var player = sim.Players.Single();
        player.X = Fixed.FromDouble(imp.X.ToDouble() + 16);
        player.Health = 40;
        var before = player.Health;
        var given = ActorRadiusGiveActions.RadiusGive(imp, "Stimpack", 128,
            ActorRadiusGiveActions.RadiusGiveFlags.Players);
        Assert.True(given > 0);
        Assert.True(player.Health > before);
    }

    [Fact]
    public void DispatchInvokesExplodeAndRadiusActions()
    {
        var sim = CombatRoom();
        var imp = sim.Actors.Single(a => a.DoomEdNum == 3001);
        sim.Players.Single().Invulnerable = true;
        Assert.True(ActorActionDispatch.TryInvoke("A_Explode", imp));
        Assert.True(ActorActionDispatch.TryInvoke("A_RadiusThrust", imp));
        Assert.True(ActorActionDispatch.TryInvoke("A_RadiusGive", imp));
    }

    private static AuthoritySimulation CombatRoom()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.UdmfText,
            Namespace = "ZDoom",
            Sectors = [new LevelSector { CeilingHeight = 128, FloorHeight = 0 }],
            Sides = geometry.Sides,
            Lines = geometry.Lines,
            Things =
            [
                new LevelThing { Type = 1, X = 0, Y = 0 },
                new LevelThing { Type = 3001, X = 64, Y = 0 },
                new LevelThing { Type = 3004, X = 128, Y = 0 },
            ],
        });
    }
}
