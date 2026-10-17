using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProbeTargetEligibilityTests
{
    [Theory]
    [InlineData(false, "ordinary", true)]
    [InlineData(false, "noclip", false)]
    [InlineData(false, "touchy", false)]
    [InlineData(true, "ordinary", true)]
    [InlineData(true, "noclip", false)]
    [InlineData(true, "touchy", false)]
    public void VerticalQuickAndFullProbesApplyNativeTargetExclusions(bool full, string kind, bool blocked)
    {
        var (sim, mover, target) = Room();
        target.NoClip = kind == "noclip"; target.Touchy = target.Armed = kind == "touchy";
        var saved = SimSavegame.Write(sim, true);
        Assert.Same(blocked ? target : null, ActorPhysics.FindZBlocker(sim, mover, quick: !full));
        Assert.Equal(saved, SimSavegame.Write(sim, true)); Assert.False(target.IsDead);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PositionProbeIgnoresNoClipTargetForBothBoundsModes(bool square)
    {
        var (sim, mover, target) = Room(); target.NoClip = true;
        var saved = SimSavegame.Write(sim, true);
        Assert.Null(ActorPhysics.FindPositionBlocker(sim, mover, out var line, squareBounds: square));
        Assert.False(line); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void FullVerticalProbeChoosesHighestEligibleTarget()
    {
        var (sim, mover, lower) = Room(); lower.Height = Fixed.FromInt(24);
        var noclip = sim.AddBot(0, 0); noclip.Brain = null; noclip.NoClip = true; noclip.Height = Fixed.FromInt(48);
        var touchy = sim.AddBot(0, 0); touchy.Brain = null; touchy.Touchy = true; touchy.Height = Fixed.FromInt(56);
        Assert.Same(lower, ActorPhysics.FindZBlocker(sim, mover, quick: false));
    }

    [Fact]
    public void TargetFlagsRestoreAndResumeProbeEligibility()
    {
        var (sim, mover, target) = Room(); target.Touchy = true;
        var saved = SimSavegame.Write(sim, true);
        target.Touchy = false; Assert.Same(target, ActorPhysics.FindZBlocker(sim, mover));
        SimSavegame.Apply(sim, saved); Assert.Null(ActorPhysics.FindZBlocker(sim, mover));
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var mover = sim.AddBot(0, 0, 3001); var target = sim.AddBot(0, 0, 3004);
        mover.Brain = target.Brain = null;
        return (sim, mover, target);
    }
}
