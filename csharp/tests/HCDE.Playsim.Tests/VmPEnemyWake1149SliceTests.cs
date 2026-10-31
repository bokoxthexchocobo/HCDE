namespace HCDE.Playsim.Tests;

public class VmPEnemyWake1149SliceTests
{
    [Fact]
    public void LookForPlayers_ReturnsEarlyForFriendsAndThenWalksTheMask()
    {
        var quiet = Look(flags: VmPEnemyWake1149Slice.Friendly, foundEnemies: false);
        Assert.False(quiet.Found);
        Assert.True(quiet.LookedForEnemies);
        Assert.Null(quiet.PlayerSlot);

        var claim = Look(flags: VmPEnemyWake1149Slice.Friendly, friendPlayer: 1, deathmatch: false, foundEnemies: false);
        Assert.True(claim.Found);
        Assert.True(claim.ClaimedWithoutTarget);

        var hunted = Look(
            flags: VmPEnemyWake1149Slice.Friendly,
            friendPlayer: 1,
            deathmatch: true,
            foundEnemies: false,
            lastLookPlayer: 5,
            playerInGame: slot => slot == 7,
            targetable: slot => slot == 7);
        Assert.Equal(7, hunted.PlayerSlot);
        Assert.Equal(7, hunted.LastLook);
        Assert.False(hunted.ClaimedWithoutTarget);

        var goal = Look(hasGoal: true, playerInGame: _ => false);
        Assert.True(goal.TookGoal);
        Assert.True(goal.Found);

        var blocked = Look(hasParams: true, lookFlags: VmPEnemyWake1149Slice.DontChaseGoal, hasGoal: true, playerInGame: _ => false, lastEnemyAlive: true, lastEnemyIsFriend: true);
        Assert.False(blocked.Found);
        Assert.True(blocked.ClearedLastEnemy);
        Assert.False(blocked.TookGoal);

        var adopted = Look(playerInGame: _ => false, lastEnemyAlive: true);
        Assert.True(adopted.TookLastEnemy);

        var tid = Look(tidToHate: 4, foundTid: false, flags3: 0);
        Assert.False(tid.Found);

        var monsters = Look(gameType: 2, primaryLevel: true, player0Health: 0, foundMonsters: true);
        Assert.True(monsters.LookedForMonsters);
        Assert.True(monsters.Found);
        Assert.False(Look(gameType: VmPEnemyWake1149Slice.DoomStrifeChex, primaryLevel: true, player0Health: 0, foundMonsters: true).LookedForMonsters);

        var react = Look(lastLookPlayer: 1, hasGoal: true, targetIsGoal: true, playerInGame: slot => slot == 2, targetable: slot => slot == 2);
        Assert.True(react.ClearedReaction);
        Assert.Equal(2, react.PlayerSlot);
    }

    [Fact]
    public void Look_WakesFromAShootableHeardActorAndRefusesANoTargetPlayer()
    {
        Assert.True(Wake(inConversation: true).Stopped);
        Assert.False(Wake(inConversation: true).ThresholdCleared);

        var goal = Wake(special: VmPEnemyWake1149Slice.ThingSetGoal, arg2: 2, arg3: 0, mapTime: 10, patrolFound: true, flags5: VmPEnemyWake1149Slice.ChaseGoal, looks: new[] { false });
        Assert.True(goal.GoalSet);
        Assert.Equal(0, goal.Special);
        Assert.Equal(80, goal.ReactionTime);
        Assert.Equal(0, goal.Flags5);
        Assert.True(goal.Stopped);

        var hidden = Wake(
            compatSoundTarget: true,
            sectorSound: new VmPEnemyWake1149Slice.Heard(true, 100, true, VmPEnemyWake1149Slice.NoTarget, false, true, false, false),
            lastHeard: new VmPEnemyWake1149Slice.Heard(true, 100, false, 0, false, true, false, false));
        Assert.True(hidden.Stopped);
        Assert.True(hidden.ThresholdCleared);
        Assert.Null(hidden.VisDir);

        var seen = Wake(
            flags: VmPEnemyWake1149Slice.Stealth,
            flags2: VmPEnemyWake1149Slice.Boss,
            lastHeard: Monster(shootable: true),
            seeSoundValid: true,
            hasSeeState: true);
        Assert.Equal(-1, seen.VisDir);
        Assert.Equal(0, seen.LookCalls);
        Assert.True(seen.SeeSound);
        Assert.True(seen.FullVolume);
        Assert.True(seen.EnteredSeeState);
        Assert.False(seen.Stopped);
    }

    [Fact]
    public void Look_AmbushAndFriendsUseTheSearchBeforeTheSeeState()
    {
        var deaf = Wake(
            flags: VmPEnemyWake1149Slice.Ambush,
            lastHeard: Monster(shootable: true),
            ambushSight: false,
            looks: new[] { false });
        Assert.True(deaf.Stopped);
        Assert.True(deaf.KeptHeardTarget);
        Assert.Equal(1, deaf.LookCalls);
        Assert.False(deaf.SeeSound);

        var friend = Wake(
            lastHeard: Monster(shootable: true, isFriend: true),
            hasSeeState: false,
            looks: new[] { false, true },
            lookTargetIsGoal: true,
            reactionTime: 40,
            mapTime: 10);
        Assert.True(friend.Wandered);
        Assert.Equal(2, friend.LookCalls);
        Assert.True(friend.ClearedGoalTarget);
        Assert.False(friend.EnteredSeeState);
        Assert.False(friend.SeeSound);

        Assert.Equal(0x00010000, VmPEnemyWake1149Slice.LookAllAround);
        Assert.Equal(35, VmPEnemyWake1149Slice.TicRate);
    }

    private static VmPEnemyWake1149Slice.Heard Monster(bool shootable, bool isFriend = false, bool isGoal = false)
    {
        return new VmPEnemyWake1149Slice.Heard(true, 100, false, 0, false, shootable, isFriend, isGoal);
    }

    private static VmPEnemyWake1149Slice.PlayerLook Look(
        int tidToHate = 0,
        bool foundTid = false,
        int flags = 0,
        int flags3 = 0,
        int flags8 = 0,
        int friendPlayer = 0,
        bool teamplay = false,
        int team = VmPEnemyWake1149Slice.TeamNone,
        bool deathmatch = false,
        bool foundEnemies = false,
        bool hasParams = false,
        int lookFlags = 0,
        int gameType = VmPEnemyWake1149Slice.DoomStrifeChex,
        bool primaryLevel = false,
        bool multiplayer = false,
        int player0Health = 100,
        bool hasGoal = false,
        int gameState = 0,
        bool foundMonsters = false,
        int look2Roll = 0,
        int lastLookPlayer = 0,
        bool hasTarget = false,
        bool targetIsGoal = false,
        bool lastEnemyAlive = false,
        bool lastEnemyIsFriend = false,
        Func<int, bool>? playerInGame = null,
        Func<int, bool>? targetable = null)
    {
        return VmPEnemyWake1149Slice.LookForPlayers(
            tidToHate, foundTid, flags, flags3, flags8, friendPlayer, teamplay, team, deathmatch, foundEnemies,
            hasParams, lookFlags, gameType, primaryLevel, multiplayer, player0Health, hasGoal, gameState, foundMonsters,
            look2Roll, lastLookPlayer, hasTarget, targetIsGoal, lastEnemyAlive, lastEnemyIsFriend, playerInGame, targetable);
    }

    private static VmPEnemyWake1149Slice.Wake Wake(
        bool inConversation = false,
        int special = 0,
        int arg0 = 0,
        int arg2 = 0,
        int arg3 = 1,
        int mapTime = 0,
        bool patrolFound = false,
        int reactionTime = 0,
        int flags = 0,
        int flags2 = 0,
        int flags5 = 0,
        int flags8 = 0,
        int tidToHate = 0,
        bool compatSoundTarget = false,
        VmPEnemyWake1149Slice.Heard sectorSound = default,
        VmPEnemyWake1149Slice.Heard lastHeard = default,
        VmPEnemyWake1149Slice.Heard currentTarget = default,
        bool ambushSight = false,
        bool lookTargetIsGoal = false,
        bool seeSoundValid = false,
        bool hasSeeState = false,
        IReadOnlyList<bool>? looks = null)
    {
        return VmPEnemyWake1149Slice.Look(
            inConversation, special, arg0, arg2, arg3, mapTime, patrolFound, reactionTime, flags, flags2, flags5, flags8,
            tidToHate, compatSoundTarget, sectorSound, lastHeard, currentTarget, ambushSight, lookTargetIsGoal,
            seeSoundValid, hasSeeState, looks ?? Array.Empty<bool>());
    }
}
