using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DontCorpseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeathRespectsDontCorpseOnPlayersAndMonsters(bool player)
    {
        var sim = Room(); var actor = player ? (Actor)sim.Players.Single() : sim.AddBot(64, 0);
        ActorPropertyActions.ChangeFlag(actor, "DONTCORPSE", true);
        actor.Health = 0; Assert.True(actor.DontCorpse); Assert.False(actor.Corpse);
    }

    [Fact]
    public void OrdinaryActorWithoutRaiseStateDoesNotBecomeCorpse()
    {
        var actor = new Actor(); actor.Health = 0; Assert.False(actor.Corpse);
    }

    [Fact]
    public void DontCorpseDoesNotClearAnAlreadySetCorpseFlag()
    {
        var actor = new Actor { IsMonster = true, Corpse = true, DontCorpse = true };
        actor.Health = 0; Assert.True(actor.Corpse);
    }

    [Fact]
    public void LivingSavePreservesDontCorpseForLaterDeath()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim); sim.Players.Single().DontCorpse = true;
        var bytes = SimSavegame.Write(sim); var loaded = Room(); SimSavegame.Apply(loaded, bytes);
        var player = loaded.Players.Single(); Assert.True(player.DontCorpse);
        Assert.Equal(bytes, SimSavegame.Write(loaded));
        player.Health = 0; Assert.False(player.Corpse);
        SimSavegame.Apply(loaded, legacy); Assert.False(player.DontCorpse);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
