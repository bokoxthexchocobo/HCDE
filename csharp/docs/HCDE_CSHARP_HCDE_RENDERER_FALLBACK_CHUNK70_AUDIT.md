# HCDE renderer fallback — chunk 70 audit

**Native:** `hcde_renderer_fallback.cpp` `HCDE_MigrateRendererCvars`, `HCDE_UsingSoftwareRenderer`, `HCDE_ForceDesktopOpenGLFallback`, `HCDE_ActivateSoftwareRendererFallback` (lines 27–97).

**Managed:** `HCDE.Client/VmHcdeRendererFallback.cs`, `VmHcdeRendererFallbackTests`.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
