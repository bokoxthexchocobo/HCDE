using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSetFriendlyTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ActionAndChangeFlagPersistExplicitValues(bool changeFlag, bool value)
    {
        var sim = Room(); var actor = sim.AddBot(200, 0, 3004);
        var target = sim.AddBot(300, 0, 3004); actor.Brain!.SetSpecialTarget(target);
        actor.Health = 7; actor.FriendPlayer = 2; actor.TidToHate = 77;
        if (changeFlag) ActorPropertyActions.ChangeFlag(actor, "FRIENDLY", value);
        else ActorPropertyActions.SetFriendly(actor, value);
        Assert.Equal(value, actor.Friendly); Assert.Equal(7, actor.Health);
        Assert.Equal(target.Id, actor.Brain.TargetId); Assert.Equal(2, actor.FriendPlayer); Assert.Equal(77, actor.TidToHate);
        RoundTrip(sim, actor, value, false);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearAtSpawnDefaultStillProducesRestorableOverride(bool noHate)
    {
        var sim = Room(); var actor = sim.AddBot(200, 0, 3004);
        Assert.False(actor.HasFriendshipOverride);
        ActorPropertyActions.ChangeFlag(actor, noHate ? "NOHATEPLAYERS" : "FRIENDLY", false);
        Assert.True(actor.HasFriendshipOverride);
        RoundTrip(sim, actor, false, false);
    }

    [Fact]
    public void SettingFriendlinessChangesFriendRelation()
    {
        var sim = Room(); var first = sim.AddBot(200, 0, 3004); var second = sim.AddBot(300, 0, 3004);
        ActorPropertyActions.SetFriendly(first, true); ActorPropertyActions.SetFriendly(second, true);
        Assert.True(first.IsFriend(second));
        ActorPropertyActions.SetFriendly(first, false);
        Assert.False(first.IsFriend(second));
    }

    private static void RoundTrip(AuthoritySimulation sim, Actor actor, bool friendly, bool noHate)
    {
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Friendly = !friendly; actor.NoHatePlayers = !noHate;
        sim.RestoreState(state);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(noHate, actor.NoHatePlayers);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
