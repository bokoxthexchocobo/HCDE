using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MeleeRangeOverrideTests
{
    [Theory]
    [InlineData(-1, true)]
    [InlineData(-20, true)]
    [InlineData(0, false)]
    [InlineData(40, false)]
    [InlineData(41, true)]
    public void ExplicitRangeUsesNativeNegativeFallbackAndStrictBoundary(double range, bool expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 50 }],
        });
        var actor = sim.AddBot(0, 0); var target = sim.Players.Single();
        actor.MeleeRange = Fixed.FromInt(44); target.Radius = Fixed.FromInt(10);
        actor.Brain!.SetSpecialTarget(target);
        Assert.Equal(expected, ActorJumpActions.CheckMeleeRange(actor, range));
        Assert.Equal(44, actor.MeleeRange.ToDouble());
    }
}
