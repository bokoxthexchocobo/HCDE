using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PuffBlockmapTraceTests
{
    [Theory]
    [InlineData(-1, false)]
    [InlineData(-1, true)]
    [InlineData(0x200, false)]
    [InlineData(0x200, true)]
    public void CosmeticPuffDoesNotHideLaterBlockmapActor(int mask, bool targetPresent)
    {
        var sim = Room(); var source = sim.Players.Single();
        var puff = sim.SpawnHitscanPuff(32, 0, 28, 77);
        Actor? target = targetPresent ? sim.AddBot(100, 0) : null;
        if (target is not null) target.NoGravity = true;
        Assert.Equal(20, puff.Radius.ToDouble()); Assert.Equal(16, puff.Height.ToDouble());
        Assert.Equal(target, CombatTrace.PickActor(sim, source, source.Angle, new BamAngle(0), 256, mask));
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 256,
            mask, requireDamageable: false);
        Assert.Equal(targetPresent, hit.Hit); Assert.Equal(target, hit.Victim);
        var stack = new List<int> { 987, 0, 0, 0, 256 << 16, 42, mask };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = source },
            [], new AcsGlobalStrings(), AcsCallFunctions.PickActor, 6, out var result));
        Assert.Equal(targetPresent ? 1 : 0, result); Assert.Equal(new[] { 987 }, stack);
        Assert.Equal(77, puff.ThingId);
        if (target is not null) Assert.Equal(42, target.ThingId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0x200)]
    public void ZeroDistanceQueryAlsoExcludesContainingPuff(int mask)
    {
        var sim = Room(); var source = sim.Players.Single(); sim.SpawnHitscanPuff(0, 0, 28);
        Assert.Null(CombatTrace.PickActor(sim, source, source.Angle, new BamAngle(0), 0, mask));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
