using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingMassacreTests
{
    [Fact]
    public void GlobalDestroyIncludesFriendlyButSkipsDormantAndPlayers()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 },
                new LevelThing { Type = 3004, X = 200 }],
        });
        sim.Actors[1].Friendly = true;
        sim.Actors[1].Invulnerable = true;
        sim.Actors[1].Shootable = false;
        sim.Actors[2].Dormant = true;
        Assert.True(ThingDamage.Execute(sim, 133, null, 0, 0, 0));
        Assert.True(sim.Actors[1].IsDead);
        Assert.Equal("Massacre", sim.Actors[1].DeathDamageType);
        Assert.False(sim.Actors[2].IsDead);
        Assert.False(sim.Players.Single().IsDead);
    }

    [Fact]
    public void LargeHealthRequiresMultipleDamagePasses()
    {
        var actor = new Actor { Health = 2500000 };
        Assert.True(ThingDamage.Massacre(actor));
        Assert.True(actor.IsDead);
    }

    [Fact]
    public void DeadActorDoesNotChangeFlags()
    {
        var actor = new Actor { Health = 0, Shootable = false, Dormant = true, Invulnerable = true };
        Assert.False(ThingDamage.Massacre(actor));
        Assert.False(actor.Shootable);
        Assert.True(actor.Dormant);
        Assert.True(actor.Invulnerable);
    }
}
