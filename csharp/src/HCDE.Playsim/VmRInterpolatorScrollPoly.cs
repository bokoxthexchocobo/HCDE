namespace HCDE.Playsim;

/// <summary>Scroll, wall, polyobj interpolation and <c>SetInterpolation</c> hooks from <c>r_interpolate.cpp</c> (547–970).</summary>
public static class VmRInterpolatorScrollPoly
{
    public const int SectorCeilingMove = 0;
    public const int SectorFloorMove = 1;
    public const int SectorCeilingScroll = 2;
    public const int SectorFloorScroll = 3;

    public static bool ShouldUnlinkScroll(int refcount, double oldX, double oldY, double bakX, double bakY) =>
        refcount == 0 && oldX == bakX && oldY == bakY;

    public static double LerpComponent(double oldV, double bakV, double smoothratio) =>
        oldV + (bakV - oldV) * smoothratio;

    public sealed class ScrollInterp2D
    {
        public double OldX { get; set; }
        public double OldY { get; set; }
        public double BakX { get; set; }
        public double BakY { get; set; }

        public void UpdateInterpolation(Func<double> readX, Func<double> readY)
        {
            OldX = readX();
            OldY = readY();
        }

        public void Restore(Action<double> setX, Action<double> setY)
        {
            setX(BakX);
            setY(BakY);
        }

        public bool InterpolateStep(
            int refcount,
            double smoothratio,
            Func<double> readX,
            Func<double> readY,
            Action<double> setX,
            Action<double> setY,
            Action unlinkDestroy)
        {
            BakX = readX();
            BakY = readY();
            if (ShouldUnlinkScroll(refcount, OldX, OldY, BakX, BakY))
            {
                unlinkDestroy();
                return true;
            }

            setX(LerpComponent(OldX, BakX, smoothratio));
            setY(LerpComponent(OldY, BakY, smoothratio));
            return false;
        }
    }

    public static void UnlinkSectorScrollSlot(bool ceiling, Action clearCeilingScroll, Action clearFloorScroll, Action clearSector)
    {
        if (ceiling)
            clearCeilingScroll();
        else
            clearFloorScroll();
        clearSector();
    }

    public static void UnlinkWallScrollSlot(Action clearSideInterpolation)
    {
        clearSideInterpolation();
    }

    public enum SectorInterpolationKind
    {
        PlaneCeiling,
        PlaneFloor,
        ScrollCeiling,
        ScrollFloor
    }

    public sealed class PolyobjInterp
    {
        public double[] OldVerts { get; private set; } = Array.Empty<double>();
        public double[] BakVerts { get; private set; } = Array.Empty<double>();
        public double OldCx { get; set; }
        public double OldCy { get; set; }
        public double BakCx { get; set; }
        public double BakCy { get; set; }

        public void ResizeForVertexCount(int vertexCount)
        {
            var size = vertexCount * 2;
            if (OldVerts.Length == size)
                return;
            var old = new double[size];
            var bak = new double[size];
            Array.Copy(OldVerts, old, Math.Min(OldVerts.Length, size));
            Array.Copy(BakVerts, bak, Math.Min(BakVerts.Length, size));
            OldVerts = old;
            BakVerts = bak;
        }

        public void UpdateInterpolation(Func<int, double> readX, Func<int, double> readY, int vertexCount, Func<double> readCx, Func<double> readCy)
        {
            ResizeForVertexCount(vertexCount);
            for (var i = 0; i < vertexCount; i++)
            {
                OldVerts[i * 2] = readX(i);
                OldVerts[i * 2 + 1] = readY(i);
            }

            OldCx = readCx();
            OldCy = readCy();
        }

        public void Restore(Action<int, double, double> setVertex, Action<double, double> setCenter, Action clearSubsectorLinks)
        {
            var vertexCount = OldVerts.Length / 2;
            for (var i = 0; i < vertexCount; i++)
                setVertex(i, BakVerts[i * 2], BakVerts[i * 2 + 1]);
            setCenter(BakCx, BakCy);
            clearSubsectorLinks();
        }

        public bool InterpolateStep(
            int refcount,
            double smoothratio,
            Func<int, double> readX,
            Func<int, double> readY,
            int vertexCount,
            Action<int, double, double> setVertex,
            Func<double> readCx,
            Func<double> readCy,
            Action<double, double> setCenter,
            Action clearSubsectorLinks,
            Action unlinkDestroy)
        {
            ResizeForVertexCount(vertexCount);
            var changed = false;
            for (var i = 0; i < vertexCount; i++)
            {
                BakVerts[i * 2] = readX(i);
                BakVerts[i * 2 + 1] = readY(i);
                if (BakVerts[i * 2] != OldVerts[i * 2] || BakVerts[i * 2 + 1] != OldVerts[i * 2 + 1])
                {
                    changed = true;
                    setVertex(
                        i,
                        LerpComponent(OldVerts[i * 2], BakVerts[i * 2], smoothratio),
                        LerpComponent(OldVerts[i * 2 + 1], BakVerts[i * 2 + 1], smoothratio));
                }
            }

            if (refcount == 0 && !changed)
            {
                unlinkDestroy();
                return true;
            }

            BakCx = readCx();
            BakCy = readCy();
            setCenter(
                BakCx + (BakCx - OldCx) * smoothratio,
                BakCy + (BakCy - OldCy) * smoothratio);
            clearSubsectorLinks();
            return false;
        }
    }

    public static object? GetOrCreateSideInterpolation(
        object? existing,
        Func<object> create,
        Action<object> writeBarrier,
        Action<object> addRef)
    {
        if (existing == null)
            existing = create();
        addRef(existing);
        writeBarrier(existing);
        return existing;
    }

    public static void StopSideInterpolation(object? existing, Action<object> delRef)
    {
        if (existing != null)
            delRef(existing);
    }

    public static object? GetOrCreateSectorInterpolation(
        int position,
        object? existing,
        Func<int, object> createForPosition,
        Action<object> writeBarrier,
        Action<object> addRef)
    {
        if (existing == null)
        {
            existing = createForPosition(position);
            if (existing == null)
                return null;
        }

        addRef(existing);
        writeBarrier(existing);
        return existing;
    }

    public static SectorInterpolationKind? MapSectorInterpolationPosition(int position) =>
        position switch
        {
            SectorCeilingMove => SectorInterpolationKind.PlaneCeiling,
            SectorFloorMove => SectorInterpolationKind.PlaneFloor,
            SectorCeilingScroll => SectorInterpolationKind.ScrollCeiling,
            SectorFloorScroll => SectorInterpolationKind.ScrollFloor,
            _ => null
        };

    public static object? GetOrCreatePolyInterpolation(
        object? existing,
        Func<object> create,
        Action<object> writeBarrier,
        Action<object> addRef)
    {
        if (existing != null)
        {
            addRef(existing);
        }
        else
        {
            existing = create();
            addRef(existing);
        }

        writeBarrier(existing);
        return existing;
    }

    public static void StopPolyInterpolation(object? existing, Action<object> delRef)
    {
        if (existing != null)
            delRef(existing);
    }
}
