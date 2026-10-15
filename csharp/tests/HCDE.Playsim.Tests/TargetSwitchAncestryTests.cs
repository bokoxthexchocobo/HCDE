using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TargetSwitchAncestryTests
{
    [Theory]
    [InlineData(3002, 58, false, false)]
    [InlineData(3002, 58, false, true)]
    [InlineData(3002, 58, true, false)]
    [InlineData(3002, 58, true, true)]
    [InlineData(3003, 69, false, false)]
    [InlineData(3003, 69, false, true)]
    [InlineData(3003, 69, true, false)]
    [InlineData(3003, 69, true, true)]
    public void ProtectionIncludesDescendantsButNotAncestors(int parent, int child, bool minion, bool reverse)
    {
        var (sim, hunter, other) = Room(reverse ? child : parent, reverse ? parent : child, minion);
        Assert.Equal(reverse, hunter.OkayToSwitchTarget(other, hunter.Brain!));
        Assert.Null(sim.NativeSwitchTargetRandomState);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    public void NativeExceptionsAllowRelatedClasses(bool minion, int exception)
    {
        var (_, hunter, other) = Room(3002, 58, minion);
        if (exception == 0) { hunter.TidToHate = 7; other.ThingId = 7; }
        else if (exception == 1) other.TidToHate = 9;
        else hunter.Friendly = true;
        Assert.True(hunter.OkayToSwitchTarget(other, hunter.Brain!));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SavedRelatedPointersRetainProtection(bool minion)
    {
        var (sim, hunter, other) = Room(3003, 69, minion);
        var saved = SimSavegame.Write(sim, true);
        hunter.MasterId = other.MasterId = null;
        SimSavegame.Apply(sim, saved);
        Assert.False(hunter.OkayToSwitchTarget(other, hunter.Brain!));
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DamageDoesNotRetargetToRelatedDescendant(bool minion)
    {
        var (_, hunter, other) = Room(3002, 58, minion);
        hunter.Health = 1000; hunter.NoPain = true;
        var damage = ActorDamage.Apply(hunter, 1, other, DamageFlags.Forced | DamageFlags.NoPain);
        Assert.Equal(1, damage.HealthLost);
        Assert.Null(hunter.Brain!.TargetId);
    }

    private static (AuthoritySimulation, Actor, Actor) Room(int masterType, int candidateType, bool minion)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var master = sim.AddBot(-128, 0, masterType);
        var hunter = sim.AddBot(0, 0, minion ? candidateType : 3004);
        var other = sim.AddBot(128, 0, minion ? 3004 : candidateType);
        if (minion) other.MasterId = master.Id; else hunter.MasterId = master.Id;
        hunter.SetChaseThreshold(0, false);
        return (sim, hunter, other);
    }
}
