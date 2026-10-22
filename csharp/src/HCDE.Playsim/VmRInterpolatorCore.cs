namespace HCDE.Playsim;

/// <summary><c>FInterpolator</c>, <c>DInterpolation</c> refcount, and <c>DSectorPlaneInterpolation</c> from <c>r_interpolate.cpp</c> (170–533).</summary>
public static class VmRInterpolatorCore
{
    public sealed class InterpolationNode
    {
        public InterpolationNode? Next { get; set; }
        public InterpolationNode? Prev { get; set; }
        public int RefCount { get; set; }
        public Action<double>? Interpolate { get; set; }
        public Action? Restore { get; set; }
        public Action? Update { get; set; }
        public Action? Unlink { get; set; }
        public Action? Destroy { get; set; }
    }

    public sealed class InterpolatorList
    {
        public InterpolationNode? Head { get; set; }
        public bool DidInterp { get; set; }

        public int CountInterpolations()
        {
            var count = 0;
            for (var probe = Head; probe != null; probe = probe.Next)
                count++;
            return count;
        }

        public void UpdateInterpolations()
        {
            for (var probe = Head; probe != null; probe = probe.Next)
                probe.Update?.Invoke();
        }

        public void AddInterpolation(InterpolationNode interp)
        {
            interp.Next = Head;
            if (Head != null)
                Head.Prev = interp;
            interp.Prev = null;
            Head = interp;
        }

        public void RemoveInterpolation(InterpolationNode interp)
        {
            if (Head == interp)
            {
                Head = interp.Next;
                if (Head != null)
                    Head.Prev = null;
            }
            else
            {
                if (interp.Prev != null)
                    interp.Prev.Next = interp.Next;
                if (interp.Next != null)
                    interp.Next.Prev = interp.Prev;
            }

            interp.Next = null;
            interp.Prev = null;
        }

        public void DoInterpolations(double smoothratio)
        {
            if (smoothratio >= 1.0)
            {
                DidInterp = false;
                return;
            }

            DidInterp = true;
            var probe = Head;
            while (probe != null)
            {
                var next = probe.Next;
                probe.Interpolate?.Invoke(smoothratio);
                probe = next;
            }
        }

        public void RestoreInterpolations()
        {
            if (!DidInterp)
                return;
            DidInterp = false;
            for (var probe = Head; probe != null; probe = probe.Next)
                probe.Restore?.Invoke();
        }

        public void ClearInterpolations()
        {
            var probe = Head;
            Head = null;
            while (probe != null)
            {
                var next = probe.Next;
                probe.Next = probe.Prev = null;
                probe.Unlink?.Invoke();
                probe.Destroy?.Invoke();
                probe = next;
            }
        }
    }

    public static int AddRef(int refcount) => refcount + 1;

    public static int DelRef(int refcount, bool force, Action unlinkDestroy)
    {
        if (refcount > 0)
            refcount--;
        if (force && refcount == 0)
            unlinkDestroy();
        return refcount;
    }

    public static bool ShouldUnlinkOnPlaneMatch(int refcount, double oldHeight, double currentHeight) =>
        refcount == 0 && oldHeight == currentHeight;

    public static double LerpHeight(double oldHeight, double currentHeight, double smoothratio) =>
        oldHeight + (currentHeight - oldHeight) * smoothratio;

    public sealed class SectorPlaneInterp
    {
        public bool Ceiling { get; init; }
        public double OldHeight { get; set; }
        public double OldTexZ { get; set; }
        public double BakHeight { get; set; }
        public double BakTexZ { get; set; }

        public void UpdateInterpolation(Func<bool> isCeiling, Func<double> floorD, Func<double> ceilingD, Func<double> floorTex, Func<double> ceilingTex)
        {
            if (!isCeiling())
            {
                OldHeight = floorD();
                OldTexZ = floorTex();
            }
            else
            {
                OldHeight = ceilingD();
                OldTexZ = ceilingTex();
            }
        }

        public void RestorePlane(Action<double> setFloorD, Action<double> setCeilingD, Action<double, bool> setFloorTex, Action<double, bool> setCeilingTex)
        {
            if (!Ceiling)
            {
                setFloorD(BakHeight);
                setFloorTex(BakTexZ, true);
            }
            else
            {
                setCeilingD(BakHeight);
                setCeilingTex(BakTexZ, true);
            }
        }

        public bool InterpolateStep(
            int refcount,
            double smoothratio,
            Func<double> readD,
            Action<double> writeD,
            Func<double> readTex,
            Action<double, bool> writeTex,
            Action unlinkDestroy)
        {
            BakHeight = readD();
            BakTexZ = readTex();
            if (ShouldUnlinkOnPlaneMatch(refcount, OldHeight, BakHeight))
            {
                unlinkDestroy();
                return true;
            }

            writeD(LerpHeight(OldHeight, BakHeight, smoothratio));
            writeTex(OldTexZ + (BakTexZ - OldTexZ) * smoothratio, true);
            return false;
        }
    }
}
