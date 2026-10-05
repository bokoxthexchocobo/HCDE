using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseDefenseDefaultsTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void RaiseRestoresClassDefenseDefaults(bool invulnerable, bool dormant, bool archvile)
    {
        var sim = Room(invulnerable, dormant);
        var corpse = sim.Actors[1];
        Assert.Equal(invulnerable, corpse.Invulnerable);
        Assert.Equal(dormant, corpse.Dormant);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Invulnerable = !invulnerable;
        corpse.Dormant = !dormant;

        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(invulnerable, corpse.Invulnerable);
        Assert.Equal(dormant, corpse.Dormant);
    }

    [Fact]
    public void MapDormancyDoesNotBecomeClassRevivalDefault()
    {
        var sim = Room(false, false, mapDormant: true);
        var corpse = sim.Actors[1];
        Assert.True(corpse.Dormant);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        Assert.True(ThingRaise.Execute(sim, 17, corpse, 0, 2));
        Assert.False(corpse.Dormant);
    }

    [Fact]
    public void DefenseDefaultsAffectChecksumWithMatchingCurrentFlags()
    {
        var first = Room(false, false);
        var second = Room(false, false);
        second.Actors[1].ResurrectionDefenseFlags = 3;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors[1].Dormant, second.Actors[1].Dormant);
        Assert.Equal(first.Actors[1].Invulnerable, second.Actors[1].Invulnerable);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(bool invulnerable, bool dormant, bool mapDormant = false)
        => AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 },
                new LevelThing { Type = 3004, X = 20, Dormant = mapDormant }],
        }, dehacked: DehackedPatch.Apply("Thing 2\nBits = SOLID + SHOOTABLE + COUNTKILL"
            + (invulnerable ? " + INVULNERABLE" : "") + (dormant ? " + DORMANT" : "") + "\n"));
}
