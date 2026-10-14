using HCDE.MapLoader;
using HCDE.Gamedata;

namespace HCDE.Playsim.Tests;

public class PlayerFriendshipDefaultSaveTests
{
    [Fact]
    public void MissingClassMetadataUsesPlayerDefaultWithoutRuntimeOverride()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ResurrectionDefenseFlags = null;
        Assert.True(player.DefaultFriendly);
        var state = sim.CaptureState(true);
        Assert.Null(state.Actors.Single(a => a.Id == player.Id).Friendship);
        var saved = SimSavegame.Write(state); player.Friendly = false; SimSavegame.Apply(sim, saved);
        Assert.True(player.Friendly);
    }

    [Fact]
    public void ExplicitRuntimeFalseSurvivesMissingClassMetadata()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ResurrectionDefenseFlags = null; player.Friendly = false;
        var state = sim.CaptureState(true); Assert.False(state.Actors.Single(a => a.Id == player.Id).Friendship!.Value.Friendly);
        var saved = SimSavegame.Write(state); player.Friendly = true; SimSavegame.Apply(sim, saved); Assert.False(player.Friendly);
    }

    [Fact]
    public void ExplicitCapturedBaselineTakesPriorityOverFallback()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ResurrectionDefenseFlags = 0;
        Assert.False(player.DefaultFriendly); player.Friendly = false;
        var saved = SimSavegame.Write(sim, true); player.Friendly = true; SimSavegame.Apply(sim, saved);
        Assert.False(player.Friendly);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("SOLID+SHOOTABLE")]
    public void DehackedPlayerBitsPreserveNativeFriendlyBaseline(string bits)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply($"Thing 1\nBits = {bits}\n"));
        var player = sim.Players.Single(); Assert.True(player.Friendly); Assert.True(player.DefaultFriendly);
        var saved = SimSavegame.Write(sim, true); player.Friendly = false; SimSavegame.Apply(sim, saved); Assert.True(player.Friendly);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
