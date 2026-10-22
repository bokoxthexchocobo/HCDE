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
            VmCallAction = (_, _) => new(true, null),
        };
        Assert.True(VmPActionCallStateChain.CallStateChain(ctx, s1));
    }

    [Fact]
    public void CallStateChain_MissingItemFlag_Fails()
    {
        var s1 = new VmPActionCallStateChain.ChainState { UseFlags = 0, Name = "Bad" };
        var logged = false;
        var ctx = new VmPActionCallStateChain.ChainContext
        {
            LogMissingItemFlag = _ => logged = true,
        };
        Assert.False(VmPActionCallStateChain.CallStateChain(ctx, s1));
        Assert.True(logged);
    }

    [Fact]
    public void CallStateChain_SelfJump_Aborts()
    {
        var s1 = new VmPActionCallStateChain.ChainState
        {
            UseFlags = VmPActionCallStateChain.SufItem,
            HasAction = false,
        };
        s1.Next = s1;
        var ctx = new VmPActionCallStateChain.ChainContext();
        Assert.False(VmPActionCallStateChain.CallStateChain(ctx, s1));
    }

    [Fact]
    public void BuildReturnPlan_StateAndInt()
    {
        var plan = VmPActionCallStateChain.BuildReturnPlan(new()
        {
            ReturnTypesCount = 2,
            ReturnType0 = VmPActionCallStateChain.TypeState,
            ReturnType1 = VmPActionCallStateChain.TypeBool,
        });
        Assert.Equal(2, plan.NumRet);
        Assert.True(plan.WantStateRet);
    }

    [Fact]
    public void GetZAtFloor_UsesNextLowest()
    {
        Assert.Equal(32, VmPActionCallStateChain.GetZAtFloor(true, 16, 32, false, false));
    }
}
