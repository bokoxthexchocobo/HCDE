namespace HCDE.Playsim;

/// <summary><c>P_ProjectileHitLinedef</c> / <c>P_ProjectileHitPlane</c> from <c>p_destructible.cpp</c> (697–784).</summary>
public static class VmPDestructibleProjectileHits
{
    public sealed class ProjectileHitLinedefContext
    {
        public bool Blocking3DFloor { get; set; }
        public int Blocking3DFloorHealth3D { get; set; }
        public int LineHealth { get; set; }
        public int WhichSide { get; set; }
        public bool HasOtherSide { get; set; }
        public int OtherFloorHealth { get; set; }
        public int OtherCeilingHealth { get; set; }
        public bool HitsLowerSector { get; set; }
        public bool HitsUpperSector { get; set; }
        public bool LineVulnerable { get; set; }
        public bool LowerVulnerable { get; set; }
        public bool UpperVulnerable { get; set; }
        public int MissileDamage { get; set; }
        public Action<int, int>? DamageSector3D { get; init; }
        public Action<int>? DamageSectorFloor { get; init; }
        public Action<int>? DamageSectorCeiling { get; init; }
        public Action<int>? DamageLine { get; init; }
    }

    public static bool ProjectileHitLinedef(ProjectileHitLinedefContext ctx)
    {
        var washit = false;
        if (ctx.Blocking3DFloor && ctx.Blocking3DFloorHealth3D > 0)
        {
            ctx.DamageSector3D?.Invoke(ctx.MissileDamage, VmPDestructibleHealthGroups.SecPart3D);
            washit = true;
        }
        if (ctx.HasOtherSide)
        {
            if (ctx.HitsLowerSector && ctx.OtherFloorHealth > 0 && ctx.LowerVulnerable)
            {
                ctx.DamageSectorFloor?.Invoke(ctx.MissileDamage);
                washit = true;
            }
            if (ctx.HitsUpperSector && ctx.OtherCeilingHealth > 0 && ctx.UpperVulnerable)
            {
                ctx.DamageSectorCeiling?.Invoke(ctx.MissileDamage);
                washit = true;
            }
        }
        if (ctx.LineHealth > 0 && ctx.LineVulnerable)
        {
            ctx.DamageLine?.Invoke(ctx.MissileDamage);
            washit = true;
        }
        return washit;
    }

    public sealed class ProjectileHitPlaneContext
    {
        public int Part { get; set; } = -1;
        public bool BlockingCeiling { get; set; }
        public bool BlockingFloor { get; set; }
        public bool Blocking3DFloor { get; set; }
        public int Blocking3DFloorHealth3D { get; set; }
        public int SectorFloorHealth { get; set; }
        public int SectorCeilingHealth { get; set; }
        public bool FloorVulnerable { get; set; }
        public bool CeilingVulnerable { get; set; }
        public int MissileDamage { get; set; }
        public Action<int>? DamageSector3D { get; init; }
        public Action<int>? DamageSectorFloor { get; init; }
        public Action<int>? DamageSectorCeiling { get; init; }
    }

    public static bool ProjectileHitPlane(ProjectileHitPlaneContext ctx)
    {
        var part = ctx.Part;
        if (part < 0)
        {
            if (ctx.BlockingCeiling)
                part = VmPDestructibleHealthGroups.SecPartCeiling;
            else if (ctx.BlockingFloor)
                part = VmPDestructibleHealthGroups.SecPartFloor;
        }
        if (ctx.Blocking3DFloor)
        {
            if (ctx.Blocking3DFloorHealth3D > 0)
            {
                ctx.DamageSector3D?.Invoke(ctx.MissileDamage);
                return true;
            }
            return false;
        }
        if (part == VmPDestructibleHealthGroups.SecPartFloor && ctx.SectorFloorHealth > 0 && ctx.FloorVulnerable)
        {
            ctx.DamageSectorFloor?.Invoke(ctx.MissileDamage);
            return true;
        }
        if (part == VmPDestructibleHealthGroups.SecPartCeiling && ctx.SectorCeilingHealth > 0 && ctx.CeilingVulnerable)
        {
            ctx.DamageSectorCeiling?.Invoke(ctx.MissileDamage);
            return true;
        }
        return false;
    }
}
