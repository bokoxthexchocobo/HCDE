using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseMapFriendTests
{
    [Theory]
    [InlineData(false, "friend")]
    [InlineData(true, "friend")]
    [InlineData(false, "strifeally")]
    [InlineData(true, "strifeally")]
    public void MapFriendSurvivesScriptRaiseButArchvileOverridesIt(bool archvile, string key)
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; thing { type = 1; x = 500; skill3 = true; single = true; } thing { type = 3004; x = 20; skill3 = true; single = true; " + key + " = true; } sector { heightceiling = 128; }",
            out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        var corpse = sim.Actors[1];
        Assert.True(corpse.Friendly);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Friendly = false;
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(!archvile, corpse.Friendly);
    }
}
