namespace HCDE.Playsim;

/// <summary>
/// Health-group and geometry damage from <c>p_destructible.cpp</c> (<b>1–238</b>). Chunk <b>1175</b>.
/// </summary>
public static class VmPDestructibleHealth1175Slice
{
    public const int SecPartFloor = 0;
    public const int SecPartCeiling = 1;
    public const int SecPart3D = 2;
    public const int SpacDamage = 1 << 11;
    public const int SpacDeath = 1 << 12;
    public const int SecDamageFloor = 1 << 11;
    public const int SecDamageCeiling = 1 << 12;
    public const int SecDeathFloor = 1 << 13;
    public const int SecDeathCeiling = 1 << 14;
    public const int SecDamage3D = 1 << 15;
    public const int SecDeath3D = 1 << 16;

    public sealed class HealthGroup
    {
        public int Id;
        public int Health;
        public List<GeoLine> Lines { get; } = new();
        public List<GeoSector> Sectors { get; } = new();
    }

    public sealed class Level
    {
        public Dictionary<int, HealthGroup> Groups { get; } = new();
    }

    public sealed class GeoLine
    {
        public int Health;
        public int HealthGroup;
        public int Activation;
        public Level Level = new();
    }

    public sealed class GeoSector
    {
        public int HealthCeiling;
        public int HealthFloor;
        public int Health3D;
        public int HealthCeilingGroup;
        public int HealthFloorGroup;
        public int Health3DGroup;
        public bool HasAction;
        public Level Level = new();
    }

    public sealed class DamageHost
    {
        public bool HasLineWorld;
        public int LineWorldDamage;
        public bool HasSectorWorld;
        public int SectorWorldDamage;
        public List<string> Events { get; } = new();
    }

    public static void SetHealth(HealthGroup? group, int health)
    {
        if (group == null)
        {
            return;
        }

        group.Health = health;
        for (var i = 0; i < group.Lines.Count; i++)
        {
            group.Lines[i].Health = health;
        }

        for (var i = 0; i < group.Sectors.Count; i++)
        {
            var sector = group.Sectors[i];
            if (sector.HealthCeilingGroup == group.Id)
            {
                sector.HealthCeiling = health;
            }

            if (sector.HealthFloorGroup == group.Id)
            {
                sector.HealthFloor = health;
            }

            if (sector.Health3DGroup == group.Id)
            {
                sector.Health3D = health;
            }
        }
    }

    public static void SetHealth(Level level, int id, int health)
    {
        SetHealth(GetGroup(level, id), health);
    }

    public static HealthGroup? GetGroup(Level level, int id)
    {
        if (level.Groups.TryGetValue(id, out var group))
        {
            return group;
        }

        return null;
    }

    public static HealthGroup GetOrNew(Level level, int id, int health)
    {
        var group = GetGroup(level, id);
        if (group == null)
        {
            group = new HealthGroup { Id = id, Health = health };
            level.Groups[id] = group;
        }

        return group;
    }

    public static void DamageGroup(
        HealthGroup? group,
        object? origin,
        int damage,
        int side,
        int part,
        bool isRadius,
        DamageHost host)
    {
        if (group == null)
        {
            return;
        }

        for (var i = 0; i < group.Lines.Count; i++)
        {
            var line = group.Lines[i];
            if (ReferenceEquals(line, origin))
            {
                continue;
            }

            line.Health = group.Health + damage;
            DamageLine(line, damage, side, isRadius, false, host);
        }

        for (var i = 0; i < group.Sectors.Count; i++)
        {
            var sector = group.Sectors[i];
            if (sector.HealthCeilingGroup == group.Id && (!ReferenceEquals(sector, origin) || part != SecPartCeiling))
            {
                sector.HealthCeiling = group.Health + damage;
                DamageSector(sector, damage, SecPartCeiling, isRadius, false, host);
            }

            if (sector.HealthFloorGroup == group.Id && (!ReferenceEquals(sector, origin) || part != SecPartFloor))
            {
                sector.HealthFloor = group.Health + damage;
                DamageSector(sector, damage, SecPartFloor, isRadius, false, host);
            }

            if (sector.Health3DGroup == group.Id && (!ReferenceEquals(sector, origin) || part != SecPart3D))
            {
                sector.Health3D = group.Health + damage;
                DamageSector(sector, damage, SecPart3D, isRadius, false, host);
            }
        }
    }

    public static void DamageLine(GeoLine line, int damage, int side, bool isRadius, bool doGroups, DamageHost host)
    {
        if (damage < 0)
        {
            damage = 0;
        }

        if (doGroups)
        {
            host.Events.Add("world-line:" + damage);
            if (host.HasLineWorld)
            {
                damage = host.LineWorldDamage;
            }

            if (damage < 0)
            {
                damage = 0;
            }
        }

        if (damage == 0)
        {
            return;
        }

        line.Health -= damage;
        if (line.Health < 0)
        {
            line.Health = 0;
        }

        if ((line.Activation & SpacDamage) != 0 || ((line.Activation & SpacDeath) != 0 && line.Health == 0))
        {
            var activateOn = SpacDamage;
            if (line.Health == 0)
            {
                activateOn |= SpacDeath;
            }

            host.Events.Add("activate:" + (activateOn & line.Activation) + ":" + side);
        }

        if (doGroups && line.HealthGroup != 0)
        {
            var group = GetGroup(line.Level, line.HealthGroup);
            if (group != null)
            {
                group.Health = line.Health;
            }

            DamageGroup(group, line, damage, side, -1, isRadius, host);
        }
    }

    public static void DamageSector(GeoSector sector, int damage, int part, bool isRadius, bool doGroups, DamageHost host)
    {
        if (damage < 0)
        {
            damage = 0;
        }

        if (doGroups)
        {
            host.Events.Add("world-sector:" + damage);
            if (host.HasSectorWorld)
            {
                damage = host.SectorWorldDamage;
            }

            if (damage < 0)
            {
                damage = 0;
            }
        }

        if (damage == 0)
        {
            return;
        }

        int health;
        int groupId;
        int damageAction;
        int deathAction;
        if (part == SecPartCeiling)
        {
            health = sector.HealthCeiling;
            groupId = sector.HealthCeilingGroup;
            damageAction = SecDamageCeiling;
            deathAction = SecDeathCeiling;
        }
        else if (part == SecPartFloor)
        {
            health = sector.HealthFloor;
            groupId = sector.HealthFloorGroup;
            damageAction = SecDamageFloor;
            deathAction = SecDeathFloor;
        }
        else if (part == SecPart3D)
        {
            health = sector.Health3D;
            groupId = sector.Health3DGroup;
            damageAction = SecDamage3D;
            deathAction = SecDeath3D;
        }
        else
        {
            return;
        }

        var newHealth = health - damage;
        if (newHealth < 0)
        {
            newHealth = 0;
        }

        if (part == SecPartCeiling)
        {
            sector.HealthCeiling = newHealth;
        }
        else if (part == SecPartFloor)
        {
            sector.HealthFloor = newHealth;
        }
        else
        {
            sector.Health3D = newHealth;
        }

        if (sector.HasAction)
        {
            host.Events.Add("trigger:" + damageAction);
            if (newHealth == 0)
            {
                host.Events.Add("trigger:" + deathAction);
            }
        }

        if (doGroups && groupId != 0)
        {
            var group = GetGroup(sector.Level, groupId);
            if (group != null)
            {
                group.Health = newHealth;
            }

            DamageGroup(group, sector, damage, 0, part, isRadius, host);
        }
    }
}
