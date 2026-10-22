namespace HCDE.Playsim;

/// <summary>Health-group serialize and ZScript entry planning from <c>p_destructible.cpp</c> (825–1067).</summary>
public static class VmPDestructibleSerializeScript
{
    public sealed class HealthGroupSnapshot
    {
        public int Id { get; set; }
        public int Health { get; set; }
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
}
