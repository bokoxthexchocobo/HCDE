using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpecialCarrierTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpecialCarrierDoesNotSelectOrCrushRider(bool special)
    {
        var (sim, carrier, rider) = Setup(); carrier.SpecialPickup = special;
        Assert.Equal(!special, ActorPhysics.IsStandingOn(carrier, rider));
        var health = rider.Health;
        ActorPhysics.CrushStandingRiders(sim, carrier, 10, 4);
        Assert.Equal(special ? health : health - 10, rider.Health);
    }

    [Fact]
    public void CarrierBecomingSpecialInvalidatesCapturedRiders()
    {
        var (sim, carrier, rider) = Setup(); Assert.True(ActorPhysics.IsStandingOn(carrier, rider));
        Actor[] riders = [rider]; carrier.SpecialPickup = true;
        var before = SimSavegame.Write(sim);
        ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, riders);
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NonsolidCarrierCannotSelectCrushOrCarry(bool solid)
    {
        var (sim, carrier, rider) = Setup(); carrier.Solid = solid;
        Assert.Equal(solid, ActorPhysics.IsStandingOn(carrier, rider));
        var health = rider.Health;
        ActorPhysics.CrushStandingRiders(sim, carrier, 10, 4);
        Assert.Equal(solid ? health - 10 : health, rider.Health);
        if (!solid)
        {
            var before = SimSavegame.Write(sim);
            ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, [rider]);
            Assert.Equal(before, SimSavegame.Write(sim));
        }
    }

    [Fact]
    public void CarrierLosingSolidityInvalidatesCapturedRiders()
    {
        var (sim, carrier, rider) = Setup(); Assert.True(ActorPhysics.IsStandingOn(carrier, rider));
        Actor[] riders = [rider]; carrier.Solid = false;
        var before = SimSavegame.Write(sim);
        ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, riders);
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(39, 39, true)]
    [InlineData(-39, -39, true)]
    [InlineData(40, 0, false)]
    [InlineData(0, -40, false)]
    [InlineData(40, 40, false)]
    public void RiderSelectionAndCrushUseStrictSquareOverlap(int x, int y, bool standing)
    {
        var (sim, carrier, rider) = Setup();
        carrier.Radius = rider.Radius = Fixed.FromInt(20);
        rider.X = Fixed.FromInt(x); rider.Y = Fixed.FromInt(y);
        Assert.Equal(standing, ActorPhysics.IsStandingOn(carrier, rider));
        var health = rider.Health;
        ActorPhysics.CrushStandingRiders(sim, carrier, 10, 4);
        Assert.Equal(standing ? health - 10 : health, rider.Health);
    }

    [Fact]
    public void CornerRiderMovementSurvivesSaveContinuation()
    {
        var (sim, carrier, rider) = Setup();
        carrier.Radius = rider.Radius = Fixed.FromInt(20);
        rider.X = rider.Y = Fixed.FromInt(39); carrier.VelocityX = Fixed.FromInt(2);
        var before = SimSavegame.Write(sim); sim.Tick(); var next = SimSavegame.Write(sim);
        Assert.True(rider.X.ToDouble() > 39);
        SimSavegame.Apply(sim, before); sim.Tick(); Assert.Equal(next, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    public void RiderPairRequiresCanPassOrMonsterOnEitherActor(int mode, bool eligible)
    {
        var (sim, carrier, rider) = Setup();
        carrier.CanPass = mode == 1; rider.CanPass = mode == 2;
        carrier.IsMonster = mode == 3; rider.IsMonster = mode == 4;
        Assert.Equal(eligible, ActorPhysics.IsStandingOn(carrier, rider));
        var health = rider.Health;
        ActorPhysics.CrushStandingRiders(sim, carrier, 10, 4);
        Assert.Equal(eligible ? health - 10 : health, rider.Health);
        if (!eligible)
        {
            var before = SimSavegame.Write(sim);
            ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, [rider]);
            Assert.Equal(before, SimSavegame.Write(sim));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CapturedRiderCollisionEligibilityIsRechecked(int mode)
    {
        var (sim, carrier, rider) = Setup(); Assert.True(ActorPhysics.IsStandingOn(carrier, rider));
        Actor[] riders = [rider];
        if (mode == 0) carrier.ThruActors = true;
        else if (mode == 1) rider.ThruActors = true;
        else if (mode == 2) rider.ThruSpecies = true;
        else rider.Solid = false;
        Assert.False(ActorPhysics.IsStandingOn(carrier, rider));
        var before = SimSavegame.Write(sim);
        ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, riders);
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpecialRiderRequiresBridgeCarrier(bool bridge)
    {
        var (sim, carrier, rider) = Setup(); carrier.ActsLikeBridge = bridge;
        rider.SpecialPickup = true;
        Assert.Equal(bridge, ActorPhysics.IsStandingOn(carrier, rider));
        var health = rider.Health;
        ActorPhysics.CrushStandingRiders(sim, carrier, 10, 4);
        Assert.Equal(bridge ? health - 10 : health, rider.Health);
        if (!bridge)
        {
            var before = SimSavegame.Write(sim);
            ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, [rider]);
            Assert.Equal(before, SimSavegame.Write(sim));
        }
    }

    [Fact]
    public void BridgeFlagRemovalInvalidatesCapturedSpecialRider()
    {
        var (sim, carrier, rider) = Setup(); rider.SpecialPickup = true;
        Assert.True(ActorPhysics.IsStandingOn(carrier, rider));
        Actor[] riders = [rider]; carrier.ActsLikeBridge = false;
        var before = SimSavegame.Write(sim);
        ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, riders);
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void CorpseCarrierRequiresFrozenRider(bool corpse, bool iceCorpse, bool eligible)
    {
        var (sim, carrier, rider) = Setup(); carrier.Corpse = corpse; rider.IceCorpse = iceCorpse;
        Assert.Equal(eligible, ActorPhysics.IsStandingOn(carrier, rider));
        var health = rider.Health;
        ActorPhysics.CrushStandingRiders(sim, carrier, 10, 4);
        Assert.Equal(eligible ? health - 10 : health, rider.Health);
        if (!eligible)
        {
            var before = SimSavegame.Write(sim);
            ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, [rider]);
            Assert.Equal(before, SimSavegame.Write(sim));
        }
    }

    [Fact]
    public void CarrierBecomingCorpseInvalidatesCapturedLivingRider()
    {
        var (sim, carrier, rider) = Setup(); Assert.True(ActorPhysics.IsStandingOn(carrier, rider));
        Actor[] riders = [rider]; carrier.Corpse = true;
        var before = SimSavegame.Write(sim);
        ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, riders);
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoBlockmapRiderIsExcludedFromSelectionAndCrush(bool noBlockmap)
    {
        var (sim, carrier, rider) = Setup(); rider.NoBlockmap = noBlockmap;
        Assert.Equal(!noBlockmap, ActorPhysics.IsStandingOn(carrier, rider));
        var health = rider.Health;
        ActorPhysics.CrushStandingRiders(sim, carrier, 10, 4);
        Assert.Equal(noBlockmap ? health : health - 10, rider.Health);
    }

    [Fact]
    public void RiderLeavingBlockmapInvalidatesCapturedCarry()
    {
        var (sim, carrier, rider) = Setup(); Assert.True(ActorPhysics.IsStandingOn(carrier, rider));
        Actor[] riders = [rider]; rider.NoBlockmap = true;
        var before = SimSavegame.Write(sim);
        ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, riders);
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void CorpseRiderRequiresFrozenCarrierRegardlessOfHealth(bool frozenCarrier, bool dead)
    {
        var (sim, carrier, rider) = Setup(); carrier.IceCorpse = frozenCarrier;
        if (dead) rider.Health = 0;
        rider.Corpse = true; rider.Solid = true;
        Assert.Equal(frozenCarrier, ActorPhysics.IsStandingOn(carrier, rider));
        if (!frozenCarrier)
        {
            var before = SimSavegame.Write(sim);
            ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 8, [rider]);
            Assert.Equal(before, SimSavegame.Write(sim));
        }
        else
        {
            ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 0, [rider]);
            Assert.Equal(8, rider.X.ToDouble());
        }
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void NonCorpseRiderUsesSolidityRatherThanHealth(bool dead, bool solid)
    {
        var (sim, carrier, rider) = Setup();
        if (dead) rider.Health = 0;
        rider.Corpse = false; rider.Solid = solid;
        Assert.Equal(solid, ActorPhysics.IsStandingOn(carrier, rider));
        var before = SimSavegame.Write(sim);
        ActorPhysics.CarryStandingRiders(sim, carrier, 8, 0, 0, [rider]);
        if (solid) Assert.Equal(8, rider.X.ToDouble());
        else Assert.Equal(before, SimSavegame.Write(sim));
    }

    private static (AuthoritySimulation Sim, Actor Carrier, Actor Rider) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var carrier = sim.AddBot(0, 0); carrier.Brain = null; carrier.Height = Fixed.FromInt(16);
        carrier.ActsLikeBridge = true;
        var rider = sim.AddBot(0, 0); rider.Brain = null; rider.Z = Fixed.FromInt(16);
        return (sim, carrier, rider);
    }
}
