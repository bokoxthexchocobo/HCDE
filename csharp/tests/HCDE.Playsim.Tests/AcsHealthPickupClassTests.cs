using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsHealthPickupClassTests
{
    [Theory]
    [InlineData(false, 2011, "MedPatch", "Stimpack")]
    [InlineData(false, 2012, "MedicalKit", "Medikit")]
    [InlineData(false, 83, "SurgeryKit", "Megasphere")]
    [InlineData(true, 82, "ArtiHealth", "SuperShotgun")]
    [InlineData(true, 32, "ArtiSuperHealth", "None")]
    public void QueriesReportGameSpecificHealthClasses(bool raven, int type, string name, string rejected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 80, Id = 9 }]
        }, spawnOptions: new SpawnOptions(StrifeHealthPickups: !raven, RavenHealthPickups: raven));
        var actor = sim.Actors.Single(a => a.ThingId == 9); var globals = new AcsGlobalStrings();
        var stack = new List<int> { 9 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [], globals,
            AcsCallFunctions.GetActorClass, 1, out var result));
        Assert.Equal(name, globals.Get(result));
        foreach (var query in new[] { name, name.ToLowerInvariant(), rejected })
        {
            stack.AddRange([0, 0]);
            Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [query], globals,
                AcsCallFunctions.CheckActorClass, 2, out result));
            Assert.Equal(query == rejected ? 0 : 1, result); Assert.Empty(stack);
        }
        Assert.True(PickupCatalog.TryEditorNumberForDropName(name, out var mapped, strifeHealth: !raven, ravenHealth: raven));
        Assert.Equal(type, mapped);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OtherDoomPickupNamesAreNotAssumedForForeignGameMode(bool raven)
    {
        Assert.False(PickupCatalog.TryClassName(2025, out _, strifeHealth: !raven, ravenHealth: raven));
    }
}
