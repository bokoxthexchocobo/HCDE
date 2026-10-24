namespace HCDE.Playsim.Tests;

public class VmActorPtrSelectTests
{
    [Fact]
    public void CopyAaptrex_DefaultReturnsOrigin()
    {
        var origin = new object();
        var result = VmActorPtrSelect.CopyAaptrex(
            origin,
            VmActorPtrSelect.AaptrDefault,
            VmActorPtrSelect.CsptrIgnore,
            _ => null,
            _ => false,
            _ => null,
            _ => null,
            _ => null,
            _ => null,
            _ => null,
            _ => 0,
            _ => false,
            0);
        Assert.Same(origin, result);
    }

    [Fact]
    public void CopyAaptrex_TargetSelectorReturnsTarget()
    {
        var origin = new object();
        var target = new object();
        var result = VmActorPtrSelect.CopyAaptrex(
            origin,
            VmActorPtrSelect.AaptrTarget,
            VmActorPtrSelect.CsptrIgnore,
            _ => null,
            _ => false,
            _ => null,
            _ => null,
            _ => target,
            _ => null,
            _ => null,
            _ => 0,
            _ => false,
            0);
        Assert.Same(target, result);
    }

    [Fact]
    public void VerifyTargetChain_ClearsLoop()
    {
        var a = new object();
        var b = new object();
        object? target = b;
        VmActorPtrSelect.VerifyTargetChain(
            a,
            false,
            _ => true,
            _ => target,
            o => target = null);
        Assert.Null(target);
    }

    [Fact]
    public void AssignAaptr_SetsTracerWithoutVerify()
    {
        object? tracer = null;
        var actor = new object();
        VmActorPtrSelect.AssignAaptr(
            actor,
            VmActorPtrSelect.AaptrTracer,
            new object(),
            0,
            (_, _) => { },
            (_, _) => { },
            (_, v) => tracer = v,
            (_, _) => { },
            _ => { });
        Assert.NotNull(tracer);
    }
}
