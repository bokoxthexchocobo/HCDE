namespace HCDE.Client;

/// <summary>Native <c>hcde_renderer_fallback.cpp</c> renderer CVAR migration and fallbacks.</summary>
public static class VmHcdeRendererFallback
{
    public const int BackendVulkan = 1;
    public const int BackendOpenGl = 0;
    public const int BackendDefault = BackendVulkan;
    public const int NumBackends = 3;
    public const int LegacyOpenGlEsBackend = 2;

    public const int HardwareRenderMode = 4;
    public const int SoftwareRenderMode = 1;

    public static int MigratePreferBackend(int preferBackend)
    {
        if (preferBackend == LegacyOpenGlEsBackend)
            return BackendVulkan;
        if (preferBackend < 0 || preferBackend >= NumBackends)
            return BackendDefault;
        return preferBackend;
    }

    public static bool UsingSoftwareRenderer(int renderMode) => renderMode != HardwareRenderMode;

    public static int ForceDesktopOpenGlFallback(int preferBackend)
    {
        preferBackend = MigratePreferBackend(preferBackend);
        return preferBackend == BackendOpenGl ? preferBackend : BackendOpenGl;
    }

    public static int ActivateSoftwareRendererFallback(int renderMode)
    {
        if (renderMode != SoftwareRenderMode)
            return SoftwareRenderMode;
        return renderMode;
    }
}
