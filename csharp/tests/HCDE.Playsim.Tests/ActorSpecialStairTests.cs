using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialStairTests
{
    [Theory]
    [InlineData(26)]
    [InlineData(27)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(204)]
    [InlineData(217)]
    [InlineData(270)]
    [InlineData(271)]
    [InlineData(272)]
    [InlineData(273)]
    public void DeathSpecialStartsTaggedStairMotion(int special)
    {
        var sim = Room(); var actor = sim.Actors[0]; Set(actor, special, 7);
        ActorDamage.Apply(actor, 1000, null);
        var motion = Assert.Single(sim.Motions);
        Assert.Equal(MotionKind.Stair, motion.Kind);
        Assert.Equal(special is 26 or 31 or 204 or 270 or 272 ? -2 : 2, motion.TargetFloor);
        Assert.Equal(0, actor.Special);
        sim.Tick(); Assert.Equal(special is 26 or 31 or 204 or 270 or 272 ? -1 : 1, sim.FloorOf(0));
    }

    [Theory]
    [InlineData(7, true)]
    [InlineData(0, false)]
    [InlineData(99, false)]
    public void ExplicitActivationReportsTargetResultAndClearsOnlyOnSuccess(int tag, bool expected)
    {
        var sim = Room(); var actor = sim.Actors[0]; Set(actor, 217, tag); actor.ActivationType = 32;
        Assert.Equal(expected, ActorSpecialActions.ActivateSpecial(sim, actor, null));
        Assert.Equal(expected ? 0 : 217, actor.Special);
        Assert.Equal(expected ? 1 : 0, sim.Motions.Count);
    }

    private static void Set(Actor actor, int special, int tag)
    {
        actor.Special = special; actor.SpecialArgs[0] = tag;
        actor.SpecialArgs[1] = 8; actor.SpecialArgs[2] = 2;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, X = 20 }],
    });
}
