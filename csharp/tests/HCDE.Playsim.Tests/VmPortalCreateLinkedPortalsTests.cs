namespace HCDE.Playsim.Tests;

public class VmPortalCreateLinkedPortalsTests
{
    [Fact]
    public void CreateLinkedPortals_CollectsWithLinkedIds()
    {
        var ctx = new VmPortalCreateLinkedPortals.CreateLinkedPortalsContext();
        ctx.LinkedSectorPortalIds.Add(1);
        ctx.LinkedSectorPortalIds.Add(2);
        var collected = 0;
        ctx.CollectSectors = (_, _) => { collected++; return true; };
        VmPortalCreateLinkedPortals.CreateLinkedPortals(ctx);
        Assert.True(collected >= 2);
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
}
