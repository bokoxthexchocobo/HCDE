namespace HCDE.Playsim;

/// <summary>
/// <c>UpdateWaterDepth</c> and <c>SplashCheck</c> from <c>p_mobj.cpp</c> (**5277–5426**). Chunk **1114**.
/// </summary>
public static class VmPMobjWaterDepth1114Slice
{
    public const double NoSectorFloor = double.NegativeInfinity;
    public const int SecUnderwater = 1;
    public const int SecUnderwaterMask = 2;
    public const int SecFakeFloorOnly = 4;
    public const int FfExists = 1;
    public const int FfSolid = 2;
    public const int FfSwimmable = 4;

    public sealed class FloorRover
    {
        public int Flags { get; init; }
        public int Alpha { get; init; }
        public double Bottom { get; init; }
        public double Top { get; init; }
    }

    public sealed class WaterDepthContext
    {
        public bool HasSector { get; init; }
        public int SectorMoreFlags { get; init; }
        public bool HasHeightSector { get; init; }
        public int HeightSectorMoreFlags { get; init; }
        public double HeightSectorFloor { get; init; }
        public double HeightSectorCeiling { get; init; }
        public double ActorZ { get; init; }
        public double ActorTop { get; init; }
        public double ActorCenter { get; init; }
        public double Height { get; init; }
        public bool Splash { get; init; }
        public bool HasPlayer { get; init; }
        public double ViewHeight { get; init; }
        public IReadOnlyList<FloorRover> Rovers { get; init; } = Array.Empty<FloorRover>();
        public double FloorHeight { get; set; } = NoSectorFloor;
        public double WaterDepth { get; set; }
        public int WaterLevel { get; set; }
    }

    public static double RunUpdateWaterDepth(WaterDepthContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ctx.WaterLevel = 0;
        ctx.WaterDepth = 0;
        ctx.FloorHeight = NoSectorFloor;
        if (!ctx.HasSector)
            return ctx.FloorHeight;

        if ((ctx.SectorMoreFlags & SecUnderwater) != 0)
        {
            ctx.WaterDepth = ctx.Height;
        }
        else if (ctx.HasHeightSector)
        {
            ctx.FloorHeight = ctx.HeightSectorFloor;
            if (ctx.Splash || (ctx.HeightSectorMoreFlags & SecUnderwaterMask) != 0)
            {
                ctx.WaterDepth = ctx.HeightSectorFloor - ctx.ActorZ;
                if (ctx.WaterDepth <= 0
                    && (ctx.HeightSectorMoreFlags & SecFakeFloorOnly) == 0
                    && ctx.ActorTop > ctx.HeightSectorCeiling)
                {
                    ctx.WaterDepth = ctx.Height;
                }
            }
        }
        else
        {
            foreach (var rover in ctx.Rovers)
            {
                if ((rover.Flags & FfExists) == 0 || (rover.Flags & FfSolid) != 0)
                    continue;

                var skip = (rover.Flags & FfSwimmable) == 0;
                if (ctx.Splash)
                    skip &= rover.Alpha == 0;
                if (skip)
                    continue;
                if (rover.Top <= ctx.ActorZ || rover.Bottom > ctx.ActorCenter)
                    continue;

                ctx.FloorHeight = rover.Top;
                ctx.WaterDepth = rover.Top - ctx.ActorZ;
                break;
            }
        }

        if (ctx.WaterDepth < 0)
            ctx.WaterDepth = 0;

        if (ctx.WaterDepth > ctx.Height / 2)
        {
            ctx.WaterLevel = (ctx.HasPlayer && ctx.WaterDepth >= ctx.ViewHeight) || ctx.WaterDepth >= ctx.Height
                ? 3
                : 2;
        }
        else if (ctx.WaterDepth > 0)
        {
            ctx.WaterLevel = 1;
        }

        return ctx.FloorHeight;
    }

    public sealed class SplashCheckContext
    {
        public int BoomWaterLevel { get; set; }
        public int WaterLevel { get; set; }
        public bool HitWater { get; set; }
    }

    public static void RunSplashCheck(SplashCheckContext ctx, double floorHeight)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.BoomWaterLevel == 0 && ctx.WaterLevel != 0)
            ctx.HitWater = true;
        ctx.BoomWaterLevel = ctx.WaterLevel;
        _ = floorHeight;
    }
}
