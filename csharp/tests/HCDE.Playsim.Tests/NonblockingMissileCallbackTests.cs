using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NonblockingMissileCallbackTests
{
    [Theory]
    [InlineData(false, -1, false)]
    [InlineData(false, 0, true)]
    [InlineData(false, 1, false)]
    [InlineData(false, 2, false)]
    [InlineData(true, -1, false)]
    [InlineData(true, 0, true)]
    [InlineData(true, 1, false)]
    [InlineData(true, 2, false)]
    public void SpecialAndTouchyOnlyTargetsReachMissileCallback(bool touchy, int result, bool stops)
    {
        var (_, missile, target) = Room(); target.SpecialPickup = !touchy; target.Touchy = touchy;
        var calls = 0; missile.SpecialMissileHitFunction = (_, victim) => { Assert.Same(target, victim); calls++; return result; };
        missile.Tick();
        Assert.Equal(1, calls); Assert.Equal(stops, missile.Destroyed); Assert.Equal(1000, target.Health);
    }

    [Fact]
    public void TargetWithoutNativeContactFlagsStillSkipsCallback()
    {
        var (_, missile, _) = Room(); var calls = 0;
        missile.SpecialMissileHitFunction = (_, _) => { calls++; return 0; };
        missile.Tick(); Assert.Equal(0, calls); Assert.False(missile.Destroyed);
    }

    [Theory]
    [InlineData(false, 1000)]
    [InlineData(true, 993)]
    public void CallbackCanMakeSpecialOnlyTargetBlockOrTakeDamage(bool shootable, int health)
    {
        var (_, missile, target) = Room(); target.SpecialPickup = true;
        missile.SpecialMissileHitFunction = (_, victim) => { victim.Solid = !shootable; victim.Shootable = shootable; return -1; };
        missile.Tick(); Assert.True(missile.Destroyed); Assert.Equal(health, target.Health);
    }

    [Theory]
    [InlineData("nonshootable")]
    [InlineData("ghost")]
    [InlineData("thruactors")]
    public void EarlierExclusionsStillPrecedeSpecialOnlyCallback(string gate)
    {
        var (_, missile, target) = Room(); target.SpecialPickup = true;
        if (gate == "nonshootable") target.NonShootable = true;
        if (gate == "ghost") { target.Ghost = true; missile.ThruGhost = true; }
        if (gate == "thruactors") target.ThruActors = true;
        var calls = 0; missile.SpecialMissileHitFunction = (_, _) => { calls++; return 0; };
        missile.Tick(); Assert.Equal(0, calls); Assert.False(missile.Destroyed);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Solid = target.Shootable = false;
        target.Health = 1000; target.NoPain = true; target.SeeState = -1;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = missile.VelocityZ = default; missile.DamageExpression = _ => 7;
        return (sim, missile, target);
    }
}
