using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsPickupClassTests
{
    [Theory]
    [InlineData(2025, "RadSuit")]
    [InlineData(2023, "Berserk")]
    [InlineData(2007, "Clip")]
    [InlineData(2001, "Shotgun")]
    [InlineData(2019, "BlueArmor")]
    [InlineData(5, "BlueCard")]
    public void PickupClassQueriesAgreeWithDropMapping(int type, string name)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 80, Id = 9 }]
        });
        var actor = sim.Actors.Single(a => a.ThingId == 9); var globals = new AcsGlobalStrings();
        var stack = new List<int> { 9 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [], globals,
            AcsCallFunctions.GetActorClass, 1, out var result));
        Assert.Equal(name, globals.Get(result)); Assert.Empty(stack);
        foreach (var query in new[] { name, name.ToLowerInvariant(), "UnrelatedClass" })
        {
            stack.AddRange([0, 0]);
            Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [query], globals,
                AcsCallFunctions.CheckActorClass, 2, out result));
            Assert.Equal(query == "UnrelatedClass" ? 0 : 1, result); Assert.Empty(stack);
        }
        Assert.True(PickupCatalog.TryEditorNumberForDropName(name, out var mapped)); Assert.Equal(type, mapped);
    }
}
