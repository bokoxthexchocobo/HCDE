using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileRaiseTargetTests
{
    [Fact]
    public void FriendlyRaiseClearsRememberedCorpseWithoutClearingCurrentEnemy()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 },
                new LevelThing { Type = 3004, Id = 7, X = 20 },
                new LevelThing { Type = 64, X = -25 }],
        });
        var corpse = sim.Actors[1];
        var archvile = sim.Actors[2];
        var player = sim.Players.Single();
        archvile.MovementSpeed = Fixed.FromInt(15);
        archvile.Brain!.SetTargetThingId(sim, 7);
        archvile.Brain.WakeOnDamage(archvile, player, 1, false);
        Assert.Equal(corpse.Id, archvile.Brain.LastEnemyId);
        Assert.Equal(player.Id, archvile.Brain.TargetId);
        archvile.Friendly = true;
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);

        Assert.True(ArchvileActions.TryRaise(sim, archvile, player));
        Assert.Null(archvile.Brain.LastEnemyId);
        Assert.Equal(player.Id, archvile.Brain.TargetId);
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void FriendlySuccessfulRaiseClearsOnlyTheCorpseTarget(bool friendly, bool targetCorpse, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, Id = 8, X = 500 },
                new LevelThing { Type = 3004, Id = 7, X = 20 },
                new LevelThing { Type = 64, X = -25 }],
        });
        var corpse = sim.Actors[1];
        var archvile = sim.Actors[2];
        archvile.Friendly = friendly;
        archvile.MovementSpeed = Fixed.FromInt(15);
        archvile.Brain!.SetTargetThingId(sim, targetCorpse ? 7 : 8);
        var originalTarget = archvile.Brain.TargetId;
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        if (blocked) sim.Ceilings[0] = 10;

        Assert.Equal(!blocked, ArchvileActions.TryRaise(sim, archvile, sim.Players.Single()));
        Assert.Equal(friendly && targetCorpse && !blocked ? null : originalTarget, archvile.Brain.TargetId);
        if (!blocked) Assert.Equal(friendly, corpse.Friendly);
    }
}
