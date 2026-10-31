namespace HCDE.Playsim;

/// <summary>
/// Projectile geometry hits and health-group actions from <c>p_destructible.cpp</c>
/// (<b>689–909</b>). Chunk <b>1168</b>.
/// </summary>
public static class VmPDestructibleHit1168Slice
{
    public const int SecPartFloor = 0;
    public const int SecPartCeiling = 1;
    public const int SecPart3D = 2;
    public const int LineHorizon = 9;
    public const int Mf4StrifeDamage = 0x00000100;
    public const double EqualEpsilon = 1d / 65536d;

    public readonly record struct Vec2(double X, double Y)
    {
        public double Length()
        {
            return Math.Sqrt((X * X) + (Y * Y));
        }

        public Vec2 Rotated90Cw()
        {
            return new Vec2(Y, -X);
        }

        public Vec2 Unit()
        {
            var len = Length();
            if (len != 0d)
            {
                len = 1d / len;
            }

            return new Vec2(X * len, Y * len);
        }

        public static double Dot(Vec2 a, Vec2 b)
        {
            return (a.X * b.X) + (a.Y * b.Y);
        }

        public static Vec2 operator -(Vec2 a, Vec2 b)
        {
            return new Vec2(a.X - b.X, a.Y - b.Y);
        }

        public static Vec2 operator *(Vec2 a, double scale)
        {
            return new Vec2(a.X * scale, a.Y * scale);
        }
    }

    public sealed class Side
    {
        public Sector? Sector;
    }

    public sealed class Line
    {
        public int Special;
        public int Health;
        public Vec2 V1;
        public Vec2 Delta;
        public Side?[] Sides { get; } = new Side?[2];
    }

    public sealed class Sector
    {
        public int HealthFloor;
        public int HealthCeiling;
        public int Health3D;
        public int FloorTexture;
        public int CeilingTexture;
        public int LastPlanePart = -2;
        public Func<Vec2, double>? FloorZ;
        public Func<Vec2, double>? CeilingZ;
    }

    public sealed class Missile
    {
        public int Flags4;
        public string DamageType = "";
        public double X;
        public double Y;
        public double Z;
        public double Height;
        public bool BlockingCeiling;
        public bool BlockingFloor;
        public Sector? Blocking3D;
        public Sector? Sector;
        public Func<int, int, int>? MissileDamage;
    }

    public sealed class Hit
    {
        public string Kind = "";
        public Sector? Sector;
        public Line? Line;
        public int Part;
        public int Side;
        public int Damage;
        public string DamageType = "";
        public double X;
        public double Y;
        public double Z;
        public bool Radius;
        public bool Groups = true;
        public Missile? Source;
    }

    public sealed class HealthGroup
    {
        public int Id;
        public int Health;
        public List<Sector> Sectors { get; } = new();
        public List<Line> Lines { get; } = new();
    }

    public sealed class GroupArchive
    {
        public bool OpenObject;
        public bool OpenArray;
        public bool Reading;
        public List<(int Id, int Health)> Saved { get; } = new();
        public List<(int Id, int Health)> Written { get; } = new();
    }

    public static int MissileFactor(Missile missile)
    {
        if ((missile.Flags4 & Mf4StrifeDamage) != 0)
        {
            return 3;
        }

        return 7;
    }

    public static bool ProjectileHitLinedef(
        Missile missile,
        Line line,
        Func<Vec2, Line, int> pointOnLineSide,
        List<Hit> hits)
    {
        var wasHit = false;
        if (missile.Blocking3D != null && missile.Blocking3D.Health3D > 0)
        {
            hits.Add(SectorHit(missile, missile.Blocking3D, SecPart3D));
            wasHit = true;
        }

        var wside = pointOnLineSide(new Vec2(missile.X, missile.Y), line);
        var oside = wside == 0 ? 1 : 0;
        var other = oside < line.Sides.Length ? line.Sides[oside] : null;
        if (other?.Sector != null)
        {
            var mo = new Vec2(missile.X, missile.Y);
            var rel = line.V1 - mo;
            var normal = line.Delta.Rotated90Cw().Unit();
            var distance = Math.Abs(Vec2.Dot(normal, rel));
            var point = mo - (normal * distance);
            var otherFloor = other.Sector.FloorZ!(point);
            var otherCeiling = other.Sector.CeilingZ!(point);
            var bottom = missile.Z;
            var top = missile.Z + missile.Height;
            if (bottom < otherFloor + EqualEpsilon
                && other.Sector.HealthFloor > 0
                && CheckLinedefVulnerable(line, wside, SecPartFloor))
            {
                hits.Add(SectorHit(missile, other.Sector, SecPartFloor));
                wasHit = true;
            }

            if (top > otherCeiling - EqualEpsilon
                && other.Sector.HealthCeiling > 0
                && CheckLinedefVulnerable(line, wside, SecPartCeiling))
            {
                hits.Add(SectorHit(missile, other.Sector, SecPartCeiling));
                wasHit = true;
            }
        }

        if (line.Health > 0 && CheckLinedefVulnerable(line, wside))
        {
            hits.Add(new Hit
            {
                Kind = "line",
                Line = line,
                Part = -1,
                Side = wside,
                Damage = missile.MissileDamage!(MissileFactor(missile), 1),
                DamageType = missile.DamageType,
                X = missile.X,
                Y = missile.Y,
                Z = missile.Z,
                Radius = false,
                Groups = true,
            });
            wasHit = true;
        }

        return wasHit;
    }

    public static bool ProjectileHitPlane(Missile missile, int part, int skyFlat, List<Hit> hits)
    {
        if (part < 0)
        {
            if (missile.BlockingCeiling)
            {
                part = SecPartCeiling;
            }
            else if (missile.BlockingFloor)
            {
                part = SecPartFloor;
            }
        }

        if (missile.Blocking3D != null)
        {
            if (missile.Blocking3D.Health3D > 0)
            {
                hits.Add(SectorHit(missile, missile.Blocking3D, SecPart3D));
                return true;
            }

            return false;
        }

        if (part == SecPartFloor
            && missile.Sector!.HealthFloor > 0
            && CheckSectorVulnerable(missile.Sector, SecPartFloor, skyFlat))
        {
            hits.Add(SectorHit(missile, missile.Sector, SecPartFloor));
            return true;
        }

        if (part == SecPartCeiling
            && missile.Sector!.HealthCeiling > 0
            && CheckSectorVulnerable(missile.Sector, SecPartCeiling, skyFlat))
        {
            hits.Add(SectorHit(missile, missile.Sector, SecPartCeiling));
            return true;
        }

        return false;
    }

    public static bool CheckLinedefVulnerable(Line line, int side, int sectorPart = -1)
    {
        _ = sectorPart;
        if (line.Special == LineHorizon)
        {
            return false;
        }

        if (line.Sides[side] == null)
        {
            return false;
        }

        return true;
    }

    public static bool CheckSectorVulnerable(Sector sector, int part, int skyFlat)
    {
        if (part == SecPart3D)
        {
            return true;
        }

        var ceiling = part == SecPartCeiling;
        var texture = ceiling ? sector.CeilingTexture : sector.FloorTexture;
        sector.LastPlanePart = ceiling ? SecPartCeiling : SecPartFloor;

        if (texture == skyFlat)
        {
            return false;
        }

        return true;
    }

    public static GroupArchive SerializeGroup(GroupArchive arc, HealthGroup group)
    {
        if (arc.OpenObject)
        {
            arc.Written.Add((group.Id, group.Health));
        }

        return arc;
    }

    public static void SerializeHealthGroups(IReadOnlyList<HealthGroup> groups, GroupArchive arc, Func<int, HealthGroup?> find)
    {
        if (!arc.OpenArray)
        {
            return;
        }

        if (arc.Reading)
        {
            for (var i = 0; i < arc.Saved.Count; i++)
            {
                var saved = arc.Saved[i];
                var existing = find(saved.Id);
                if (existing == null)
                {
                    continue;
                }

                existing.Health = saved.Health;
            }
        }
        else
        {
            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                arc.Written.Add((group.Id, group.Health));
            }
        }
    }

    public static HealthGroup? FindHealthGroup(Func<int, HealthGroup?> find, int id)
    {
        return find(id);
    }

    public static int SetHealth(HealthGroup group, int health, Action<HealthGroup, int> apply)
    {
        apply(group, health);
        return 0;
    }

    public static int DamageSectorAction(
        Sector sector,
        Missile source,
        int damage,
        string damageType,
        int part,
        double x,
        double y,
        double z,
        bool radius,
        List<Hit> hits)
    {
        hits.Add(new Hit
        {
            Kind = "sector",
            Sector = sector,
            Part = part,
            Damage = damage,
            DamageType = damageType,
            X = x,
            Y = y,
            Z = z,
            Radius = radius,
            Groups = true,
            Source = source,
        });
        return 0;
    }

    private static Hit SectorHit(Missile missile, Sector sector, int part)
    {
        return new Hit
        {
            Kind = "sector",
            Sector = sector,
            Part = part,
            Damage = missile.MissileDamage!(MissileFactor(missile), 1),
            DamageType = missile.DamageType,
            X = missile.X,
            Y = missile.Y,
            Z = missile.Z,
            Radius = false,
            Groups = true,
        };
    }
}
