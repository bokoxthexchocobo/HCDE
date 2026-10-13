using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoPassMobjProbeTests
{
    [Theory]
    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void PositionAndActionProbesHonorSeparatedHeightCompatibility(bool legacy, bool above)
    {
        var sim = Room(legacy); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var blocker = sim.AddBot(0, 0); blocker.Brain = null;
        if (above) actor.Z = Fixed.FromInt(64);
        else blocker.Z = Fixed.FromInt(64);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(legacy, ActorPhysics.HasPositionBlocker(sim, actor));
        Assert.Equal(legacy ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CompatibilityProbePreservesActorExclusions(bool thruActors)
    {
        var sim = Room(true); var actor = sim.AddBot(0, 0); actor.Z = Fixed.FromInt(64);
        var blocker = sim.AddBot(0, 0);
        if (thruActors) blocker.ThruActors = true;
        else blocker.NoBlockmap = true;
        Assert.False(ActorPhysics.HasPositionBlocker(sim, actor));
        Assert.Null(ActorJumpActions.CheckBlock(actor, 2));
    }

    [Theory]
    [InlineData(false, false, false, true)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(true, true, false, false)]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    public void HeightPassageRequiresCanPassOrBridgeUnlessCompatibilityDisablesIt(
        bool canPass, bool bridge, bool legacy, bool blocked)
    {
        var sim = Room(legacy); var actor = sim.AddBot(-100, 0); actor.Brain = null;
        actor.CanPass = canPass; actor.Z = Fixed.FromInt(64); actor.OnGround = false;
        var blocker = sim.AddBot(0, 0); blocker.Brain = null; blocker.ActsLikeBridge = bridge;
        actor.X = default;
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked, ActorPhysics.HasPositionBlocker(sim, actor));
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2));
        Assert.Equal(before, SimSavegame.Write(sim));
        actor.X = Fixed.FromInt(-100);
        Assert.Equal(!blocked, ActorPhysics.TryMove(sim, actor, 0, 0, out _));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SpecialBlockerUsesStrictVerticalClearance(bool special, bool above)
    {
        var sim = Room(false); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var blocker = sim.AddBot(0, 0); blocker.Brain = null; blocker.SpecialPickup = special;
        if (above) actor.Z = blocker.Height;
        else blocker.Z = actor.Height;
        Assert.Equal(special, ActorPhysics.HasPositionBlocker(sim, actor));
        Assert.Equal(special ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2));
        actor.X = Fixed.FromInt(-100); actor.OnGround = false;
        Assert.Equal(!special, ActorPhysics.TryMove(sim, actor, 0, 0, out _));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void BridgeHorizontalContactsUseSolidityRatherThanHealth(bool dead, bool solid)
    {
        var sim = Room(false); var actor = sim.AddBot(0, 0); actor.Brain = null; actor.OnGround = false;
        var bridge = sim.AddBot(0, 0); bridge.Brain = null; bridge.ActsLikeBridge = true;
        if (dead) bridge.Health = 0;
        bridge.Corpse = false; bridge.Solid = solid;
        Assert.Equal(solid, ActorPhysics.HasPositionBlocker(sim, actor));
        Assert.Equal(solid ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2));
        actor.X = Fixed.FromInt(-100);
        Assert.Equal(!solid, ActorPhysics.TryMove(sim, actor, 0, 0, out _));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void OrdinaryNonCorpseContactUsesSolidityRatherThanHealth(bool dead, bool solid)
    {
        var sim = Room(false); var actor = sim.AddBot(0, 0); actor.Brain = null; actor.OnGround = false;
        var obstacle = sim.AddBot(0, 0); obstacle.Brain = null;
        if (dead) obstacle.Health = 0;
        obstacle.Corpse = false; obstacle.Solid = solid;
        Assert.Equal(solid, ActorPhysics.HasPositionBlocker(sim, actor));
        Assert.Equal(solid ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2));
        actor.X = Fixed.FromInt(-100);
        Assert.Equal(!solid, ActorPhysics.TryMove(sim, actor, 0, 0, out _));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void NonCorpseMoverCollisionUsesSolidityRatherThanHealth(bool dead, bool solid)
    {
        var sim = Room(false); var actor = sim.AddBot(-100, 0); actor.Brain = null;
        if (dead) actor.Health = 0;
        actor.Corpse = false; actor.Solid = solid; actor.OnGround = false;
        var obstacle = sim.AddBot(0, 0); obstacle.Brain = null;
        Assert.Equal(!solid, ActorPhysics.TryMove(sim, actor, 0, 0, out _));
        Assert.Equal(solid ? -100 : 0, actor.X.ToDouble());
    }

    private static AuthoritySimulation Room(bool legacy) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }]
    }, compat: legacy ? CompatSurface.NoPassMobj : CompatSurface.None);
}
