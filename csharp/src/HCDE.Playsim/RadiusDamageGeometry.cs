namespace HCDE.Playsim;

/// <summary>Represented flat-world non-circular GetRadiusDamage surface distance.</summary>
internal static class RadiusDamageGeometry
{
    internal static double Distance(Actor origin, Actor target)
    {
        var offset = origin.Vec2To(target);
        var horizontal = Math.Max(0, Math.Max(Math.Abs(offset.X), Math.Abs(offset.Y)) - target.Radius.ToDouble());
        var vertical = Math.Max(0, Math.Max(target.Z.ToDouble() - origin.Z.ToDouble(), origin.Z.ToDouble() - target.Top()));
        return Math.Sqrt(horizontal * horizontal + vertical * vertical);
    }
}
