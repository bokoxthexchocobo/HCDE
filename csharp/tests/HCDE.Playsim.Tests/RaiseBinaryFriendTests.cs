using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseBinaryFriendTests
{
    [Theory]
    [InlineData(false, 0x82, true)]
    [InlineData(true, 0x82, false)]
    [InlineData(false, 0x182, false)]
    [InlineData(true, 0x182, false)]
    public void BinaryAllegianceSurvivesScriptRaiseAndArchvileOverridesIt(bool archvile, short options, bool expected)
    {
        var core = new BinaryMapRecords([new MapThingRecord(500, 0, 0, 1, 2),
            new MapThingRecord(20, 0, 0, 3004, options)], [],
            [new MapSectorRecord(0, 128, "", "", 160, 0, 0)]);
        var map = new BinaryMap(core, new BinaryMapGeometry([], [], []),
            new BinaryMapSurface([], []), default, default);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromBinary(map, "MAP01"));
        var corpse = sim.Actors[1];
        Assert.Equal(options == 0x82, corpse.Friendly);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Friendly = !corpse.Friendly;
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(expected, corpse.Friendly);
    }
}
