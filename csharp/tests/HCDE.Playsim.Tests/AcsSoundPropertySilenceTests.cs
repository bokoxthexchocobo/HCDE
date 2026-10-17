using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSoundPropertySilenceTests
{
    public static IEnumerable<object[]> Cases => Enumerable.Range(5, 5)
        .SelectMany(property => new[] { "None", "none", "NONE" }.Select(value => new object[] { property, value }));

    [Theory]
    [MemberData(nameof(Cases))]
    public void NativeSilentPropertiesReportEmptyAndPersistForActivatorAndTaggedActors(int property, string value)
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsActorStringProperties.Set(sim, player, 0, property, "custom/original");
        AcsActorStringProperties.Set(sim, player, 0, property, value);
        AcsActorStringProperties.Set(sim, player, 7, property, value);
        Assert.Equal("", AcsActorStringProperties.Get(sim, player, 0, property));
        Assert.Equal("", AcsActorStringProperties.Get(sim, player, 7, property));
        Assert.All(sim.Actors, actor => Assert.Equal("", actor.ActorSounds[(ActorSoundType)(property - 5)]));
        var pool = new AcsGlobalStrings();
        foreach (var (text, expected) in new[] { ("", 1), ("None", 0) })
        {
            var stack = new List<int> { 0, property, 0 };
            Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = player }, [text], pool, 22, 3, out var result));
            Assert.Equal(expected, result); Assert.Empty(stack);
        }
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Equal("", AcsActorStringProperties.Get(fresh, fresh.Players.Single(), 0, property));
        Assert.Equal("", AcsActorStringProperties.Get(fresh, null, 7, property));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public void LegacyLiteralNoneSlotQueriesAsNativeEmptyName(int property)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.ActorSounds = new Dictionary<ActorSoundType, string> { [(ActorSoundType)(property - 5)] = "nOnE" };
        Assert.Equal("", AcsActorStringProperties.Get(sim, player, 0, property));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2007, Id = 7, X = 200 },
            new LevelThing { Type = 2001, Id = 7, X = 400 }],
        Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
