namespace HCDE.Playsim.Tests;

public class VmPSecnodesPortalRender966SliceTests
{
    [Fact]
    public void DelPortalnode_UnlinksBothThreadsAndFreesNode()
    {
        var portal = new object();
        VmPSecnodes.MsecNode? portalHead = null;
        var first = VmPSecnodesPortalRender966Slice.AddPortalnode(portal, new object(), null, ref portalHead);
        var second = VmPSecnodesPortalRender966Slice.AddPortalnode(portal, new object(), first, ref portalHead);
        Assert.Same(first, VmPSecnodesPortalRender966Slice.DelPortalnode(second, ref portalHead));
        Assert.Null(second.ThingNext);
        Assert.Same(first, portalHead);
    }

    [Fact]
    public void AddPortalnode_PrependsPortalThread()
    {
        var portal = new object();
        VmPSecnodes.MsecNode? head = null;
        var first = VmPSecnodesPortalRender966Slice.AddPortalnode(portal, new object(), null, ref head);
        var second = VmPSecnodesPortalRender966Slice.AddPortalnode(portal, new object(), first, ref head);
        Assert.Same(second, head);
        Assert.Same(first, second.ThingNext);
        Assert.Same(second, first.ThingPrev);
    }

    [Fact]
    public void ShouldRefreshRenderLists_RequiresMoveWithoutNosector()
    {
        Assert.True(VmPSecnodesPortalRender966Slice.ShouldRefreshRenderLists(noSectorFlag: false, positionChanged: true));
        Assert.False(VmPSecnodesPortalRender966Slice.ShouldRefreshRenderLists(noSectorFlag: true, positionChanged: true));
    }

    [Fact]
    public void ShouldScanBlockForLinePortals_AllGatesRequired()
    {
        Assert.True(VmPSecnodesPortalRender966Slice.ShouldScanBlockForLinePortals(true, true, true));
        Assert.False(VmPSecnodesPortalRender966Slice.ShouldScanBlockForLinePortals(true, false, true));
    }

    [Fact]
    public void CeilingPortalLoopContinues_MatchesHeightGuards()
    {
        Assert.True(VmPSecnodesPortalRender966Slice.CeilingPortalLoopContinues(
            blocksMovement: false, planeZ: 64, lastHeight: 32, actorTop: 40));
        Assert.False(VmPSecnodesPortalRender966Slice.CeilingPortalLoopContinues(
            blocksMovement: true, planeZ: 64, lastHeight: 32, actorTop: 40));
    }

    [Fact]
    public void FloorPortalLoopContinues_MatchesHeightGuards()
    {
        Assert.True(VmPSecnodesPortalRender966Slice.FloorPortalLoopContinues(
            blocksMovement: false, planeZ: 16, lastHeight: 32, actorZ: 20));
        Assert.False(VmPSecnodesPortalRender966Slice.FloorPortalLoopContinues(
            blocksMovement: false, planeZ: 40, lastHeight: 32, actorZ: 20));
    }

    [Fact]
    public void ClearRenderSectorPortalList_EmptiesTouchingList()
    {
        var sector = new object();
        VmPSecnodes.MsecNode? head = null;
        var list = VmPSecnodes.AddSecnode(sector, new object(), null, ref head);
        VmPSecnodesPortalRender966Slice.ClearRenderSectorPortalList(ref list, ref head);
        Assert.Null(list);
        Assert.Null(head);
    }

    [Fact]
    public void RenderBoundingRadius_CapsAt128()
    {
        Assert.Equal(128, VmPSecnodesPortalRender966Slice.RenderBoundingRadius(200));
        Assert.Equal(30, VmPSecnodesPortalRender966Slice.RenderBoundingRadius(20));
    }
}
