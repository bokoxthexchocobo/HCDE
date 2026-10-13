using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CanPassSupportTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void GroundStepUsesCanPassOrMonsterBridgeException(bool playerActor, bool canPass)
    {
        var sim = Room(); Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(-40, 0);
        var support = Support(sim); support.ActsLikeBridge = !playerActor;
        ActorPropertyActions.ChangeFlag(actor, "CANPASS", canPass);
        Assert.Equal(canPass || !playerActor, ActorPhysics.TryMove(sim, actor, 0, 0, out _));
        Assert.True(actor.Solid); Assert.True(support.Solid);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void StandingSupportRequiresCanPass(bool canPass)
    {
        var sim = Room(); var actor = sim.Players.Single(); Support(sim);
        actor.X = default; actor.Z = Fixed.FromInt(16);
        ActorPropertyActions.ChangeFlag(actor, "CANPASS", canPass);
        ActorPhysics.FitToSector(sim, actor, carryFloor: false);
        Assert.Equal(canPass, actor.OnGround); Assert.Equal(canPass, actor.OnMobj);
        Assert.Equal(16, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FallingPlayerLandingRequiresCanPass(bool canPass)
    {
        var sim = Room(); var actor = sim.Players.Single(); Support(sim);
        actor.X = default; actor.Z = Fixed.FromInt(24); actor.OnGround = false;
        actor.VelocityZ = Fixed.FromInt(-8);
        ActorPropertyActions.ChangeFlag(actor, "CANPASS", canPass);
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(sim, actor);
        if (canPass) { Assert.Equal(16, actor.Z.ToDouble()); Assert.True(actor.OnMobj); }
        else { Assert.True(actor.Z.ToDouble() < 16); Assert.False(actor.OnMobj); }
    }

    [Fact]
    public void DisabledSupportSaveContinuationIsDeterministic()
    {
        var sim = Room(); var actor = sim.Players.Single(); Support(sim);
        actor.X = default; actor.Z = Fixed.FromInt(24); actor.OnGround = false;
        ActorPropertyActions.ChangeFlag(actor, "CANPASS", false);
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.False(actor.CanPass); Assert.False(actor.OnMobj);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void SpecialActorWithoutCanPassLandsOnlyOnBridge(bool playerActor, bool bridge)
    {
        var sim = Room(); Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(-40, 0);
        actor.Brain = null; actor.CanPass = false; actor.SpecialPickup = true;
        var support = Support(sim); support.ActsLikeBridge = bridge;
        actor.X = default; actor.Z = Fixed.FromInt(24); actor.OnGround = false;
        actor.VelocityZ = Fixed.FromInt(-8);
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(sim, actor);
        if (bridge)
        {
            Assert.Equal(16, actor.Z.ToDouble()); Assert.True(actor.OnMobj);
            Assert.Equal(0, actor.VelocityZ.ToDouble());
        }
        else
        {
            Assert.True(actor.Z.ToDouble() < 16); Assert.False(actor.OnMobj);
        }
        Assert.False(actor.CanPass);
    }

    [Fact]
    public void SpecialBridgeLandingSurvivesSaveContinuation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.SpecialPickup = true; actor.CanPass = false;
        actor.Z = Fixed.FromInt(24); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-8);
        Support(sim).ActsLikeBridge = true;
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.True(actor.OnMobj); Assert.False(actor.CanPass);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void NonPlayerLandingUsesCanPassAndSquareOverlap(bool canPass, bool corner)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        var support = Support(sim);
        actor.CanPass = canPass; actor.Z = Fixed.FromInt(24); actor.OnGround = false;
        actor.VelocityZ = Fixed.FromInt(-8);
        if (corner)
        {
            var offset = actor.Radius.ToDouble() + support.Radius.ToDouble() - 1;
            actor.X = Fixed.FromDouble(offset); actor.Y = Fixed.FromDouble(offset);
        }
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(sim, actor);
        Assert.Equal(canPass, actor.OnMobj);
        if (canPass) { Assert.Equal(16, actor.Z.ToDouble()); Assert.Equal(0, actor.VelocityZ.ToDouble()); }
        else Assert.True(actor.Z.ToDouble() < 16);
        Assert.False(support.ActsLikeBridge);
    }

    [Fact]
    public void NonPlayerDoesNotStepOntoOrdinaryActorFromFloor()
    {
        var sim = Room(); var actor = sim.AddBot(-40, 0); actor.Brain = null;
        Support(sim);
        Assert.False(ActorPhysics.TryMove(sim, actor, 0, 0, out _));
        Assert.Equal(0, actor.Z.ToDouble());
    }

    [Fact]
    public void NonPlayerOrdinaryActorLandingSaveContinuation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        Support(sim); actor.Z = Fixed.FromInt(24); actor.OnGround = false;
        actor.VelocityZ = Fixed.FromInt(-8);
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.Equal(16, actor.Z.ToDouble()); Assert.True(actor.OnMobj);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void NonPlayerCanLandOnPlayerUnlessPassThrough(bool canPass, bool thruActors)
    {
        var sim = Room(); var support = sim.Players.Single(); support.X = default;
        support.Height = Fixed.FromInt(16); support.ThruActors = thruActors;
        var actor = sim.AddBot(0, 0); actor.Brain = null; actor.CanPass = canPass;
        actor.Z = Fixed.FromInt(24); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-8);
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(sim, actor);
        var landed = canPass && !thruActors;
        Assert.Equal(landed, actor.OnMobj);
        if (landed) { Assert.Equal(16, actor.Z.ToDouble()); Assert.Equal(0, actor.VelocityZ.ToDouble()); }
        else Assert.True(actor.Z.ToDouble() < 16);
    }

    [Fact]
    public void NonPlayerOnPlayerSaveContinuation()
    {
        var sim = Room(); var support = sim.Players.Single(); support.X = default;
        support.Height = Fixed.FromInt(16);
        var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.Z = Fixed.FromInt(24); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-8);
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.Equal(16, actor.Z.ToDouble()); Assert.True(actor.OnMobj);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void UpwardSupportProbeUsesPredictedOverlap(bool playerActor, bool overlaps)
    {
        var sim = Room(); Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(0, 0);
        actor.Brain = null; actor.X = default; Support(sim);
        actor.Z = Fixed.FromInt(overlaps ? 12 : 24); actor.OnGround = false;
        actor.VelocityZ = Fixed.FromInt(2);
        ActorPhysics.Step(sim, actor);
        if (overlaps)
        {
            Assert.Equal(16, actor.Z.ToDouble()); Assert.Equal(0, actor.VelocityZ.ToDouble());
            Assert.True(actor.OnMobj);
        }
        else
        {
            Assert.Equal(26, actor.Z.ToDouble()); Assert.Equal(1, actor.VelocityZ.ToDouble());
        }
    }

    [Fact]
    public void UpwardActorSupportSaveContinuation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null; Support(sim);
        actor.Z = Fixed.FromInt(12); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(2);
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.Equal(16, actor.Z.ToDouble()); Assert.True(actor.OnMobj);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public void NoPassMobjDisablesLandingIncludingSpecialException(bool playerActor, bool special)
    {
        var sim = Room(CompatSurface.NoPassMobj);
        Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(0, 0);
        actor.Brain = null; actor.X = default; actor.SpecialPickup = special;
        Support(sim).ActsLikeBridge = special;
        actor.Z = Fixed.FromInt(24); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-8);
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(sim, actor);
        Assert.True(actor.Z.ToDouble() < 16); Assert.False(actor.OnMobj); Assert.True(actor.CanPass);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NoPassMobjDisablesGroundSteps(bool playerActor)
    {
        var sim = Room(CompatSurface.NoPassMobj);
        Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(-40, 0);
        Support(sim).ActsLikeBridge = !playerActor;
        Assert.False(ActorPhysics.TryMove(sim, actor, 0, 0, out _));
    }

    [Fact]
    public void NoPassMobjSaveContinuation()
    {
        var sim = Room(CompatSurface.NoPassMobj); var actor = sim.Players.Single();
        actor.X = default; Support(sim); actor.Z = Fixed.FromInt(24); actor.OnGround = false;
        actor.VelocityZ = Fixed.FromInt(-8);
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.False(actor.OnMobj); Assert.True(actor.CanPass);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void NoPassMobjBlocksHorizontalMovementAcrossSeparatedHeights(bool legacy, bool above)
    {
        var sim = Room(legacy ? CompatSurface.NoPassMobj : CompatSurface.None);
        var actor = sim.Players.Single(); var support = Support(sim);
        if (above) actor.Z = Fixed.FromInt(32);
        else support.Z = Fixed.FromInt(80);
        actor.OnGround = false;
        Assert.Equal(!legacy, ActorPhysics.TryMove(sim, actor, 0, 0, out _));
        Assert.Equal(legacy ? -40 : 0, actor.X.ToDouble());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NoPassMobjPreservesPassThroughExemptions(bool thruActor)
    {
        var sim = Room(CompatSurface.NoPassMobj); var actor = sim.Players.Single();
        var support = Support(sim); actor.Z = Fixed.FromInt(32); actor.OnGround = false;
        if (thruActor) support.ThruActors = true;
        else support.NoBlockmap = true;
        Assert.True(ActorPhysics.TryMove(sim, actor, 0, 0, out _));
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 2)]
    [InlineData(true, -2)]
    [InlineData(false, -2)]
    public void FloorLevelVerticalMovementStillProbesActorSupport(bool playerActor, int velocity)
    {
        var sim = Room(); Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(0, 0);
        actor.Brain = null; actor.X = default; actor.OnGround = false;
        Support(sim); actor.VelocityZ = Fixed.FromInt(velocity);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(16, actor.Z.ToDouble()); Assert.Equal(0, actor.VelocityZ.ToDouble());
        Assert.True(actor.OnMobj);
    }

    [Fact]
    public void FloorLevelVerticalSupportSaveContinuation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(2); Support(sim);
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.Equal(16, actor.Z.ToDouble()); Assert.True(actor.OnMobj);
    }

    [Theory]
    [InlineData(true, -2)]
    [InlineData(false, -2)]
    [InlineData(true, 2)]
    [InlineData(false, 2)]
    public void ExcessiveSupportStepStopsVerticalMotionWithoutSnapping(bool playerActor, int velocity)
    {
        var sim = Room(); Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(0, 0);
        actor.Brain = null; actor.X = default; actor.Z = Fixed.FromInt(24); actor.OnGround = false;
        Support(sim).Height = Fixed.FromInt(64); actor.VelocityZ = Fixed.FromInt(velocity);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(24, actor.Z.ToDouble()); Assert.Equal(0, actor.VelocityZ.ToDouble());
        Assert.True(actor.OnMobj);
    }

    [Fact]
    public void ExcessiveSupportStepSaveContinuation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.Z = Fixed.FromInt(24); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-2);
        Support(sim).Height = Fixed.FromInt(64);
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.Equal(24, actor.Z.ToDouble()); Assert.Equal(0, actor.VelocityZ.ToDouble());
        Assert.True(actor.OnMobj);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UnsnappedOnMobjClearsWhenBlockerOrEligibilityChanges(bool removeBlocker)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.Z = Fixed.FromInt(24); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-2);
        var support = Support(sim); support.Height = Fixed.FromInt(64);
        ActorPhysics.Step(sim, actor); Assert.True(actor.OnMobj);
        if (removeBlocker) support.X = Fixed.FromInt(200);
        else actor.CanPass = false;
        ActorPhysics.FitToSector(sim, actor, carryFloor: false);
        Assert.False(actor.OnMobj);
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 2)]
    [InlineData(true, -2)]
    [InlineData(false, -2)]
    public void InsufficientSupportHeadroomStillStopsVerticalMotion(bool playerActor, int velocity)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 96 }],
            Things = [new LevelThing { Type = 1, X = -40 }]
        });
        Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(0, 0);
        actor.Brain = null; actor.X = default; actor.Z = Fixed.FromInt(24); actor.OnGround = false;
        Support(sim).Height = Fixed.FromInt(64); actor.VelocityZ = Fixed.FromInt(velocity);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(24, actor.Z.ToDouble()); Assert.Equal(0, actor.VelocityZ.ToDouble());
        Assert.True(actor.Z.ToDouble() + actor.Height.ToDouble() <= 96);
        Assert.True(actor.OnMobj);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InsufficientHeadroomOnMobjRestoresAndClears(bool removeBlocker)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 72 }],
            Things = [new LevelThing { Type = 1, X = -40 }]
        });
        var actor = sim.Players.Single(); actor.X = default; actor.Z = Fixed.FromInt(8);
        actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-2);
        var support = Support(sim); support.Height = Fixed.FromInt(24);
        ActorPhysics.Step(sim, actor); Assert.True(actor.OnMobj);
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        Assert.True(actor.OnMobj);
        sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        if (removeBlocker) support.X = Fixed.FromInt(200);
        else actor.CanPass = false;
        ActorPhysics.FitToSector(sim, actor, carryFloor: false);
        Assert.False(actor.OnMobj);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SpecialBridgeDoesNotProvideStandingSupport(bool playerActor)
    {
        var sim = Room(); Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(0, 0);
        actor.Brain = null; actor.X = default; actor.Z = Fixed.FromInt(16);
        var support = Support(sim); support.ActsLikeBridge = true; support.SpecialPickup = true;
        ActorPhysics.FitToSector(sim, actor, carryFloor: false);
        Assert.False(actor.OnMobj); Assert.False(actor.OnGround);
        Assert.Null(ActorPhysics.FindZBlocker(sim, actor));
    }

    [Fact]
    public void SpecialBridgeDoesNotSupportNonPlayerAfterSaveRestore()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.Z = Fixed.FromInt(16); actor.OnGround = false;
        var support = Support(sim); support.ActsLikeBridge = true; support.SpecialPickup = true;
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        Assert.False(actor.OnMobj);
        sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.False(actor.OnMobj);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LegalStepSnapsBeforeLowCeilingFit(bool playerActor)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 72 }],
            Things = [new LevelThing { Type = 1, X = -40 }]
        });
        Actor actor = playerActor ? sim.Players.Single() : sim.AddBot(0, 0);
        actor.Brain = null; actor.X = default; actor.Height = Fixed.FromInt(56);
        actor.Z = Fixed.FromInt(8); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-2);
        Support(sim).Height = Fixed.FromInt(24);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(16, actor.Z.ToDouble()); Assert.Equal(0, actor.VelocityZ.ToDouble());
        Assert.True(actor.OnMobj);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public void FloatingActorUsesCanPassLanding(bool canPass, bool noGravity)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.Floating = true; actor.NoGravity = noGravity; actor.CanPass = canPass;
        actor.Z = Fixed.FromInt(24); actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-8);
        Support(sim);
        ActorPhysics.Step(sim, actor); ActorPhysics.Step(sim, actor);
        Assert.Equal(canPass, actor.OnMobj);
        if (canPass) { Assert.Equal(16, actor.Z.ToDouble()); Assert.Equal(0, actor.VelocityZ.ToDouble()); }
        else Assert.True(actor.Z.ToDouble() < 16);
    }

    [Fact]
    public void FloatingActorLandingSaveContinuation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.Floating = actor.NoGravity = true; actor.Z = Fixed.FromInt(24);
        actor.OnGround = false; actor.VelocityZ = Fixed.FromInt(-8); Support(sim);
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.True(actor.OnMobj); Assert.Equal(16, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FloatingSupportDoesNotPromoteGroundedActorOntoBridge(bool alreadyStanding)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.Floating = actor.NoGravity = true; actor.OnGround = true;
        actor.Z = Fixed.FromInt(alreadyStanding ? 16 : 0);
        Support(sim).ActsLikeBridge = true;
        ActorPhysics.FitToSector(sim, actor);
        Assert.Equal(alreadyStanding ? 16 : 0, actor.Z.ToDouble());
        Assert.Equal(alreadyStanding, actor.OnMobj);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(alreadyStanding ? 16 : 0, actor.Z.ToDouble());
    }

    [Fact]
    public void FloatingGroundedBridgeOverlapSaveContinuation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.Floating = actor.NoGravity = true; actor.OnGround = true;
        Support(sim).ActsLikeBridge = true;
        var bytes = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
        Assert.Equal(0, actor.Z.ToDouble()); Assert.False(actor.OnMobj);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NonPlayerNoGravityCannotGroundStepOntoBridge(bool noGravity)
    {
        var sim = Room(); var actor = sim.AddBot(-40, 0); actor.Brain = null;
        actor.NoGravity = noGravity; actor.OnGround = true; Support(sim).ActsLikeBridge = true;
        Assert.Equal(!noGravity, ActorPhysics.TryMove(sim, actor, 0, 0, out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoGravityBridgeSupportPreservesStandingWithoutGroundPromotion(bool standing)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.NoGravity = true; actor.OnGround = true; actor.Z = Fixed.FromInt(standing ? 16 : 0);
        Support(sim).ActsLikeBridge = true;
        ActorPhysics.FitToSector(sim, actor);
        Assert.Equal(standing ? 16 : 0, actor.Z.ToDouble()); Assert.Equal(standing, actor.OnMobj);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NonPlayerBridgeGroundStepRequiresMonsterFlag(bool monster)
    {
        var sim = Room(); var actor = sim.AddBot(-40, 0); actor.Brain = null;
        actor.IsMonster = monster; actor.OnGround = true; Support(sim).ActsLikeBridge = true;
        Assert.Equal(monster, ActorPhysics.TryMove(sim, actor, 0, 0, out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NonMonsterCanLandFromAboveWithoutGroundStepPromotion(bool above)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null; actor.IsMonster = false;
        Support(sim).ActsLikeBridge = true;
        actor.Z = Fixed.FromInt(above ? 24 : 0); actor.OnGround = !above;
        actor.VelocityZ = Fixed.FromInt(above ? -8 : 0);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(above ? 16 : 0, actor.Z.ToDouble()); Assert.Equal(above, actor.OnMobj);
    }

    [Theory]
    [InlineData(39, 39, true)]
    [InlineData(-39, -39, true)]
    [InlineData(40, 0, false)]
    [InlineData(0, 40, false)]
    public void MonsterBridgeSupportUsesStrictSquareBounds(int x, int y, bool supported)
    {
        var sim = Room(); var actor = sim.AddBot(x, y); actor.Brain = null;
        actor.Radius = Fixed.FromInt(20); actor.OnGround = true;
        var support = Support(sim); support.ActsLikeBridge = true; support.Radius = Fixed.FromInt(20);
        ActorPhysics.FitToSector(sim, actor);
        Assert.Equal(supported ? 16 : 0, actor.Z.ToDouble()); Assert.Equal(supported, actor.OnMobj);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ChargingMonsterCannotUseGroundBridgeStep(bool charging)
    {
        var sim = Room(); var actor = sim.AddBot(-40, 0); actor.OnGround = true;
        actor.Brain!.Enabled = false; actor.Brain.RestoreCharge(charging);
        Support(sim).ActsLikeBridge = true;
        Assert.Equal(!charging, ActorPhysics.TryMove(sim, actor, 0, 0, out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ChargingSupportDoesNotPromoteGroundedActor(bool standing)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.OnGround = true;
        actor.Brain!.Enabled = false; actor.Brain.RestoreCharge(true);
        actor.Z = Fixed.FromInt(standing ? 16 : 0); Support(sim).ActsLikeBridge = true;
        ActorPhysics.FitToSector(sim, actor);
        Assert.Equal(standing ? 16 : 0, actor.Z.ToDouble()); Assert.Equal(standing, actor.OnMobj);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MonsterBridgeSupportDoesNotRequireCanPass(bool canPass)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.CanPass = canPass; actor.OnGround = true; Support(sim).ActsLikeBridge = true;
        ActorPhysics.FitToSector(sim, actor);
        Assert.Equal(16, actor.Z.ToDouble()); Assert.True(actor.OnMobj); Assert.Equal(canPass, actor.CanPass);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    public void BridgeStandingSupportUsesCorpseFlagRatherThanHealth(bool dead, bool corpse, bool supported)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.Z = Fixed.FromInt(16); actor.OnGround = false;
        var support = Support(sim); support.ActsLikeBridge = true;
        if (dead) support.Health = 0;
        support.Corpse = corpse; support.Solid = true;
        ActorPhysics.FitToSector(sim, actor, carryFloor: false);
        Assert.Equal(supported, actor.OnMobj); Assert.Equal(supported, actor.OnGround);
    }

    private static Actor Support(AuthoritySimulation sim)
    {
        var actor = sim.AddBot(0, 0); actor.Brain = null; actor.Height = Fixed.FromInt(16); return actor;
    }

    private static AuthoritySimulation Room(CompatSurface compat = CompatSurface.None) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = -40 }] }, compat: compat);
}
