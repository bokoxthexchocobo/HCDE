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

        public static InterpolatorListSerializeFields SerializeInterpolatorList(InterpolatorList list, bool beginObject) =>
            beginObject ? new InterpolatorListSerializeFields(list.Head) : new InterpolatorListSerializeFields(list.Head);
    }

    public readonly record struct InterpolatorListSerializeFields(InterpolationNode? Head);

    public readonly record struct DInterpolationSerializeFields(
        int RefCount,
        InterpolationNode? Next,
        InterpolationNode? Prev,
        object? Level);

    public static DInterpolationSerializeFields SerializeDInterpolation(
        int refcount,
        InterpolationNode? next,
        InterpolationNode? prev,
        object? level) => new(refcount, next, prev, level);

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

    public const int SectorPlaneSlotCeilingMove = 0;
    public const int SectorPlaneSlotFloorMove = 1;

    public sealed class SectorPlaneInterp
    {
        public bool Ceiling { get; init; }
        public object? Sector { get; private set; }
        public IList<object> Attached { get; } = new List<object>();
        public double OldHeight { get; set; }
        public double OldTexZ { get; set; }
        public double BakHeight { get; set; }
        public double BakTexZ { get; set; }

        public void FinishConstruction(
            object sector,
            bool attach,
            Func<double> floorD,
            Func<double> ceilingD,
            Func<double> floorTex,
            Func<double> ceilingTex,
            Action<IList<object>, object, bool> startMidtexAttached,
            Action<IList<object>, object, bool> startLinkedSectorAttached,
            Action<InterpolationNode> registerOnLevel)
        {
            Sector = sector;
            UpdateInterpolation(() => Ceiling, floorD, ceilingD, floorTex, ceilingTex);
            if (attach)
            {
                startMidtexAttached(Attached, sector, Ceiling);
                startLinkedSectorAttached(Attached, sector, Ceiling);
            }

            registerOnLevel(new InterpolationNode());
        }

        public void UnlinkPlaneFromMap(
            Action<int, object?> clearInterpolationSlot,
            Action<object> delRefAttached,
            Action superUnlinkFromMap)
        {
            if (Sector != null)
            {
                clearInterpolationSlot(Ceiling ? SectorPlaneSlotCeilingMove : SectorPlaneSlotFloorMove, null);
                Sector = null;
            }

            for (var i = 0; i < Attached.Count; i++)
                delRefAttached(Attached[i]);
            Attached.Clear();
            superUnlinkFromMap();
        }

        public static SectorPlaneSerializeFields CaptureSerializeFields(
            object? sector,
            bool ceiling,
            double oldHeight,
            double oldTexZ,
            IList<object> attached) =>
            new(sector, ceiling, oldHeight, oldTexZ, attached);

        public static int PropagateMarkAttached(IList<object> attached, Action<object> mark, Func<int> superPropagateMark)
        {
            for (var i = 0; i < attached.Count; i++)
                mark(attached[i]);
            return superPropagateMark();
        }

        public readonly record struct SectorPlaneSerializeFields(
            object? Sector,
            bool Ceiling,
            double OldHeight,
            double OldTexZ,
            IList<object> Attached);

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
