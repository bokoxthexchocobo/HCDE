using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UdmfFriendlySeeBlocksTests
{
    [Theory]
    [InlineData("ZDoom", 37, 37)]
    [InlineData("ZDoomTranslated", 37, 37)]
    [InlineData("ZDoom", 0, 10)]
    [InlineData("ZDoom", -1, 10)]
    [InlineData("ZDoom", int.MinValue, 10)]
    [InlineData("ZDoom", int.MaxValue, int.MaxValue)]
    [InlineData("Doom", 37, 10)]
    [InlineData("Hexen", 37, 10)]
    public void MapRadiusAppliesOnlyWhenPositiveInSupportedNamespace(string mapNamespace, int value, int expected)
    {
        var text = FormattableString.Invariant($"namespace=\"{mapNamespace}\"; sector {{ heightceiling=128; }} thing {{ type=3004; friendlyseeblocks={value}; skill3=true; single=true; }}");
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(value, map.Things.Single().FriendlySeeBlocks);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        var sim = AuthoritySimulation.Start(level); var actor = Assert.Single(sim.Actors);
        Assert.Equal(expected, actor.FriendlySeeBlocks);
        Assert.Equal(expected, AcsActorProperties.Get(sim, actor, 0, 48));
        Assert.Equal(level.Things.Single().FriendlySeeBlocks, level.CopyForSimulation().Things.Single().FriendlySeeBlocks);
        var saved = SimSavegame.Write(sim, true);
        AcsActorProperties.Set(sim, actor, 0, 48, -7);
        SimSavegame.Apply(sim, saved); Assert.Equal(expected, actor.FriendlySeeBlocks);
    }

    [Fact]
    public void MissingMapRadiusKeepsNativeDefault()
    {
        Assert.True(UdmfTextMapParser.TryParse(
            "namespace=\"ZDoom\"; sector { heightceiling=128; } thing { type=3004; skill3=true; single=true; }",
            out var map, out var error), error);
        Assert.Equal(-1, map.Things.Single().FriendlySeeBlocks);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        Assert.Equal(10, Assert.Single(sim.Actors).FriendlySeeBlocks);
    }
}
