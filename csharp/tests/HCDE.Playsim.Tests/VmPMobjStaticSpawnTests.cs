namespace HCDE.Playsim.Tests;

public class VmPMobjStaticSpawnTests
{
    [Fact]
    public void StaticSpawn_RejectsNullType() =>
        Assert.True(VmPMobjStaticSpawn.RejectNullType(true));

    [Fact]
    public void ClientSideSpawn_RequiresClientClass() =>
        Assert.True(VmPMobjStaticSpawn.ClientSideSpawnRequiresClientClass(true));

    [Fact]
    public void LevelSpawned_RandomizesUnsyncedTics() =>
        Assert.Equal(2, VmPMobjStaticSpawn.LevelSpawnedTics(3, false, 1));

    [Fact]
    public void HandleSpawnFlags_FriendlyClearsCountKill() =>
        Assert.Equal(-1, VmPMobjStaticSpawn.ApplyHandleSpawnFlags(
            new VmPMobjStaticSpawn.HandleSpawnFlagsContext(
                VmPMobjStaticSpawn.MtfFriendly, VmPMobjStaticSpawn.MfCountkill, 0, 0, false, false, false)).MonsterDelta);

    [Fact]
    public void HandleSpawnFlags_SecretIncrements() =>
        Assert.Equal(1, VmPMobjStaticSpawn.ApplyHandleSpawnFlags(
            new VmPMobjStaticSpawn.HandleSpawnFlagsContext(
                VmPMobjStaticSpawn.MtfSecret, 0, 0, 0, false, false, false)).SecretDelta);

    [Fact]
    public void BeginPlay_ClearsDormantWhenSet() =>
        Assert.True(VmPMobjStaticSpawn.BeginPlayClearsDormant(VmPMobjStaticSpawn.Mf2Dormant));
}
