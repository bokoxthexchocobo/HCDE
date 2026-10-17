using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileTouchyTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void OnlySolidMissileDetonatesArmedTargetBeforeBumpAndDamage(bool solid, bool dies)
    {
        var (sim, missile, target) = Room(); missile.Solid = solid;
        var damageCalls = 0; missile.DamageExpression = _ => { damageCalls++; return 7; };
        missile.Tick();
        Assert.Equal(dies, target.IsDead); Assert.Equal(!dies, target.Armed);
        Assert.Equal(dies ? 0 : 1, damageCalls); Assert.Equal(dies ? 0 : 36, target.LastBumpTic);
        Assert.Equal(!dies, missile.Destroyed);
    }

    [Theory]
    [InlineData("unarmed")]
    [InlineData("minemaster")]
    [InlineData("missilemaster")]
    [InlineData("sameclass")]
    [InlineData("species")]
    public void TouchExclusionsRetainSubsequentMissilePassage(string policy)
    {
        var (_, missile, target) = Room(); missile.Solid = true; target.NonShootable = true;
        if (policy == "unarmed") target.Armed = false;
        if (policy == "minemaster") target.MasterId = missile.Id;
        if (policy == "missilemaster") missile.MasterId = target.Id;
        if (policy == "sameclass") missile.DefinitionDoomEdNum = target.ClassDoomEdNum;
        if (policy == "species")
        {
            target.DontHarmSpecies = true; target.Species = missile.Species = "touch/test";
        }
        missile.Tick();
        Assert.False(target.IsDead); Assert.False(missile.Destroyed); Assert.Equal(36, target.LastBumpTic);
    }

    [Fact]
    public void ForcedTouchDeathPrecedesNonshootableExclusionAndBypassesInvulnerability()
    {
        var (_, missile, target) = Room(); missile.Solid = true; target.NonShootable = target.Invulnerable = true;
        missile.Tick();
        Assert.True(target.IsDead); Assert.False(target.Armed); Assert.False(missile.Destroyed);
        Assert.Equal(0, target.LastBumpTic);
    }

    [Fact]
    public void SentientTargetDoesNotRequireArmedFlag()
    {
        var (_, missile, target) = Room(); missile.Solid = true; target.Armed = false; target.SeeState = 0;
        missile.Tick(); Assert.True(target.IsDead);
    }

    [Fact]
    public void EarlierThroughActorsGateSuppressesTouchDeathAndBump()
    {
        var (_, missile, target) = Room(); missile.Solid = true; target.ThruActors = true;
        missile.Tick(); Assert.False(target.IsDead); Assert.True(target.Armed); Assert.Equal(0, target.LastBumpTic);
    }

    [Fact]
    public void TouchyOnlyTargetHonorsEarlierDontOverlapReturn()
    {
        var (_, missile, target) = Room(); target.Solid = target.Shootable = false;
        missile.Solid = missile.CanPass = true; missile.DontOverlap = target.DontOverlap = true;
        missile.Tick();
        Assert.False(target.IsDead); Assert.True(target.Armed); Assert.Equal(0, target.LastBumpTic);
        Assert.True(missile.Destroyed);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        target.Touchy = target.Armed = target.BumpSpecial = true; target.SeeState = -1;
        target.ActivationType = 16 | 1024; target.Special = 0;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = missile.VelocityZ = default;
        missile.DamageExpression = _ => 7; sim.Thinkers.Clock.Restore(1);
        return (sim, missile, target);
    }
}
