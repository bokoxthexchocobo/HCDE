namespace HCDE.Playsim.Tests;

public class VmPActionCallStateChainTests
{
    [Fact]
    public void CallStateChain_SucceedsWhenActionReturnsTrue()
    {
        var s1 = new VmPActionCallStateChain.ChainState
        {
            UseFlags = VmPActionCallStateChain.SufItem,
            HasAction = true,
        };
        var ctx = new VmPActionCallStateChain.ChainContext
        {
            VmCallAction = (_, _) => new VmPActionCallStateChain.ChainCallResult(true, null),
        };
        Assert.True(VmPActionCallStateChain.CallStateChain(ctx, s1));
    }

    [Fact]
    public void CallStateChain_MissingItemFlag_Fails()
    {
        var s1 = new VmPActionCallStateChain.ChainState { UseFlags = 0, Name = "Bad" };
        var ctx = new VmPActionCallStateChain.ChainContext();
        Assert.False(VmPActionCallStateChain.CallStateChain(ctx, s1));
    }

    [Fact]
    public void CallStateChain_SelfJump_Aborts()
    {
        var s1 = new VmPActionCallStateChain.ChainState
        {
            UseFlags = VmPActionCallStateChain.SufItem,
            HasAction = true,
        };
        var ctx = new VmPActionCallStateChain.ChainContext
        {
            VmCallAction = (_, _) => new VmPActionCallStateChain.ChainCallResult(true, s1),
        };
        Assert.False(VmPActionCallStateChain.CallStateChain(ctx, s1));
    }

    [Fact]
    public void GetZAt_FloorNo3dNoPortal_UsesPlane()
    {
        var z = VmPActionCallStateChain.GetZAt(0, 0, 0, 0, 0,
            VmPActionCallStateChain.GzfNo3dfloor | VmPActionCallStateChain.GzfNoportals,
            hasSector: true, floorZ: 32, ceilingZ: 128, nextLowest: 16, nextHighest: 200);
        Assert.Equal(32, z);
    }

    [Fact]
    public void GetZAt_NullMobj_ReturnsZero()
    {
        Assert.Equal(0, VmPActionCallStateChain.GetZAt(0, 0, 0, 0, 0, 0, hasSector: false, 0, 0, 0, 0));
    }
}
