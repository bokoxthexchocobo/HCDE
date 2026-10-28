using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPmobjTickBridgeTests
{
    [Fact]
    public void UsesManagedTick_IsFalseForProjectilesAndPlayers()
    {
        var sim = OpenRoom();
        var player = sim.Players.Single();
        var projectile = sim.SpawnProjectile(player, ProjectileKind.Rocket);
        var bot = sim.AddBot(32, 32);
        var brainless = sim.AddBot(48, 48);
        brainless.Brain = null;
        Assert.False(ActorPmobjTickBridge.UsesManagedTick(projectile));
        Assert.False(ActorPmobjTickBridge.UsesManagedTick(player));
        Assert.True(ActorPmobjTickBridge.UsesManagedTick(bot));
        Assert.False(ActorPmobjTickBridge.UsesManagedTick(brainless));
    }

    [Fact]
    public void LiveActorTick_ConsumesFreezeTicsThroughBridge()
    {
        var sim = OpenRoom();
        var bot = sim.AddBot(64, 64);
        bot.Brain!.Enabled = false;
        bot.FreezeTics = 2;
        bot.Tick();
        Assert.Equal(1, bot.FreezeTics);
        bot.Tick();
        Assert.Equal(0, bot.FreezeTics);
    }

    [Fact]
    public void LiveActorTick_NoInteraction_ClearsInScrollSector()
    {
        var sim = OpenRoom();
        var bot = sim.AddBot(64, 64);
        bot.Brain!.Enabled = false;
        bot.NoInteraction = true;
        bot.InScrollSector = true;
        bot.Tick();
        Assert.False(bot.InScrollSector);
    }

    [Fact]
    public void Execute_RoutesThroughVmPMobjActorTick()
    {
        var sim = OpenRoom();
        var bot = sim.AddBot(64, 64);
        bot.Brain!.Enabled = false;
        bot.VerticalFriction = true;
        bot.VelocityZ = Fixed.FromDouble(0.1);
        ActorPmobjTickBridge.Execute(sim, bot);
        Assert.False(bot.VerticalFriction);
    }

    static AuthoritySimulation OpenRoom() =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 0 }],
        });

}
