namespace HCDE.Playsim.Tests;

public class VmPMobjSetStateUseTests
{
    [Fact]
    public void SetState_NullState_Destroys()
    {
        var actor = new VmPMobjSetStateUse.SetStateActor();
        var destroyed = false;
        var ctx = new VmPMobjSetStateUse.SetStateContext { Destroy = _ => destroyed = true };
        Assert.False(VmPMobjSetStateUse.SetState(ctx, actor, null, noFunction: true));
        Assert.True(destroyed);
    }

    [Fact]
    public void SetState_BadUseFlags_Destroys()
    {
        var actor = new VmPMobjSetStateUse.SetStateActor();
        var destroyed = false;
        var bad = new VmPMobjSetStateUse.StateNode { UseFlags = 0, Name = "Bad" };
        var ctx = new VmPMobjSetStateUse.SetStateContext { Destroy = _ => destroyed = true };
        Assert.False(VmPMobjSetStateUse.SetState(ctx, actor, bad, noFunction: true));
        Assert.True(destroyed);
    }

    [Fact]
    public void FindInventoryByClass_MatchesExactType()
    {
        var match = new VmPMobjSetStateUse.InventoryLink { TypeId = 3 };
        var head = new VmPMobjSetStateUse.InventoryLink { TypeId = 1, Next = match };
        Assert.Same(match, VmPMobjSetStateUse.FindInventoryByClass(head, 3, subclass: false));
    }

    [Fact]
    public void FindInventoryByName_ResolvesType()
    {
        var match = new VmPMobjSetStateUse.InventoryLink { TypeId = 9 };
        var head = new VmPMobjSetStateUse.InventoryLink { TypeId = 1, Next = match };
        Assert.Same(match, VmPMobjSetStateUse.FindInventoryByName(_ => 9, head, 0, subclass: false));
    }

    [Fact]
    public void DestroyAllInventory_EmptiesList()
    {
        var list = new List<object> { new(), new() };
        VmPMobjSetStateUse.DestroyAllInventory(list, _ => { });
        Assert.Empty(list);
    }

    [Fact]
    public void UseInventory_VirtualPath_Used()
    {
        Assert.True(VmPMobjSetStateUse.UseInventory(hasVirtual: true, () => true));
    }
}
