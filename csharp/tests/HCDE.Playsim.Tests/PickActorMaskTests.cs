using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickActorMaskTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Theory]
    [InlineData(2, true)]
    [InlineData(4, false)]
    [InlineData(6, true)]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(0x200, false)]
    public void SolidNonshootableActorUsesAnyRequestedFlag(int mask, bool expected)
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(64, 0);
        target.Shootable = false;
        var picked = CombatTrace.PickActor(sim, source, source.Angle, new BamAngle(0), 128, mask);
        if (expected) Assert.Same(target, picked); else Assert.Null(picked);
        Assert.Null(CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 128).Victim);
    }

    [Theory]
    [InlineData(0x200)]
    [InlineData(0x4000)]
    public void RepresentedMovementFlagsCanSelectNonshootableActors(int mask)
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(64, 0);
        target.Solid = target.Shootable = false;
        target.NoGravity = mask == 0x200; target.Floating = mask == 0x4000;
        Assert.Same(target, CombatTrace.PickActor(sim, source, source.Angle, new BamAngle(0), 128, mask));
    }

    [Fact]
    public void MissileMaskSelectsProjectileWithoutApplyingDamage()
    {
        var sim = Room(); var source = sim.Players.Single();
        var target = sim.SpawnProjectile(source, ProjectileKind.Plasma); target.X = Fixed.FromInt(64);
        Assert.Same(target, CombatTrace.PickActor(sim, source, source.Angle, new BamAngle(0), 128, 0x10000));
        Assert.False(target.Destroyed);
    }

    [Fact]
    public void AcsCanAssignTidToSolidNonshootableActor()
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(64, 0);
        target.Shootable = false;
        var stack = new List<int> { 0, 0, 0, 128 * 65536, 123, 2 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = source },
            [], new AcsGlobalStrings(), AcsCallFunctions.PickActor, 6, out var result));
        Assert.Equal(1, result);
        Assert.Equal(123, target.ThingId);
    }
}
