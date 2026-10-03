using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TargetTraceRangeTests
{
    private static AuthoritySimulation Room(int ceiling = 256) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = ceiling }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Theory]
    [InlineData(100, false)]
    [InlineData(140, true)]
    public void ElevatedTargetUsesThreeDimensionalRange(double range, bool hit)
    {
        var sim = Room(); var source = CenterOrigin(sim); var target = sim.AddBot(100, 0);
        target.Z = Fixed.FromInt(100);
        Assert.Equal(hit, ReferenceEquals(target, CombatTrace.FindTarget(sim, source, range, pitchOffset: -45)));
    }

    [Theory]
    [InlineData(51, false)]
    [InlineData(53, true)]
    public void VerticalTargetUsesPitchOffsetWithoutClamping(double range, bool hit)
    {
        var sim = Room(); var source = CenterOrigin(sim); var target = sim.AddBot(0, 0);
        target.Z = Fixed.FromInt(80);
        Assert.Equal(hit, ReferenceEquals(target, CombatTrace.FindTarget(sim, source, range, pitchOffset: -90)));
    }

    [Fact]
    public void OrdinaryCeilingIsResolvedAfterSuccessfulActorTraversal()
    {
        var sim = Room(128); var target = sim.AddBot(200, 0); target.Z = Fixed.FromInt(200);
        Assert.Same(target, CombatTrace.FindTarget(sim, CenterOrigin(sim), 1024, pitchOffset: -45));
    }

    [Theory]
    [InlineData(90)]
    [InlineData(450)]
    public void YawOffsetsWrap(double offset)
    {
        var sim = Room(); var target = sim.AddBot(0, 100);
        Assert.Same(target, CombatTrace.FindTarget(sim, CenterOrigin(sim), 256, offset));
    }

    [Fact]
    public void QueryHasNoDamagePuffOrRandomSideEffects()
    {
        var sim = Room(); var target = sim.AddBot(100, 0); var health = target.Health;
        var count = sim.Actors.Count; var rng = sim.CombatRandomState;
        Assert.Same(target, CombatTrace.FindTarget(sim, CenterOrigin(sim), 256));
        Assert.Equal(health, target.Health); Assert.Equal(count, sim.Actors.Count);
        Assert.Equal(rng, sim.CombatRandomState);
    }
    // These geometry fixtures intentionally trace from the actor center.
    private static PlayerPawn CenterOrigin(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.AttackZOffset = new Fixed(0); return player;
    }
}
