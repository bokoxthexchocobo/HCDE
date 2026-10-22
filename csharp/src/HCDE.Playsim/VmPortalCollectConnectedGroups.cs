namespace HCDE.Playsim;

/// <summary><c>FLevelLocals::CollectConnectedGroups</c> from <c>portal.cpp</c> (1049–1184).</summary>
public static class VmPortalCollectConnectedGroups
{
    public const int PgaNoSectorPortals = 0;
    public const int PgaCheckPosition = 1;
    public const int PgaFull3d = 2;
    public const int Lower = 0x4000;
    public const int Upper = 0x8000;
    public const int Flat = 0xc000;

    public sealed class PortalGroupArray
    {
        public bool Inited { get; set; }
        public int Method { get; set; } = PgaCheckPosition;
        public List<int> Groups { get; } = new();

        public void Add(int group) => Groups.Add(group);
    }

    public sealed class LinkedPortal
    {
        public int OriginLineIndex { get; init; }
        public int DestinationLineIndex { get; init; }
        public int OriginFrontPortalGroup { get; init; }
        public int DestinationFrontPortalGroup { get; init; }
    }

    public sealed class Displacement
    {
        public bool IsSet { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
    }

    public sealed class CollectContext
    {
        public int DisplacementSize { get; init; } = 1;
        public IReadOnlyList<LinkedPortal> LinkedPortals { get; init; } = Array.Empty<LinkedPortal>();
        public Func<int, int, Displacement>? DisplacementAt { get; init; }
        public Func<double, double, double, bool>? InRange { get; init; }
        public Func<double, double, double, double, int>? BoxOnLineSide { get; init; }
        public Func<double, double, object?>? PointInSector { get; init; }
        public Func<object, bool>? PortalBlocksMovementCeiling { get; init; }
        public Func<object, bool>? PortalBlocksMovementFloor { get; init; }
        public Func<object, double>? PortalPlaneZCeiling { get; init; }
        public Func<object, double>? PortalPlaneZFloor { get; init; }
        public Func<object, int>? OppositePortalGroupCeiling { get; init; }
        public Func<object, int>? OppositePortalGroupFloor { get; init; }
        public Func<int, int, (double X, double Y)>? DisplacementOffset { get; init; }
        public bool HasLinkedSectorPortals { get; init; }
        public Func<int, double, double, double, IEnumerable<LineProbe>>? BlockLinesInBox { get; init; }
    }

    public readonly record struct LineProbe(
        int LineIndex,
        object? FrontSector,
        object? BackSector);

    public sealed class BitMask
    {
        private bool[] _bits = Array.Empty<bool>();

        public void SetSize(int size)
        {
            if (_bits.Length == size)
                return;
            _bits = new bool[size];
        }

        public void Clear()
        {
            for (var i = 0; i < _bits.Length; i++)
                _bits[i] = false;
        }

        public void SetBit(int group)
        {
            if (group >= 0 && group < _bits.Length)
                _bits[group] = true;
        }

        public bool GetBit(int group) => group >= 0 && group < _bits.Length && _bits[group];
    }

    public static bool CollectConnectedGroups(
        CollectContext ctx,
        BitMask processMask,
        List<LinkedPortal> foundPortals,
        List<int> groupsToCheck,
        int startGroup,
        double posX,
        double posY,
        double posZ,
        double upperZ,
        double checkRadius,
        PortalGroupArray outGroups)
    {
        var retval = false;
        outGroups.Inited = true;

        processMask.SetSize(ctx.DisplacementSize);
        if (ctx.DisplacementSize == 1)
        {
            processMask.SetBit(startGroup);
            return false;
        }

        if (ctx.LinkedPortals.Count != 0)
        {
            processMask.Clear();
            foundPortals.Clear();

            var thisgroup = startGroup;
            processMask.SetBit(thisgroup);

            foreach (var lp in ctx.LinkedPortals)
            {
                var othergroup = lp.OriginFrontPortalGroup;
                var disp = ctx.DisplacementAt?.Invoke(thisgroup, othergroup) ?? new Displacement();
                if (!disp.IsSet)
                    continue;

                var boxX = posX + disp.PosX;
                var boxY = posY + disp.PosY;
                if (ctx.InRange?.Invoke(boxX, boxY, checkRadius) != true ||
                    ctx.BoxOnLineSide?.Invoke(boxX, boxY, checkRadius, lp.OriginLineIndex) != -1)
                    continue;
                foundPortals.Add(lp);
            }

            var foundone = true;
            while (foundone)
            {
                foundone = false;
                for (var i = foundPortals.Count - 1; i >= 0; i--)
                {
                    var fp = foundPortals[i];
                    if (processMask.GetBit(fp.OriginFrontPortalGroup) &&
                        !processMask.GetBit(fp.DestinationFrontPortalGroup))
                    {
                        processMask.SetBit(fp.DestinationFrontPortalGroup);
                        outGroups.Add(fp.DestinationFrontPortalGroup);
                        foundone = true;
                        retval = true;
                        foundPortals.RemoveAt(i);
                    }
                }
            }
        }

        if (outGroups.Method != PgaNoSectorPortals)
        {
            var sec = ctx.PointInSector?.Invoke(posX, posY);
            var wsec = sec;
            while (sec is not null && ctx.PortalBlocksMovementCeiling?.Invoke(wsec!) == false &&
                   upperZ > (ctx.PortalPlaneZCeiling?.Invoke(wsec!) ?? double.MaxValue))
            {
                var othergroup = ctx.OppositePortalGroupCeiling?.Invoke(wsec!) ?? 0;
                var off = ctx.DisplacementOffset?.Invoke(startGroup, othergroup) ?? (0, 0);
                var sx = off.X + posX;
                var sy = off.Y + posY;
                if (processMask.GetBit(othergroup))
                    break;
                processMask.SetBit(othergroup);
                outGroups.Add(othergroup | Upper);
                wsec = ctx.PointInSector?.Invoke(sx, sy);
                retval = true;
            }

            wsec = sec;
            while (sec is not null && ctx.PortalBlocksMovementFloor?.Invoke(wsec!) == false &&
                   posZ < (ctx.PortalPlaneZFloor?.Invoke(wsec!) ?? double.MinValue))
            {
                var othergroup = ctx.OppositePortalGroupFloor?.Invoke(wsec!) ?? 0;
                var off = ctx.DisplacementOffset?.Invoke(startGroup, othergroup) ?? (0, 0);
                var sx = off.X + posX;
                var sy = off.Y + posY;
                if (processMask.GetBit(othergroup))
                    break;
                processMask.SetBit(othergroup);
                outGroups.Add(othergroup | Lower);
                wsec = ctx.PointInSector?.Invoke(sx, sy);
                retval = true;
            }

            if (outGroups.Method == PgaFull3d && ctx.HasLinkedSectorPortals)
            {
                groupsToCheck.Clear();
                groupsToCheck.Add(startGroup);
                for (var gi = 0; gi < groupsToCheck.Count; gi++)
                {
                    var thisgroup = groupsToCheck[gi];
                    var disp = ctx.DisplacementOffset?.Invoke(startGroup, thisgroup & ~Flat) ?? (0, 0);
                    var boxX = posX + disp.X;
                    var boxY = posY + disp.Y;
                    foreach (var ld in ctx.BlockLinesInBox?.Invoke(thisgroup, boxX, boxY, checkRadius) ?? Array.Empty<LineProbe>())
                    {
                        if (ctx.InRange?.Invoke(boxX, boxY, checkRadius) != true ||
                            ctx.BoxOnLineSide?.Invoke(boxX, boxY, checkRadius, ld.LineIndex) != -1)
                            continue;

                        if ((thisgroup & Lower) == 0)
                        {
                            for (var s = 0; s < 2; s++)
                            {
                                var sideSec = s == 1 ? ld.BackSector : ld.FrontSector;
                                if (sideSec is not null && ctx.PortalBlocksMovementCeiling?.Invoke(sideSec) == false)
                                {
                                    if ((ctx.PortalPlaneZCeiling?.Invoke(sideSec) ?? double.MaxValue) < upperZ)
                                    {
                                        var grp = ctx.OppositePortalGroupCeiling?.Invoke(sideSec) ?? 0;
                                        if (!processMask.GetBit(grp))
                                        {
                                            processMask.SetBit(grp);
                                            groupsToCheck.Add(grp | Upper);
                                        }
                                    }
                                }
                            }
                        }

                        if ((thisgroup & Upper) == 0)
                        {
                            for (var s = 0; s < 2; s++)
                            {
                                var sideSec = s == 1 ? ld.BackSector : ld.FrontSector;
                                if (sideSec is not null && ctx.PortalBlocksMovementFloor?.Invoke(sideSec) == false)
                                {
                                    if ((ctx.PortalPlaneZFloor?.Invoke(sideSec) ?? double.MinValue) > posZ)
                                    {
                                        var grp = ctx.OppositePortalGroupFloor?.Invoke(sideSec) ?? 0;
                                        if (!processMask.GetBit(grp))
                                        {
                                            processMask.SetBit(grp);
                                            groupsToCheck.Add(grp | Lower);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return retval;
    }
}
