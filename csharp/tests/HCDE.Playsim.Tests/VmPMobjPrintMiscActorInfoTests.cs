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

    [Fact]
    public void Print_LegacyStyleTable() =>
        Assert.Equal("Fuzzy", VmPMobjPrintMiscActorInfo.RenderStyleLabel(2));

    [Fact]
    public void Print_ScanLegacyMatch() =>
        Assert.Equal(1, VmPMobjPrintMiscActorInfo.ScanLegacyRenderStyle([9, 3, 1], 3));
}
