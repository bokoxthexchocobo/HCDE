using HCDE.Client;

namespace HCDE.Client.Tests;

public class VmHcdeRendererFallbackTests
{
    [Fact]
    public void MigratePreferBackend_MapsLegacyOpenGlEsToVulkan()
        => Assert.Equal(VmHcdeRendererFallback.BackendVulkan,
            VmHcdeRendererFallback.MigratePreferBackend(VmHcdeRendererFallback.LegacyOpenGlEsBackend));

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    public void MigratePreferBackend_ClampsInvalidToDefault(int invalid)
        => Assert.Equal(VmHcdeRendererFallback.BackendDefault, VmHcdeRendererFallback.MigratePreferBackend(invalid));

    [Fact]
    public void UsingSoftwareRenderer_MatchesNativeVidRendermodeCheck()
    {
        Assert.True(VmHcdeRendererFallback.UsingSoftwareRenderer(1));
        Assert.False(VmHcdeRendererFallback.UsingSoftwareRenderer(4));
    }

    [Fact]
    public void ForceDesktopOpenGlFallback_SwitchesNonOpenGlBackend()
        => Assert.Equal(VmHcdeRendererFallback.BackendOpenGl,
            VmHcdeRendererFallback.ForceDesktopOpenGlFallback(VmHcdeRendererFallback.BackendVulkan));

    [Fact]
    public void ActivateSoftwareRendererFallback_SetsTruecolorSoftwareMode()
        => Assert.Equal(VmHcdeRendererFallback.SoftwareRenderMode,
            VmHcdeRendererFallback.ActivateSoftwareRendererFallback(4));
}
