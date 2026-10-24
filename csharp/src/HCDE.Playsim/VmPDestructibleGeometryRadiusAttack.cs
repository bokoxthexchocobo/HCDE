namespace HCDE.Playsim;

/// <summary><c>P_GeometryRadiusAttack</c> from <c>p_destructible.cpp</c> (442–688).</summary>
public static class VmPDestructibleGeometryRadiusAttack
{
    public sealed class GeometryRadiusAttackContext
    {
        public object? BombSpot { get; set; }
        public object? BombSource { get; set; }
        public int BombDamage { get; set; }
        public double BombDistance { get; set; }
        public double FullDamageDistance { get; set; }
        public double BombX { get; set; }
        public double BombY { get; set; }
        public double BombZ { get; set; }
        public SectorGeometry? SourceSector { get; set; }
        public IReadOnlyList<FFloorGeometry> FFloors { get; set; } = Array.Empty<FFloorGeometry>();
        public IReadOnlyList<LineGeometry> NearbyLines { get; set; } = Array.Empty<LineGeometry>();
        public Dictionary<int, DamageGroupEntry> DamageGroups { get; } = new();

        public Func<LineGeometry, int, bool>? LineVulnerable { get; init; }
        public Action<DamageGroupEntry, int, double>? ApplyDamage { get; init; }
    }

    public readonly record struct SectorGeometry(
        int SectorNum,
        int HealthCeiling,
        int HealthFloor,
        int HealthCeilingGroup,
        int HealthFloorGroup,
        double CeilingNormalX,
        double CeilingNormalY,
        double CeilingNormalZ,
        double CeilingD,
        double FloorNormalX,
        double FloorNormalY,
        double FloorNormalZ,
        double FloorD);

    public readonly record struct FFloorGeometry(
        bool Exists,
        bool Solid,
        int Health3d,
        int Health3dGroup,
        int ModelSectorNum,
        double TopZ,
        double BottomZ,
        double TopNormalX,
        double TopNormalY,
        double TopNormalZ,
        double TopD,
        double BottomNormalX,
        double BottomNormalY,
        double BottomNormalZ,
        double BottomD);

    public readonly record struct LineGeometry(
        int Index,
        int Health,
        bool BlockEverything,
        int Side,
        bool HasOtherSector,
        int OtherHealthFloor,
        int OtherHealthCeiling,
        int OtherFFloorCount,
        double V1X,
        double V1Y,
        double V2X,
        double V2Y,
        double CeilingZ1,
        double CeilingZ2,
        double FloorZ1,
        double FloorZ2);

    public readonly record struct DamageGroupEntry(
        int Group,
        double SpotX,
        double SpotY,
        double SpotZ,
        int Part);

    public static void GeometryRadiusAttack(GeometryRadiusAttackContext ctx)
    {
        if (ctx.BombSpot is null)
            return;
        if (ctx.BombSource is null)
            ctx.BombSource = ctx.BombSpot;

        var invRange = 1.0 / (ctx.BombDistance - ctx.FullDamageDistance);
        var sector = ctx.SourceSector;
        if (sector is null)
            return;

        if (sector.Value.HealthCeiling > 0)
        {
            var dst = sector.Value.CeilingNormalX * ctx.BombX + sector.Value.CeilingNormalY * ctx.BombY
                + sector.Value.CeilingNormalZ * (ctx.BombZ + sector.Value.CeilingD);
            var grp = sector.Value.HealthCeilingGroup;
            if (grp <= 0)
                grp = unchecked((int)(0x80000000 | (sector.Value.SectorNum & 0x0FFFFFFF)));
            InsertIfCloser(ctx, grp, ctx.BombX, ctx.BombY, ctx.BombZ, 1);
        }

        if (sector.Value.HealthFloor > 0)
        {
            var grp = sector.Value.HealthFloorGroup;
            if (grp <= 0)
                grp = unchecked((int)(0x40000000 | (sector.Value.SectorNum & 0x0FFFFFFF)));
            InsertIfCloser(ctx, grp, ctx.BombX, ctx.BombY, ctx.BombZ, 0);
        }

        for (var i = 0; i < ctx.FFloors.Count; i++)
        {
            var f = ctx.FFloors[i];
            if (!f.Exists || !f.Solid || f.Health3d == 0)
                continue;
            if (f.TopZ < f.BottomZ)
                continue;

            var grp = f.Health3dGroup;
            if (grp <= 0)
                grp = unchecked((int)(0x20000000 | (f.ModelSectorNum & 0x0FFFFFFF)));

            double sx, sy, sz;
            if (ctx.BombZ < f.BottomZ)
            {
                sx = ctx.BombX;
                sy = ctx.BombY;
                sz = ctx.BombZ;
            }
            else if (ctx.BombZ > f.TopZ)
            {
                sx = ctx.BombX;
                sy = ctx.BombY;
                sz = ctx.BombZ;
            }
            else
            {
                sx = ctx.BombX;
                sy = ctx.BombY;
                sz = ctx.BombZ;
            }

            InsertIfCloser(ctx, grp, sx, sy, sz, 2);
        }

        for (var i = 0; i < ctx.NearbyLines.Count; i++)
        {
            var ln = ctx.NearbyLines[i];
            if (ln.Health == 0 && (!ln.HasOtherSector
                || (ln.OtherHealthFloor == 0 && ln.OtherHealthCeiling == 0 && ln.OtherFFloorCount == 0)))
                continue;

            var to2dX = ClosestPointOnLine(ctx.BombX, ctx.BombY, ln.V1X, ln.V1Y, ln.V2X, ln.V2Y).X;
            var to2dY = ClosestPointOnLine(ctx.BombX, ctx.BombY, ln.V1X, ln.V1Y, ln.V2X, ln.V2Y).Y;
            var dist2d = Math.Sqrt((to2dX - ctx.BombX) * (to2dX - ctx.BombX) + (to2dY - ctx.BombY) * (to2dY - ctx.BombY));
            if (dist2d > ctx.BombDistance)
                continue;

            if (ln.Health > 0 && ctx.LineVulnerable?.Invoke(ln, ln.Side) == true)
            {
                var damage = (int)(ctx.BombDamage * (1.0 - dist2d * invRange));
                if (damage > 0)
                    ctx.ApplyDamage?.Invoke(new DamageGroupEntry(ln.Index, to2dX, to2dY, ctx.BombZ, 3), damage, dist2d);
            }
        }
    }

    private static void InsertIfCloser(GeometryRadiusAttackContext ctx, int grp, double x, double y, double z, int part)
    {
        var entry = new DamageGroupEntry(grp, x, y, z, part);
        if (!ctx.DamageGroups.TryGetValue(grp, out var existing))
            ctx.DamageGroups[grp] = entry;
    }

    private static (double X, double Y) ClosestPointOnLine(double px, double py, double x1, double y1, double x2, double y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        var lenSq = dx * dx + dy * dy;
        if (lenSq == 0)
            return (x1, y1);
        var t = Math.Clamp(((px - x1) * dx + (py - y1) * dy) / lenSq, 0, 1);
        return (x1 + t * dx, y1 + t * dy);
    }
}
