using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSectorKillMonstersTests
{
    private static AuthoritySimulation Room(int runtimeFlags = 0)
        => AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { FloorHeight = 0, CeilingHeight = 128, RuntimeFlags = runtimeFlags, Tag = 7 }],
            Things =
            [
                new LevelThing { Type = 1 },
                new LevelThing { Type = 3001, X = 64, Y = 64 },
                new LevelThing { Type = 3001, X = 96, Y = 64 },
            ],
        });

    private static Actor Monster(AuthoritySimulation sim) => sim.Actors.First(actor => actor is not PlayerPawn);

    [Fact]
    public void Tick_KillsGroundedShootableMonsterOnKillSector()
    {
        var sim = Room(SectorEnvironmentLineSpecials.KillMonstersFlag);
        var monster = Monster(sim);
        monster.Tick();
        Assert.True(monster.IsDead);
    }

    [Fact]
    public void Tick_SparesPlayersFloatingAndAirborneMonsters()
    {
        var sim = Room(SectorEnvironmentLineSpecials.KillMonstersFlag);
        var player = sim.Players.Single();
        player.Tick();
        Assert.False(player.IsDead);
        var monsters = sim.Actors.Where(actor => actor is not PlayerPawn).ToArray();
        monsters[0].Floating = true;
        monsters[0].Tick();
        Assert.False(monsters[0].IsDead);
        monsters[1].Z = Fixed.FromInt(32);
        monsters[1].Tick();
        Assert.False(monsters[1].IsDead);
    }

    [Fact]
    public void ChangeFlagsSpecial_SetsKillMonstersRuntimeFlag()
    {
        var sim = Room();
        Assert.True(SectorChangeFlagsSpecial.Execute(sim, SectorChangeFlagsSpecial.Special, 7,
            SectorEnvironmentLineSpecials.KillMonstersFlag, 0));
        Assert.NotEqual(0, sim.Level.Sectors[0].RuntimeFlags & SectorEnvironmentLineSpecials.KillMonstersFlag);
    }
}
