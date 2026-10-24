namespace HCDE.Playsim.Tests;

public class VmPMapTestMobjZTests
{
    private static VmPMapTestMobjZ.ThingProbe Solid(string id, double x, double y, double z, double h) => new()
    {
        Id = id,
        Flags = VmPMapTestMobjZ.MfSolid,
        Radius = 16,
        PosX = x,
        PosY = y,
        Z = z,
        Top = z + h,
        Height = h,
        Species = 1,
    };

    [Fact]
    public void TestMobjZ_NoclipPasses()
    {
        var s = new VmPMapTestMobjZ.TestMobjZState
        {
            ActorId = "a",
            ActorFlags = VmPMapTestMobjZ.MfNoclip,
            EnumerateThings = () => new[] { Solid("b", 0, 0, 0, 40) },
        };
        Assert.True(VmPMapTestMobjZ.TestMobjZ(s, false, out var on));
        Assert.Null(on);
    }

    [Fact]
    public void TestMobjZ_BlocksOverlappingSolid()
    {
        var s = new VmPMapTestMobjZ.TestMobjZState { ActorId = "a" };
        s.ActorRadius = 16;
        s.ActorPosX = 0;
        s.ActorPosY = 0;
        s.ActorZ = 5;
        s.ActorTop = 35;
        s.Things = new[] { Solid("b", 0, 0, 0, 40) };
        Assert.Single(s.Things);
        Assert.False(VmPMapTestMobjZ.TestMobjZ(s, false, out var on));
        Assert.Equal("b", on!.Id);
    }

    [Fact]
    public void CheckOnmobj_RestoresZAndCollides()
    {
        double? restored = null;
        VmPMapTestMobjZ.ThingProbe? hit = null;
        var s = new VmPMapTestMobjZ.CheckOnmobjState
        {
            ActorZ = 12,
            TestMobj = new VmPMapTestMobjZ.TestMobjZState
            {
                ActorId = "a",
                ActorRadius = 16,
                ActorPosX = 0,
                ActorPosY = 0,
                ActorZ = 12,
                ActorTop = 52,
                Things = new[] { Solid("b", 0, 0, 0, 40) },
            },
            SetZ = z => restored = z,
            CollidedWith = t => hit = t,
        };
        var blocker = VmPMapTestMobjZ.CheckOnmobj(s);
        Assert.NotNull(blocker);
        Assert.Equal(12, restored);
        Assert.Equal("b", hit!.Id);
    }
}
