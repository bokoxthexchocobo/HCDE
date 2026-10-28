namespace HCDE.Playsim;

/// <summary>Sight line policy from <c>p_sight.cpp</c> (156–199, 388–523).</summary>
public static class VmPSightLinePolicy
{
    public const int MlTwosided = 1;
    public const int MlBlocksight = 1 << 1;
    public const int MlBlockeverything = 1 << 2;
    public const int MlPortalconnect = 1 << 3;
    public const int SfSeepastblockeverything = 1;
    public const int SfSeepastshootablelines = 2;
    public const int SpacImpact = 1;
    public const int AcsExecute = 226;
    public const int AcsExecuteAlways = 227;
    public const double LineOpenMin = -3.402823466e38;
    public const double LineOpenMax = 3.402823466e38;

    public sealed class SightOpeningState
    {
        public int PortalFlags { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Range { get; set; }
    }

    public static void SightOpening(
        SightOpeningState open,
        bool oneSided,
        int lineFlags,
        bool frontCeilPortalOpen,
        bool frontFloorPortalOpen,
        bool backCeilPortalOpen,
        bool backFloorPortalOpen,
        double frontCeilZ,
        double frontFloorZ,
        double backCeilZ,
        double backFloorZ)
    {
        open.PortalFlags = 0;
        open.Bottom = open.Top = 0;

        if (oneSided)
        {
            if ((lineFlags & MlPortalconnect) != 0)
            {
                if (frontCeilPortalOpen)
                {
                    open.Top = LineOpenMax;
                    open.PortalFlags |= 1;
                }

                if (frontFloorPortalOpen)
                {
                    open.Bottom = LineOpenMin;
                    open.PortalFlags |= 2;
                }

                if (open.Top == 0) open.Top = frontFloorZ;
                if (open.Bottom == 0) open.Bottom = frontCeilZ;
            }

            open.Range = 0;
            return;
        }

        var fc = frontCeilPortalOpen ? LineOpenMax : frontCeilZ;
        var bc = backCeilPortalOpen ? LineOpenMax : backCeilZ;
        var ff = frontFloorPortalOpen ? LineOpenMin : frontFloorZ;
        var bf = backFloorPortalOpen ? LineOpenMin : backFloorZ;

        if ((lineFlags & MlPortalconnect) != 0)
        {
            if (frontCeilPortalOpen) open.PortalFlags |= 4;
            if (backCeilPortalOpen) open.PortalFlags |= 8;
            if (frontFloorPortalOpen) open.PortalFlags |= 16;
            if (backFloorPortalOpen) open.PortalFlags |= 32;
        }

        open.Bottom = Math.Max(ff, bf);
        open.Top = Math.Min(fc, bc);
        open.Range = open.Bottom < open.Top ? 1 : 0;
    }

    public static bool LineBlocksSight(
        bool hasBacksector,
        int lineFlags,
        int sightFlags,
        int activation,
        int special,
        int arg1,
        int levelNum)
    {
        if (!hasBacksector || (lineFlags & MlTwosided) == 0 || (lineFlags & MlBlocksight) != 0)
            return true;

        if ((lineFlags & MlBlockeverything) == 0)
            return false;

        if ((sightFlags & SfSeepastblockeverything) == 0)
            return true;

        if ((sightFlags & SfSeepastshootablelines) == 0)
        {
            if ((activation & SpacImpact) == 0)
                return true;
            if (special != AcsExecute && special != AcsExecuteAlways)
                return true;
            if (arg1 != 0 && arg1 != levelNum)
                return true;
        }

        return false;
    }

    public sealed class SightCheckLineContext
    {
        public int ValidCount { get; set; }
        public int LineValidCount { get; set; }
        public double TraceX { get; set; }
        public double TraceY { get; set; }
        public double TraceDx { get; set; }
        public double TraceDy { get; set; }
        public bool PortalFound { get; set; }
        public List<object> InterceptLines { get; } = new();

        public Func<double, double, int>? EndpointSideOfTrace { get; init; }
        public Func<double, double, int>? TraceSideOfDivline { get; init; }
        public Func<object, bool>? LineBlocksSight { get; init; }
    }

    public static bool SightCheckLine(SightCheckLineContext ctx, object line, double v1x, double v1y, double v2x, double v2y)
    {
        if (ctx.LineValidCount == ctx.ValidCount)
            return true;
        ctx.LineValidCount = ctx.ValidCount;

        var sideA = ctx.EndpointSideOfTrace?.Invoke(v1x, v1y) ?? 0;
        var sideB = ctx.EndpointSideOfTrace?.Invoke(v2x, v2y) ?? 0;
        if (sideA == sideB)
            return true;

        var dlSideA = ctx.TraceSideOfDivline?.Invoke(ctx.TraceX, ctx.TraceY) ?? 0;
        var dlSideB = ctx.TraceSideOfDivline?.Invoke(ctx.TraceX + ctx.TraceDx, ctx.TraceY + ctx.TraceDy) ?? 0;
        if (dlSideA == dlSideB)
            return true;

        if (!ctx.PortalFound && ctx.LineBlocksSight?.Invoke(line) == true)
            return false;

        ctx.InterceptLines.Add(line);
        return true;
    }

    public sealed class SightBlockLinesIteratorContext
    {
        public int ValidCount { get; set; }
        public bool PortalFound { get; set; }
        public int BlockWidth { get; set; }
        public int Result { get; set; } = 1;

        public Func<int, int, bool>? BlockHasLinkedPortals { get; init; }
        public Func<int, bool>? PolyBlockHasLinkedPortals { get; init; }
        public Func<int, IEnumerable<object>>? PolyLines { get; init; }
        public Func<int, IEnumerable<object>>? MapLines { get; init; }
        public Func<object, bool>? SightCheckLine { get; init; }
    }

    public static int SightBlockLinesIterator(SightBlockLinesIteratorContext ctx, int x, int y)
    {
        var offset = y * ctx.BlockWidth + x;
        ctx.PortalFound = ctx.PortalFound
            || ctx.BlockHasLinkedPortals?.Invoke(x, y) == true
            || ctx.PolyBlockHasLinkedPortals?.Invoke(offset) == true;

        foreach (var line in ctx.PolyLines?.Invoke(offset) ?? Array.Empty<object>())
        {
            if (ctx.SightCheckLine?.Invoke(line) == false)
            {
                if (!ctx.PortalFound) return 0;
                ctx.Result = -1;
            }
        }

        foreach (var line in ctx.MapLines?.Invoke(offset) ?? Array.Empty<object>())
        {
            if (ctx.SightCheckLine?.Invoke(line) == false)
            {
                if (!ctx.PortalFound) return 0;
                ctx.Result = -1;
            }
        }

        return ctx.Result;
    }
}
