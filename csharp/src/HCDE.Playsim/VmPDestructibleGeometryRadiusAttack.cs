namespace HCDE.Playsim;

/// <summary><c>P_GeometryRadiusAttack</c> from <c>p_destructible.cpp</c> (442–688).</summary>
public static class VmPDestructibleGeometryRadiusAttack
{
    public const int SecPartFloor = 0;
    public const int SecPartCeiling = 1;
    public const int SecPart3D = 2;
    public const int GroupMaskCeiling = unchecked((int)0x80000000);
    public const int GroupMaskFloor = unchecked((int)0x40000000);
    public const int GroupMask3D = unchecked((int)0x20000000);

    public sealed class GeometryRadiusAttackContext
    {
        public object? BombSpot { get; set; }
        public object? BombSource { get; set; }
        public int BombDamage { get; set; }
        public double BombDistance { get; set; }
        public double FullDamageDistance { get; set; }
        public double SplashFactor { get; set; } = 1.0;
        public double BombX { get; set; }
        public double BombY { get; set; }
        public double BombZ { get; set; }
        public SectorGeometry? SourceSector { get; set; }
        public IReadOnlyList<FFloorGeometry> FFloors { get; set; } = Array.Empty<FFloorGeometry>();
        public IReadOnlyList<LineGeometry> NearbyLines { get; set; } = Array.Empty<LineGeometry>();

        public Dictionary<int, DamageGroupEntry> DamageGroups { get; } = new();

        public Func<LineGeometry, int, bool>? LineVulnerable { get; init; }
        public Action<DamageGroupEntry, int, double>? ApplyDamage { get; init; }
        public Action<DamageGroupEntry, int>? ApplyGroupDamage { get; init; }
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

    public static double ComputeInverseDamageRange(double bombDistance, double fullDamageDistance) =>
        1.0 / (bombDistance - fullDamageDistance);

    public static int ResolveCeilingGroupId(int healthGroup, int sectorNum) =>
        healthGroup > 0 ? healthGroup : unchecked(GroupMaskCeiling | (sectorNum & 0x0FFFFFFF));

    public static int ResolveFloorGroupId(int healthGroup, int sectorNum) =>
        healthGroup > 0 ? healthGroup : unchecked(GroupMaskFloor | (sectorNum & 0x0FFFFFFF));

    public static int ResolveFfGroupId(int healthGroup, int modelSectorNum) =>
        healthGroup > 0 ? healthGroup : unchecked(GroupMask3D | (modelSectorNum & 0x0FFFFFFF));

    public static (double X, double Y, double Z) ProjectToPlane(
        double px, double py, double pz,
        double nx, double ny, double nz, double d)
    {
        var dst = nx * px + ny * py + nz * (pz + nz * d);
        return (px - nx * dst, py - ny * dst, pz - nz * dst);
    }

    public static (double X, double Y, double Z) ComputeFfSpot(
        double bombX, double bombY, double bombZ,
        FFloorGeometry f)
    {
        if (bombZ < f.BottomZ)
            return ProjectToPlane(bombX, bombY, bombZ, f.BottomNormalX, f.BottomNormalY, f.BottomNormalZ, f.BottomD);
        if (bombZ > f.TopZ)
            return ProjectToPlane(bombX, bombY, bombZ, f.TopNormalX, f.TopNormalY, f.TopNormalZ, f.TopD);
        return (bombX, bombY, bombZ);
    }

    public static bool FfloorEligible(FFloorGeometry f) =>
        f.Exists && f.Solid && f.Health3d != 0 && f.TopZ >= f.BottomZ;

    public static void InsertIfCloser(GeometryRadiusAttackContext ctx, int grp, double x, double y, double z, int part)
    {
        var entry = new DamageGroupEntry(grp, x, y, z, part);
        if (!ctx.DamageGroups.ContainsKey(grp))
            ctx.DamageGroups[grp] = entry;
    }

    public static void RunSourceSectorCeilingInsert(GeometryRadiusAttackContext ctx, SectorGeometry sector)
    {
        if (sector.HealthCeiling <= 0)
            return;
        var grp = ResolveCeilingGroupId(sector.HealthCeilingGroup, sector.SectorNum);
        var spot = ProjectToPlane(
            ctx.BombX, ctx.BombY, ctx.BombZ,
            sector.CeilingNormalX, sector.CeilingNormalY, sector.CeilingNormalZ, sector.CeilingD);
        InsertIfCloser(ctx, grp, spot.X, spot.Y, spot.Z, SecPartCeiling);
    }

    public static void RunSourceSectorFloorInsert(GeometryRadiusAttackContext ctx, SectorGeometry sector)
    {
        if (sector.HealthFloor <= 0)
            return;
        var grp = ResolveFloorGroupId(sector.HealthFloorGroup, sector.SectorNum);
        var spot = ProjectToPlane(
            ctx.BombX, ctx.BombY, ctx.BombZ,
            sector.FloorNormalX, sector.FloorNormalY, sector.FloorNormalZ, sector.FloorD);
        InsertIfCloser(ctx, grp, spot.X, spot.Y, spot.Z, SecPartFloor);
    }

    public static void RunSourceFfloorsPipeline(GeometryRadiusAttackContext ctx)
    {
        foreach (var f in ctx.FFloors)
        {
            if (!FfloorEligible(f))
                continue;
            var grp = ResolveFfGroupId(f.Health3dGroup, f.ModelSectorNum);
            var spot = ComputeFfSpot(ctx.BombX, ctx.BombY, ctx.BombZ, f);
            InsertIfCloser(ctx, grp, spot.X, spot.Y, spot.Z, SecPart3D);
        }
    }

    public static bool LineHasInteractiveGeometry(LineGeometry ln) =>
        ln.Health != 0
        || (ln.HasOtherSector
            && (ln.OtherHealthFloor != 0 || ln.OtherHealthCeiling != 0 || ln.OtherFFloorCount != 0));

    public static int ComputeScaledLineDamage(
        GeometryRadiusAttackContext ctx,
        double dist2d,
        double invRange)
    {
        var damage = (int)(ctx.BombDamage * (1.0 - dist2d * invRange));
        if (damage <= 0)
            return 0;
        if (ReferenceEquals(ctx.BombSource, ctx.BombSpot))
            damage = (int)(damage * ctx.SplashFactor);
        return damage;
    }

    public static void RunNearbyLinesPipeline(GeometryRadiusAttackContext ctx, double invRange)
    {
        foreach (var ln in ctx.NearbyLines)
        {
            if (!LineHasInteractiveGeometry(ln))
                continue;
            var closest = ClosestPointOnLine(ctx.BombX, ctx.BombY, ln.V1X, ln.V1Y, ln.V2X, ln.V2Y);
            var dist2d = Math.Sqrt((closest.X - ctx.BombX) * (closest.X - ctx.BombX) + (closest.Y - ctx.BombY) * (closest.Y - ctx.BombY));
            if (dist2d > ctx.BombDistance)
                continue;
            if (ln.Health > 0 && ctx.LineVulnerable?.Invoke(ln, ln.Side) == true)
            {
                var damage = ComputeScaledLineDamage(ctx, dist2d, invRange);
                if (damage > 0)
                    ctx.ApplyDamage?.Invoke(new DamageGroupEntry(ln.Index, closest.X, closest.Y, ctx.BombZ, 3), damage, dist2d);
            }
        }
    }

    public static int ComputeGroupFalloffDamage(
        GeometryRadiusAttackContext ctx,
        double spotX,
        double spotY,
        double spotZ,
        double invRange)
    {
        var dx = spotX - ctx.BombX;
        var dy = spotY - ctx.BombY;
        var dz = spotZ - ctx.BombZ;
        var dst = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        if (dst >= ctx.BombDistance)
            return 0;
        var clamped = Math.Clamp(dst - ctx.FullDamageDistance, 0, dst);
        var damage = (int)(ctx.BombDamage * (1.0 - clamped * invRange));
        if (damage <= 0)
            return 0;
        if (ReferenceEquals(ctx.BombSource, ctx.BombSpot))
            damage = (int)(damage * ctx.SplashFactor);
        return damage;
    }

    public static void RunDamageGroupsApplyPipeline(GeometryRadiusAttackContext ctx, double invRange)
    {
        foreach (var pair in ctx.DamageGroups)
        {
            var entry = pair.Value;
            var damage = ComputeGroupFalloffDamage(ctx, entry.SpotX, entry.SpotY, entry.SpotZ, invRange);
            if (damage > 0)
                ctx.ApplyGroupDamage?.Invoke(entry, damage);
        }
    }

    public static bool RunGeometryRadiusAttackPipeline(GeometryRadiusAttackContext ctx)
    {
        if (ctx.BombSpot is null)
            return false;
        if (ctx.BombSource is null)
            ctx.BombSource = ctx.BombSpot;
        var invRange = ComputeInverseDamageRange(ctx.BombDistance, ctx.FullDamageDistance);
        var sector = ctx.SourceSector;
        if (sector is null)
            return false;
        RunSourceSectorCeilingInsert(ctx, sector.Value);
        RunSourceSectorFloorInsert(ctx, sector.Value);
        RunSourceFfloorsPipeline(ctx);
        RunNearbyLinesPipeline(ctx, invRange);
        RunDamageGroupsApplyPipeline(ctx, invRange);
        return true;
    }

    public static void GeometryRadiusAttack(GeometryRadiusAttackContext ctx) =>
        RunGeometryRadiusAttackPipeline(ctx);

    public static (double X, double Y) ClosestPointOnLine(double px, double py, double x1, double y1, double x2, double y2)
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
