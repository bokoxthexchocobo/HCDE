namespace HCDE.Playsim.Tests;

public class VmPMobjPrintMiscActorInfoTests
{
    [Fact]
    public void Print_SkipsNullQuery() =>
        Assert.False(VmPMobjPrintMiscActorInfo.ShouldPrint(
            new VmPMobjPrintMiscActorInfo.MiscInfoHeader(false, 0, 0, false)));

    [Fact]
    public void Print_LegacyStyleMatch() =>
        Assert.Equal(3, VmPMobjPrintMiscActorInfo.MatchLegacyRenderStyle(true, 3));

    [Fact]
    public void Print_CountsFlagBits() =>
        Assert.Equal(2, VmPMobjPrintMiscActorInfo.CountFlagBits(0b101, 31));

    [Fact]
    public void Print_CustomStyleLabel() =>
        Assert.Equal("Custom", VmPMobjPrintMiscActorInfo.RenderStyleLabel(99));
}
