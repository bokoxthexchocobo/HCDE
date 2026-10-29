namespace HCDE.Playsim.Tests;

public class VmPLnspecHate1156SliceTests
{
    [Fact]
    public void SpecificHate_SkipsAPlayerActivatorAndRemembersTheOldTarget()
    {
        var hater = Actor(1, 4, health: 10, see: true, targetId: 8, targetTid: 2, lastLook: 6);
        var dead = Actor(2, 9, health: 0);
        var foe = Actor(3, 9);
        var blocked = VmPLnspecHate1156Slice.Hate(0, 9, 0, true, true, hater, new[] { foe }, new[] { hater });
        Assert.False(blocked.Accepted);
        Assert.Equal(8, blocked.Activator.TargetId);

        var quiet = VmPLnspecHate1156Slice.Hate(0, 9, 0, false, false, hater, new[] { foe }, new[] { hater });
        Assert.True(quiet.Accepted);
        Assert.False(quiet.UsedActivator);

        var hated = VmPLnspecHate1156Slice.Hate(0, 9, 0, true, false, hater, new[] { dead, foe }, Array.Empty<VmPLnspecHate1156Slice.HateActor>());
        Assert.True(hated.UsedActivator);
        Assert.Equal(3, hated.Activator.TargetId);
        Assert.Equal(9, hated.Activator.TargetTid);
        Assert.Equal(8, hated.Activator.LastEnemyId);
        Assert.Equal(2, hated.Activator.LastEnemyTid);
        Assert.Equal(6, hated.Activator.LastLookId);
        Assert.Equal(-1, hated.Activator.TidToHate);
        Assert.True(hated.Activator.EnteredSee);
        Assert.Equal(0, hated.Activator.Flags3 & VmPLnspecHate1156Slice.NoSightCheck);
        Assert.Equal(0, hated.Activator.Flags4 & VmPLnspecHate1156Slice.NoHatePlayers);

        var blind = hater with { SeeState = false, Flags3 = VmPLnspecHate1156Slice.NoSightCheck };
        var skipped = VmPLnspecHate1156Slice.Hate(4, 9, 0, false, false, hater, new[] { foe }, new[] { blind, Actor(5, 4, targetId: 1, targetTid: 1) });
        Assert.False(skipped.Haters[0].EnteredSee);
        Assert.Equal(VmPLnspecHate1156Slice.NoSightCheck, skipped.Haters[0].Flags3);
        Assert.Equal(3, skipped.Haters[1].TargetId);
        Assert.True(skipped.Haters[1].EnteredSee);
    }

    [Fact]
    public void GroupModes_SetTheHuntBitsAndStillRequireAGoal()
    {
        int all = VmPLnspecHate1156Slice.NoSightCheck | VmPLnspecHate1156Slice.HuntPlayers;
        var hater = Actor(1, 4, flags3: all, flags4: VmPLnspecHate1156Slice.NoHatePlayers, targetId: 8, targetTid: 3, lastId: 7, lastTid: 3, goal: -1);
        var none = VmPLnspecHate1156Slice.Hate(4, 9, 2, false, false, hater, new[] { Actor(2, 8, health: 0) }, new[] { hater });
        Assert.Equal(9, none.Haters[0].TidToHate);
        Assert.Equal(-1, none.Haters[0].LastLookId);
        Assert.Equal(-1, none.Haters[0].TargetId);
        Assert.Equal(-1, none.Haters[0].LastEnemyId);
        Assert.Equal(VmPLnspecHate1156Slice.NoSightCheck, none.Haters[0].Flags3 & VmPLnspecHate1156Slice.NoSightCheck);
        Assert.Equal(0, none.Haters[0].Flags3 & VmPLnspecHate1156Slice.HuntPlayers);
        Assert.False(none.Haters[0].EnteredSee);

        var noGoal = VmPLnspecHate1156Slice.Hate(4, 9, 2, false, false, hater, new[] { Actor(3, 9) }, new[] { hater with { TargetId = -1, TargetTid = 0 } });
        Assert.Equal(-1, noGoal.Haters[0].TargetId);
        Assert.False(noGoal.Haters[0].EnteredSee);

        var aimed = VmPLnspecHate1156Slice.Hate(
            4, 9, 4, false, false, hater,
            new[] { Actor(3, 9) },
            new[] { hater with { GoalId = 5, TargetId = 8, TargetTid = 9 } });
        Assert.Equal(3, aimed.Haters[0].TargetId);
        Assert.Equal(8, aimed.Haters[0].LastEnemyId);
        Assert.True(aimed.Haters[0].EnteredSee);
        Assert.Equal(VmPLnspecHate1156Slice.NoSightCheck, aimed.Haters[0].Flags3 & VmPLnspecHate1156Slice.NoSightCheck);
        Assert.Equal(VmPLnspecHate1156Slice.HuntPlayers, aimed.Haters[0].Flags3 & VmPLnspecHate1156Slice.HuntPlayers);
        Assert.Equal(0, aimed.Haters[0].Flags4 & VmPLnspecHate1156Slice.NoHatePlayers);

        var ignore = VmPLnspecHate1156Slice.Hate(4, 9, 6, false, false, hater, new[] { Actor(3, 9) }, new[] { hater with { GoalId = 5, TargetId = -1 } });
        Assert.Equal(VmPLnspecHate1156Slice.NoHatePlayers, ignore.Haters[0].Flags4 & VmPLnspecHate1156Slice.NoHatePlayers);
        Assert.Equal(0, ignore.Haters[0].Flags3 & VmPLnspecHate1156Slice.HuntPlayers);

        var players = VmPLnspecHate1156Slice.Hate(4, 9, 5, false, false, hater, new[] { Actor(3, 9) }, new[] { hater with { GoalId = 5, TargetId = -1, Flags3 = all } });
        Assert.Equal(0, players.Haters[0].Flags3 & VmPLnspecHate1156Slice.NoSightCheck);
        Assert.Equal(VmPLnspecHate1156Slice.NoHatePlayers, players.Haters[0].Flags4);

        var dormant = Actor(1, 4, flags2: VmPLnspecHate1156Slice.Dormant, goal: 5, targetId: -1);
        var asleep = VmPLnspecHate1156Slice.Hate(4, 9, 1, false, false, dormant, new[] { Actor(3, 9) }, new[] { dormant });
        Assert.Equal(3, asleep.Haters[0].TargetId);
        Assert.False(asleep.Haters[0].EnteredSee);
        Assert.Equal(0, asleep.Haters[0].Flags3);
    }

    [Fact]
    public void Rescan_WrapsPastTheFirstHateeAndSharesASpecificOne()
    {
        var first = Actor(1, 4, goal: 5, targetId: 2, targetTid: 9);
        var second = Actor(2, 4, goal: 5, targetId: -1);
        var shared = VmPLnspecHate1156Slice.Hate(4, 9, 0, false, false, first, new[] { Actor(11, 9), Actor(12, 9) }, new[] { first, second });
        Assert.Equal(11, shared.Haters[0].TargetId);
        Assert.Equal(11, shared.Haters[1].TargetId);

        var wrapped = VmPLnspecHate1156Slice.Hate(
            4, 9, 2, false, false, first,
            new[] { Actor(11, 9), Actor(12, 9, health: 0) },
            new[] { first });
        Assert.Equal(11, wrapped.Haters[0].TargetId);

        var advanced = VmPLnspecHate1156Slice.Hate(
            4, 9, 2, false, false, first,
            new[] { Actor(11, 9), Actor(12, 9) },
            new[] { first, second });
        Assert.Equal(12, advanced.Haters[0].TargetId);
        Assert.Equal(11, advanced.Haters[1].TargetId);

        var self = VmPLnspecHate1156Slice.Hate(4, 4, 2, false, false, first, new[] { Actor(1, 4) }, new[] { first with { GoalId = 5, TargetTid = 4 } });
        Assert.Equal(2, self.Haters[0].TargetId);
        Assert.False(self.Haters[0].EnteredSee);

        var activator = Actor(40, 1);
        var chased = VmPLnspecHate1156Slice.Hate(4, 0, 2, true, false, activator, Array.Empty<VmPLnspecHate1156Slice.HateActor>(), new[] { first with { GoalId = 5, TargetId = -1 } });
        Assert.Equal(40, chased.Haters[0].TargetId);
        Assert.Equal(0, chased.Haters[0].TidToHate);
        Assert.Equal(-1, chased.Haters[0].LastLookId);
    }

    private static VmPLnspecHate1156Slice.HateActor Actor(
        int id,
        int tid,
        int flags3 = 0,
        int flags4 = 0,
        int flags2 = 0,
        int health = 5,
        bool see = true,
        int targetId = -1,
        int targetTid = 0,
        int lastId = -1,
        int lastTid = 0,
        int goal = -1,
        int lastLook = -1)
    {
        return new VmPLnspecHate1156Slice.HateActor(
            id, tid, VmPLnspecHate1156Slice.Shootable, flags2, flags3, flags4, health, see,
            targetId, targetTid, lastId, lastTid, goal, -1, lastLook, false);
    }
}
