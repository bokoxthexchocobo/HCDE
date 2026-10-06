using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorRadiusSelfFlashTests
{
    [Theory]
    [InlineData(false, 100)]
    [InlineData(true, 75)]
    public void FlashFoilInvulnerabilityControlsDamage(bool foil, int expected)
    {
        var (source, target) = Actors(); target.Invulnerable = true; target.SetDamageFactor("Fire", 0.5);
        var flash = new Actor { FoilInvul = foil, DamageType = "Fire" };
        ActorRadiusSelfActions.Apply(source, 100, 100, spawnFlash: victim =>
        { Assert.Same(target, victim); return flash; });
        Assert.Equal(expected, target.Health);
        Assert.Equal(target.X, flash.X); Assert.Equal(target.Y, flash.Y);
        Assert.Equal(target.Height.ToDouble() / 4, flash.Z.ToDouble());

    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    public void FlashFoilBuddhaAllowsLethalDamage(bool foil, int expected)
    {
        var (source, target) = Actors(); target.Buddha = true;
        ActorRadiusSelfActions.Apply(source, 200, 100, spawnFlash: _ => new Actor { FoilBuddha = foil });
        Assert.Equal(expected, target.Health);
    }

    [Fact]
    public void NullFlashRetainsDefaultDamageContext()
    {
        var (source, target) = Actors();
        ActorRadiusSelfActions.Apply(source, 100, 100, spawnFlash: _ => null);
        Assert.Equal(50, target.Health);
    }

    [Fact]
    public void FlashFactoryRunsOnlyInsideRadius()
    {
        var (source, target) = Actors(); target.X = Fixed.FromInt(100); var calls = 0;
        ActorRadiusSelfActions.Apply(source, 100, 100, spawnFlash: _ => { calls++; return new Actor(); });
        Assert.Equal(0, calls); Assert.Equal(100, target.Health);
    }

    private static (Actor Source, Actor Target) Actors()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 50 }] });
        var source = sim.AddBot(0, 0); var target = sim.AddBot(50, 0, 3001, thingId: 7); target.Health = 100; target.DoHarmSpecies = true;
        source.Brain!.SetTargetThingId(sim, 7); return (source, target);
    }
}
