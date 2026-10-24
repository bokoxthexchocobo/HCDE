namespace HCDE.Playsim.Tests;

public class VmDThinkerInstanceTests
{
    [Fact]
    public void Remove_UnlinksDoublyLinkedNode()
    {
        var list = new VmDThinkerListOps.ThinkerListModel
        {
            CreateSentinel = () => new VmDThinkerListOps.ThinkerNodeModel()
        };
        var node = new VmDThinkerListOps.ThinkerNodeModel();
        VmDThinkerListOps.AddTail(list, node);
        VmDThinkerInstance.Remove(node, null);
        Assert.Null(node.NextThinker);
        Assert.Null(VmDThinkerListOps.GetHead(list));
    }

    [Fact]
    public void CallPostBeginPlay_UsesVirtualWhenPresent()
    {
        var ran = false;
        var node = new VmDThinkerListOps.ThinkerNodeModel { HasVirtualPostBeginPlay = true, VirtualPostBeginPlay = () => ran = true };
        VmDThinkerInstance.CallPostBeginPlay(node);
        Assert.True(ran);
        Assert.NotEqual(0, node.ObjectFlags & VmDThinkerInstance.OfSpawned);
    }

    [Fact]
    public void ChangeStatNumAction_BlocksTravellingAndStaticActor()
    {
        var messages = new List<string>();
        var ctx = new VmDThinkerInstance.ChangeStatContext { Printf = messages.Add };
        var travelling = new VmDThinkerListOps.ThinkerNodeModel { ObjectFlags = VmDThinkerInstance.OfTravelling };
        VmDThinkerInstance.ChangeStatNumAction(travelling, ThinkerStat.Default, ctx);
        Assert.Contains(messages, m => m.Contains("travelling", StringComparison.OrdinalIgnoreCase));

        var actor = new VmDThinkerListOps.ThinkerNodeModel { IsKindOf = name => name == "Actor" };
        VmDThinkerInstance.ChangeStatNumAction(actor, VmDThinkerInstance.StatStatic, ctx);
        Assert.Contains(messages, m => m.Contains("STAT_STATIC", StringComparison.Ordinal));
    }

    [Fact]
    public void ParseProfileThinkersCommand_ParsesModes()
    {
        var (mode, limit, parsed) = VmDThinkerInstance.ParseProfileThinkersCommand(["profilethinkers", "+t", "5"]);
        Assert.True(parsed);
        Assert.Equal(7, mode);
        Assert.Equal(5, limit);
    }

    [Fact]
    public void PropagateMark_MarksNeighbors()
    {
        var marked = new List<VmDThinkerListOps.ThinkerNodeModel?>();
        var next = new VmDThinkerListOps.ThinkerNodeModel();
        var prev = new VmDThinkerListOps.ThinkerNodeModel();
        var self = new VmDThinkerListOps.ThinkerNodeModel { NextThinker = next, PrevThinker = prev };
        VmDThinkerInstance.PropagateMark(self, marked.Add, () => 1);
        Assert.Contains(next, marked);
        Assert.Contains(prev, marked);
    }
}
