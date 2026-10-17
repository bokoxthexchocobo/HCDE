using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileBumpSpecialTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(8, false)]
    [InlineData(16, true)]
    public void MissileTriggerFlagControlsBumpBeforeImpact(int activation, bool runs)
    {
        var (sim, missile, target) = Room(); target.ActivationType = activation;
        missile.Tick();
        Assert.Equal(runs ? 200 : 128, sim.LightOf(0)); Assert.Equal(runs ? 36 : 0, target.LastBumpTic);
        Assert.True(missile.Destroyed); Assert.Equal(993, target.Health);
    }

    [Theory]
    [InlineData("nonshootable")]
    [InlineData("ghost")]
    [InlineData("passheight")]
    public void BumpPrecedesMissileSpecificPassage(string policy)
    {
        var (sim, missile, target) = Room();
        if (policy == "nonshootable") target.NonShootable = true;
        if (policy == "ghost") { target.Ghost = true; missile.ThruGhost = true; }
        if (policy == "passheight") target.ProjectilePassHeight = Fixed.FromInt(1);
        missile.Tick();
        Assert.Equal(200, sim.LightOf(0)); Assert.Equal(36, target.LastBumpTic);
        Assert.False(missile.Destroyed); Assert.Equal(1000, target.Health);
    }

    [Theory]
    [InlineData("thruactors")]
    [InlineData("noblockmap")]
    [InlineData("height")]
    [InlineData("dontoverlap")]
    public void EarlierContactGatesSuppressBump(string policy)
    {
        var (sim, missile, target) = Room();
        if (policy == "thruactors") target.ThruActors = true;
        if (policy == "noblockmap") target.NoBlockmap = true;
        if (policy == "height") { missile.CanPass = true; target.Z = Fixed.FromInt(80); }
        if (policy == "dontoverlap") { missile.CanPass = true; missile.DontOverlap = target.DontOverlap = true; }
        missile.Tick();
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, target.LastBumpTic);
    }

    [Fact]
    public void BumpCallbackCanRemoveMissileBeforeDamage()
    {
        var (sim, missile, target) = Room(); target.Special = 132; // Thing_Remove, TID zero uses trigger.
        target.SpecialArgs[0] = 0;
        missile.Tick();
        Assert.True(missile.Destroyed); Assert.Equal(1000, target.Health);
        Assert.Equal(36, target.LastBumpTic);
    }

    [Fact]
    public void RipperSubstepsShareSuccessfulBumpCooldown()
    {
        var (sim, missile, target) = Room(); missile.Rip = true; missile.VelocityX = Fixed.FromInt(8);
        target.ActivationType = 16 | 1024; target.Special = 0;
        missile.Tick();
        Assert.Equal(36, target.LastBumpTic); Assert.Equal(16 | 512 | 1024, target.ActivationType);
        Assert.False(missile.Destroyed);
    }

    [Fact]
    public void InclusiveSpecialHeightTouchAtExcludedHorizontalEdgeDoesNotBump()
    {
        var (sim, missile, target) = Room(); target.SpecialPickup = true; missile.CanPass = true;
        target.X = Fixed.FromInt(-32); target.Z = Fixed.FromInt(29); missile.VelocityZ = Fixed.FromInt(2);
        missile.Tick();
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(0, target.LastBumpTic);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128, LightLevel = 128, Tag = 7 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        target.BumpSpecial = true; target.ActivationType = 16; target.Special = 112;
        target.SpecialArgs[0] = 7; target.SpecialArgs[1] = 200;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = missile.VelocityZ = default; missile.DamageExpression = _ => 7;
        sim.Thinkers.Clock.Restore(1);
        return (sim, missile, target);
    }
}
