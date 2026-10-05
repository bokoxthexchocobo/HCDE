using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseTelefragFlagsTests
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
    public void RevivalRestoresSupportedSpawnStompFlags(bool archvile, bool blocked, bool alwaysTelefrag)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.NoTelefrag = true;
        corpse.AlwaysTelefrag = alwaysTelefrag;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.NoTelefrag);
        Assert.Equal(blocked && alwaysTelefrag, corpse.AlwaysTelefrag);
        if (!blocked)
        {
            var player = sim.Players.Single();
            corpse.Brain!.Enabled = false;
            corpse.Solid = false;
            corpse.X = player.X; corpse.Y = player.Y;
            ActorDamage.Apply(player, 1000);
            for (var tic = 0; tic < GameTicClock.TicRate; tic++) sim.Tick();
            sim.QueueCommand(0, new PlayerCommand { Use = true });
            sim.Tick();
            Assert.False(player.IsDead);
            Assert.True(corpse.IsDead);
        }
    }
}
