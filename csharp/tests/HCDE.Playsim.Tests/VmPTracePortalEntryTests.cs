namespace HCDE.Playsim.Tests;

public class VmPTracePortalEntryTests
{
    [Fact]
    public void IsLiquid_TrueForSwimmableOrTerrainSplash()
    {
        Assert.True(VmPTracePortalEntry.IsLiquid(true, 0, null));
        Assert.True(VmPTracePortalEntry.IsLiquid(false, 1, _ => (true, 3)));
        Assert.False(VmPTracePortalEntry.IsLiquid(false, 1, _ => (true, -1)));
    }

    [Fact]
    public void GetPortalTransition_MovesThroughCeilingPortal()
    {
        var once = false;
        var ctx = new VmPTracePortalEntry.PortalTransitionContext
        {
            PosZ = 20,
            CeilingPlaneZ = 10,
            BlocksCeiling = false,
            DispZ = 100,
        };
        ctx.PointInSector = (_, _, _) =>
        {
            if (!once)
            {
                once = true;
                ctx.BlocksCeiling = true;
            }
            return new object();
        };
        VmPTracePortalEntry.GetPortalTransition(ctx);
        Assert.Equal(120, ctx.PosZ);
    }

    [Fact]
    public void EnterSectorPortal_AppliesDisplacement()
    {
        var ctx = new VmPTracePortalEntry.EnterSectorPortalContext
        {
            StartX = 1,
            StartY = 2,
            MaxDist = 10,
            VecX = 1,
            Frac = 0.5,
            DispX = 5,
            DispY = 6,
        };
        VmPTracePortalEntry.EnterSectorPortal(ctx);
        Assert.Equal(6, ctx.StartX);
        Assert.Equal(8, ctx.StartY);
    }
}
