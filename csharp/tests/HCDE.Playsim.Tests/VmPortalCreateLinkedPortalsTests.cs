namespace HCDE.Playsim.Tests;

public class VmPortalCreateLinkedPortalsTests
{
    [Fact]
    public void CreateLinkedPortals_CollectsWithLinkedIds()
    {
        var collected = 0;
        var ctx = new VmPortalCreateLinkedPortals.CreateLinkedPortalsContext
        {
            CollectSectors = (_, _) => { collected++; return true; },
            LinkedOrgs =
            [
                new VmPortalCreateLinkedPortals.LinkedPortalOrg(10, 20),
                new VmPortalCreateLinkedPortals.LinkedPortalOrg(30, 40),
            ],
        };
        VmPortalCreateLinkedPortals.CreateLinkedPortals(ctx);
        Assert.Equal(4, collected);
    }

    [Fact]
    public void CreateLinkedPortals_FlagsDisplacementMismatch()
    {
        var ctx = new VmPortalCreateLinkedPortals.CreateLinkedPortalsContext
        {
            NextGroupId = 3,
            GetDisplacement = (x, y) => x == 1 && y == 2 ? (true, 10, 0) : (true, -9, 0),
        };
        VmPortalCreateLinkedPortals.CreateLinkedPortals(ctx);
        Assert.True(ctx.Bogus);
    }

    [Fact]
    public void ProcessLineLinkedPortals_DisablesUnconnected()
    {
        var disabled = new List<int>();
        var ctx = new VmPortalCreateLinkedPortals.CreateLinkedPortalsContext
        {
            LinePortals = [new VmPortalCreateLinkedPortals.PortalLine(0, 2, true, 5)],
            DisableUnconnectedLinePortal = disabled.Add,
        };
        var id = VmPortalCreateLinkedPortals.ProcessLineLinkedPortals(ctx, 1);
        Assert.Equal(1, id);
        Assert.Equal([0], disabled);
    }

    [Fact]
    public void ResolveCeilingBelowFloorClearPlane_PicksCeilingWhenPlaneDLower()
    {
        var sec = new VmPortalCreateLinkedPortals.PortalSector(
            0, 1, 1, false, false, 1, 0, true, true, 10, 20, 5, 20);
        Assert.Equal(VmPortalCreateLinkedPortals.CeilingPlane, VmPortalCreateLinkedPortals.ResolveCeilingBelowFloorClearPlane(sec));
    }
}
