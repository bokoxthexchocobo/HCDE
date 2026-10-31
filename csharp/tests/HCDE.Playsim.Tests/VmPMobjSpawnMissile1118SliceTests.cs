namespace HCDE.Playsim.Tests;

public class VmPMobjSpawnMissile1118SliceTests
{
    [Fact]
    public void ResolveSpawnSound_UsesPositionWhenSpawnerMissingAndSectorAudible()
    {
        var site = VmPMobjSpawnMissile1118Slice.ResolveSpawnSound(
            seeSound: 3,
            flags: VmPMobjSpawnMissile1118Slice.MfSpawnSoundSource,
            hasSpawner: false,
            sectorFlags: 0);
        Assert.Equal(VmPMobjSpawnMissile1118Slice.SpawnSoundSite.SpawnPosition, site);
    }

    [Fact]
    public void GetDefaultSpeed_PrefersFastSpeedWhenSkillIsFast()
    {
        Assert.Equal(20, VmPMobjSpawnMissile1118Slice.GetDefaultSpeed(true, true, 20, 8));
        Assert.Equal(8, VmPMobjSpawnMissile1118Slice.GetDefaultSpeed(true, true, -1, 8));
        Assert.Equal(0, VmPMobjSpawnMissile1118Slice.GetDefaultSpeed(false, true, 20, 8));
    }

    [Fact]
    public void RunSpawnMissileXyz_ZerosHuggerVelocityAndAbortsFailedCheck()
    {
        var hugger = new VmPMobjSpawnMissile1118Slice.MissileAimContext
        {
            HasSource = true,
            HasType = true,
            HasDestination = true,
            Flags3 = VmPMobjSpawnMissile1118Slice.Mf3FloorHugger,
            VelocityZ = 9
        };
        Assert.True(VmPMobjSpawnMissile1118Slice.RunSpawnMissileXyz(hugger));
        Assert.Equal(0, hugger.VelocityZ);

        var blocked = new VmPMobjSpawnMissile1118Slice.MissileAimContext
        {
            HasSource = true,
            HasType = true,
            HasDestination = true,
            CheckSpawn = true,
            SpawnCheckPassed = false
        };
        Assert.False(VmPMobjSpawnMissile1118Slice.RunSpawnMissileXyz(blocked));
        Assert.Equal(VmPMobjSpawnMissile1118Slice.MissileAbort.FailedSpawnCheck, blocked.Abort);
    }

    [Fact]
    public void TrySpawnMissileZ_RejectsMissingSource()
    {
        Assert.False(VmPMobjSpawnMissile1118Slice.TrySpawnMissileZ(false, 64, out _));
        Assert.True(VmPMobjSpawnMissile1118Slice.TrySpawnMissileZ(true, 64, out var z));
        Assert.Equal(64, z);
        Assert.Equal(72, VmPMobjSpawnMissile1118Slice.SpawnMissileLaunchZ(40, 0));
        Assert.True(VmPMobjSpawnMissile1118Slice.OldSpawnMissileMissingArgs(false, true));
    }
}
