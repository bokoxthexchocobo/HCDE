namespace HCDE.Playsim.Tests;

public class VmPTeleportEvTests
{
    [Fact]
    public void EvTeleport_RejectsBackSide()
    {
        var s = new VmPTeleportEv.EvTeleportState { Side = 1 };
        Assert.False(VmPTeleportEv.EvTeleport(s));
    }

    [Fact]
    public void EvTeleport_SucceedsWhenDestFound()
    {
        var s = new VmPTeleportEv.EvTeleportState
        {
            Side = 0,
            SelectTeleDest = (_, _, _, _) => new object(),
            Teleport = _ => true,
        };
        Assert.True(VmPTeleportEv.EvTeleport(s));
    }

    [Fact]
    public void EvTeleportOther_TeleportsEachVictim()
    {
        var count = 0;
        var s = new VmPTeleportEv.TeleportOtherState
        {
            OtherTid = 1,
            DestTid = 2,
            Victims = new object[] { new(), new() },
            TeleportVictim = _ => { count++; return true; },
        };
        Assert.True(VmPTeleportEv.EvTeleportOther(s));
        Assert.Equal(2, count);
    }


    [Fact]
    public void EvTeleportGroup_FallsBackWhenNoSource()
    {
        var s = new VmPTeleportEv.TeleportGroupState
        {
            SourceTid = 1,
            DestTid = 2,
            GroupTid = 3,
            Fog = true,
            FindActor = _ => null,
            TeleportOtherFallback = (_, _, fog) => fog,
        };
        Assert.True(VmPTeleportEv.EvTeleportGroup(s));
    }
}
