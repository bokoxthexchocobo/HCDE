using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoInfightSpeciesTests
{
    [Theory]
    [InlineData(false, 3004, false)]
    [InlineData(true, 3004, true)]
    [InlineData(true, 3001, false)]
    public void FlagBlocksSameSpeciesRetaliationWithoutBlockingDamage(bool flag, int attackerType, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }, new LevelThing { Type = attackerType, X = 100 }],
        });
        var target = sim.Actors[1]; var attacker = sim.Actors[2];
        target.DoHarmSpecies = true;
        Assert.True(AcsActorFlags.TrySet(target, "Actor.NOINFIGHTSPECIES", flag));
        Assert.Equal(1, ActorDamage.Apply(target, 1, attacker).HealthLost);
        Assert.Equal(blocked ? null : (uint?)attacker.Id, target.Brain!.TargetId);
    }

    [Fact]
    public void QualifiedFlagSupportsQueryAndClear()
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TrySet(actor, "actor.noinfightspecies", true));
        Assert.True(AcsActorFlags.TryGet(actor, "NOINFIGHTSPECIES", out var enabled)); Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, "NOINFIGHTSPECIES", false));
        Assert.False(actor.NoInfightSpecies);
    }

    [Fact]
    public void NativeSpeciesAncestryIsUsed()
    {
        var target = new Actor { DoomEdNum = 3002, NoInfightSpecies = true };
        var spectre = new Actor { DoomEdNum = 58, Id = 9 };
        Assert.False(target.OkayToSwitchTarget(spectre, new MonsterBrain(MonsterAttack.Melee)));
    }
}
