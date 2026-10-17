using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SupportTargetEligibilityTests
{
    [Theory]
    [InlineData("ordinary", true)]
    [InlineData("noclip", false)]
    [InlineData("touchy", false)]
    [InlineData("ridernoclip", false)]
    public void PhysicalStandingFlagsExcludeTargetsWithoutChangingGeometricRelation(string kind, bool supported)
    {
        var (sim, rider, support) = Room(kind);
        ActorPhysics.FitToSector(sim, rider, carryFloor: false);
        Assert.Equal(supported, rider.OnMobj); Assert.Equal(supported, rider.OnGround);
        Assert.True(ActorPhysics.IsStandingOn(support, rider));
    }

    [Theory]
    [InlineData("noclip")]
    [InlineData("touchy")]
    public void ExcludedSupportDoesNotStopDownwardMotion(string kind)
    {
        var (sim, rider, support) = Room(kind); rider.VelocityZ = Fixed.FromInt(-1);
        ActorPhysics.Step(sim, rider);
        Assert.True(rider.Z.ToDouble() < 16); Assert.True(rider.VelocityZ.Raw < 0);
        Assert.False(rider.OnMobj); Assert.False(rider.OnGround); Assert.False(support.IsDead);
    }

    [Theory]
    [InlineData("noclip")]
    [InlineData("touchy")]
    public void ExcludedCarrierDoesNotMoveRiderHorizontally(string kind)
    {
        var (sim, rider, support) = Room(kind); support.VelocityX = Fixed.FromInt(8);
        ActorPhysics.Step(sim, support);
        Assert.Equal(8, support.X.ToDouble()); Assert.Equal(0, rider.X.Raw);
    }

    [Theory]
    [InlineData("noclip")]
    [InlineData("touchy")]
    public void SupportExclusionAndFallingReplayAfterRestore(string kind)
    {
        var (sim, rider, _) = Room(kind); rider.VelocityZ = Fixed.FromInt(-1);
        var saved = SimSavegame.Write(sim, true);
        ActorPhysics.Step(sim, rider);
        var (restored, loaded, _) = Room(kind); SimSavegame.Apply(restored, saved); ActorPhysics.Step(restored, loaded);
        Assert.Equal(rider.Z, loaded.Z); Assert.Equal(rider.VelocityZ, loaded.VelocityZ);
        Assert.False(loaded.OnMobj);
        sim.RestoreState(sim.CaptureState()); restored.RestoreState(restored.CaptureState());
        Assert.Equal(sim.Checksum, restored.Checksum);
    }

    [Theory]
    [InlineData("ordinary", 90)]
    [InlineData("noclip", 100)]
    [InlineData("touchy", 100)]
    public void ExcludedSupportDoesNotTransmitCrushDamage(string kind, int health)
    {
        var (sim, rider, support) = Room(kind); rider.NoPain = true;
        ActorPhysics.CrushStandingRiders(sim, support, 10, sim.Thinkers.Clock.Tic, "Crush");
        Assert.Equal(health, rider.Health);
    }

    [Fact]
    public void NoClipRiderDoesNotFollowOrdinaryCarrier()
    {
        var (sim, rider, support) = Room("ridernoclip");
        ActorPhysics.CarryStandingRiders(sim, support, 8, 0, 8, [rider]);
        Assert.Equal(0, rider.X.Raw); Assert.Equal(16, rider.Z.ToDouble());
    }

    private static (AuthoritySimulation Sim, PlayerPawn Rider, Actor Support) Room(string kind)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
        var rider = sim.Players.Single(); var support = sim.AddBot(0, 0); support.Brain = null;
        ActorPropertyActions.SetSize(support, 20, 16); support.NoGravity = true;
        rider.Z = Fixed.FromInt(16);
        if (kind == "noclip") support.NoClip = true;
        if (kind == "touchy") support.Touchy = true;
        if (kind == "ridernoclip") rider.NoClip = true;
        return (sim, rider, support);
    }
}
