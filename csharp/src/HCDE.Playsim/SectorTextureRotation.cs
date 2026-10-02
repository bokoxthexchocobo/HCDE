using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>RotationComp</c> for floor/ceiling texture scroll.</summary>
internal static class SectorTextureRotation
{
    public static (double Dx, double Dy) Compensate(LevelSector sector, SectorTextureScrollPlane plane, double dx, double dy)
    {
        var angle = plane == SectorTextureScrollPlane.Floor
            ? unchecked(sector.FloorTextureAngle + sector.FloorTextureBaseAngle)
            : unchecked(sector.CeilingTextureAngle + sector.CeilingTextureBaseAngle);
        if (angle == 0)
            return (dx, dy);

        var radians = angle * (Math.PI * 2 / 4294967296.0);
        var cosine = -Math.Cos(radians);
        var sine = -Math.Sin(radians);
        return (dx * cosine - dy * sine, dy * cosine + dx * sine);
    }
}
