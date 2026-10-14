using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpecialMissileHitTests
{
    [Theory]
    [InlineData(-1, true, true)]
    [InlineData(0, true, false)]
    [InlineData(1, false, false)]
    [InlineData(2, true, true)]
    public void CallbackControlsTravelBeforeDirectDamage(int result, bool stops, bool damages)
    {
        var (_, missile, target) = Setup(); var calls = 0; var damageCalls = 0;
        missile.SpecialMissileHitFunction = (self, victim) =>
        { Assert.Same(missile, self); Assert.Same(target, victim); calls++; return result; };
        missile.DamageExpression = _ => { damageCalls++; return 7; };
        missile.Tick();
        Assert.True(calls > 0); Assert.Equal(stops, missile.Destroyed);
        Assert.Equal(damages ? 1 : 0, damageCalls); Assert.Equal(damages ? 993 : 1000, target.Health);
    }

    [Theory]
    [InlineData("owner", true)]
    [InlineData("spectral", true)]
    [InlineData("ghost", false)]
    [InlineData("nonshootable", false)]
    [InlineData("thruactors", false)]
    [InlineData("dontoverlap", true)]
    public void NativeOrderingGatesCallback(string rule, bool stops)
    {
        var (_, missile, target) = Setup();
        if (rule == "owner") { target.X = Fixed.FromInt(200); target = Assert.IsAssignableFrom<Actor>(missile.Owner); target.X = Fixed.FromInt(10); }
        target.Spectral = rule == "spectral";
        target.Ghost = missile.ThruGhost = rule == "ghost";
        target.NonShootable = rule == "nonshootable";
        target.ThruActors = rule == "thruactors";
        if (rule == "dontoverlap") { missile.CanPass = true; missile.DontOverlap = target.DontOverlap = true; }
        var calls = 0;
        missile.SpecialMissileHitFunction = (_, _) => { calls++; return 0; };
        missile.Tick();
        Assert.Equal(stops, missile.Destroyed);
        Assert.Equal(rule is "owner" or "spectral", calls > 0);
    }

    [Fact]
    public void CallbackCanRemoveMissileWithoutDoubleImpact()
    {
        var (sim, missile, target) = Setup(); sim.DrainSoundRequests();
        missile.SpecialMissileHitFunction = (self, _) => { self.Destroy(); return 0; };
        missile.Tick();
        Assert.True(missile.Destroyed); Assert.Equal(1000, target.Health); Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void CallbackCanSpawnActorWithoutInvalidatingContactScan()
    {
        var (sim, missile, _) = Setup(); var spawned = false;
        missile.SpecialMissileHitFunction = (_, _) =>
        { if (!spawned) { sim.AddBot(200, 0, 3001).Brain = null; spawned = true; } return 1; };
        missile.Tick(); Assert.True(spawned); Assert.False(missile.Destroyed);
    }

    [Theory]
    [InlineData(false, false, true, false)]
    [InlineData(true, false, true, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, true, true, true)]
    [InlineData(false, false, false, true)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(true, true, false, true)]
    public void CallbackFlagChangesUseNativeAttackAndSolidityOrdering(bool solid, bool shootable, bool canHarm, bool stops)
    {
        var (_, missile, target) = Setup();
        target.ProjectileGroup = canHarm ? -1 : 7;
        Assert.IsAssignableFrom<Actor>(missile.Owner).ProjectileGroup = 7;
        var damageCalls = 0;
        missile.DamageExpression = _ => { damageCalls++; return 7; };
        missile.SpecialMissileHitFunction = (_, victim) =>
        { victim.Solid = solid; victim.Shootable = shootable; return -1; };
        missile.Tick();
        Assert.Equal(stops, missile.Destroyed);
        Assert.Equal(canHarm && shootable ? 1 : 0, damageCalls);
        Assert.Equal(canHarm && shootable ? 993 : 1000, target.Health);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void MovementCapturesRippingButDamageUsesCurrentFlag(bool initialRip, bool dontRip)
    {
        var (sim, missile, target) = Setup();
        missile.Rip = initialRip; target.DontRip = dontRip;
        missile.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Rip] = "custom/rip" };
        missile.SpecialMissileHitFunction = (self, _) => { self.Rip = !initialRip; return -1; };
        missile.DamageExpression = _ => 7;
        sim.DrainSoundRequests();
        missile.Tick();
        Assert.Equal(!initialRip || dontRip, missile.Destroyed);
        Assert.Equal(!initialRip, missile.Rip); Assert.Equal(993, target.Health);
        var sounds = sim.DrainSoundRequests();
        if (!initialRip) Assert.Equal("custom/rip", Assert.Single(sounds).Sound);
        else Assert.Empty(sounds);
    }

    private static (AuthoritySimulation, ProjectileActor, Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
