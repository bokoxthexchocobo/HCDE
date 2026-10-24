namespace HCDE.Playsim;

/// <summary><c>SprayDecal</c> from <c>a_decals.cpp</c> (811–849).</summary>
public static class VmADecalSpray
{
    public const int TraceHitWall = 1;
    public const int MlBlockeverything = 1 << 0;

    public static void SprayDecal(
        object? shooter,
        string name,
        double distance,
        double offsetX,
        double offsetY,
        double offsetZ,
        double dirX,
        double dirY,
        double dirZ,
        bool useBloodColor,
        uint decalColor,
        int translation,
        Func<object, double> shooterHeight,
        Func<object, double, double> posPlusZ,
        Func<object, (double x, double y, double z)> shooterPos,
        Func<object, double> shooterYawCos,
        Func<object, double> shooterYawSin,
        Func<object, double> shooterPitchCos,
        Func<object, double> shooterPitchSin,
        Func<object, int> bloodTranslation,
        Func<object, uint> bloodColor,
        Func<(double x, double y, double z), object?> shooterSector,
        Func<(double x, double y, double z), object?, (double dx, double dy, double dz), double, object?, object?, (int hitType, (double x, double y, double z) hitPos, object? line, int side, object? ffloor)> trace,
        int clMaxDecals,
        Func<string, object?> getDecalByName,
        Func<object?, object?> resolveDecal,
        Func<(double x, double y, double z), object?, object?, object?, VmADecalImpact.StaticCreateContext> buildCreateCtx)
    {
        if (shooter == null)
            return;

        double ox, oy, oz;
        double dx, dy, dz;
        if (offsetX == 0 && offsetY == 0 && offsetZ == 0)
        {
            var z = posPlusZ(shooter, shooterHeight(shooter) / 2);
            ox = shooterPos(shooter).x;
            oy = shooterPos(shooter).y;
            oz = z;
        }
        else
        {
            var p = shooterPos(shooter);
            ox = p.x + offsetX;
            oy = p.y + offsetY;
            oz = p.z + offsetZ;
        }

        if (dirX == 0 && dirY == 0 && dirZ == 0)
        {
            var c = shooterPitchCos(shooter);
            dx = c * shooterYawCos(shooter);
            dy = c * shooterYawSin(shooter);
            dz = -shooterPitchSin(shooter);
        }
        else
        {
            dx = dirX;
            dy = dirY;
            dz = dirZ;
        }

        var trans = useBloodColor ? bloodTranslation(shooter) : translation;
        var entry = useBloodColor ? bloodColor(shooter) : decalColor;
        var off = (ox, oy, oz);
        var sector = shooterSector(off);
        var dir = (dx, dy, dz);
        var hit = trace(off, sector, dir, distance, shooter, null);
        if (hit.hitType != TraceHitWall)
            return;
        var createCtx = buildCreateCtx(hit.hitPos, hit.line, hit.ffloor, shooter);
        createCtx.Color = entry;
        createCtx.Translation = trans;
        VmADecalImpact.StaticCreateByName(clMaxDecals, getDecalByName, resolveDecal, name, createCtx);
    }
}
