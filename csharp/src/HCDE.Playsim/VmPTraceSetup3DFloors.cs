namespace HCDE.Playsim;

/// <summary><c>FTraceInfo::Setup3DFloors</c> from <c>p_trace.cpp</c> (279–365).</summary>
public static class VmPTraceSetup3DFloors
{
    public const int FfExists = 1;
    public const int FfShootThrough = 2;
    public const int FfInvertPlanes = 4;

    public sealed class FFloorSlice
    {
        public int Flags { get; set; }
        public double BottomZ { get; set; }
        public double TopZ { get; set; }
        public bool ModelHealth3D { get; set; }
        public bool Liquid { get; set; }
    }

    public sealed class Setup3DFloorsContext
    {
        public IReadOnlyList<FFloorSlice> Floors { get; set; } = Array.Empty<FFloorSlice>();
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double BaseFloorZ { get; set; }
        public double BaseCeilingZ { get; set; }
        public double HitPosZ { get; set; }
        public bool UseDummySector { get; set; }
        public double ClampedFloorZ { get; set; }
        public double ClampedCeilingZ { get; set; }
        public bool InShootThrough { get; set; } = true;
        public object? Crossed3DWater { get; set; }
        public double Crossed3DWaterDistance { get; set; }
    }

    public static void Setup3DFloors(Setup3DFloorsContext ctx)
    {
        if (ctx.Floors.Count == 0)
            return;

        ctx.UseDummySector = true;
        var bf = ctx.BaseFloorZ;
        var bc = ctx.BaseCeilingZ;
        var posZ = ctx.PosZ;

        foreach (var rover in ctx.Floors)
        {
            if ((rover.Flags & FfExists) == 0)
                continue;

            if (ctx.Crossed3DWater is null && rover.Liquid && posZ > rover.BottomZ)
            {
                if (rover.TopZ > bf)
                {
                    ctx.Crossed3DWater = rover;
                    ctx.Crossed3DWaterDistance = 0;
                }
            }

            if ((rover.Flags & FfShootThrough) != 0)
                continue;

            var ffBottom = rover.BottomZ;
            var ffTop = rover.TopZ;
            if (posZ > ffTop)
            {
                if (bf < ffTop)
                    bf = ffTop;
            }
            else if (posZ < ffBottom)
            {
                if (bc > ffBottom)
                    bc = ffBottom;
            }
            else
            {
                if (bf < ffBottom)
                    bf = ffBottom;
                if (bc > ffTop)
                    bc = ffTop;
                ctx.InShootThrough = false;
            }
        }

        ctx.ClampedFloorZ = bf;
        ctx.ClampedCeilingZ = bc;
    }
}
