using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorActionClassFilterTests
{
    [Theory]
    [InlineData("DoomPlayer", false, 90)]
    [InlineData("doomplayer", false, 90)]
    [InlineData("DoomPlayer", true, 100)]
    [InlineData("PlayerPawn", false, 100)]
    [InlineData("Actor", false, 100)]
    public void PlayerDamageFiltersUseExactRepresentedClass(string filter, bool exclude, int expected)
    {
        var sim = Room();
        ActorHealthActions.DamageSelf(sim.Players.Single(), 10, classFilter: filter, flags: exclude ? 64 : 0);
        Assert.Equal(expected, sim.Players.Single().Health);
    }

    [Theory]
    [InlineData("Clip", false, true)]
    [InlineData("clip", false, true)]
    [InlineData("Shell", false, false)]
    [InlineData("Clip", true, false)]
    public void PickupRemovalFiltersWorkAcrossFreshSave(string filter, bool exclude, bool removed)
    {
        var sim = Room(); var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var pickup = world.Actors.Single(a => a.DoomEdNum == PickupCatalog.Clip);
            ActorRemovalActions.Remove(pickup, flags: 8 | (exclude ? 16 : 0), classFilter: filter);
            Assert.Equal(removed, pickup.Destroyed);
        }
    }

    [Fact]
    public void ExactPlayerKillFilterCanReachUnprotectedPlayer()
    {
        var sim = Room(); var pickup = sim.Actors.Single(a => a.DoomEdNum == PickupCatalog.Clip);
        pickup.SetTargetPointer(sim.Players.Single());
        ActorHealthActions.KillTarget(pickup, classFilter: "DoomPlayer");
        Assert.True(sim.Players.Single().IsDead);
    }

    [Fact]
    public void MonsterParentClassDoesNotBecomeAnExactMatch()
    {
        var sim = Room(); var spectre = sim.AddBot(400, 0, 58); var health = spectre.Health;
        ActorHealthActions.DamageSelf(spectre, 10, classFilter: "Demon", sourceSelector: AcsActorPointer.Null,
            inflictorSelector: AcsActorPointer.Null);
        Assert.Equal(health, spectre.Health);
        ActorHealthActions.DamageSelf(spectre, 10, classFilter: "Spectre", sourceSelector: AcsActorPointer.Null,
            inflictorSelector: AcsActorPointer.Null);
        Assert.Equal(health - 10, spectre.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.Clip, X = 200 }],
        Sectors = [new LevelSector { CeilingHeight = 128 }],
    });
}
