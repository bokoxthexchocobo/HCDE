using HCDE.MapLoader;
using HCDE.Gamedata;

namespace HCDE.Playsim.Tests;

public class RaiseSpawnCeilingTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void RevivalRestoresSpawnCeilingWithoutRepositioning(bool ceiling, bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL" + (ceiling ? " + SPAWNCEILING" : "") + "\n"));
        var corpse = sim.Actors[1];
        Assert.Equal(ceiling, corpse.SpawnCeiling);
        Assert.Equal(ceiling, corpse.Z.Raw > 0);
        corpse.Z = default;
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.SpawnCeiling = !ceiling;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked ? !ceiling : ceiling, corpse.SpawnCeiling);
        Assert.Equal(0, corpse.Z.Raw);
        var bot = sim.AddBot(200, 0, 3004);
        Assert.Equal(ceiling ? 64 : 0, bot.ResurrectionCollisionFlags!.Value & 64);
    }
}
