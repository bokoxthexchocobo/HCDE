using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaisePickupDefaultsTests
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
    public void RevivalRestoresPatchedPickupDefaults(bool pickup, bool special, bool archvile)
    {
        var sim = Room(pickup, special); var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.CanPickupItems = !pickup;
        corpse.SpecialPickup = !special;
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 2) == true);
        Assert.Equal(pickup, corpse.CanPickupItems);
        Assert.Equal(special, corpse.SpecialPickup);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DifferentPickupRevivalDefaultsAffectChecksum(bool special)
    {
        var first = Room(false, false); var second = Room(false, false);
        if (special) second.Actors[1].SpawnSpecialPickup = true;
        else second.Actors[1].SpawnCanPickupItems = true;
        first.Tick(); second.Tick();
        Assert.Equal(first.Actors[1].CanPickupItems, second.Actors[1].CanPickupItems);
        Assert.Equal(first.Actors[1].SpecialPickup, second.Actors[1].SpecialPickup);
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(bool pickup, bool special) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    }, dehacked: DehackedPatch.Apply($"Thing 2\nBits = {6 | (pickup ? 2048 : 0) | (special ? 1 : 0)}\n"));
}
