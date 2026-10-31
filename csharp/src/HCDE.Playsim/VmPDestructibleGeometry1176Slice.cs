namespace HCDE.Playsim;

/// <summary>
/// Health-group init, geometry line attack, and radius-attack helpers from <c>p_destructible.cpp</c> (<b>239–441</b>). Chunk <b>1176</b>.
/// </summary>
public static class VmPDestructibleGeometry1176Slice
{
    public const int TraceHitFloor = 1;
    public const int TraceHitCeiling = 2;
    public const int TraceHitWall = 3;
    public const int TierUpper = 1;
    public const int TierLower = 2;
    public const int TierFFloor = 3;
    public const int FfExists = 0x1;
    public const int FfSolid = 0x2;
    public const int FfShootThrough = 0x4000000;
    public const int MlBlockEverything = 0x00008000;
    public const double EqualEpsilon = 1d / 65536d;
    public const string TextRed = "\u001cG";
    public const string TextGold = "\u001cF";

    public sealed class Floor3D
    {
        public int Flags;
        public VmPDestructibleHealth1175Slice.GeoSector? Model;
        public double TopZ;
        public double BottomZ;
    }

    public sealed class Hit
    {
        public int HitType;
        public int Tier;
        public double X;
        public double Y;
        public double Z;
        public int Side;
        public VmPDestructibleHealth1175Slice.GeoLine? Line;
        public bool LineVulnerable = true;
        public bool FloorPartVulnerable = true;
        public bool CeilingPartVulnerable = true;
        public bool SectorFloorVulnerable = true;
        public bool SectorCeilingVulnerable = true;
        public VmPDestructibleHealth1175Slice.GeoSector? Back;
        public VmPDestructibleHealth1175Slice.GeoSector? Sector;
        public VmPDestructibleHealth1175Slice.GeoSector? Model;
        public List<Floor3D> Floors { get; } = new();
    }

    public sealed class GroupPoint
    {
        public double X;
        public double Y;
        public double Z;
        public VmPDestructibleHealth1175Slice.GeoLine? Line;
        public VmPDestructibleHealth1175Slice.GeoSector? Sector;
        public int Part;
    }

    public sealed class Probe
    {
        public double X;
        public double Y;
        public double Z;
        public double Dx;
        public double Dy;
        public double Dz;
        public double Distance;
        public int Flags;
    }

    public sealed class GeometryHost
    {
        public List<VmPDestructibleHealth1175Slice.GeoLine> Lines { get; } = new();
        public List<VmPDestructibleHealth1175Slice.GeoSector> Sectors { get; } = new();
        public List<string> Warnings { get; } = new();
        public bool TraceHits;
        public Probe? LastProbe;
        public Dictionary<int, GroupPoint> Points { get; } = new();
    }

    public static void Init(VmPDestructibleHealth1175Slice.Level level, GeometryHost host)
    {
        level.Groups.Clear();
        var errors = new List<int>();
        for (var i = 0; i < host.Lines.Count; i++)
        {
            var line = host.Lines[i];
            if (line.HealthGroup <= 0)
            {
                continue;
            }

            var group = VmPDestructibleHealth1175Slice.GetOrNew(level, line.HealthGroup, line.Health);
            if (group.Health != line.Health)
            {
                host.Warnings.Add(TextRed + "Warning: line " + i + " in health group " + line.HealthGroup + " has different starting health (line health = " + line.Health + ", group health = " + group.Health + ")\n");
                errors.Add(line.HealthGroup);
                if (line.Health > group.Health)
                {
                    group.Health = line.Health;
                }
            }

            group.Lines.Add(line);
        }

        for (var i = 0; i < host.Sectors.Count; i++)
        {
            var sector = host.Sectors[i];
            if (sector.HealthCeilingGroup > 0)
            {
                var group = VmPDestructibleHealth1175Slice.GetOrNew(level, sector.HealthCeilingGroup, sector.HealthCeiling);
                if (group.Health != sector.HealthCeiling)
                {
                    host.Warnings.Add(TextRed + "Warning: sector ceiling " + i + " in health group " + sector.HealthCeilingGroup + " has different starting health (ceiling health = " + sector.HealthCeiling + ", group health = " + group.Health + ")\n");
                    errors.Add(sector.HealthCeilingGroup);
                    if (sector.HealthCeiling > group.Health)
                    {
                        group.Health = sector.HealthCeiling;
                    }
                }

                group.Sectors.Add(sector);
            }

            if (sector.HealthFloorGroup > 0)
            {
                var group = VmPDestructibleHealth1175Slice.GetOrNew(level, sector.HealthFloorGroup, sector.HealthFloor);
                if (group.Health != sector.HealthFloor)
                {
                    host.Warnings.Add(TextRed + "Warning: sector floor " + i + " in health group " + sector.HealthFloorGroup + " has different starting health (floor health = " + sector.HealthFloor + ", group health = " + group.Health + ")\n");
                    errors.Add(sector.HealthFloorGroup);
                    if (sector.HealthFloor > group.Health)
                    {
                        group.Health = sector.HealthFloor;
                    }
                }

                if (sector.HealthCeilingGroup != sector.HealthFloorGroup)
                {
                    group.Sectors.Add(sector);
                }
            }
        }

        for (var i = 0; i < errors.Count; i++)
        {
            var group = VmPDestructibleHealth1175Slice.GetGroup(level, errors[i]);
            if (group != null)
            {
                host.Warnings.Add(TextGold + "Health group " + errors[i] + " is using the highest found health value of " + group.Health);
            }
        }
    }

    public static void LineAttack(Hit hit, int damage, VmPDestructibleHealth1175Slice.DamageHost damageHost)
    {
        if (hit.HitType == TraceHitWall && hit.Tier == TierFFloor && hit.Model != null && hit.Model.Health3D != 0)
        {
            VmPDestructibleHealth1175Slice.DamageSector(hit.Model, damage, VmPDestructibleHealth1175Slice.SecPart3D, false, true, damageHost);
        }

        if (hit.HitType == TraceHitWall && hit.LineVulnerable && hit.Line != null)
        {
            if (hit.Tier == TierLower || hit.Tier == TierUpper)
            {
                var sectorHealth = 0;
                if (hit.Back != null && hit.Tier == TierLower && hit.Back.HealthFloor > 0 && hit.FloorPartVulnerable)
                {
                    sectorHealth = hit.Back.HealthFloor;
                }

                if (hit.Back != null && hit.Tier == TierUpper && hit.Back.HealthCeiling > 0 && hit.CeilingPartVulnerable)
                {
                    sectorHealth = hit.Back.HealthCeiling;
                }

                if (sectorHealth > 0 && hit.Back != null)
                {
                    var part = hit.Tier == TierUpper
                        ? VmPDestructibleHealth1175Slice.SecPartCeiling
                        : VmPDestructibleHealth1175Slice.SecPartFloor;
                    VmPDestructibleHealth1175Slice.DamageSector(hit.Back, damage, part, false, true, damageHost);
                }
            }

            if (hit.Line.Health > 0)
            {
                VmPDestructibleHealth1175Slice.DamageLine(hit.Line, damage, hit.Side, false, true, damageHost);
            }
        }
        else if (hit.HitType == TraceHitFloor || hit.HitType == TraceHitCeiling)
        {
            var hit3D = false;
            for (var i = 0; i < hit.Floors.Count; i++)
            {
                var floor = hit.Floors[i];
                if ((floor.Flags & FfExists) == 0)
                {
                    continue;
                }

                if ((floor.Flags & FfSolid) == 0 || (floor.Flags & FfShootThrough) != 0)
                {
                    continue;
                }

                if (floor.Model == null)
                {
                    continue;
                }

                if (hit.HitType == TraceHitFloor && Math.Abs(floor.TopZ - hit.Z) <= EqualEpsilon)
                {
                    if (floor.Model.Health3D != 0)
                    {
                        VmPDestructibleHealth1175Slice.DamageSector(floor.Model, damage, VmPDestructibleHealth1175Slice.SecPart3D, false, true, damageHost);
                    }

                    hit3D = true;
                }
                else if (hit.HitType == TraceHitCeiling && Math.Abs(floor.BottomZ - hit.Z) <= EqualEpsilon)
                {
                    if (floor.Model.Health3D != 0)
                    {
                        VmPDestructibleHealth1175Slice.DamageSector(floor.Model, damage, VmPDestructibleHealth1175Slice.SecPart3D, false, true, damageHost);
                    }

                    hit3D = true;
                }
            }

            if (hit3D)
            {
                return;
            }

            if (hit.Sector == null)
            {
                return;
            }

            var sectorHealth = 0;
            if (hit.HitType == TraceHitFloor && hit.Sector.HealthFloor > 0 && hit.SectorFloorVulnerable)
            {
                sectorHealth = hit.Sector.HealthFloor;
            }

            if (hit.HitType == TraceHitCeiling && hit.Sector.HealthCeiling > 0 && hit.SectorCeilingVulnerable)
            {
                sectorHealth = hit.Sector.HealthCeiling;
            }

            if (sectorHealth > 0)
            {
                var part = hit.HitType == TraceHitCeiling
                    ? VmPDestructibleHealth1175Slice.SecPartCeiling
                    : VmPDestructibleHealth1175Slice.SecPartFloor;
                VmPDestructibleHealth1175Slice.DamageSector(hit.Sector, damage, part, false, true, damageHost);
            }
        }
    }

    public static (double X, double Y) ClosestPoint(double x, double y, double ax, double ay, double bx, double by)
    {
        var dx = bx - ax;
        var dy = by - ay;
        var len = Math.Sqrt(dx * dx + dy * dy);
        var r = (dx * (x - ax) + dy * (y - ay)) / len;
        if (r < 0)
        {
            return (ax, ay);
        }

        if (r > len)
        {
            return (bx, by);
        }

        return (ax + dx / len * r, ay + dy / len * r);
    }

    public static bool ExplosionBlocked(double px, double py, double pz, double cx, double cy, double cz, GeometryHost host)
    {
        var z = cz + EqualEpsilon;
        var dx = px - cx;
        var dy = py - cy;
        var dz = pz - z;
        var len = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        host.LastProbe = new Probe
        {
            X = cx,
            Y = cy,
            Z = z,
            Dx = len == 0 ? 0 : dx / len,
            Dy = len == 0 ? 0 : dy / len,
            Dz = len == 0 ? 0 : dz / len,
            Distance = len - 0.5,
            Flags = MlBlockEverything,
        };
        return host.TraceHits;
    }

    public static void InsertIfCloser(
        int group,
        double px,
        double py,
        double pz,
        double cx,
        double cy,
        double cz,
        VmPDestructibleHealth1175Slice.GeoLine? line,
        VmPDestructibleHealth1175Slice.GeoSector? sector,
        int part,
        GeometryHost host)
    {
        if (ExplosionBlocked(px, py, pz, cx, cy, cz, host))
        {
            return;
        }

        var next = Distance(px, py, pz, cx, cy, cz);
        if (!host.Points.TryGetValue(group, out var existing) || Distance(existing.X, existing.Y, existing.Z, cx, cy, cz) > next)
        {
            host.Points[group] = new GroupPoint
            {
                X = px,
                Y = py,
                Z = pz,
                Line = line,
                Sector = sector,
                Part = part,
            };
        }
    }

    static double Distance(double ax, double ay, double az, double bx, double by, double bz)
    {
        var dx = ax - bx;
        var dy = ay - by;
        var dz = az - bz;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
