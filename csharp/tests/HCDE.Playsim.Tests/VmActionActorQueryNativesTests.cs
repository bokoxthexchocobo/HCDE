namespace HCDE.Playsim.Tests;

public class VmActionActorQueryNativesTests
{
    [Fact]
    public void ResolveCrouchFactor_DefaultsWithoutPlayer() =>
        Assert.Equal(1.0, VmActionActorQueryNatives.ResolveCrouchFactor(true, false, 0.5));

    [Fact]
    public void ResolveCrouchFactor_UsesPlayerFactor() =>
        Assert.Equal(0.5, VmActionActorQueryNatives.ResolveCrouchFactor(true, true, 0.5));

    [Fact]
    public void CvarFloatOrZero_WhenMissing() =>
        Assert.Equal(0, VmActionActorQueryNatives.CvarFloatOrZero(false, 12));

    [Fact]
    public void CvarStringOrEmpty_WhenMissing() =>
        Assert.Equal("", VmActionActorQueryNatives.CvarStringOrEmpty(false, "x"));

    [Fact]
    public void PlayerInputOrZero_WithoutPlayer() =>
        Assert.Equal(0, VmActionActorQueryNatives.PlayerInputOrZero(true, false, 5));

    [Fact]
    public void ShouldQueryProximity_WhenMobjSet() =>
        Assert.True(VmActionActorQueryNatives.ShouldQueryProximity(new object()));

    [Fact]
    public void IsDecorateInternalInt_MatchesName() =>
        Assert.True(VmActionActorQueryNatives.IsDecorateInternalInt("__decorate_internal_int__"));

    [Fact]
    public void DecorateFloatReturnCode_WhenNumretSet() =>
        Assert.Equal(1, VmActionActorQueryNatives.DecorateFloatReturnCode(true));
}
