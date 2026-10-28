namespace HCDE.Playsim.Tests;

public class VmActionTeleportNativeFlagsTests
{
    [Fact]
    public void TelefragFlag_MatchesNativeEnum() =>
        Assert.Equal(0x00000001, VmActionTeleportNativeFlags.TfTelefrag);

    [Fact]
    public void OverrideFlag_MatchesNativeEnum() =>
        Assert.Equal(0x00000400, VmActionTeleportNativeFlags.TfOverride);

    [Fact]
    public void SensitiveZFlag_MatchesNativeEnum() =>
        Assert.Equal(0x00000800, VmActionTeleportNativeFlags.TfSensitiveZ);

    [Fact]
    public void UsesRandomDecide_WhenFlagSet() =>
        Assert.True(VmActionTeleportNativeFlags.UsesRandomDecide(VmActionTeleportNativeFlags.TfRandomDecide));

    [Fact]
    public void OverridesNoteleport_WhenOverrideBitSet() =>
        Assert.True(VmActionTeleportNativeFlags.OverridesNoteleport(VmActionTeleportNativeFlags.TfOverride));
}
