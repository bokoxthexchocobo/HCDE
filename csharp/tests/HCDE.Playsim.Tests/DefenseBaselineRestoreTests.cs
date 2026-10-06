using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DefenseBaselineRestoreTests
{
    [Theory]
    [InlineData("mass")]
    [InlineData("invulnerable")]
    [InlineData("nonshootable")]
    public void BaselineRestoreUndoesLaterDefenseChangesAndPermitsDamage(string property)
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        var bytes = SimSavegame.Write(sim);
        switch (property)
        {
            case "mass": actor.Mass = 500; break;
            case "invulnerable": actor.Invulnerable = true; break;
            case "nonshootable": actor.NonShootable = true; break;
        }
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(100, actor.Mass);
        Assert.False(actor.Invulnerable); Assert.False(actor.NonShootable);
        Assert.False(actor.HasDefensePropertyOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.Equal(1, ActorDamage.Apply(actor, 1, flags: DamageFlags.NoPain).HealthLost);
    }

    [Fact]
    public void AbsentDefenseUsesRecordedInvulnerabilityDefault()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        actor.ResurrectionDefenseFlags = actor.ResurrectionDefenseFlags.GetValueOrDefault() | 1;
        actor.Invulnerable = true;
        var bytes = SimSavegame.Write(sim);
        actor.Invulnerable = false;
        SimSavegame.Apply(sim, bytes);
        Assert.True(actor.Invulnerable);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.Equal(0, ActorDamage.Apply(actor, 1).HealthLost);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }],
    });
}
