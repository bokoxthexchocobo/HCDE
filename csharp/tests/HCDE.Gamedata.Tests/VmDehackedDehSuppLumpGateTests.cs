namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppLumpGateTests
{
    [Fact]
    public void TryOpenDehsupp_RejectsUserWad()
    {
        Assert.False(VmDehackedDehSuppLumpGate.TryOpenDehsupp(0, 1, out var warning));
        Assert.NotNull(warning);
    }

    [Fact]
    public void TryOpenDehsupp_AcceptsCoreLump()
    {
        Assert.True(VmDehackedDehSuppLumpGate.TryOpenDehsupp(0, 0, out _));
    }

    [Fact]
    public void FormatUnknownSectionError_MatchesNativeMessage()
    {
        Assert.Equal("Unknown section 'Foo'", VmDehackedDehSuppLumpGate.FormatUnknownSectionError("Foo"));
    }

    [Fact]
    public void TryHandleRecoverableLoadError_ReturnsFalse()
    {
        Assert.False(VmDehackedDehSuppLumpGate.TryHandleRecoverableLoadError("err", out var printed));
        Assert.Equal("err", printed);
    }
}
