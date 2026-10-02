namespace HCDE.Playsim;

public enum SectorTextureScrollPlane
{
    Floor,
    Ceiling,
}

/// <summary>Native <c>DScroller::sc_floor</c> / <c>sc_ceiling</c> rate for one sector.</summary>
internal readonly record struct SectorTextureScroll(
    int SectorIndex,
    double Dx,
    double Dy,
    SectorTextureScrollPlane Plane);
