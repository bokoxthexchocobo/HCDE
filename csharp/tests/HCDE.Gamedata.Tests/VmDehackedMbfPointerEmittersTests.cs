namespace HCDE.Gamedata.Tests;

public class VmDehackedMbfPointerEmittersTests
{
    [Fact]
    public void FindTracerArgs_DefaultsRange()
    {
        var args = VmDehackedMbfPointerEmitters.FindTracerArgs(1.5f, 0);
        Assert.Equal(10, args.range);
    }

    [Fact]
    public void SeekerMissileArgs_AppliesDefaults()
    {
        var args = VmDehackedMbfPointerEmitters.SeekerMissileArgs(1, 2, 3, 0, 0);
        Assert.Equal(50, args.d);
        Assert.Equal(10, args.e);
    }

    [Fact]
    public void Tracer2Angle_UsesDefaultWhenZero()
    {
        Assert.Equal(19.6875f, VmDehackedMbfPointerEmitters.Tracer2Angle(0));
    }
}
