namespace HCDE.Playsim;

/// <summary>
/// <c>_Line</c> / <c>_Sector</c> GetHealth and SetHealth ZScript actions from <c>p_destructible.cpp</c> (**984–1067**).
/// Chunk **1021** hull **918–1067**; <c>FDestructible</c> DEFINE cluster (**918–982**) stays on <see cref="VmPDestructibleSerializeScript"/>.
/// </summary>
public static class VmPDestructibleLineSectorHealth1021Slice
{
    public const int SecPartFloor = 0;
    public const int SecPartCeiling = 1;
    public const int SecPart3D = 2;

    public static int ClampDestructibleHealth(int health) => health < 0 ? 0 : health;

    public static int ResolveLineGetHealth(int lineHealth, int healthGroupId, int? groupHealth) =>
        healthGroupId != 0 && groupHealth is int gh ? gh : lineHealth;

    public static int ExecuteLineGetHealth(int lineHealth, int healthGroupId, Func<int, int?>? lookupGroupHealth)
    {
        if (healthGroupId == 0)
            return lineHealth;
        var gh = lookupGroupHealth?.Invoke(healthGroupId);
        return gh is int h ? h : lineHealth;
    }

    public readonly record struct LineSetHealthMutation(int LineHealth, int HealthGroupId, int GroupHealth, bool ApplyGroup);

    public static LineSetHealthMutation BuildLineSetHealthMutation(int newHealth, int healthGroupId, Func<int, VmPDestructibleHealthGroups.HealthGroup?>? resolveGroup)
    {
        var clamped = ClampDestructibleHealth(newHealth);
        if (healthGroupId == 0)
            return new LineSetHealthMutation(clamped, 0, 0, false);

        var grp = resolveGroup?.Invoke(healthGroupId);
        if (grp is null)
            return new LineSetHealthMutation(clamped, healthGroupId, 0, false);

        return new LineSetHealthMutation(clamped, healthGroupId, clamped, true);
    }

    public static (int LineHealth, bool PropagateGroup) ApplyLineSetHealth(int currentLineHealth, int newHealth, int healthGroupId)
    {
        var clamped = ClampDestructibleHealth(newHealth);
        return (clamped, healthGroupId != 0);
    }

    public static void ApplyLineSetHealthGroup(VmPDestructibleHealthGroups.HealthGroup? grp, int clampedHealth) =>
        VmPDestructibleSerializeScript.PlanSetHealthGroupHealth(grp, clampedHealth);

    public static int ResolveSectorGetHealth(int part, int floorHealth, int ceilingHealth, int health3D,
        int floorGroupId, int ceilingGroupId, int health3DGroupId,
        int? floorGroupHealth, int? ceilingGroupHealth, int? health3DGroupHealth) =>
        part switch
        {
            SecPartFloor => floorGroupId != 0 && floorGroupHealth is int fh ? fh : floorHealth,
            SecPartCeiling => ceilingGroupId != 0 && ceilingGroupHealth is int ch ? ch : ceilingHealth,
            SecPart3D => health3DGroupId != 0 && health3DGroupHealth is int th ? th : health3D,
            _ => 0,
        };

    public static int ExecuteSectorGetHealth(int part,
        int floorHealth, int ceilingHealth, int health3D,
        int floorGroupId, int ceilingGroupId, int health3DGroupId,
        Func<int, int?>? lookupGroupHealth) =>
        part switch
        {
            SecPartFloor => ExecuteLineGetHealth(floorHealth, floorGroupId, lookupGroupHealth),
            SecPartCeiling => ExecuteLineGetHealth(ceilingHealth, ceilingGroupId, lookupGroupHealth),
            SecPart3D => ExecuteLineGetHealth(health3D, health3DGroupId, lookupGroupHealth),
            _ => 0,
        };

    public readonly record struct SectorSetHealthRouting(
        bool ValidPart,
        int GroupId,
        int ClampedHealth,
        int PartIndex);

    public static SectorSetHealthRouting RouteSectorSetHealth(int part, int newHealth,
        int floorGroupId, int ceilingGroupId, int health3DGroupId)
    {
        var clamped = ClampDestructibleHealth(newHealth);
        return part switch
        {
            SecPartFloor => new SectorSetHealthRouting(true, floorGroupId, clamped, SecPartFloor),
            SecPartCeiling => new SectorSetHealthRouting(true, ceilingGroupId, clamped, SecPartCeiling),
            SecPart3D => new SectorSetHealthRouting(true, health3DGroupId, clamped, SecPart3D),
            _ => new SectorSetHealthRouting(false, 0, 0, part),
        };
    }

    public readonly record struct SectorSetHealthMutation(
        bool Handled,
        int PartIndex,
        int StoredHealth,
        int GroupId,
        bool ApplyGroup,
        int GroupHealth);

    public static SectorSetHealthMutation BuildSectorSetHealthMutation(int part, int newHealth,
        int floorGroupId, int ceilingGroupId, int health3DGroupId,
        Func<int, VmPDestructibleHealthGroups.HealthGroup?>? resolveGroup)
    {
        var plan = RouteSectorSetHealth(part, newHealth, floorGroupId, ceilingGroupId, health3DGroupId);
        if (!plan.ValidPart)
            return new SectorSetHealthMutation(false, part, 0, 0, false, 0);

        if (plan.GroupId == 0)
            return new SectorSetHealthMutation(true, plan.PartIndex, plan.ClampedHealth, 0, false, 0);

        var grp = resolveGroup?.Invoke(plan.GroupId);
        if (grp is null)
            return new SectorSetHealthMutation(true, plan.PartIndex, plan.ClampedHealth, plan.GroupId, false, 0);

        return new SectorSetHealthMutation(true, plan.PartIndex, plan.ClampedHealth, plan.GroupId, true, plan.ClampedHealth);
    }

    public static bool ShouldPropagateGroupHealth(int groupId) => groupId != 0;
}
