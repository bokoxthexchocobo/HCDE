namespace HCDE.Playsim;

/// <summary>
/// k8vavoom lighting profile, capability probe, and preset reset from
/// <c>hw_k8vavoom_lighting.cpp</c> (1-277). Chunk 1279. Assigning the profile
/// reenters the callback. A zero profile clears the snapshot and leaves the
/// live cvars in place.
/// </summary>
public sealed class VmK8Screen1279
{
    public int Backend;
    public bool HardwareShadowmaps;
    public bool RayQueries;
    public bool RaytracingActive;
}

public sealed class VmK8Snap1279
{
    public bool Applied;
    public bool Shadowmap;
    public bool Prioritize;
    public int Quality;
    public int MaxLights;
    public int Filter;
    public bool AutoBudget;
    public bool AutoFallback;
    public bool Bloom;
    public int Tonemap;
    public int Ssao;
    public bool ShadowBoost;
    public bool Raylight;
    public bool VkRaytrace;
}

public sealed class VmK8Caps1279
{
    public string BackendName = "unknown";
    public bool Hardware;
    public bool SupportsShadowmaps;
    public bool SupportsRayQueries;
    public bool RayActive;
    public bool Meaningful;
    public string Notes = "";
}

public sealed class VmK8Lighting1279
{
    public const int BackendOpenGl = 0;
    public const int BackendVulkan = 1;

    public bool Callbacks = true;
    public bool AutoProfile = true;
    public bool ShadowBoost;
    public bool RaylightProbe;
    public int Profile;
    public bool LightShadowmap;
    public bool Prioritize;
    public int Quality;
    public int MaxLights;
    public int Filter;
    public bool AutoBudget;
    public bool AutoFallback;
    public bool Bloom;
    public int Tonemap;
    public int Ssao;
    public bool VkRaytrace;
    public bool HardwareRenderer;
    public VmK8Screen1279? Screen;
    public VmK8Snap1279 Snap = new();
    public List<string> Log = new();

    public void SetProfile(int value)
    {
        int prev = Profile;
        Profile = value;
        if (Callbacks)
            OnProfile(prev);
    }

    public void OnProfile(int prev)
    {
        if (Profile < 0)
            SetProfile(0);
        else if (Profile > 1)
            SetProfile(1);
        else if (Profile == 0 && prev > 0)
            AutoProfile = false;
        Apply(Profile, ShadowBoost, RaylightProbe);
    }

    public void Apply(int profile, bool shadowBoost, bool raylightProbe)
    {
        Snap = new VmK8Snap1279();
        Snap.Applied = profile > 0;
        if (profile <= 0)
            return;
        LightShadowmap = true;
        Prioritize = true;
        int qualityFloor = shadowBoost ? 1024 : 512;
        if (Quality < qualityFloor)
            Quality = qualityFloor;
        Bloom = true;
        if (Tonemap == 0)
            Tonemap = 1;
        if (Ssao == 0)
            Ssao = 1;
        AutoFallback = true;
        AutoBudget = true;
        if (MaxLights == 0)
            MaxLights = shadowBoost ? 512 : 256;
        if (Filter == 0)
            Filter = 1;
        if (raylightProbe || shadowBoost)
            TryEnable(Screen);
        Snap.Shadowmap = LightShadowmap;
        Snap.Prioritize = Prioritize;
        Snap.Quality = Quality;
        Snap.MaxLights = MaxLights;
        Snap.Filter = Filter;
        Snap.AutoBudget = AutoBudget;
        Snap.AutoFallback = AutoFallback;
        Snap.Bloom = Bloom;
        Snap.Tonemap = Tonemap;
        Snap.Ssao = Ssao;
        Snap.ShadowBoost = shadowBoost;
        Snap.Raylight = raylightProbe || shadowBoost;
        Snap.VkRaytrace = VkRaytrace;
        Log.Add(
            "hcde_k8vavoom_lighting_profile=1 applied: shadowmap=" + On(Snap.Shadowmap) +
            " prioritize=" + On(Snap.Prioritize) +
            " quality=" + Snap.Quality +
            " maxlights=" + Snap.MaxLights +
            " filter=" + Snap.Filter +
            " autobudget=" + On(Snap.AutoBudget) +
            " autofallback=" + On(Snap.AutoFallback) +
            " bloom=" + On(Snap.Bloom) +
            " tonemap=" + Snap.Tonemap +
            " ssao=" + Snap.Ssao +
            " boost=" + On(Snap.ShadowBoost) +
            " raylight=" + On(Snap.Raylight) +
            " vk_raytrace=" + On(Snap.VkRaytrace) + "\n");
    }

    public void TryEnable(VmK8Screen1279? framebuffer)
    {
        if (framebuffer == null || !framebuffer.RayQueries)
            return;
        if (!VkRaytrace)
        {
            VkRaytrace = true;
            Log.Add("Enabled vk_raytrace for k8vavoom ray-style dynamic light shadows.\n");
        }
    }

    public bool RaylightRequested()
    {
        return RaylightProbe || ShadowBoost;
    }

    public VmK8Caps1279 Probe()
    {
        var caps = new VmK8Caps1279();
        caps.Hardware = HardwareRenderer;
        if (Screen != null)
        {
            caps.BackendName = BackendName(Screen);
            caps.SupportsShadowmaps = caps.Hardware && Screen.HardwareShadowmaps;
            caps.SupportsRayQueries = Screen.RayQueries;
            caps.RayActive = Screen.RaytracingActive;
            caps.Meaningful = caps.SupportsRayQueries && RaylightRequested();
            if (!caps.Hardware)
                caps.Notes = "software backend; shadowmap and raylight features inactive";
            else if (!caps.SupportsShadowmaps)
                caps.Notes = "hardware backend active but SSBO shadowmap path unavailable";
            else if (caps.RayActive)
                caps.Notes = "Vulkan ray-query path active for dynamic light shadows";
            else if (caps.SupportsRayQueries)
                caps.Notes = "Vulkan ray-query extension available; enable hcde_k8vavoom_raylight_probe or shadow_boost";
            else if (Screen.Backend == BackendVulkan)
                caps.Notes = "Vulkan backend without VK_KHR_ray_query; using shadowmap path";
            else
                caps.Notes = "OpenGL backend; shadowmap path (ray queries require Vulkan)";
            return caps;
        }
        caps.BackendName = caps.Hardware ? "hardware (screen pending)" : "software (screen pending)";
        caps.Notes = "screen not initialized; capability probe deferred";
        return caps;
    }

    public void Prepare(VmK8Screen1279? framebuffer)
    {
        if (framebuffer == null || !HardwareRenderer)
            return;
        bool supportsShadowmaps = framebuffer.HardwareShadowmaps;
        if (AutoProfile && Profile == 0 && supportsShadowmaps)
        {
            ShadowBoost = framebuffer.RayQueries;
            if (framebuffer.RayQueries)
                RaylightProbe = true;
            SetProfile(1);
            Log.Add("Auto-enabled hcde_k8vavoom_lighting_profile on capable hardware.\n");
        }
        if (Profile > 0 && RaylightRequested())
            TryEnable(framebuffer);
    }

    public string Status()
    {
        VmK8Caps1279 caps = Probe();
        return
            "k8vavoom lighting profile: " + (Profile > 0 ? "ENABLED" : "disabled") + "\n" +
            "  auto_profile              = " + On(AutoProfile) + "\n" +
            "  composed snapshot (last apply): applied=" + (Snap.Applied ? "yes" : "no") + "\n" +
            "    gl_light_shadowmap        = " + On(Snap.Shadowmap) + " (live=" + On(LightShadowmap) + ")\n" +
            "    gl_shadowmap_prioritize   = " + On(Snap.Prioritize) + " (live=" + On(Prioritize) + ")\n" +
            "    gl_shadowmap_quality      = " + Snap.Quality + " (live=" + Quality + ")\n" +
            "    gl_shadowmap_maxlights    = " + Snap.MaxLights + " (live=" + MaxLights + ")\n" +
            "    gl_shadowmap_filter       = " + Snap.Filter + " (live=" + Filter + ")\n" +
            "    hcde_shadow_autobudget    = " + On(Snap.AutoBudget) + " (live=" + On(AutoBudget) + ")\n" +
            "    hcde_shadow_autofallback  = " + On(Snap.AutoFallback) + " (live=" + On(AutoFallback) + ")\n" +
            "    gl_bloom                  = " + On(Snap.Bloom) + " (live=" + On(Bloom) + ")\n" +
            "    gl_tonemap                = " + Snap.Tonemap + " (live=" + Tonemap + ")\n" +
            "    gl_ssao                   = " + Snap.Ssao + " (live=" + Ssao + ")\n" +
            "    vk_raytrace               = " + On(Snap.VkRaytrace) + " (live=" + On(VkRaytrace) + ")\n" +
            "  sub-flags: shadow_boost=" + On(ShadowBoost) + " raylight_probe=" + On(RaylightProbe) + "\n" +
            "  backend capabilities:\n" +
            "    backend                   = " + caps.BackendName + "\n" +
            "    hardware-renderer         = " + Yes(caps.Hardware) + "\n" +
            "    supports-shadowmaps       = " + Yes(caps.SupportsShadowmaps) + "\n" +
            "    supports-ray-queries      = " + Yes(caps.SupportsRayQueries) + "\n" +
            "    raylight-path-active      = " + Yes(caps.RayActive) + "\n" +
            "    raylight-probe-meaningful = " + Yes(caps.Meaningful) + "\n" +
            "    notes                     = " + caps.Notes + "\n";
    }

    public void Reset()
    {
        SetProfile(0);
        ShadowBoost = false;
        RaylightProbe = false;
        AutoProfile = false;
        LightShadowmap = false;
        Prioritize = true;
        Quality = 512;
        MaxLights = 0;
        Filter = 0;
        AutoBudget = false;
        AutoFallback = true;
        Bloom = false;
        Tonemap = 0;
        Ssao = 0;
        VkRaytrace = false;
        Snap = new VmK8Snap1279();
        Log.Add("k8vavoom lighting preset reset to HCDE defaults.\n");
    }

    public static string BackendName(VmK8Screen1279? framebuffer)
    {
        if (framebuffer == null)
            return "unknown";
        if (framebuffer.Backend == BackendVulkan)
            return "vulkan";
        if (framebuffer.Backend == BackendOpenGl)
            return "opengl";
        return "unknown";
    }

    static string On(bool value)
    {
        return value ? "on" : "off";
    }

    static string Yes(bool value)
    {
        return value ? "yes" : "no";
    }
}
