namespace HCDE.Playsim;

/// <summary>Health group setup and damage propagation from <c>p_destructible.cpp</c> (43–211).</summary>
public static class VmPDestructibleHealthGroups
{
    public const int SecPartCeiling = 1;
    public const int SecPartFloor = 2;
    public const int SecPart3D = 3;

    public sealed class HealthGroup
    {
        public int Id { get; set; }
        public int Health { get; set; }
        public List<LineHealth> Lines { get; } = new();
        public List<SectorHealth> Sectors { get; } = new();
    }

    public sealed class LineHealth
    {
        public int Index { get; set; }
        public int Health { get; set; }
        public int HealthGroup { get; set; }
    }

    public sealed class SectorHealth
    {
        public int Index { get; set; }
        public int HealthCeiling { get; set; }
        public int HealthFloor { get; set; }
        public int Health3d { get; set; }
        public int HealthCeilingGroup { get; set; }
        public int HealthFloorGroup { get; set; }
        public int Health3dGroup { get; set; }
    }

    public static void SetHealthGroupHealth(HealthGroup? grp, int health, IList<LineHealth> allLines, IList<SectorHealth> allSectors)
    {
        if (grp is null)
            return;
        grp.Health = health;
        foreach (var line in grp.Lines)
            line.Health = health;
        foreach (var sec in grp.Sectors)
        {
            if (sec.HealthCeilingGroup == grp.Id)
                sec.HealthCeiling = health;
            if (sec.HealthFloorGroup == grp.Id)
                sec.HealthFloor = health;
            if (sec.Health3dGroup == grp.Id)
                sec.Health3d = health;
        }
    }

    public sealed class DamageHealthGroupContext
    {
        public HealthGroup? Group { get; set; }
        public object? DamagedObject { get; set; }
        public int Damage { get; set; }
        public int Part { get; set; } = -1;
        public Action<LineHealth, int>? DamageLine { get; init; }
        public Action<SectorHealth, int, int>? DamageSector { get; init; }
    }

    public static void DamageHealthGroup(DamageHealthGroupContext ctx)
    {
        var grp = ctx.Group;
        if (grp is null)
            return;
        var group = grp.Id;
        foreach (var line in grp.Lines)
        {
            if (ReferenceEquals(line, ctx.DamagedObject))
                continue;
            line.Health = grp.Health + ctx.Damage;
            ctx.DamageLine?.Invoke(line, ctx.Damage);
        }
        foreach (var sec in grp.Sectors)
        {
            if (sec.HealthCeilingGroup == group && (!ReferenceEquals(sec, ctx.DamagedObject) || ctx.Part != SecPartCeiling))
            {
                sec.HealthCeiling = grp.Health + ctx.Damage;
                ctx.DamageSector?.Invoke(sec, SecPartCeiling, ctx.Damage);
            }
            if (sec.HealthFloorGroup == group && (!ReferenceEquals(sec, ctx.DamagedObject) || ctx.Part != SecPartFloor))
            {
                sec.HealthFloor = grp.Health + ctx.Damage;
                ctx.DamageSector?.Invoke(sec, SecPartFloor, ctx.Damage);
            }
            if (sec.Health3dGroup == group && (!ReferenceEquals(sec, ctx.DamagedObject) || ctx.Part != SecPart3D))
            {
                sec.Health3d = grp.Health + ctx.Damage;
                ctx.DamageSector?.Invoke(sec, SecPart3D, ctx.Damage);
            }
        }
    }

    public sealed class DamageLinedefContext
    {
        public LineHealth Line { get; set; } = null!;
        public int Damage { get; set; }
        public bool DoGroups { get; set; }
        public Func<int>? WorldLineDamaged { get; init; }
        public Action<int>? ActivateLine { get; init; }
        public Func<int, HealthGroup?>? GetHealthGroup { get; init; }
        public DamageHealthGroupContext? GroupDamage { get; init; }
    }

    public static void DamageLinedef(DamageLinedefContext ctx)
    {
        var damage = ctx.Damage;
        if (damage < 0)
            damage = 0;
        if (ctx.DoGroups && ctx.WorldLineDamaged is not null)
        {
            damage = ctx.WorldLineDamaged();
            if (damage < 0)
                damage = 0;
        }
        if (damage == 0)
            return;

        ctx.Line.Health -= damage;
        if (ctx.Line.Health < 0)
            ctx.Line.Health = 0;

        if (ctx.Line.Health == 0)
            ctx.ActivateLine?.Invoke(2);
        else
            ctx.ActivateLine?.Invoke(1);

        if (ctx.DoGroups && ctx.Line.HealthGroup != 0)
        {
            var grp = ctx.GetHealthGroup?.Invoke(ctx.Line.HealthGroup);
            if (grp is not null)
                grp.Health = ctx.Line.Health;
            if (ctx.GroupDamage is not null)
            {
                ctx.GroupDamage.Group = grp;
                ctx.GroupDamage.DamagedObject = ctx.Line;
                ctx.GroupDamage.Damage = damage;
                DamageHealthGroup(ctx.GroupDamage);
            }
        }
    }

    public sealed class DamageSectorContext
    {
        public SectorHealth Sector { get; set; } = null!;
        public int Damage { get; set; }
        public int Part { get; set; }
        public bool DoGroups { get; set; }
        public Func<int>? WorldSectorDamaged { get; init; }
        public Action<int>? TriggerSectorAction { get; init; }
        public Func<int, HealthGroup?>? GetHealthGroup { get; init; }
        public DamageHealthGroupContext? GroupDamage { get; init; }
    }

    public static void DamageSector(DamageSectorContext ctx)
    {
        var damage = ctx.Damage;
        if (damage < 0)
            damage = 0;
        if (ctx.DoGroups && ctx.WorldSectorDamaged is not null)
        {
            damage = ctx.WorldSectorDamaged();
            if (damage < 0)
                damage = 0;
        }
        if (damage == 0)
            return;

        var sec = ctx.Sector;
        int group;
        int dmgAction;
        int deathAction;
        int currentHealth;
        switch (ctx.Part)
        {
            case SecPartCeiling:
                currentHealth = sec.HealthCeiling;
                group = sec.HealthCeilingGroup;
                dmgAction = 1;
                deathAction = 2;
                break;
            case SecPartFloor:
                currentHealth = sec.HealthFloor;
                group = sec.HealthFloorGroup;
                dmgAction = 3;
                deathAction = 4;
                break;
            case SecPart3D:
                currentHealth = sec.Health3d;
                group = sec.Health3dGroup;
                dmgAction = 5;
                deathAction = 6;
                break;
            default:
                return;
        }

        var newHealth = currentHealth - damage;
        if (newHealth < 0)
            newHealth = 0;
        switch (ctx.Part)
        {
            case SecPartCeiling:
                sec.HealthCeiling = newHealth;
                break;
            case SecPartFloor:
                sec.HealthFloor = newHealth;
                break;
            case SecPart3D:
                sec.Health3d = newHealth;
                break;
        }

        ctx.TriggerSectorAction?.Invoke(dmgAction);
        if (newHealth == 0)
            ctx.TriggerSectorAction?.Invoke(deathAction);

        if (ctx.DoGroups && group != 0)
        {
            var grp = ctx.GetHealthGroup?.Invoke(group);
            if (grp is not null)
                grp.Health = newHealth;
            if (ctx.GroupDamage is not null)
            {
                ctx.GroupDamage.Group = grp;
                ctx.GroupDamage.DamagedObject = sec;
                ctx.GroupDamage.Damage = damage;
                ctx.GroupDamage.Part = ctx.Part;
                DamageHealthGroup(ctx.GroupDamage);
            }
        }
    }

    public sealed class InitHealthGroupsContext
    {
        public Dictionary<int, HealthGroup> Groups { get; } = new();
        public IReadOnlyList<LineHealth> Lines { get; set; } = Array.Empty<LineHealth>();
        public IReadOnlyList<SectorHealth> Sectors { get; set; } = Array.Empty<SectorHealth>();
        public List<int> GroupsInError { get; } = new();
        public Action<string>? Warn { get; init; }
    }

    public static void InitHealthGroups(InitHealthGroupsContext ctx)
    {
        ctx.Groups.Clear();
        ctx.GroupsInError.Clear();
        foreach (var line in ctx.Lines)
        {
            if (line.HealthGroup <= 0)
                continue;
            var grp = GetOrNew(ctx, line.HealthGroup, line.Health);
            if (grp.Health != line.Health)
            {
                ctx.Warn?.Invoke($"line {line.Index} group {line.HealthGroup}");
                ctx.GroupsInError.Add(line.HealthGroup);
                if (line.Health > grp.Health)
                    grp.Health = line.Health;
            }
            grp.Lines.Add(line);
        }
        foreach (var sec in ctx.Sectors)
        {
            if (sec.HealthCeilingGroup > 0)
            {
                var grp = GetOrNew(ctx, sec.HealthCeilingGroup, sec.HealthCeiling);
                if (grp.Health != sec.HealthCeiling)
                {
                    ctx.Warn?.Invoke($"ceiling {sec.Index}");
                    ctx.GroupsInError.Add(sec.HealthCeilingGroup);
                    if (sec.HealthCeiling > grp.Health)
                        grp.Health = sec.HealthCeiling;
                }
                grp.Sectors.Add(sec);
            }
            if (sec.HealthFloorGroup > 0)
            {
                var grp = GetOrNew(ctx, sec.HealthFloorGroup, sec.HealthFloor);
                if (grp.Health != sec.HealthFloor)
                {
                    ctx.Warn?.Invoke($"floor {sec.Index}");
                    ctx.GroupsInError.Add(sec.HealthFloorGroup);
                    if (sec.HealthFloor > grp.Health)
                        grp.Health = sec.HealthFloor;
                }
                if (sec.HealthCeilingGroup != sec.HealthFloorGroup)
                    grp.Sectors.Add(sec);
            }
        }
    }

    private static HealthGroup GetOrNew(InitHealthGroupsContext ctx, int id, int health)
    {
        if (!ctx.Groups.TryGetValue(id, out var grp))
        {
            grp = new HealthGroup { Id = id, Health = health };
            ctx.Groups[id] = grp;
        }
        return grp;
    }
}
