namespace HCDE.Playsim.Tests;

public class VmPTeleportCoreTests
{
    [Fact]
    public void SpawnTeleportFog_SkipsWhenTypeNull()
    {
        var s = new VmPTeleportCore.SpawnTeleportFogState { FogTypeNull = true };
        VmPTeleportCore.SpawnTeleportFog(s);
        Assert.Null(s.SpawnedFog);
    }

    [Fact]
    public void Teleport_AbortsWhenPreTeleportCancels()
    {
        var s = new VmPTeleportCore.TeleportState
        {
            PosZ = 0,
            FloorHeight = 0,
            PreTeleport = () => false,
            TeleportMove = (_, _, _) => true,
        };
        Assert.False(VmPTeleportCore.Teleport(s));
    }

    [Fact]
    public void Teleport_OnFloorZSnapsPlayerToFloor()
    {
        var s = new VmPTeleportCore.TeleportState
        {
            PosZ = VmPTeleportCore.OnFloorZ,
            FloorHeight = 64,
            HasPlayer = true,
            PlayerIsMo = true,
            PreTeleport = () => true,
            TeleportMove = (_, _, _) => true,
        };
        Assert.True(VmPTeleportCore.Teleport(s));
        Assert.Equal(64, s.PosZ);
    }
}
