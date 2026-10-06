using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DamageReactionResetTests
{
    [Theory]
    [InlineData(-5)]
    [InlineData(17)]
    public void BrainIndependentDamageWakeClearsSignedReactionTime(int reaction)
    {
        var actor = new Actor { Health = 100, ReactionTime = reaction, NoPain = true };
        ActorDamage.Apply(actor, 1);
        Assert.Equal(99, actor.Health);
        Assert.Equal(0, actor.ReactionTime);
    }

    [Fact]
    public void PlayerDamagePreservesReactionTimer()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.ReactionTime = 17;
        ActorDamage.Apply(player, 1);
        Assert.Equal(99, player.Health);
        Assert.Equal(17, player.ReactionTime);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RejectedDamagePreservesReactionTimer(bool invulnerable)
    {
        var actor = new Actor { Health = 100, ReactionTime = 17,
            Invulnerable = invulnerable, Dormant = !invulnerable };
        ActorDamage.Apply(actor, 1);
        Assert.Equal(100, actor.Health);
        Assert.Equal(17, actor.ReactionTime);
    }

    [Fact]
    public void LaterBrainAttachmentKeepsWakeReset()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004);
        actor.Brain = null; actor.ReactionTime = 17; actor.NoPain = true;
        ActorDamage.Apply(actor, 1);
        actor.Brain = MonsterBrain.ForType(3004);
        Assert.Equal(0, actor.ReactionTime);
        Assert.Equal(0, actor.Brain!.ReactionTics);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }],
    });
}
