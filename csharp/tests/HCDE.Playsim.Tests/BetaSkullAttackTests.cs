using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BetaSkullAttackTests
{
    [Fact]
    public void StateActionDealsDirectDamageWithoutStartingCharge()
    {
        var sim = Room(); var soul = Soul(sim); var target = sim.Players.Single();
        soul.Brain!.SetSpecialTarget(target); soul.Damage = 10; soul.Ambush = true;
        soul.VelocityX = Fixed.FromInt(3);
        soul.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "custom/betaskull" };
        soul.States.Configure(soul, [new ActorFrame(-1, 0, ActorSkullActions.BetaSkullAttack)], 0);
        var damage = 100 - target.Health; Assert.InRange(damage, 10, 80); Assert.Equal(0, damage % 10);
        Assert.False(soul.Brain.Charging); Assert.False(soul.Ambush);
        Assert.Equal(Fixed.FromInt(3), soul.VelocityX); Assert.Equal(0, soul.Angle.ToDegrees());
        var sound = Assert.Single(sim.DrainSoundRequests(), s => s.ActorId == soul.Id);
        Assert.Equal("custom/betaskull", sound.Sound); Assert.Equal(1, sound.Channel);
        Assert.Equal(0, sound.Flags); Assert.Equal(1, sound.Attenuation);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrSameSpeciesTargetSkipsAllAttackEffects(bool sameSpecies)
    {
        var sim = Room(); var soul = Soul(sim); soul.Ambush = true;
        if (sameSpecies)
        {
            soul.Species = "shared"; sim.Players.Single().Species = "SHARED";
            soul.Brain!.SetSpecialTarget(sim.Players.Single());
        }
        var random = sim.CombatRandomState;
        ActorSkullActions.BetaSkullAttack(soul);
        Assert.Equal(100, sim.Players.Single().Health); Assert.Equal(random, sim.CombatRandomState);
        Assert.True(soul.Ambush); Assert.Empty(sim.DrainSoundRequests());
    }

    [Fact]
    public void SaveLoadRepeatsDirectDamageAndRandomState()
    {
        var sim = Room(); var soul = Soul(sim); soul.Brain!.SetSpecialTarget(sim.Players.Single());
        soul.Damage = 10;
        var bytes = SimSavegame.Write(sim);
        ActorSkullActions.BetaSkullAttack(soul);
        var health = sim.Players.Single().Health; var random = sim.CombatRandomState;
        SimSavegame.Apply(sim, bytes); ActorSkullActions.BetaSkullAttack(soul);
        Assert.Equal(health, sim.Players.Single().Health); Assert.Equal(random, sim.CombatRandomState);
    }

    [Fact]
    public void ZeroMaskDamageDoesNotNeedSimulationOrRandomness()
    {
        var actor = new Actor { Damage = 7 };
        Assert.Equal(21, actor.GetMissileDamage(0, 3));
        actor.SetDamage(-1);
        Assert.Throws<NotSupportedException>(() => actor.GetMissileDamage(0, 1));
    }

    private static Actor Soul(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3006);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 100 }, new LevelThing { Type = 3006 }] });
}
