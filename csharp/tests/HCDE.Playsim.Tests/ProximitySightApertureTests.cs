namespace HCDE.Playsim.Tests;

public class ProximitySightApertureTests
{
    [Fact]
    public void SightFlagUsesCandidateAsObserver()
    {
        var (sim, candidate) = NativeMeleeSightIntegrationTests.Room(); var reference = sim.Players.Single();
        sim.Floors[1] = 45; sim.Ceilings[1] = 47; candidate.Height = Fixed.FromInt(128);
        Assert.Equal(0, ActorProximity.CountProximity(reference, "DoomImp", 256, flags: 4096));
        candidate.Height = Fixed.FromInt(56);
        Assert.Equal(1, ActorProximity.CountProximity(reference, "DoomImp", 256, flags: 4096));
    }

    [Fact]
    public void SightFlagIgnoresReferenceVisibilityAndStillSetsSelectedPointer()
    {
        var (sim, candidate) = NativeMeleeSightIntegrationTests.Room(); var reference = sim.Players.Single();
        reference.Invisible = reference.MVisBlocked = true;
        Assert.True(ActorProximity.Check(reference, "DoomImp", 256, flags: 4096 | 64));
        Assert.Equal(candidate.Id, reference.TargetId); Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void MissingSightFlagRetainsCountAcrossClosedOpening()
    {
        var (sim, _) = NativeMeleeSightIntegrationTests.Room(); var reference = sim.Players.Single();
        sim.Ceilings[1] = 0;
        Assert.Equal(0, ActorProximity.CountProximity(reference, "DoomImp", 256, flags: 4096));
        Assert.Equal(1, ActorProximity.CountProximity(reference, "DoomImp", 256));
    }
}
