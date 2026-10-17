using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VirtualReflectionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CallbackControlsAngleOrRejectsWithoutConsumingStockRandom(bool reject)
    {
        var (sim, missile, target) = Room(); var owner = missile.Owner; var calls = 0;
        missile.AdjustReflectionAngleFunction = (Actor self, Actor blocking, ref double angle) =>
        { Assert.Same(missile, self); Assert.Same(target, blocking); calls++; angle = 90; return reject; };
        Assert.Equal(!reject, ActorReflectionActions.ReflectOffActor(missile, target)); Assert.Equal(1, calls);
        Assert.Null(sim.CaptureState().ReflectRandomState); Assert.Same(reject ? owner : target, missile.Owner);
        if (!reject) { Assert.Equal(BamAngle.FromDegrees(90), missile.Angle); Assert.Equal(12.5, missile.VelocityY.ToDouble()); }
    }

    [Fact]
    public void VirtualOverrideCanReplaceDefaultDontReflectRule()
    {
        var (sim, _, target) = Room();
        var actor = new OverrideActor { Simulation = sim, Level = sim.Level, DontReflect = true, MovementSpeed = Fixed.FromInt(8) };
        Assert.True(ActorReflectionActions.ReflectOffActor(actor, target));
        Assert.Equal(1, actor.Calls); Assert.Equal(BamAngle.FromDegrees(90), actor.Angle); Assert.Equal(4, actor.VelocityY.ToDouble());
    }

    [Fact]
    public void ReflectorThruFlagSkipsVirtualAdjustment()
    {
        var (_, missile, target) = Room(); target.ThruReflect = true;
        missile.AdjustReflectionAngleFunction = (Actor _, Actor _, ref double _) => throw new InvalidOperationException("Must bypass");
        Assert.True(ActorReflectionActions.ReflectOffActor(missile, target)); Assert.Equal(2, missile.VelocityX.ToDouble());
    }

    [Fact]
    public void CallbackMayDelegateToStockAdjustment()
    {
        var (sim, missile, target) = Room();
        missile.AdjustReflectionAngleFunction = (Actor self, Actor blocking, ref double angle) =>
            ActorReflectionActions.AdjustReflectionAngle(self, blocking, ref angle);
        Assert.True(ActorReflectionActions.ReflectOffActor(missile, target)); Assert.NotNull(sim.CaptureState().ReflectRandomState);
    }

    [Fact]
    public void RejectedReflectionContinuesOrdinaryExplosionHandling()
    {
        var (_, missile, target) = Room(); missile.AdjustReflectionAngleFunction = (Actor _, Actor _, ref double _) => true;
        missile.Tick(); Assert.True(missile.Destroyed); Assert.Equal(993, target.Health);
    }

    [Fact]
    public void CallbackCanRemoveMissileWithoutExplosionSoundOrSecondDamage()
    {
        var (sim, missile, target) = Room(); sim.DrainSoundRequests();
        missile.AdjustReflectionAngleFunction = (Actor self, Actor _, ref double _) => { self.Destroy(); return false; };
        missile.Tick(); Assert.True(missile.Destroyed); Assert.Equal(993, target.Health); Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void CustomAimAdjustmentDoesNotImplicitlyRunStockFacingSideEffects()
    {
        var (_, missile, target) = Room(); target.AimReflect = true; missile.Ambush = true;
        missile.Angle = BamAngle.FromDegrees(30);
        missile.AdjustReflectionAngleFunction = (Actor _, Actor _, ref double angle) => { angle = 90; return false; };
        Assert.True(ActorReflectionActions.ReflectOffActor(missile, target));
        Assert.True(missile.Ambush); Assert.Equal(BamAngle.FromDegrees(30), missile.Angle);
        Assert.True(missile.VelocityX.Raw < 0);
    }

    private sealed class OverrideActor : Actor
    {
        public int Calls { get; private set; }
        public override bool AdjustReflectionAngle(Actor reflector, ref double angleDegrees)
        { Calls++; angleDegrees = 90; return false; }
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Reflective = true; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = missile.VelocityZ = default;
        missile.DamageExpression = _ => 7;
        return (sim, missile, target);
    }
}
