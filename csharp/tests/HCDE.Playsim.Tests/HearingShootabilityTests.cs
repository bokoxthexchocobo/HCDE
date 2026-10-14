using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HearingShootabilityTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void HearingRequiresShootableRegardlessOfVulnerable(bool shootable, bool vulnerable, bool remembered)
    {
        var (sim, actor, enemy) = Room(); enemy.Shootable = shootable; enemy.Vulnerable = vulnerable;
        if (remembered)
        {
            actor.LastHeardTargetId = enemy.Id;
            actor.Brain!.Tick(sim, actor, useMelee: false, useMissile: false);
        }
        else actor.Brain!.Hear(sim, actor, enemy);
        Assert.Equal(shootable ? enemy.Id : (uint?)null, actor.TargetId);
        if (!shootable && vulnerable)
        {
            var health = enemy.Health; ActorDamage.Apply(enemy, 1, actor, DamageFlags.NoPain);
            Assert.Equal(health - 1, enemy.Health);
        }
    }

    [Fact]
    public void SavedNoiseDoesNotAcquireVulnerableOnlyTarget()
    {
        var (sim, actor, enemy) = Room(); enemy.Shootable = false; enemy.Vulnerable = true;
        actor.LastHeardTargetId = enemy.Id; var saved = SimSavegame.Write(sim, true);
        enemy.Shootable = true; SimSavegame.Apply(sim, saved);
        actor.Brain!.Tick(sim, actor); Assert.Null(actor.TargetId); Assert.Equal(enemy.Id, actor.LastHeardTargetId);
        enemy.Shootable = true; actor.Brain.Tick(sim, actor); Assert.Equal(enemy.Id, actor.TargetId);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Enemy) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 300 }] });
        var actor = sim.Actors[0];
        return (sim, actor, sim.Actors[1]);
    }
}
