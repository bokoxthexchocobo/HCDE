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
        };
        ctx.LinkedSectorPortalIds.Add(1);
        ctx.LinkedSectorPortalIds.Add(2);
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
