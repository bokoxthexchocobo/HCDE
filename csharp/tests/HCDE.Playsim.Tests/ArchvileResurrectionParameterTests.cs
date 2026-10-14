using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileResurrectionParameterTests
{
    [Theory]
    [InlineData(null, "vile/raise")]
    [InlineData("", "vile/raise")]
    [InlineData("custom/heal", "custom/heal")]
    public void OptionalHealSoundSelectsDefaultOrOverride(string? requested, string expected)
    {
        var sim = Room(); var corpse = Ready(sim); sim.DrainSoundRequests();
        Assert.True(ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single(), healSound: requested));
        var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal(expected, sound.Sound); Assert.Equal(corpse.Id, sound.ActorId);
        Assert.Equal(4, sound.Channel); Assert.Equal(1.001f, sound.Attenuation);
    }

    [Fact]
    public void HealStateRunsBeforeSoundAndCorpseRevival()
    {
        var sim = Room(); var corpse = Ready(sim); sim.DrainSoundRequests();
        var raiser = new Actor(); bool? wasDead = null; int? sounds = null;
        raiser.States.Configure(raiser, [new(-1, 0), new(-1, 1, _ =>
        {
            wasDead = corpse.IsDead; sounds = sim.PendingSoundRequests.Count;
        })], 0);
        Assert.True(ArchvileActions.TryRaise(sim, raiser, sim.Players.Single(), 1, "custom/heal"));
        Assert.True(wasDead); Assert.Equal(0, sounds); Assert.False(corpse.IsDead); Assert.Equal(1, raiser.States.Current);
    }

    [Fact]
    public void BlockedCorpseDoesNotEnterHealStateOrPlaySound()
    {
        var sim = Room(); Ready(sim); sim.Ceilings[0] = 10; sim.DrainSoundRequests(); var raiser = new Actor();
        Assert.False(ArchvileActions.TryRaise(sim, raiser, sim.Players.Single(), ActorStateMachine.Corpse, "custom/heal"));
        Assert.Equal(ActorStateMachine.Spawn, raiser.States.Current); Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void InvalidHealStateRejectsBeforeMutatingCorpse()
    {
        var sim = Room(); var corpse = Ready(sim); sim.DrainSoundRequests();
        Assert.Throws<ArgumentOutOfRangeException>(() => ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single(), 1000));
        Assert.True(corpse.IsDead); Assert.Empty(sim.DrainSoundRequests());
    }

    private static Actor Ready(AuthoritySimulation sim)
    {
        var actor = sim.Actors.Single(a => a.IsMonster); actor.Health = 0;
        actor.States.Enter(actor, ActorStateMachine.Corpse); return actor;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }]
    });
}
