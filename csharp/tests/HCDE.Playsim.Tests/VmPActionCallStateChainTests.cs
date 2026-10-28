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

    [Fact]
    public void BuildReturnPlan_StateOnly_IsJumpOnly()
    {
        var state = new VmPActionCallStateChain.ChainState
        {
            ReturnTypesCount = 1,
            ReturnType0 = VmPActionCallStateChain.TypeState,
        };
        var plan = VmPActionCallStateChain.BuildReturnPlan(state);
        Assert.True(plan.JumpOnlyStateReturn);
        Assert.False(VmPActionCallStateChain.DefaultActionRetval(plan));
    }

    [Fact]
    public void GetZAt_CeilingNo3dNoPortal_UsesPlane()
    {
        var z = VmPActionCallStateChain.GetZAt(0, 0, 0, 0, 0,
            VmPActionCallStateChain.GzfCeiling | VmPActionCallStateChain.GzfNo3dfloor | VmPActionCallStateChain.GzfNoportals,
            hasSector: true, floorZ: 8, ceilingZ: 96, nextLowest: 4, nextHighest: 88);
        Assert.Equal(96, z);
    }

    [Fact]
    public void PrepareGetZAtPosition_AppliesOffsetAtYaw()
    {
        var px = 0.0;
        var py = 0.0;
        var ang = 0.0;
        VmPActionCallStateChain.PrepareGetZAtPosition(ref px, ref py, ref ang, 10, 0, 0, 0);
        Assert.Equal(10, px);
        Assert.Equal(0, py);
    }

    [Fact]
    public void BuildFloorClipFlags_MapsRestrictAndNoPortals()
    {
        var flags = VmPActionCallStateChain.Gzf3Drestrict | VmPActionCallStateChain.GzfNoportals;
        Assert.Equal(
            VmPActionCallStateChain.Ffcf3dRestrict | VmPActionCallStateChain.FfcfNoportals,
            VmPActionCallStateChain.BuildFloorClipFlags(flags));
    }
}
