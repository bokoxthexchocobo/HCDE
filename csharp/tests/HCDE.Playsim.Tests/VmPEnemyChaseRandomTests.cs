namespace HCDE.Playsim.Tests;

public class VmPEnemyChaseRandomTests
{
    [Fact]
    public void CheckMeleeRange_RejectsFriend()
    {
        var s = new VmPEnemyChaseRandom.MeleeRangeState
        {
            Distance2D = 32,
            MeleeRange = 64,
            TargetRadius = 16,
            IsFriend = true,
            CheckSight = () => true,
        };
        Assert.False(VmPEnemyChaseRandom.CheckMeleeRange(s, -1));
    }

    [Fact]
    public void CheckMissileRange_ClearsJustHitWhenFriendlyInfight()
    {
        var s = new VmPEnemyChaseRandom.MissileRangeState
        {
            Flags = VmPEnemyChaseRandom.MfJusthit | VmPEnemyChaseRandom.MfFriendly,
            TargetHealth = 50,
            TargetIsFriend = true,
            TargetHasPlayer = true,
            CheckSight = () => true,
            PrDefect = () => 200,
        };
        Assert.True(VmPEnemyChaseRandom.CheckMissileRange(s));
        Assert.Equal(VmPEnemyChaseRandom.MfFriendly, s.Flags);
    }

    [Fact]
    public void CheckMissileRange_BlocksDuringReactionTime()
    {
        var s = new VmPEnemyChaseRandom.MissileRangeState
        {
            ReactionTime = 5,
            CheckSight = () => true,
        };
        Assert.False(VmPEnemyChaseRandom.CheckMissileRange(s));
    }

    [Fact]
    public void RandomChaseDir_TurnsWhenWalkFails()
    {
        var tries = 0;
        var s = new VmPEnemyChaseRandom.RandomChaseDirState
        {
            Movedir = VmPEnemyChaseRandom.DiEast,
            PrNewChaseDir = () => 300,
            TryWalk = () => ++tries > 1,
        };
        VmPEnemyChaseRandom.RandomChaseDir(s);
        Assert.NotEqual(VmPEnemyChaseRandom.DiNodir, s.Movedir);
    }
}
