using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BfgImpactPlacementTests
{
    [Theory]
    [InlineData(false, 32)]
    [InlineData(false, 56)]
    [InlineData(false, 80)]
    [InlineData(true, 32)]
    [InlineData(true, 56)]
    [InlineData(true, 80)]
    public void EffectsUseVictimQuarterHeightWithoutInterpolatingFromFactoryPosition(bool radius, int height)
    {
        var sim = Room(); var owner = sim.Players.Single();
        var target = sim.Actors.Single(a => a.ThingId == 7);
        var effect = sim.Actors.Single(a => a.ThingId == 8);
        target.Z = Fixed.FromInt(8); target.Height = Fixed.FromInt(height);
        effect.FloorClipEnabled = true; effect.FloorClip = 10;
        if (radius)
        {
            owner.SetTargetPointer(target);
            ActorRadiusSelfActions.Apply(owner, 25, 500, spawnFlash: _ => effect);
        }
        else BfgSprayActions.Apply(sim, new Actor { Angle = BamAngle.FromDegrees(45) }, owner,
            numRays: 1, fixedDamage: 25, spawnSpray: _ => effect);
        Assert.Equal(target.X, effect.X); Assert.Equal(target.Y, effect.Y);
        Assert.Equal(8 + height / 4.0, effect.Z.ToDouble());
        Assert.Equal(effect.Pos(), effect.InterpolatedPosition(0));
        Assert.Equal(effect.Pos(), effect.InterpolatedPosition(0.5));
        Assert.False(effect.OnGround); Assert.Equal(0, effect.FloorClip);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Equal(effect.Pos(), fresh.Actors.Single(a => a.Id == effect.Id).Pos());
    }

    [Fact]
    public void SpeciesRejectedEffectIsPlacedBeforeDestructionWithoutDamageRolls()
    {
        var sim = Room(); var target = sim.Actors.Single(a => a.ThingId == 7);
        var effect = new Actor { MThruSpecies = true, X = Fixed.FromInt(-100) };
        target.Species = "DoomPlayer";
        BfgSprayActions.Apply(sim, new Actor { Angle = BamAngle.FromDegrees(45) }, sim.Players.Single(),
            numRays: 1, spawnSpray: _ => effect);
        Assert.True(effect.Destroyed); Assert.Equal(target.X, effect.X);
        Assert.Equal(target.Height.ToDouble() / 4, effect.Z.ToDouble());
        Assert.Null(sim.CaptureState().BfgSprayRandomState);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 512 }],
        });
        var target = sim.AddBot(200, 0, 3001, thingId: 7); target.Brain = null; target.Health = 500;
        var effect = sim.AddBot(500, 500, 3001, thingId: 8); effect.Brain = null;
        return sim;
    }
}
