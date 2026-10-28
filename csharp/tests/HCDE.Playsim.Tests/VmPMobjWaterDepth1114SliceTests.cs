namespace HCDE.Playsim.Tests;

public class VmPMobjWaterDepth1114SliceTests
{
    [Fact]
    public void RunUpdateWaterDepth_UsesFullHeightWhenSectorUnderwater()
    {
        var ctx = new VmPMobjWaterDepth1114Slice.WaterDepthContext
        {
            HasSector = true,
            SectorMoreFlags = VmPMobjWaterDepth1114Slice.SecUnderwater,
            Height = 56
        };
        VmPMobjWaterDepth1114Slice.RunUpdateWaterDepth(ctx);
        Assert.Equal(56, ctx.WaterDepth);
        Assert.Equal(3, ctx.WaterLevel);
    }

    [Fact]
    public void RunUpdateWaterDepth_SkipsSolidRoversAndUsesSwimmableTop()
    {
        var ctx = new VmPMobjWaterDepth1114Slice.WaterDepthContext
        {
            HasSector = true,
            ActorZ = 0,
            ActorCenter = 28,
            Height = 56,
            Rovers = new[]
            {
                new VmPMobjWaterDepth1114Slice.FloorRover
                {
                    Flags = VmPMobjWaterDepth1114Slice.FfExists | VmPMobjWaterDepth1114Slice.FfSolid,
                    Top = 40,
                    Bottom = 0
                },
                new VmPMobjWaterDepth1114Slice.FloorRover
                {
                    Flags = VmPMobjWaterDepth1114Slice.FfExists | VmPMobjWaterDepth1114Slice.FfSwimmable,
                    Top = 20,
                    Bottom = 0
                }
            }
        };
        var floor = VmPMobjWaterDepth1114Slice.RunUpdateWaterDepth(ctx);
        Assert.Equal(20, floor);
        Assert.Equal(20, ctx.WaterDepth);
        Assert.Equal(1, ctx.WaterLevel);
    }

    [Fact]
    public void RunSplashCheck_HitsWaterOnRisingLevel()
    {
        var ctx = new VmPMobjWaterDepth1114Slice.SplashCheckContext
        {
            BoomWaterLevel = 0,
            WaterLevel = 2
        };
        VmPMobjWaterDepth1114Slice.RunSplashCheck(ctx, 10);
        Assert.True(ctx.HitWater);
        Assert.Equal(2, ctx.BoomWaterLevel);
    }
}
