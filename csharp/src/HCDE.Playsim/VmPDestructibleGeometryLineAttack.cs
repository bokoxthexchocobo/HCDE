namespace HCDE.Playsim;

/// <summary><c>P_GeometryLineAttack</c> planning from <c>p_destructible.cpp</c> (303–376).</summary>
public static class VmPDestructibleGeometryLineAttack
{
    public const int TraceHitWall = 1;
    public const int TraceHitFloor = 2;
    public const int TraceHitCeiling = 3;
    public const int TierFFloor = 4;
    public const int TierLower = 5;
    public const int TierUpper = 6;

    public sealed class GeometryLineAttackContext
    {
        public int HitType { get; set; }
        public int Tier { get; set; }
        public int LineHealth { get; set; }
        public bool LineVulnerable { get; set; }
        public bool HasBackSector { get; set; }
        public int BackFloorHealth { get; set; }
        public int BackCeilingHealth { get; set; }
        public bool BackFloorVulnerable { get; set; }
        public bool BackCeilingVulnerable { get; set; }
        public bool FFloorModelHealth3D { get; set; }
        public int SectorFloorHealth { get; set; }
        public int SectorCeilingHealth { get; set; }
        public bool SectorFloorVulnerable { get; set; }
        public bool SectorCeilingVulnerable { get; set; }
        public bool Hit3DFloorsBlockSector { get; set; }
        public int Damage { get; set; }
        public Action<int, int>? DamageSector { get; init; }
        public Action<int>? DamageLine { get; init; }
    }

    public static void GeometryLineAttack(GeometryLineAttackContext ctx)
    {
        var damage = ctx.Damage;
        if (damage <= 0)
            return;

        if (ctx.HitType == TraceHitWall && ctx.Tier == TierFFloor && ctx.FFloorModelHealth3D)
            ctx.DamageSector?.Invoke(damage, VmPDestructibleHealthGroups.SecPart3D);

        if (ctx.HitType == TraceHitWall && ctx.LineVulnerable)
        {
            if (ctx.Tier == TierLower || ctx.Tier == TierUpper)
            {
                if (ctx.HasBackSector)
                {
                    if (ctx.Tier == TierLower && ctx.BackFloorHealth > 0 && ctx.BackFloorVulnerable)
                        ctx.DamageSector?.Invoke(damage, VmPDestructibleHealthGroups.SecPartFloor);
                    if (ctx.Tier == TierUpper && ctx.BackCeilingHealth > 0 && ctx.BackCeilingVulnerable)
                        ctx.DamageSector?.Invoke(damage, VmPDestructibleHealthGroups.SecPartCeiling);
                }
            }
            if (ctx.LineHealth > 0)
                ctx.DamageLine?.Invoke(damage);
        }
        else if (ctx.HitType == TraceHitFloor || ctx.HitType == TraceHitCeiling)
        {
            if (ctx.Hit3DFloorsBlockSector)
                return;
            if (ctx.HitType == TraceHitFloor && ctx.SectorFloorHealth > 0 && ctx.SectorFloorVulnerable)
                ctx.DamageSector?.Invoke(damage, VmPDestructibleHealthGroups.SecPartFloor);
            if (ctx.HitType == TraceHitCeiling && ctx.SectorCeilingHealth > 0 && ctx.SectorCeilingVulnerable)
                ctx.DamageSector?.Invoke(damage, VmPDestructibleHealthGroups.SecPartCeiling);
        }
    }
}
