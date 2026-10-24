# r_view_pain_smooth — chunk 74 audit

**Native:** `r_view_pain_smooth.cpp` strength clamp (35–39) and `HCDEViewPainSmoothScale` (65–73).

**Managed:** `VmViewPainSmooth.cs`, `VmViewPainSmoothTests`.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
