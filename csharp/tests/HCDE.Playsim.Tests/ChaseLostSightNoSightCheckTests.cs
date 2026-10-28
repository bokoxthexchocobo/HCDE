using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseLostSightNoSightCheckTests
{
    [Fact]
    public void LostSightRetargetPreservesNoSightCheckForHateSearch()
    {
        var sim = Room();
        var hunter = Hunter(sim);
        var hatee = Hatee(sim);
        hunter.TidToHate = hatee.ThingId;
        hunter.NoSightCheck = true;
        hunter.SetTargetPointer(hatee);
        var brain = hunter.Brain ?? throw new InvalidOperationException("Hunter requires a brain.");
        brain.SetChaseThreshold(0, false);
        hatee.X = Fixed.FromInt(512);
        brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.True(hunter.NoSightCheck);
    }

    private static AuthoritySimulation Room()
    {
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things =
            [
                new LevelThing { Type = 1, X = 0 },
                new LevelThing { Type = 3004, Id = 5, X = 0 },
                new LevelThing { Type = 3001, Id = 7, X = 64 },
            ],
        }, spawnOptions: new SpawnOptions(NativeMonsterMovement: true));
    }

    private static Actor Hunter(AuthoritySimulation sim) =>
        sim.Actors.Single(actor => actor.ThingId == 5);

    private static Actor Hatee(AuthoritySimulation sim) =>
        sim.Actors.Single(actor => actor.ThingId == 7);
}
