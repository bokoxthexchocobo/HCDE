using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TargetSwitchContractTests
{
    [Fact]
    public void NullAndSelfRejectBeforeRandomDraws()
    {
        var (sim, hunter, _) = Room();
        Assert.False(hunter.OkayToSwitchTarget(null));
        Assert.False(hunter.OkayToSwitchTarget(hunter));
        Assert.Null(sim.NativeSwitchTargetRandomState);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void DistinctDetachedActorsAreNotSelfBecauseTheirIdsAreZero()
    {
        var hunter = new Actor { Shootable = true }; var other = new Actor { Shootable = true };
        Assert.Equal(hunter.Id, other.Id);
        Assert.True(hunter.OkayToSwitchTarget(other));
        Assert.False(hunter.OkayToSwitchTarget(hunter));
    }

    [Theory]
    [InlineData(0, false, true)]
    [InlineData(1, false, false)]
    [InlineData(1, true, true)]
    public void BrainlessPredicateUsesActorThreshold(int threshold, bool quick, bool eligible)
    {
        var (_, hunter, current) = Room(); hunter.Brain = null;
        hunter.SetChaseThreshold(threshold, false); hunter.QuickToRetaliate = quick;
        Assert.Equal(eligible, hunter.OkayToSwitchTarget(current));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BrainlessNoTargetSwitchUsesActorTargetPointer(bool hasTarget)
    {
        var (_, hunter, current) = Room(); hunter.Brain = null; hunter.NoTargetSwitch = true;
        hunter.SetTargetPointer(hasTarget ? current : null);
        Assert.Equal(!hasTarget, hunter.OkayToSwitchTarget(current));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DirectCurrentTargetPredicateStillEvaluatesRetention(bool invisible)
    {
        var (sim, hunter, current) = Room(); var (reference, _, _) = Room();
        hunter.TidToHate = current.ThingId = 7; hunter.SetTargetPointer(current);
        current.Invisible = invisible;
        for (var i = 0; i < 256; i++)
        {
            var retained = reference.NextSwitchTargetByte() < 128
                && (!invisible || reference.NextCheckSightByte() <= 50);
            Assert.Equal(!retained, hunter.OkayToSwitchTarget(current));
            Assert.Equal(reference.NativeSwitchTargetRandomState, sim.NativeSwitchTargetRandomState);
            Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DamageFromCurrentTargetRefreshesThresholdWithoutPredicateDraws(bool noSwitch)
    {
        var (sim, hunter, current) = Room(); hunter.NoTargetSwitch = noSwitch;
        hunter.TidToHate = current.ThingId = 7; hunter.SetTargetPointer(current);
        hunter.SetChaseThreshold(37, true); hunter.SetChaseThreshold(0, false);
        hunter.Health = 1000; hunter.NoPain = true; current.Invisible = true;
        Assert.Equal(1, ActorDamage.Apply(hunter, 1, current, DamageFlags.NoPain).HealthLost);
        Assert.Equal(current.Id, hunter.TargetId); Assert.Equal(37, hunter.ChaseThreshold);
        Assert.Null(sim.NativeSwitchTargetRandomState);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void SavedBrainlessPredicateRetainsTargetAndThreshold()
    {
        var (sim, hunter, current) = Room(); hunter.Brain = null;
        hunter.SetTargetPointer(current); hunter.NoTargetSwitch = true;
        hunter.SetChaseThreshold(11, false);
        var saved = SimSavegame.Write(sim, true);
        hunter.NoTargetSwitch = false; hunter.SetTargetPointer(null); hunter.SetChaseThreshold(0, false);
        SimSavegame.Apply(sim, saved);
        Assert.Null(hunter.Brain); Assert.Equal(current.Id, hunter.TargetId);
        Assert.Equal(11, hunter.ChaseThreshold); Assert.False(hunter.OkayToSwitchTarget(current));
    }

    private static (AuthoritySimulation, Actor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }] }, rngSeed: 42);
        var hunter = sim.AddBot(0, 0); var current = sim.AddBot(128, 0, 3001);
        hunter.SetChaseThreshold(0, false);
        return (sim, hunter, current);
    }
}
