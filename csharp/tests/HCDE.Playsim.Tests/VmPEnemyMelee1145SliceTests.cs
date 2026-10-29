namespace HCDE.Playsim.Tests;

public class VmPEnemyMelee1145SliceTests
{
    [Fact]
    public void SoundLines_StopAtClosedDoorsAndASecondSoundBlock()
    {
        Assert.True(VmPEnemyMelee1145Slice.MarkAdjacentPlane(true));
        Assert.False(VmPEnemyMelee1145Slice.MarkAdjacentPlane(false));
        Assert.True(VmPEnemyMelee1145Slice.MarkSoundPortal(true, VmPEnemyMelee1145Slice.PortalSound, true));
        Assert.False(VmPEnemyMelee1145Slice.MarkSoundPortal(true, VmPEnemyMelee1145Slice.PortalSound, false));
        Assert.True(VmPEnemyMelee1145Slice.DoorBlocksSound(128, 64, 128, 64, 0, 192, 0, 192));
        Assert.True(VmPEnemyMelee1145Slice.DoorBlocksSound(0, 128, 0, 128, 64, 64, 64, 64));
        Assert.False(VmPEnemyMelee1145Slice.DoorBlocksSound(0, 128, 0, 128, 0, 128, 0, 128));

        var blocked = VmPEnemyMelee1145Slice.VisitOtherSector(true, VmPEnemyMelee1145Slice.TwoSided | VmPEnemyMelee1145Slice.SoundBlock, false, false, 1);
        Assert.False(blocked.Visit);
        var first = VmPEnemyMelee1145Slice.VisitOtherSector(true, VmPEnemyMelee1145Slice.TwoSided | VmPEnemyMelee1145Slice.SoundBlock, false, false, 0);
        Assert.True(first.Visit);
        Assert.Equal(1, first.SoundBlocks);
        var open = VmPEnemyMelee1145Slice.VisitOtherSector(true, VmPEnemyMelee1145Slice.TwoSided, false, false, 4);
        Assert.True(open.Visit);
        Assert.Equal(4, open.SoundBlocks);
        Assert.False(VmPEnemyMelee1145Slice.VisitOtherSector(true, VmPEnemyMelee1145Slice.TwoSided, true, false, 0).Visit);
        Assert.False(VmPEnemyMelee1145Slice.VisitOtherSector(false, VmPEnemyMelee1145Slice.TwoSided, false, false, 0).Visit);
    }

    [Fact]
    public void BeginNoiseAlert_IgnoresAMissingEmitterAndANoTargetPlayer()
    {
        Assert.False(VmPEnemyMelee1145Slice.BeginNoiseAlert(false, true, 0, 3).Run);
        var hidden = VmPEnemyMelee1145Slice.BeginNoiseAlert(true, true, VmPEnemyMelee1145Slice.NoTarget, 3);
        Assert.False(hidden.Run);
        Assert.Equal(3, hidden.ValidCount);
        var alert = VmPEnemyMelee1145Slice.BeginNoiseAlert(true, false, VmPEnemyMelee1145Slice.NoTarget, 3);
        Assert.True(alert.Run);
        Assert.Equal(4, alert.ValidCount);
    }

    [Fact]
    public void CheckMeleeRange_ReachesAGoalBeforeSectorAndFriendChecks()
    {
        Assert.Equal(0, Melee(hasTarget: false));
        Assert.Equal(0, Melee(distance: 80, range: 64, targetRadius: 16));
        Assert.Equal(1, Melee(distance: 70, range: -1, meleeRange: 64, targetRadius: 16, targetIsGoal: true, sectorFlags: VmPEnemyMelee1145Slice.SectorNoAttack, isFriend: true, hasSight: false));
        Assert.Equal(0, Melee(sectorFlags: VmPEnemyMelee1145Slice.SectorNoAttack));
        Assert.Equal(0, Melee(targetZ: 80, actorTop: 56));
        Assert.Equal(0, Melee(targetTop: 10, actorZ: 20));
        Assert.Equal(1, Melee(actorFlags5: VmPEnemyMelee1145Slice.NoVerticalMelee, targetZ: 80, actorTop: 56));
        Assert.Equal(0, Melee(isFriend: true));
        Assert.Equal(0, Melee(hasSight: false));
        Assert.Equal(1, Melee());
    }

    private static int Melee(
        bool hasTarget = true,
        double distance = 40,
        double range = 64,
        double meleeRange = 64,
        double targetRadius = 16,
        bool targetIsGoal = false,
        int sectorFlags = 0,
        int actorFlags5 = 0,
        double targetZ = 0,
        double actorTop = 56,
        double targetTop = 56,
        double actorZ = 0,
        bool isFriend = false,
        bool hasSight = true)
    {
        return VmPEnemyMelee1145Slice.CheckMeleeRange(
            hasTarget, distance, range, meleeRange, targetRadius, targetIsGoal, sectorFlags, actorFlags5,
            targetZ, actorTop, targetTop, actorZ, isFriend, hasSight);
    }
}
