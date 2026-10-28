namespace HCDE.Playsim;

/// <summary>Sector portal factories, offset position, displacements, and group connect from <c>portal.cpp</c> (535–835).</summary>
public static class VmPortalDisplacementConnect
{
    public const int PortsSkyViewpoint = 1;
    public const int PortsLinkedPortal = 2;
    public const int PortsStackedSectorThing = 3;
    public const int PortfSkyFlatOnly = 1;
    public const int PorttLinked = 2;
    public const int PortfInteractive = 4;
    public const int PtAddlines = 1;
    public const int PtDelta = 2;

    public sealed class SectorPortalSlot
    {
        public int Type { get; set; }
        public int Flags { get; set; }
        public object? Skybox { get; set; }
        public object? Origin { get; set; }
        public object? Destination { get; set; }
        public double DisplacementX { get; set; }
        public double DisplacementY { get; set; }
        public double PlaneZ { get; set; } = double.MaxValue;
        public int Plane { get; set; }
    }

    public static int GetSkyboxPortalIndex(IList<SectorPortalSlot> sectorPortals, object? actor, bool skyCamCompat)
    {
        if (actor is null)
            return 1;
        for (var i = 0; i < sectorPortals.Count; i++)
        {
            if (ReferenceEquals(sectorPortals[i].Skybox, actor))
                return i;
        }
        sectorPortals.Add(new SectorPortalSlot
        {
            Type = PortsSkyViewpoint,
            Flags = skyCamCompat ? 0 : PortfSkyFlatOnly,
            Skybox = actor,
            Destination = actor,
        });
        return sectorPortals.Count - 1;
    }

    public static int ReservePortal(IList<SectorPortalSlot> sectorPortals, int type, int plane, object? from, object? to, double dispX, double dispY, double planeZ)
    {
        sectorPortals.Add(new SectorPortalSlot
        {
            Type = type,
            Plane = plane,
            Origin = from,
            Destination = to,
            DisplacementX = dispX,
            DisplacementY = dispY,
            PlaneZ = planeZ,
        });
        return sectorPortals.Count - 1;
    }

    public static int GetStackPortal(IList<SectorPortalSlot> sectorPortals, object point, object targetSector, object pointSector, int plane)
    {
        sectorPortals.Add(new SectorPortalSlot
        {
            Type = PortsStackedSectorThing,
            Plane = plane,
            Origin = targetSector,
            Destination = pointSector,
            PlaneZ = double.MaxValue,
            Skybox = point,
        });
        return sectorPortals.Count - 1;
    }

    public sealed class PortalOffsetContext
    {
        public bool BlockmapContainsLines { get; set; }
        public int BlockmapDx { get; set; }
        public int BlockmapDy { get; set; }
        public Func<double, double, (int Bx, int By)>? GetBlock { get; init; }
        public Func<int, int, bool>? NeighborContainsLines { get; init; }
        public Func<double, double, double, double, PortalInterceptHit?>? FirstInteractivePortalHit { get; init; }
    }

    public sealed class PortalInterceptHit
    {
        public bool Linked { get; set; }
        public double DispX { get; set; }
        public double DispY { get; set; }
        public double HitX { get; set; }
        public double HitY { get; set; }
        public Action<double, double>? TranslateDest { get; init; }
    }

    public static (double X, double Y) GetPortalOffsetPosition(PortalOffsetContext ctx, double x, double y, double dx, double dy)
    {
        var destX = x + dx;
        var destY = y + dy;
        if (!ctx.BlockmapContainsLines)
            return (destX, destY);

        var actX = x;
        var actY = y;
        if (dx < 128 && dy < 128 && ctx.GetBlock is not null)
        {
            var (blockx, blocky) = ctx.GetBlock(actX, actY);
            if (blockx < 0 || blocky < 0 || blockx >= ctx.BlockmapDx || blocky >= ctx.BlockmapDy ||
                ctx.NeighborContainsLines?.Invoke(blockx, blocky) != true)
                return (destX, destY);
        }

        bool repeat;
        do
        {
            repeat = false;
            var hit = ctx.FirstInteractivePortalHit?.Invoke(actX, actY, destX - actX, destY - actY);
            if (hit is null)
                break;
            if (hit.Linked)
            {
                hit.HitX += hit.DispX;
                hit.HitY += hit.DispY;
                destX += hit.DispX;
                destY += hit.DispY;
            }
            else
            {
                var hx = hit.HitX;
                var hy = hit.HitY;
                var dx2 = destX;
                var dy2 = destY;
                hit.TranslateDest?.Invoke(hx, hy);
                hit.TranslateDest?.Invoke(dx2, dy2);
                destX = dx2;
                destY = dy2;
            }
            dx = destX - hit.HitX;
            dy = destY - hit.HitY;
            actX = hit.HitX;
            actY = hit.HitY;
            repeat = true;
        } while (repeat);

        return (destX, destY);
    }

    public static bool CollectSectors(int groupId, SectorGroupNode origin, Dictionary<int, SectorGroupNode> sectorsById)
    {
        if (origin.PortalGroup != 0)
            return false;
        origin.PortalGroup = groupId;
        var list = new List<SectorGroupNode> { origin };
        for (var i = 0; i < list.Count; i++)
        {
            var sec = list[i];
            foreach (var neighbor in sec.Neighbors)
            {
                if (neighbor is null || ReferenceEquals(neighbor, sec) || neighbor.PortalGroup == groupId)
                    continue;
                neighbor.PortalGroup = groupId;
                list.Add(neighbor);
            }
        }
        return true;
    }

    public sealed class SectorGroupNode
    {
        public int Id { get; set; }
        public int PortalGroup { get; set; }
        public List<SectorGroupNode?> Neighbors { get; } = new();
    }

    public struct DisplacementCell
    {
        public bool IsSet { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public byte Indirect { get; set; }
    }

    public static void AddDisplacementForSectorPortal(
        int thisGroup, int otherGroup, double dispX, double dispY,
        DisplacementCell[,] table,
        Action<string>? disablePortal = null)
    {
        if (thisGroup == otherGroup)
        {
            disablePortal?.Invoke("same-group");
            return;
        }
        if (thisGroup <= 0 || otherGroup <= 0 || thisGroup >= table.GetLength(0) || otherGroup >= table.GetLength(0))
        {
            disablePortal?.Invoke("invalid-group");
            return;
        }
        ref var disp = ref table[thisGroup, otherGroup];
        if (!disp.IsSet)
        {
            disp.X = dispX;
            disp.Y = dispY;
            disp.IsSet = true;
        }
        else if (disp.X != dispX || disp.Y != dispY)
            disablePortal?.Invoke("mismatch");
    }

    public static bool ConnectPortalGroups(DisplacementCell[,] table)
    {
        var bogus = false;
        var indirect = (byte)1;
        bool changed;
        do
        {
            changed = false;
            var size = table.GetLength(0);
            for (var x = 1; x < size; x++)
            {
                for (var y = 1; y < size; y++)
                {
                    ref var dispxy = ref table[x, y];
                    if (!dispxy.IsSet)
                        continue;
                    for (var z = 1; z < size; z++)
                    {
                        ref var dispyz = ref table[y, z];
                        if (!dispyz.IsSet)
                            continue;
                        ref var dispxz = ref table[x, z];
                        if (dispxz.IsSet)
                        {
                            if (dispxy.X + dispyz.X != dispxz.X || dispxy.Y + dispyz.Y != dispxz.Y)
                                bogus = true;
                        }
                        else
                        {
                            dispxz.X = dispxy.X + dispyz.X;
                            dispxz.Y = dispxy.Y + dispyz.Y;
                            dispxz.IsSet = true;
                            dispxz.Indirect = indirect;
                            changed = true;
                        }
                    }
                }
            }
            indirect++;
        } while (changed);
        return bogus;
    }
}
