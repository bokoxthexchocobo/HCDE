namespace HCDE.Playsim.Tests;

public class VmPTraceSetup3DFloorsTests
{
    [Fact]
    public void Setup3DFloors_ClipsInsideSolid3DFloor()
    {
        var ctx = new VmPTraceSetup3DFloors.Setup3DFloorsContext
        {
            Floors =
            [
                new VmPTraceSetup3DFloors.FFloorSlice
                {
                    Flags = VmPTraceSetup3DFloors.FfExists,
                    BottomZ = 10,
                    TopZ = 20,
                },
            ],
            PosZ = 15,
            BaseFloorZ = 0,
            BaseCeilingZ = 128,
        };
        VmPTraceSetup3DFloors.Setup3DFloors(ctx);
        Assert.True(ctx.UseDummySector);
        Assert.False(ctx.InShootThrough);
        Assert.Equal(10, ctx.ClampedFloorZ);
        Assert.Equal(20, ctx.ClampedCeilingZ);
    }
}
