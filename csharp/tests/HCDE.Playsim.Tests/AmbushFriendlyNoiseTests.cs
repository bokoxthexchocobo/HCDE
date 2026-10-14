using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AmbushFriendlyNoiseTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void AmbushSightGateAppliesToHostileNoiseOnly(bool friendly, bool remembered)
    {
        var (sim, actor, source) = Room(friendly); Assert.False(CombatTrace.HasLineOfSight(sim, actor, source));
        if (remembered)
        {
            actor.LastHeardTargetId = source.Id; actor.Brain!.Tick(sim, actor);
        }
        else actor.Brain!.Hear(sim, actor, source);
        Assert.Equal(friendly ? 1 : 0, actor.States.Current); Assert.Null(actor.TargetId);
    }

    [Fact]
    public void OrdinaryHostileHearingDoesNotRequireSight()
    {
        var (sim, actor, source) = Room(false); actor.Ambush = false;
        actor.Brain!.Hear(sim, actor, source); Assert.Equal(source.Id, actor.TargetId); Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void SavedFriendlyNoiseBypassesAmbushSightGate()
    {
        var (sim, actor, source) = Room(true); actor.LastHeardTargetId = source.Id;
        var saved = SimSavegame.Write(sim, true); actor.Brain!.Tick(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor);
        Assert.Equal(1, actor.States.Current); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Source) Room(bool friendly)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Lines = [new LevelLine { X1 = 100, Y1 = -256, X2 = 100, Y2 = 256, SideBack = -1 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 300 }] });
        var actor = sim.Actors[0]; var source = sim.Actors[1]; actor.Ambush = true;
        actor.Friendly = source.Friendly = friendly; actor.PainState = -1; actor.SeeState = 1;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1)], 0);
        return (sim, actor, source);
    }
}
