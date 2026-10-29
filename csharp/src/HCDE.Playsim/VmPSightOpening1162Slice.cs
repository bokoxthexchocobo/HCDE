namespace HCDE.Playsim;

/// <summary>
/// Sight opening and the sight-check setup from <c>p_sight.cpp</c> (<b>1–209</b>).
/// Chunk <b>1162</b>.
/// </summary>
public static class VmPSightOpening1162Slice
{
    public const int TopFront = 1;
    public const int TopBack = 2;
    public const int BottomFront = 4;
    public const int BottomBack = 8;
    public const int PortalConnect = unchecked((int)0x80000000);
    public const int SeeThrough = 0x2000000;
    public const int InterceptCapacity = 128;
    public const int PortalCapacity = 32;
    public const string BotSightStream = "BotCheckSight";
    public const string SightStream = "CheckSight";
    public static readonly double LineOpenMin = -(double)float.MaxValue;
    public static readonly double LineOpenMax = (double)float.MaxValue;

    public readonly record struct ActorView(double X, double Y, double Z, double Height, int SectorGroup);

    public readonly record struct SightTask(
        double Frac,
        double TopSlope,
        double BottomSlope,
        int Direction,
        int PortalGroup);

    public readonly record struct Opening(double Top, double Bottom, int Range, int PortalFlags);

    public sealed class Sight
    {
        public double StartX;
        public double StartY;
        public double StartZ;
        public double EndX;
        public double EndY;
        public double TraceDx;
        public double TraceDy;
        public double StartFrac;
        public double LastZTop;
        public double LastZBottom;
        public int LastSector;
        public int TargetId;
        public double TopSlope;
        public double BottomSlope;
        public int Flags;
        public int PortalDir;
        public int PortalGroup;
        public bool PortalFound;
        public int SeeThroughFlags;
        public List<int> Intercepts { get; } = new(InterceptCapacity);
        public List<SightTask> Portals { get; } = new(PortalCapacity);
    }

    public static int SwapSides(int portalFlags)
    {
        int front = portalFlags & (TopFront | BottomFront);
        int back = portalFlags & (TopBack | BottomBack);
        return (front << 1) | (back >> 1);
    }

    public static Sight Begin(
        ActorView viewer,
        ActorView target,
        int targetId,
        int startSector,
        SightTask task,
        int flags,
        Func<int, int, (double X, double Y)>? offset)
    {
        var from = Shift(viewer.SectorGroup, task.PortalGroup, offset);
        var to = Shift(target.SectorGroup, task.PortalGroup, offset);
        var sight = new Sight
        {
            StartX = viewer.X + from.X,
            StartY = viewer.Y + from.Y,
            StartZ = viewer.Z + viewer.Height * 0.75d,
            EndX = target.X + to.X,
            EndY = target.Y + to.Y,
            StartFrac = task.Frac,
            LastSector = startSector,
            TargetId = targetId,
            TopSlope = task.TopSlope,
            BottomSlope = task.BottomSlope,
            Flags = flags,
            PortalDir = task.Direction,
            PortalGroup = task.PortalGroup,
            PortalFound = false,
            SeeThroughFlags = SeeThrough,
        };
        sight.TraceDx = sight.EndX - sight.StartX;
        sight.TraceDy = sight.EndY - sight.StartY;
        sight.LastZTop = sight.StartZ;
        sight.LastZBottom = sight.StartZ;
        return sight;
    }

    public static Opening Open(
        int lineFlags,
        bool hasBack,
        bool frontCeilingBlocks,
        bool frontFloorBlocks,
        bool backCeilingBlocks,
        bool backFloorBlocks,
        double frontCeilingZ,
        double frontFloorZ,
        double backCeilingZ,
        double backFloorZ)
    {
        int portalFlags = 0;
        double top = 0d;
        double bottom = 0d;
        bool portal = (lineFlags & PortalConnect) != 0;
        if (!hasBack)
        {
            if (portal)
            {
                if (!frontCeilingBlocks)
                {
                    top = LineOpenMax;
                    portalFlags |= TopFront;
                }

                if (!frontFloorBlocks)
                {
                    bottom = LineOpenMin;
                    portalFlags |= BottomFront;
                }

                if (top == 0d)
                    top = frontFloorZ;
                if (bottom == 0d)
                    bottom = frontCeilingZ;
            }

            return new Opening(top, bottom, 0, portalFlags);
        }

        double frontCeiling = 0d;
        double frontFloor = 0d;
        double backCeiling = 0d;
        double backFloor = 0d;
        if (portal)
        {
            if (!frontCeilingBlocks)
            {
                frontCeiling = LineOpenMax;
                portalFlags |= TopFront;
            }

            if (!backCeilingBlocks)
            {
                backCeiling = LineOpenMax;
                portalFlags |= TopBack;
            }

            if (!frontFloorBlocks)
            {
                frontFloor = LineOpenMin;
                portalFlags |= BottomFront;
            }

            if (!backFloorBlocks)
            {
                backFloor = LineOpenMin;
                portalFlags |= BottomBack;
            }
        }

        if (frontCeiling == 0d)
            frontCeiling = frontCeilingZ;
        if (backCeiling == 0d)
            backCeiling = backCeilingZ;
        if (frontFloor == 0d)
            frontFloor = frontFloorZ;
        if (backFloor == 0d)
            backFloor = backFloorZ;

        bottom = Math.Max(frontFloor, backFloor);
        top = Math.Min(frontCeiling, backCeiling);
        int range = 0;
        if (bottom < top)
            range = 1;
        return new Opening(top, bottom, range, portalFlags);
    }

    static (double X, double Y) Shift(int from, int to, Func<int, int, (double X, double Y)>? offset)
    {
        if (from == to)
            return (0d, 0d);
        if (offset == null)
            return (0d, 0d);
        return offset(from, to);
    }
}
