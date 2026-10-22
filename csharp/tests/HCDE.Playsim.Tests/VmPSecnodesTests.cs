namespace HCDE.Playsim.Tests;

public class VmPSecnodesTests
{
    [Fact]
    public void GetSecnode_ReusesFreelist()
    {
        var n = VmPSecnodes.GetSecnode();
        VmPSecnodes.PutSecnode(n);
        var n2 = VmPSecnodes.GetSecnode();
        Assert.Same(n, n2);
    }

    [Fact]
    public void AddSecnode_LinksSectorHead()
    {
        var sector = new object();
        VmPSecnodes.MsecNode? head = null;
        VmPSecnodes.AddSecnode(sector, new object(), null, ref head);
        Assert.NotNull(head);
        Assert.Same(sector, head!.Sector);
    }

    [Fact]
    public void AddSecnode_ReusesExistingSectorNode()
    {
        var sector = new object();
        var thingA = new object();
        var thingB = new object();
        VmPSecnodes.MsecNode? head = null;
        var list = VmPSecnodes.AddSecnode(sector, thingA, null, ref head);
        var again = VmPSecnodes.AddSecnode(sector, thingB, list, ref head);
        Assert.Same(list, again);
        Assert.Same(thingB, list!.Thing);
        Assert.Same(head, list);
    }

    [Fact]
    public void DelSecnode_UnlinksThreads()
    {
        var sector = new object();
        VmPSecnodes.MsecNode? head = null;
        var n1 = VmPSecnodes.AddSecnode(sector, new object(), null, ref head);
        var n2 = VmPSecnodes.AddSecnode(sector, new object(), n1, ref head);
        var next = VmPSecnodes.DelSecnode(n2, ref head);
        Assert.Same(n1, next);
        Assert.Same(n1, head);
    }

    [Fact]
    public void DelSeclist_ClearsThingList()
    {
        var sector = new object();
        VmPSecnodes.MsecNode? head = null;
        var list = VmPSecnodes.AddSecnode(sector, new object(), null, ref head);
        list = VmPSecnodes.AddSecnode(sector, new object(), list, ref head);
        VmPSecnodes.DelSeclist(list, ref head);
        Assert.Null(head);
    }

    [Fact]
    public void CreateSecNodeList_AddsLineCrossingSectors()
    {
        var home = new object();
        var front = new object();
        var back = new object();
        VmPSecnodes.MsecNode? homeHead = null;
        VmPSecnodes.MsecNode? frontHead = null;
        VmPSecnodes.MsecNode? backHead = null;
        var thing = new object();
        var crossings = new[]
        {
            new VmPSecnodes.LineCrossing(front, back),
        };
        var result = VmPSecnodes.CreateSecNodeList(null, new VmPSecnodes.CreateListContext
        {
            Thing = thing,
            HomeSector = home,
            LineCrossings = crossings,
            SectorHeadAccessor = sec => sec switch
            {
                _ when ReferenceEquals(sec, home) => ref homeHead,
                _ when ReferenceEquals(sec, front) => ref frontHead,
                _ => ref backHead,
            },
        });
        Assert.NotNull(result);
        Assert.NotNull(frontHead);
        Assert.NotNull(backHead);
        Assert.NotNull(homeHead);
    }

    [Fact]
    public void CreateSecNodeList_PrunesUnusedNodes()
    {
        var s1 = new object();
        var s2 = new object();
        VmPSecnodes.MsecNode? head1 = null;
        VmPSecnodes.MsecNode? head2 = null;
        var list = VmPSecnodes.AddSecnode(s1, new object(), null, ref head1);
        list = VmPSecnodes.AddSecnode(s2, new object(), list, ref head2);
        var thing = new object();
        var result = VmPSecnodes.CreateSecNodeList(list, new VmPSecnodes.CreateListContext
        {
            Thing = thing,
            HomeSector = s1,
            LineCrossings = Array.Empty<VmPSecnodes.LineCrossing>(),
            SectorHeadAccessor = sec => sec == s1 ? ref head1 : ref head2,
        });
        Assert.NotNull(result);
        Assert.Same(thing, result!.Thing);
    }
}
