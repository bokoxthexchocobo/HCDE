using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LostSoulFriendshipTests
{
    [Theory]
    [InlineData(false, 0, 0, false)]
    [InlineData(true, 1, 42, true)]
    [InlineData(true, 0, -7, false)]
    public void SpawnedSoulCopiesSupportedFriendshipAndStillCharges(bool friendly, int friendPlayer, int hateTid, bool noHatePlayers)
    {
        var (sim, parent) = Room();
        parent.Friendly = friendly; parent.FriendPlayer = friendPlayer;
        parent.TidToHate = hateTid; parent.NoHatePlayers = noHatePlayers;
        var target = sim.Players.Single();
        var soul = sim.SpawnLostSoul(parent, target, 0);
        Assert.NotNull(soul);
        CheckPolicy(parent, soul);
        Assert.True(soul.Brain!.Charging);
        Assert.Equal(target.Id, soul.Brain.TargetId);
    }

    [Fact]
    public void TargetlessSoulStillCopiesPolicyWithoutCharging()
    {
        var (sim, parent) = Room();
        SetPolicy(parent);
        var soul = sim.SpawnLostSoul(parent, null, 0);
        Assert.NotNull(soul);
        CheckPolicy(parent, soul);
        Assert.False(soul.Brain!.Charging);
        Assert.Null(soul.Brain.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AttackAndDeathActionsPassPolicyToAllChildren(bool death)
    {
        var (sim, parent) = Room();
        SetPolicy(parent);
        parent.Brain!.SetTargetThingId(sim, 3);
        if (death) parent.Health = 0;
        for (var i = 0; i < (death ? 32 : 16); i++) sim.Tick();
        var souls = sim.Actors.Where(actor => actor.DoomEdNum == 3006).ToArray();
        Assert.Equal(death ? 3 : 1, souls.Length);
        foreach (var soul in souls) CheckPolicy(parent, soul);
    }

    private static void SetPolicy(Actor parent)
    {
        parent.Friendly = true; parent.FriendPlayer = 1;
        parent.TidToHate = 42; parent.NoHatePlayers = true;
    }

    private static void CheckPolicy(Actor parent, Actor soul)
    {
        Assert.Equal(parent.Friendly, soul.Friendly);
        Assert.Equal(parent.FriendPlayer, soul.FriendPlayer);
        Assert.Equal(parent.TidToHate, soul.TidToHate);
        Assert.Equal(parent.NoHatePlayers, soul.NoHatePlayers);
    }

    private static (AuthoritySimulation Sim, Actor Parent) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1, Id = 3, X = 800 }, new LevelThing { Type = 71 }],
        });
        sim.Players.Single().Invulnerable = true;
        var parent = sim.Actors.Single(actor => actor.DoomEdNum == 71);
        parent.ReactionTime = 0;
        return (sim, parent);
    }
}
