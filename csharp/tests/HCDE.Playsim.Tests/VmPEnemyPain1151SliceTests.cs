namespace HCDE.Playsim.Tests;

public class VmPEnemyPain1151SliceTests
{
    [Fact]
    public void DoChase_StopsForAResurrectionAndMasksTheHighFlag()
    {
        int flags = VmPEnemyPain1151Slice.FastChase
            | VmPEnemyPain1151Slice.NoPlayActive
            | VmPEnemyPain1151Slice.NightmareFast
            | VmPEnemyPain1151Slice.DontMove
            | unchecked((int)0xC0000000);
        var chased = VmPEnemyPain1151Slice.DoChase(flags, true);
        Assert.True(chased.Chased);
        Assert.True(chased.Fast);
        Assert.False(chased.PlayActive);
        Assert.True(chased.Nightmare);
        Assert.True(chased.DontMove);
        Assert.Equal(flags & VmPEnemyPain1151Slice.FlagMask, chased.PassedFlags);

        var raised = VmPEnemyPain1151Slice.DoChase(flags | VmPEnemyPain1151Slice.Resurrect, true);
        Assert.False(raised.Chased);
        Assert.Equal(2048, VmPEnemyPain1151Slice.MissileRange);
        Assert.Equal(60, VmPEnemyPain1151Slice.AimWindowDegrees);
    }

    [Fact]
    public void Turn_LimitsYawAndAimsPitchFromTheChosenBodyPoint()
    {
        var none = VmPEnemyPain1151Slice.Turn(false, VmPEnemyPain1151Slice.Ambush, 0, 10, 4, 0, 0, 270, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, false, 1, 1, 0, 0);
        Assert.False(none.Turned);
        Assert.Equal(VmPEnemyPain1151Slice.Ambush, none.Flags);

        var toward = Turn(yaw: 0, otherAngle: 90, maxTurn: 10, angleOffset: 1);
        Assert.Equal(11d, toward.Yaw);
        var away = Turn(yaw: 0, otherAngle: -90, maxTurn: 10, angleOffset: 1);
        Assert.Equal(-11d, away.Yaw);
        var snap = Turn(yaw: 0, otherAngle: 90, angleOffset: 4, pitch: 12);
        Assert.Equal(94d, snap.Yaw);
        Assert.Equal(12d, snap.Pitch);
        Assert.False(snap.Pitched);

        var shadow = Turn(yaw: 0, otherAngle: 90, shadowed: true, yawRoll: 256);
        Assert.Equal(135d, shadow.Yaw);
        var stealth = Turn(flags: VmPEnemyPain1151Slice.Stealth | VmPEnemyPain1151Slice.Ambush, yaw: 0, otherAngle: 90);
        Assert.Equal(1, stealth.VisDir);
        Assert.Equal(VmPEnemyPain1151Slice.Stealth, stealth.Flags);

        var pitch = Turn(maxPitch: 0, deltaX: 3, selfZ: -32, otherZ: -28, selfTop: 100, otherTop: 100);
        Assert.Equal(-Math.Asin(0.8) * (180d / Math.PI), pitch.Pitch, 5);

        var top = Turn(maxPitch: 0, faceFlags: VmPEnemyPain1151Slice.FaceBottom | VmPEnemyPain1151Slice.FaceTop, deltaX: 3, selfZ: -32, otherZ: 0, selfTop: 100, otherTop: 56, zAdd: 0);
        Assert.NotEqual(pitch.Pitch, top.Pitch);

        var limited = Turn(maxPitch: 10, pitch: 0, deltaX: 3, selfZ: -32, otherZ: -28, selfTop: 100, otherTop: 100);
        Assert.Equal(-10d, limited.Pitch);

        var vert = Turn(maxPitch: 0, flags9: VmPEnemyPain1151Slice.ShadowAimVertical, shadowed: true, pitchRoll: 256, deltaX: 3, selfZ: -32, otherZ: -28, selfTop: 100, otherTop: 100);
        Assert.Equal(pitch.Pitch + 45d, vert.Pitch, 5);
    }

    [Fact]
    public void MonsterRail_LeadsTheShotAndRestoresPitch()
    {
        var idle = VmPEnemyPain1151Slice.MonsterRail(false, VmPEnemyPain1151Slice.Ambush, 0, 8, 0, false, 0, 0, 0, 0, 1, false, 1, 1, 0, 0, 7);
        Assert.False(idle.Fired);
        Assert.Equal(VmPEnemyPain1151Slice.Ambush, idle.Flags);

        var aimed = VmPEnemyPain1151Slice.MonsterRail(
            true, VmPEnemyPain1151Slice.Stealth | VmPEnemyPain1151Slice.Ambush, VmPEnemyPain1151Slice.ShadowAimVertical,
            8, 40, false, 12, 10, 14, 0, 3, true, 1, 1, 256, 128, 9);
        Assert.True(aimed.Fired);
        Assert.Equal(1, aimed.VisDir);
        Assert.Equal(VmPEnemyPain1151Slice.Stealth, aimed.Flags);
        Assert.Equal(85d, aimed.Yaw);
        Assert.Equal(-Math.Atan2(4d, 3d) * (180d / Math.PI) + 22.5d, aimed.AttackPitch, 5);
        Assert.Equal(8d, aimed.RestoredPitch);
        Assert.Equal(9, aimed.Damage);
        Assert.False(VmPEnemyPain1151Slice.DropItem(false, true));
        Assert.False(VmPEnemyPain1151Slice.DropItem(true, false));
        Assert.True(VmPEnemyPain1151Slice.DropItem(true, true));
    }

    [Fact]
    public void Pain_PrefersADamageTypeAndUsesHealthBands()
    {
        Assert.Equal("*pain25", VmPEnemyPain1151Slice.PainAmount(24));
        Assert.Equal("*pain50", VmPEnemyPain1151Slice.PainAmount(25));
        Assert.Equal("*pain75", VmPEnemyPain1151Slice.PainAmount(50));
        Assert.Equal("*pain100", VmPEnemyPain1151Slice.PainAmount(75));

        var typed = VmPEnemyPain1151Slice.Pain(true, false, true, "pain", 10, true, "Fire", name => name == "*pain25-Fire");
        Assert.Equal("*pain25-Fire", typed.Name);
        Assert.True(typed.Rumble);
        Assert.Equal(VmPEnemyPain1151Slice.NoRumble, typed.ChannelFlags);

        var generic = VmPEnemyPain1151Slice.Pain(true, false, true, "pain", 10, true, "Fire", name => name == "*pain-Fire");
        Assert.Equal("*pain-Fire", generic.Name);

        var plain = VmPEnemyPain1151Slice.Pain(true, false, true, "pain", 80, false, "", _ => false);
        Assert.Equal("*pain100", plain.Name);

        var monster = VmPEnemyPain1151Slice.Pain(false, false, true, "grunt/pain", 10, true, "Fire", _ => true);
        Assert.Equal("grunt/pain", monster.Name);
        Assert.False(monster.Rumble);
        Assert.Equal(0, monster.ChannelFlags);

        var quiet = VmPEnemyPain1151Slice.Pain(true, true, false, "", 10, false, "", _ => true);
        Assert.False(quiet.Played);
        Assert.Equal(2, VmPEnemyPain1151Slice.VoiceChannel);
        Assert.Equal(1f, VmPEnemyPain1151Slice.NormalAttenuation);
    }

    private static VmPEnemyPain1151Slice.Face Turn(
        int flags = 0,
        int flags9 = 0,
        double yaw = 0,
        double pitch = 0,
        double otherAngle = 90,
        double maxTurn = 0,
        double maxPitch = 270,
        double angleOffset = 0,
        double pitchOffset = 0,
        int faceFlags = 0,
        double zAdd = 0,
        double selfZ = 0,
        double selfBob = 0,
        double selfTop = 64,
        double selfCenter = 32,
        double otherZ = 0,
        double otherBob = 0,
        double otherTop = 64,
        double otherCenter = 32,
        double deltaX = 0,
        double deltaY = 0,
        bool shadowed = false,
        double aimFactor = 1,
        double penalty = 1,
        int yawRoll = 0,
        int pitchRoll = 0)
    {
        return VmPEnemyPain1151Slice.Turn(
            true, flags, flags9, yaw, pitch, otherAngle, maxTurn, maxPitch, angleOffset, pitchOffset, faceFlags, zAdd,
            selfZ, selfBob, selfTop, selfCenter, otherZ, otherBob, otherTop, otherCenter, deltaX, deltaY,
            shadowed, aimFactor, penalty, yawRoll, pitchRoll);
    }
}
