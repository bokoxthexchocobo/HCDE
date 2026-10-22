namespace HCDE.Playsim.Tests;

public class VmPUserHcdeWeaponPhysicsTests
{
    [Fact]
    public void ClientPredictsLocalWeapon_RequiresDedicatedClientSlot() =>
        Assert.True(VmPUserHcdeWeaponPhysics.ClientPredictsLocalWeapon(
            true, true, false, true, false, 0, true));

    [Fact]
    public void ShouldTickWeaponPsprites_SuppressesAuthoritativeWhenPredicting() =>
        Assert.False(VmPUserHcdeWeaponPhysics.ShouldTickWeaponPsprites(
            true, 0, false, VmPUserHcdeWeaponPhysics.PstLive, VmPUserHcdeWeaponPhysics.PstLive));

    [Fact]
    public void ShouldTickWeaponPsprites_TicksOnNewPredictTic() =>
        Assert.True(VmPUserHcdeWeaponPhysics.ShouldTickWeaponPsprites(
            true, VmPUserHcdeWeaponPhysics.CfPredicting, true, VmPUserHcdeWeaponPhysics.PstLive, VmPUserHcdeWeaponPhysics.PstLive));

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
        Assert.Equal(0, VmPUserHcdeWeaponPhysics.PhysicsLinkBackup.IndexOnSectorThingList(map.Head(sector), target));
        Assert.NotNull(second);
    }
}
