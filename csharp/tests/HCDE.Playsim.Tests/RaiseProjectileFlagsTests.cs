using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseProjectileFlagsTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (var flag in new[] { "NOEXPLODEFLOOR", "CEILINGHUGGER", "FLOORHUGGER", "MTHRUSPECIES", "HITOWNER" })
        foreach (var archvile in new[] { false, true })
        foreach (var blocked in new[] { false, true })
            yield return [flag, archvile, blocked];
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void RevivalRestoresProjectileFlagsThroughAcs(string flag, bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        Assert.True(AcsActorFlags.TrySet(corpse, flag, true));
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.True(AcsActorFlags.TryGet(corpse, flag, out var value));
        Assert.Equal(blocked, value);
    }
}
