using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedNotDeathmatchTests
{
    [Theory]
    [InlineData(SpawnGameMode.Single, "NOTDMATCH+6", true)]
    [InlineData(SpawnGameMode.Cooperative, "NOTDMATCH+6", true)]
    [InlineData(SpawnGameMode.Deathmatch, "NOTDMATCH+6", false)]
    [InlineData(SpawnGameMode.Deathmatch, "33554438", false)]
    [InlineData(SpawnGameMode.Deathmatch, "6", true)]
    public void PatchedClassFlagFiltersOnlyDeathmatchMapSpawn(SpawnGameMode mode, string bits, bool spawns)
    {
        var sim = Start(Patch($"Thing 2\nBits = {bits}\n"), mode);
        Assert.Equal(spawns, sim.Actors.Any(a => a.ClassDoomEdNum == 3004));
        Assert.Single(sim.Actors, a => a.ClassDoomEdNum == 3001);
        Assert.Equal(1u, sim.Actors.First().Id);
        var saved = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void FilteringDoesNotConsumeActorIdentityOrActorSpawnRandom()
    {
        var patch = Patch("Thing 2\nBits = NOTDMATCH+6\n");
        var sim = Start(patch, SpawnGameMode.Deathmatch);
        var control = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 3001, X = 200 }] },
            dehacked: patch, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Deathmatch));
        Assert.Equal(control.Actors.Single().Id, sim.Actors.Single().Id);
        Assert.Equal(control.Actors.Single().FloatBobPhase, sim.Actors.Single().FloatBobPhase);
        Assert.Equal(control.Actors.Single().LastLookPlayerNumber, sim.Actors.Single().LastLookPlayerNumber);
    }

    [Fact]
    public void FilteredMapClassCanStillSpawnDynamicallyAndRestore()
    {
        var patch = Patch("Thing 2\nBits = NOTDMATCH+COUNTKILL+6\n");
        var sim = Start(patch, SpawnGameMode.Deathmatch); var dynamic = sim.AddBot(400, 0);
        Assert.Equal(3004, dynamic.ClassDoomEdNum); Assert.True(dynamic.IsMonster);
        var saved = SimSavegame.Write(sim, true); var fresh = Start(patch, SpawnGameMode.Deathmatch);
        SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Contains(fresh.Actors, a => a.Id == dynamic.Id && a.ClassDoomEdNum == 3004);
    }

    [Fact]
    public void InheritedFlagPersistsUntilPrimaryReplacement()
    {
        var original = Patch("Thing 2\nBits = NOTDMATCH+6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ\n", original);
        var cleared = DehackedPatch.Apply("Thing 2\nBits = 6\n", inherited);
        Assert.DoesNotContain(Start(inherited, SpawnGameMode.Deathmatch).Actors, a => a.ClassDoomEdNum == 3004);
        Assert.Contains(Start(cleared, SpawnGameMode.Deathmatch).Actors, a => a.ClassDoomEdNum == 3004);
    }

    [Fact]
    public void FilterResolvesRemappedEditorNumber()
    {
        var patch = Patch("Thing 2\nID # = 9000\nBits = NOTDMATCH+6\n");
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 9000 }] },
            dehacked: patch, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Deathmatch));
        Assert.Empty(sim.Actors);
    }

    private static DehackedPatchResult Patch(string text)
    { var patch = DehackedPatch.Apply(text); Assert.Empty(patch.Errors); return patch; }
    private static AuthoritySimulation Start(DehackedPatchResult patch, SpawnGameMode mode) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200 }] },
        dehacked: patch, spawnOptions: new SpawnOptions(Mode: mode));
}
