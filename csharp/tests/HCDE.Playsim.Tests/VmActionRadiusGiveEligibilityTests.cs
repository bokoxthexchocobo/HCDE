namespace HCDE.Playsim.Tests;

public class VmActionRadiusGiveEligibilityTests
{
    [Fact]
    public void SpeciesPasses_WhenNoFilter() =>
        Assert.True(VmActionRadiusGiveEligibility.SpeciesPasses(null, "imp", exclude: false));

    [Fact]
    public void ClassPasses_WhenFilterNull() =>
        Assert.True(VmActionRadiusGiveEligibility.ClassPasses(filterNull: true, sameClass: false, exclude: false));

    [Fact]
    public void FilterSpeciesCombined_EitherMode() =>
        Assert.True(VmActionRadiusGiveEligibility.FilterSpeciesCombined(false, true, VmActionRadiusGiveEligibility.RgfEither));

    [Fact]
    public void OutsideCube_WhenAxisBeyondDistance() =>
        Assert.True(VmActionRadiusGiveEligibility.OutsideCube(200, 0, 0, 100, 0));

    [Fact]
    public void PointerRelationPass_InclusiveRequiresOne() =>
        Assert.True(VmActionRadiusGiveEligibility.PointerRelationPass(true, false, false, VmActionRadiusGiveEligibility.RgfInclusive));
}
