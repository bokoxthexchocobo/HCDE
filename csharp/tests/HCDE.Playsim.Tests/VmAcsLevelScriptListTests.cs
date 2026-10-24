namespace HCDE.Playsim.Tests;

public class VmAcsLevelScriptListTests
{
    [Fact]
    public void GetControllerNative_PicksClientSideThinker()
    {
        var client = new VmAcsLevelScriptList.AcsScriptNode();
        var server = new VmAcsLevelScriptList.AcsScriptNode();
        Assert.Same(client, VmAcsLevelScriptList.GetControllerNative(true, client, server));
        Assert.Same(server, VmAcsLevelScriptList.GetControllerNative(false, client, server));
    }

    [Fact]
    public void PutFirstNative_MovesNodeToHead()
    {
        var controller = new VmAcsLevelScriptList.AcsScriptListController();
        var a = new VmAcsLevelScriptList.AcsScriptNode();
        var b = new VmAcsLevelScriptList.AcsScriptNode();
        VmAcsLevelScriptList.LinkNative(controller, a);
        VmAcsLevelScriptList.LinkNative(controller, b);
        VmAcsLevelScriptList.PutFirstNative(controller, a);
        Assert.Same(a, controller.Scripts);
        Assert.Same(b, a.Next);
        Assert.Same(a, b.Prev);
    }

    [Fact]
    public void PutLastNative_AppendsWhenListNonempty()
    {
        var controller = new VmAcsLevelScriptList.AcsScriptListController();
        var a = new VmAcsLevelScriptList.AcsScriptNode();
        var b = new VmAcsLevelScriptList.AcsScriptNode();
        VmAcsLevelScriptList.LinkNative(controller, a);
        VmAcsLevelScriptList.LinkNative(controller, b);
        VmAcsLevelScriptList.PutLastNative(controller, a);
        Assert.Same(b, controller.Scripts);
        Assert.Same(a, controller.LastScript);
        Assert.Null(a.Next);
    }

}
