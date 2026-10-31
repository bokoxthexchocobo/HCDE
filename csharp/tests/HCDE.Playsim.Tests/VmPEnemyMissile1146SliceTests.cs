namespace HCDE.Playsim.Tests;

public class VmPEnemyMissile1146SliceTests
{
    [Fact]
    public void CheckMissileRange_FightsBackOnceThenUsesChance()
    {
        var back = Missile(actorFlags: VmPEnemyMissile1146Slice.JustHit);
        Assert.True(back.Attack);
        Assert.True(back.ClearedJustHit);

        var corpse = Missile(actorFlags: VmPEnemyMissile1146Slice.JustHit | VmPEnemyMissile1146Slice.Friendly, targetHealth: 0, isFriend: false);
        Assert.False(corpse.Attack);
        Assert.True(corpse.ClearedJustHit);

        var player = Missile(actorFlags: VmPEnemyMissile1146Slice.JustHit | VmPEnemyMissile1146Slice.Friendly, isFriend: true, targetIsPlayer: true, defectRoll: 128);
        Assert.False(player.Attack);
        Assert.True(Missile(actorFlags: VmPEnemyMissile1146Slice.JustHit | VmPEnemyMissile1146Slice.Friendly, isFriend: true, targetIsPlayer: true, defectRoll: 129).Attack);

        var struck = Missile(actorFlags: VmPEnemyMissile1146Slice.JustHit | VmPEnemyMissile1146Slice.Friendly, isFriend: true, targetFlags: VmPEnemyMissile1146Slice.JustHit, defectRoll: 200);
        Assert.False(struck.Attack);
        Assert.True(Missile(actorFlags: VmPEnemyMissile1146Slice.JustHit | VmPEnemyMissile1146Slice.Friendly, isFriend: true, defectRoll: 129).Attack);

        Assert.False(Missile(sectorFlags: VmPEnemyMissile1146Slice.SectorNoAttack, actorFlags: VmPEnemyMissile1146Slice.JustHit).ClearedJustHit);
        Assert.False(Missile(hasSight: false).Attack);
        Assert.False(Missile(reactionTime: 1).Attack);
        Assert.False(Missile(isFriend: true).Attack);
        Assert.False(Missile(actorFlags: VmPEnemyMissile1146Slice.Friendly, hitFriend: true).Attack);
        Assert.True(Missile(actorFlags: 0, hitFriend: true).Attack);

        Assert.False(Missile(distance2D: 400, hasMeleeState: false, maxTargetRange: 200).Attack);
        Assert.False(Missile(distance2D: 200, hasMeleeState: true, meleeThreshold: 200).Attack);
        Assert.True(Missile(distance2D: 200, hasMeleeState: false, missileRangeRoll: 8).Attack);
        Assert.False(Missile(distance2D: 200, hasMeleeState: false, missileRangeRoll: 7).Attack);
        Assert.True(Missile(distance2D: 10, hasMeleeState: false, missileRangeRoll: 0).Attack);
        Assert.Equal(5, VmPEnemyMissile1146Slice.SkillAggressiveness);
        Assert.Equal(4, VmPEnemyMissile1146Slice.SeePastBlockEverything);
    }

    [Fact]
    public void HitFriend_StopsOnlyOnADifferentFriend()
    {
        Assert.False(VmPEnemyMissile1146Slice.HitFriend(0, true, true, true));
        Assert.False(VmPEnemyMissile1146Slice.HitFriend(VmPEnemyMissile1146Slice.Friendly, false, true, true));
        Assert.False(VmPEnemyMissile1146Slice.HitFriend(VmPEnemyMissile1146Slice.Friendly, true, false, true));
        Assert.False(VmPEnemyMissile1146Slice.HitFriend(VmPEnemyMissile1146Slice.Friendly, true, true, false));
        Assert.True(VmPEnemyMissile1146Slice.HitFriend(VmPEnemyMissile1146Slice.Friendly, true, true, true));
    }

    [Fact]
    public void LiftAndCrush_UsePlatAndCeilingDirection()
    {
        Assert.True(VmPEnemyMissile1146Slice.IsOnLift(true, 0));
        Assert.True(VmPEnemyMissile1146Slice.IsOnLift(false, VmPEnemyMissile1146Slice.Lift));
        Assert.False(VmPEnemyMissile1146Slice.IsOnLift(false, 0));

        int dir = VmPEnemyMissile1146Slice.AccumulateCrushDirection(new[]
        {
            new VmPEnemyMissile1146Slice.CrushContact(false, VmPEnemyMissile1146Slice.CeilingDown),
            new VmPEnemyMissile1146Slice.CrushContact(true, VmPEnemyMissile1146Slice.CeilingWaiting),
            new VmPEnemyMissile1146Slice.CrushContact(true, VmPEnemyMissile1146Slice.CeilingUp),
            new VmPEnemyMissile1146Slice.CrushContact(true, VmPEnemyMissile1146Slice.CeilingDown),
        });
        Assert.Equal(-1, dir);
    }

    private static VmPEnemyMissile1146Slice.MissileDecision Missile(
        int sectorFlags = 0,
        bool hasSight = true,
        int actorFlags = 0,
        int targetHealth = 100,
        bool isFriend = false,
        bool targetIsPlayer = false,
        int targetFlags = 0,
        int defectRoll = 0,
        int reactionTime = 0,
        bool hitFriend = false,
        double distance2D = 200,
        bool hasMeleeState = true,
        double maxTargetRange = 0,
        double meleeThreshold = 0,
        double missileChanceMult = 1,
        double minMissileChance = 200,
        double aggressiveness = 1,
        int missileRangeRoll = 255)
    {
        return VmPEnemyMissile1146Slice.CheckMissileRange(
            sectorFlags, hasSight, actorFlags, targetHealth, isFriend, targetIsPlayer, targetFlags,
            defectRoll, reactionTime, hitFriend, distance2D, hasMeleeState, maxTargetRange, meleeThreshold,
            missileChanceMult, minMissileChance, aggressiveness, missileRangeRoll);
    }
}
