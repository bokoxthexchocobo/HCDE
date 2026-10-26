namespace HCDE.Playsim.Tests;

public class VmPUserHcdeWeaponPhysicsTests
{
    [Fact]
    public void ClientPredictsLocalWeapon_RequiresDedicatedClientSlot() =>
        Assert.True(VmPUserHcdeWeaponPhysics.ClientPredictsLocalWeapon(
            true, true, false, true, false, 0, true));

    [Fact]
    public void ShouldTickWeaponPsprites_ReturnsFalseWithoutPlayer() =>
        Assert.False(VmPUserHcdeWeaponPhysics.ShouldTickWeaponPsprites(
            false, true, VmPUserHcdeWeaponPhysics.CfPredicting, true, VmPUserHcdeWeaponPhysics.PstLive, VmPUserHcdeWeaponPhysics.PstLive));

    [Fact]
    public void ShouldTickWeaponPsprites_SuppressesAuthoritativeWhenPredicting() =>
        Assert.False(VmPUserHcdeWeaponPhysics.ShouldTickWeaponPsprites(
            true, true, 0, false, VmPUserHcdeWeaponPhysics.PstLive, VmPUserHcdeWeaponPhysics.PstLive));

    [Fact]
    public void ShouldTickWeaponPsprites_TicksOnNewPredictTic() =>
        Assert.True(VmPUserHcdeWeaponPhysics.ShouldTickWeaponPsprites(
            true, true, VmPUserHcdeWeaponPhysics.CfPredicting, true, VmPUserHcdeWeaponPhysics.PstLive, VmPUserHcdeWeaponPhysics.PstLive));

    [Fact]
    public void CaptureFromTouching_RecordsSectorIndices()
    {
        var map = new VmPSecnodes.SectorHeadMap();
        var sector = new object();
        var actor = new object();
        var node = VmPSecnodes.AddSecnode(sector, actor, null, ref map.Head(sector));
        var bak = VmPUserHcdeWeaponPhysics.PhysicsLinkBackup.CaptureFromTouching(
            node, map, Array.Empty<(int, VmPSecnodes.MsecNode?, object)>(), sector, new object?[] { actor }, actor);
        Assert.Single(bak.TouchingSectorPositions);
        Assert.True(bak.ValidSectors.ContainsKey(sector));
    }

    [Fact]
    public void IndexOnSectorThingList_FindsThingPosition()
    {
        var map = new VmPSecnodes.SectorHeadMap();
        var sector = new object();
        var target = new object();
        var first = VmPSecnodes.AddSecnode(sector, new object(), null, ref map.Head(sector));
        var second = VmPSecnodes.AddSecnode(sector, target, first, ref map.Head(sector));
        Assert.Equal(-1, VmPUserHcdeWeaponPhysics.PhysicsLinkBackup.IndexOnSectorThingList(map.Head(sector), target));
        Assert.NotNull(second);
    }

    [Fact]
    public void Restore_RelinksTouchingSectorNodeAfterReorder()
    {
        var map = new VmPSecnodes.SectorHeadMap();
        var sector = new object();
        var actor = new object();
        var node = VmPSecnodes.AddSecnode(sector, actor, null, ref map.Head(sector));
        var bak = VmPUserHcdeWeaponPhysics.PhysicsLinkBackup.CaptureFromTouching(
            node, map, Array.Empty<(int, VmPSecnodes.MsecNode?, object)>(), null, Array.Empty<object?>(), actor);
        var extraNode = VmPSecnodes.GetSecnode();
        extraNode.Sector = sector;
        extraNode.Thing = new object();
        extraNode.SectorPrev = null;
        extraNode.SectorNext = map.Head(sector);
        if (map.Head(sector) is not null)
            map.Head(sector)!.SectorPrev = extraNode;
        ref var sectorHead = ref map.Head(sector);
        sectorHead = extraNode;
        VmPSecnodes.MsecNode? list = node;
        var ctx = new VmPUserHcdeWeaponPhysics.PhysicsLinkBackup.PhysicsRestoreContext
        {
            SectorHeads = map,
            GetTouchingSectorList = () => list,
            SetTouchingSectorList = v => list = v,
        };
        bak.Restore(ctx);
        Assert.NotNull(node);
        Assert.Same(node, map.Head(sector));
        Assert.Same(actor, node!.Thing);
    }

    [Fact]
    public void Restore_DropsTouchingNodeFromUnknownSector()
    {
        var map = new VmPSecnodes.SectorHeadMap();
        var known = new object();
        var unknown = new object();
        var actor = new object();
        var node = VmPSecnodes.AddSecnode(unknown, actor, null, ref map.Head(unknown));
        var bak = new VmPUserHcdeWeaponPhysics.PhysicsLinkBackup();
        bak.ValidSectors[known] = true;
        VmPSecnodes.MsecNode? list = node;
        var ctx = new VmPUserHcdeWeaponPhysics.PhysicsLinkBackup.PhysicsRestoreContext
        {
            SectorHeads = map,
            GetTouchingSectorList = () => list,
            SetTouchingSectorList = v => list = v,
        };
        bak.Restore(ctx);
        Assert.Null(list);
        Assert.Null(map.Head(unknown));
    }

    [Fact]
    public void Restore_RelinksActorInSectorThingList()
    {
        var sectorList = new VmPMapUtlActorWorldLink.SectorThingListHead();
        var first = new VmPMapUtlActorWorldLink.ActorLinkState();
        var actor = new VmPMapUtlActorWorldLink.ActorLinkState();
        var third = new VmPMapUtlActorWorldLink.ActorLinkState();
        sectorList.Head = first;
        first.NextInSector = actor;
        actor.PrevHead = sectorList;
        actor.NextInSector = third;
        third.PrevHead = sectorList;
        actor.PrevHead = sectorList;
        var bak = new VmPUserHcdeWeaponPhysics.PhysicsLinkBackup { ThingInSector = true, ThingPosition = -1 };
        var ctx = new VmPUserHcdeWeaponPhysics.PhysicsLinkBackup.PhysicsRestoreContext
        {
            ActorInSector = actor,
            SectorThingList = sectorList,
        };
        bak.Restore(ctx);
        Assert.Same(actor, sectorList.Head);
    }
}
