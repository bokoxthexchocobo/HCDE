namespace HCDE.Playsim;

/// <summary>Health-group lookup, serialize, and ZScript entry planning from <c>p_destructible.cpp</c>.</summary>
public static class VmPDestructibleSerializeScript
{
    public sealed class HealthGroupSnapshot
    {
        public int Id { get; set; }
        public int Health { get; set; }
    }

    public static VmPDestructibleHealthGroups.HealthGroup? GetHealthGroup(
        Dictionary<int, VmPDestructibleHealthGroups.HealthGroup> groups,
        int id) =>
        groups.GetValueOrDefault(id);

    public static VmPDestructibleHealthGroups.HealthGroup GetHealthGroupOrNew(
        Dictionary<int, VmPDestructibleHealthGroups.HealthGroup> groups,
        int id,
        int health)
    {
        if (!groups.TryGetValue(id, out var grp))
        {
            grp = new VmPDestructibleHealthGroups.HealthGroup { Id = id, Health = health };
            groups[id] = grp;
        }
        return grp;
    }

    public static void ApplyReadGroup(Dictionary<int, VmPDestructibleHealthGroups.HealthGroup> groups, HealthGroupSnapshot snap)
    {
        if (!groups.TryGetValue(snap.Id, out var grp))
            return;
        grp.Health = snap.Health;
    }

    public static IEnumerable<HealthGroupSnapshot> EnumerateWriteGroups(IEnumerable<VmPDestructibleHealthGroups.HealthGroup> groups) =>
        groups.Select(g => new HealthGroupSnapshot { Id = g.Id, Health = g.Health });

    public static int ResolveLineHealth(int lineHealth, int groupId, Func<int, int?>? groupHealth) =>
        groupId != 0 && groupHealth?.Invoke(groupId) is int gh ? gh : lineHealth;

    public static int ResolveSectorHealth(int partHealth, int groupId, Func<int, int?>? groupHealth) =>
        groupId != 0 && groupHealth?.Invoke(groupId) is int gh ? gh : partHealth;

    public static int ClampHealth(int health) => health < 0 ? 0 : health;

    public sealed class DamageSectorScriptContext
    {
        public int Damage { get; set; }
        public int Part { get; set; }
        public Action<int, int, int>? DamageSector { get; init; }
    }

    public static void PlanDamageSectorScript(DamageSectorScriptContext ctx) =>
        ctx.DamageSector?.Invoke(ctx.Damage, ctx.Part, ctx.Damage);

    public sealed class DamageLinedefScriptContext
    {
        public int Damage { get; set; }
        public int Side { get; set; }
        public Action<int, int>? DamageLine { get; init; }
    }

    public static void PlanDamageLinedefScript(DamageLinedefScriptContext ctx) =>
        ctx.DamageLine?.Invoke(ctx.Damage, ctx.Side);

    public static bool PlanCheckLinedefVulnerable(int special, bool hasSide, int part) =>
        VmPDestructibleVulnerability.CheckLinedefVulnerable(special, hasSide, part);

    public static bool PlanCheckSectorVulnerable(int part, int texture, int sky, bool isSky) =>
        VmPDestructibleVulnerability.CheckSectorVulnerable(part, texture, sky, isSky);

    public sealed class SerializeHealthGroupsContext
    {
        public bool Reading { get; set; }
        public IReadOnlyList<HealthGroupSnapshot> ReadBuffer { get; set; } = Array.Empty<HealthGroupSnapshot>();
        public Dictionary<int, VmPDestructibleHealthGroups.HealthGroup> Groups { get; } = new();
    }

    public static void SerializeHealthGroups(SerializeHealthGroupsContext ctx)
    {
        if (ctx.Reading)
        {
            foreach (var snap in ctx.ReadBuffer)
                ApplyReadGroup(ctx.Groups, snap);
        }
    }

    public static void PlanGeometryLineAttack(Action? invoke) => invoke?.Invoke();

    public static void PlanGeometryRadiusAttack(Action? invoke) => invoke?.Invoke();

    public static bool PlanProjectileHitLinedef(Func<bool>? invoke) => invoke?.Invoke() ?? false;

    public static bool PlanProjectileHitPlane(Func<bool>? invoke) => invoke?.Invoke() ?? false;

    public static int PlanFindHealthGroup(Func<int?>? lookup) => lookup?.Invoke() ?? 0;

    public static void PlanSetHealthGroupHealth(VmPDestructibleHealthGroups.HealthGroup? grp, int health) =>
        VmPDestructibleHealthGroups.SetHealthGroupHealth(grp, health, Array.Empty<VmPDestructibleHealthGroups.LineHealth>(), Array.Empty<VmPDestructibleHealthGroups.SectorHealth>());
}
