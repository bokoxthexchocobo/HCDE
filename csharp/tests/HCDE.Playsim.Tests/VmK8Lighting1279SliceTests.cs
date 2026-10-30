using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmK8Lighting1279SliceTests
{
    [Fact]
    public void Profile_ClampsOutsideTheRangeAndRaisesTheShadowFloor()
    {
        var held = new VmK8Lighting1279
        {
            Quality = 77,
            Tonemap = 2,
            Ssao = 4,
            MaxLights = 3,
            Filter = 2,
            AutoProfile = true,
        };
        held.SetProfile(0);
        Assert.Equal(0, held.Profile);
        Assert.True(held.AutoProfile);
        Assert.Equal(77, held.Quality);
        Assert.Equal(3, held.MaxLights);
        Assert.False(held.Snap.Applied);
        Assert.Empty(held.Log);

        var boost = new VmK8Lighting1279
        {
            Quality = 100,
            Tonemap = 2,
            MaxLights = 3,
            ShadowBoost = true,
            HardwareRenderer = true,
            Screen = new VmK8Screen1279
            {
                Backend = VmK8Lighting1279.BackendVulkan,
                RayQueries = true,
                HardwareShadowmaps = true,
                RaytracingActive = true,
            },
        };
        boost.SetProfile(1);
        Assert.Equal(1, boost.Profile);
        Assert.False(boost.RaylightProbe);
        Assert.Equal(1024, boost.Quality);
        Assert.Equal(3, boost.MaxLights);
        Assert.Equal(1, boost.Filter);
        Assert.Equal(2, boost.Tonemap);
        Assert.Equal(1, boost.Ssao);
        Assert.True(boost.LightShadowmap);
        Assert.True(boost.Prioritize);
        Assert.True(boost.Bloom);
        Assert.True(boost.AutoBudget);
        Assert.True(boost.AutoFallback);
        Assert.True(boost.VkRaytrace);
        Assert.True(boost.Snap.Applied);
        Assert.True(boost.Snap.Raylight);
        Assert.True(boost.Snap.VkRaytrace);
        Assert.Equal(3, boost.Snap.MaxLights);
        Assert.Equal(2, boost.Log.Count);
        Assert.Equal("Enabled vk_raytrace for k8vavoom ray-style dynamic light shadows.\n", boost.Log[0]);
        Assert.Equal(
            "hcde_k8vavoom_lighting_profile=1 applied: shadowmap=on prioritize=on quality=1024 maxlights=3 filter=1 autobudget=on autofallback=on bloom=on tonemap=2 ssao=1 boost=on raylight=on vk_raytrace=on\n",
            boost.Log[1]);

        var high = new VmK8Lighting1279
        {
            Quality = -4,
            Filter = -2,
            RaylightProbe = true,
            Screen = new VmK8Screen1279 { RayQueries = false },
        };
        high.SetProfile(5);
        Assert.Equal(1, high.Profile);
        Assert.True(high.AutoProfile);
        Assert.Equal(512, high.Quality);
        Assert.Equal(256, high.MaxLights);
        Assert.Equal(-2, high.Filter);
        Assert.Equal(1, high.Tonemap);
        Assert.Equal(1, high.Ssao);
        Assert.False(high.VkRaytrace);
        Assert.True(high.Snap.Raylight);
        Assert.False(high.Snap.ShadowBoost);
        Assert.Equal(2, high.Log.Count);
        Assert.Equal(high.Log[0], high.Log[1]);
        Assert.StartsWith("hcde_k8vavoom_lighting_profile=1 applied:", high.Log[0]);

        var floor = new VmK8Lighting1279 { Quality = 512 };
        floor.SetProfile(1);
        Assert.Equal(512, floor.Quality);
        floor.Quality = 511;
        floor.Log.Clear();
        floor.SetProfile(1);
        Assert.Equal(512, floor.Quality);

        var dropped = new VmK8Lighting1279 { AutoProfile = true, Quality = 2000, MaxLights = -1, ShadowBoost = true };
        dropped.SetProfile(1);
        dropped.Log.Clear();
        dropped.SetProfile(-3);
        Assert.Equal(0, dropped.Profile);
        Assert.True(dropped.AutoProfile);
        Assert.Equal(2000, dropped.Quality);
        Assert.Equal(-1, dropped.MaxLights);
        Assert.True(dropped.LightShadowmap);
        Assert.False(dropped.Snap.Applied);
        Assert.Equal(0, dropped.Snap.Quality);
        Assert.Empty(dropped.Log);

        var off = new VmK8Lighting1279();
        off.SetProfile(1);
        off.Log.Clear();
        off.SetProfile(0);
        Assert.Equal(0, off.Profile);
        Assert.False(off.AutoProfile);
        Assert.Equal(512, off.Quality);
        Assert.Equal(256, off.MaxLights);
        Assert.False(off.Snap.Applied);
        Assert.Empty(off.Log);

        var stat = new VmK8Lighting1279 { HardwareRenderer = true, Quality = 100, ShadowBoost = true };
        stat.Screen = new VmK8Screen1279
        {
            Backend = VmK8Lighting1279.BackendVulkan,
            HardwareShadowmaps = true,
            RayQueries = true,
            RaytracingActive = true,
        };
        stat.SetProfile(1);
        Assert.Equal(
            "k8vavoom lighting profile: ENABLED\n" +
            "  auto_profile              = on\n" +
            "  composed snapshot (last apply): applied=yes\n" +
            "    gl_light_shadowmap        = on (live=on)\n" +
            "    gl_shadowmap_prioritize   = on (live=on)\n" +
            "    gl_shadowmap_quality      = 1024 (live=1024)\n" +
            "    gl_shadowmap_maxlights    = 512 (live=512)\n" +
            "    gl_shadowmap_filter       = 1 (live=1)\n" +
            "    hcde_shadow_autobudget    = on (live=on)\n" +
            "    hcde_shadow_autofallback  = on (live=on)\n" +
            "    gl_bloom                  = on (live=on)\n" +
            "    gl_tonemap                = 1 (live=1)\n" +
            "    gl_ssao                   = 1 (live=1)\n" +
            "    vk_raytrace               = on (live=on)\n" +
            "  sub-flags: shadow_boost=on raylight_probe=off\n" +
            "  backend capabilities:\n" +
            "    backend                   = vulkan\n" +
            "    hardware-renderer         = yes\n" +
            "    supports-shadowmaps       = yes\n" +
            "    supports-ray-queries      = yes\n" +
            "    raylight-path-active      = yes\n" +
            "    raylight-probe-meaningful = yes\n" +
            "    notes                     = Vulkan ray-query path active for dynamic light shadows\n",
            stat.Status());
        stat.Reset();
        Assert.Equal(0, stat.Profile);
        Assert.False(stat.AutoProfile);
        Assert.False(stat.ShadowBoost);
        Assert.False(stat.RaylightProbe);
        Assert.False(stat.LightShadowmap);
        Assert.True(stat.Prioritize);
        Assert.Equal(512, stat.Quality);
        Assert.Equal(0, stat.MaxLights);
        Assert.Equal(0, stat.Filter);
        Assert.False(stat.AutoBudget);
        Assert.True(stat.AutoFallback);
        Assert.False(stat.Bloom);
        Assert.Equal(0, stat.Tonemap);
        Assert.Equal(0, stat.Ssao);
        Assert.False(stat.VkRaytrace);
        Assert.False(stat.Snap.Applied);
        Assert.False(stat.Snap.Prioritize);
        Assert.Equal("k8vavoom lighting preset reset to HCDE defaults.\n", stat.Log[^1]);
        Assert.Equal("off (live=on)", Slice(stat.Status(), "gl_shadowmap_prioritize   = ", "\n"));
    }

    [Fact]
    public void Prepare_UsesThePassedFramebufferAndKeepsTheNoteOrder()
    {
        var soft = new VmK8Lighting1279 { HardwareRenderer = false, AutoProfile = true };
        soft.Prepare(new VmK8Screen1279 { HardwareShadowmaps = true, RayQueries = true });
        Assert.Equal(0, soft.Profile);
        Assert.Empty(soft.Log);
        soft.HardwareRenderer = true;
        soft.Prepare(null);
        Assert.Equal(0, soft.Profile);

        var apart = new VmK8Lighting1279 { HardwareRenderer = true, AutoProfile = true, ShadowBoost = true };
        var passed = new VmK8Screen1279
        {
            Backend = VmK8Lighting1279.BackendVulkan,
            HardwareShadowmaps = true,
            RayQueries = true,
        };
        apart.Prepare(passed);
        Assert.Equal(1, apart.Profile);
        Assert.True(apart.ShadowBoost);
        Assert.True(apart.RaylightProbe);
        Assert.Equal(1024, apart.Quality);
        Assert.Equal(512, apart.MaxLights);
        Assert.True(apart.VkRaytrace);
        Assert.False(apart.Snap.VkRaytrace);
        Assert.Equal(
            "hcde_k8vavoom_lighting_profile=1 applied: shadowmap=on prioritize=on quality=1024 maxlights=512 filter=1 autobudget=on autofallback=on bloom=on tonemap=1 ssao=1 boost=on raylight=on vk_raytrace=off\n",
            apart.Log[0]);
        Assert.Equal("Auto-enabled hcde_k8vavoom_lighting_profile on capable hardware.\n", apart.Log[1]);
        Assert.Equal("Enabled vk_raytrace for k8vavoom ray-style dynamic light shadows.\n", apart.Log[2]);

        var same = new VmK8Lighting1279 { HardwareRenderer = true };
        var both = new VmK8Screen1279
        {
            Backend = VmK8Lighting1279.BackendOpenGl,
            HardwareShadowmaps = true,
            RayQueries = true,
        };
        same.Screen = both;
        same.Prepare(both);
        Assert.Equal(3, same.Log.Count);
        Assert.Equal("Enabled vk_raytrace for k8vavoom ray-style dynamic light shadows.\n", same.Log[0]);
        Assert.Contains("vk_raytrace=on", same.Log[1]);
        Assert.Equal("Auto-enabled hcde_k8vavoom_lighting_profile on capable hardware.\n", same.Log[2]);

        var noray = new VmK8Lighting1279 { HardwareRenderer = true, RaylightProbe = true, ShadowBoost = true };
        noray.Prepare(new VmK8Screen1279
        {
            HardwareShadowmaps = true,
            RayQueries = false,
            Backend = VmK8Lighting1279.BackendVulkan,
        });
        Assert.False(noray.ShadowBoost);
        Assert.True(noray.RaylightProbe);
        Assert.Equal(512, noray.Quality);
        Assert.Equal(256, noray.MaxLights);
        Assert.False(noray.VkRaytrace);
        Assert.Contains("boost=off raylight=on vk_raytrace=off", noray.Log[0]);
        Assert.Equal("Auto-enabled hcde_k8vavoom_lighting_profile on capable hardware.\n", noray.Log[1]);
        Assert.Equal(2, noray.Log.Count);

        var blocked = new VmK8Lighting1279 { HardwareRenderer = true, AutoProfile = true };
        blocked.Prepare(new VmK8Screen1279 { HardwareShadowmaps = false, RayQueries = true });
        Assert.Equal(0, blocked.Profile);
        Assert.False(blocked.ShadowBoost);
        Assert.Empty(blocked.Log);

        var live = new VmK8Lighting1279
        {
            HardwareRenderer = true,
            AutoProfile = true,
            Profile = 1,
            ShadowBoost = true,
            Screen = new VmK8Screen1279 { RayQueries = false },
        };
        live.Prepare(new VmK8Screen1279 { RayQueries = true, HardwareShadowmaps = true });
        Assert.Equal(1, live.Profile);
        Assert.Equal(0, live.Quality);
        Assert.False(live.Snap.Applied);
        Assert.True(live.VkRaytrace);
        Assert.Equal("Enabled vk_raytrace for k8vavoom ray-style dynamic light shadows.\n", Assert.Single(live.Log));

        var notes = new VmK8Lighting1279
        {
            HardwareRenderer = false,
            ShadowBoost = true,
            Screen = new VmK8Screen1279 { RayQueries = true, HardwareShadowmaps = true, RaytracingActive = true },
        };
        VmK8Caps1279 softCaps = notes.Probe();
        Assert.Equal("software backend; shadowmap and raylight features inactive", softCaps.Notes);
        Assert.True(softCaps.Meaningful);
        Assert.False(softCaps.SupportsShadowmaps);
        Assert.True(softCaps.SupportsRayQueries);

        notes.HardwareRenderer = true;
        notes.Screen.HardwareShadowmaps = false;
        Assert.Equal("hardware backend active but SSBO shadowmap path unavailable", notes.Probe().Notes);

        notes.Screen.HardwareShadowmaps = true;
        notes.Screen.Backend = VmK8Lighting1279.BackendVulkan;
        notes.Screen.RaytracingActive = true;
        Assert.Equal("Vulkan ray-query path active for dynamic light shadows", notes.Probe().Notes);
        notes.Screen.RaytracingActive = false;
        Assert.Equal(
            "Vulkan ray-query extension available; enable hcde_k8vavoom_raylight_probe or shadow_boost",
            notes.Probe().Notes);
        notes.ShadowBoost = false;
        notes.RaylightProbe = false;
        notes.Screen.RayQueries = false;
        Assert.Equal("Vulkan backend without VK_KHR_ray_query; using shadowmap path", notes.Probe().Notes);
        notes.Screen.Backend = VmK8Lighting1279.BackendOpenGl;
        Assert.Equal("OpenGL backend; shadowmap path (ray queries require Vulkan)", notes.Probe().Notes);
        notes.Screen.Backend = 4;
        Assert.Equal("unknown", notes.Probe().BackendName);
        Assert.Equal("OpenGL backend; shadowmap path (ray queries require Vulkan)", notes.Probe().Notes);

        notes.Screen = null;
        notes.HardwareRenderer = true;
        Assert.Equal("hardware (screen pending)", notes.Probe().BackendName);
        Assert.Equal("screen not initialized; capability probe deferred", notes.Probe().Notes);
        notes.HardwareRenderer = false;
        Assert.Equal("software (screen pending)", notes.Probe().BackendName);
    }

    static string Slice(string text, string start, string end)
    {
        int at = text.IndexOf(start, StringComparison.Ordinal);
        int from = at + start.Length;
        int to = text.IndexOf(end, from, StringComparison.Ordinal);
        return text.Substring(from, to - from);
    }
}
