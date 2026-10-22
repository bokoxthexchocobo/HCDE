namespace HCDE.Playsim;

/// <summary><c>DScroller</c> and BOOM scroll specials from <c>mapthinkers/a_scroll.cpp</c>.</summary>
public static class VmAScroll
{
    public static int ScrollPosAnd(int one, int two) => one & two;

    public static void RotationComp(double sectorAngle, double dx, double dy, out double tdx, out double tdy)
    {
        if (sectorAngle == 0)
        {
            tdx = dx;
            tdy = dy;
            return;
        }

        var ca = -Math.Cos(sectorAngle);
        var sa = -Math.Sin(sectorAngle);
        tdx = dx * ca - dy * sa;
        tdy = dy * ca + dx * sa;
    }

    public sealed class ScrollState
    {
        public int Type { get; set; }
        public double Dx { get; set; }
        public double Dy { get; set; }
        public double Vdx { get; set; }
        public double Vdy { get; set; }
        public double LastHeight { get; set; }
        public int Accel { get; set; }
        public int Parts { get; set; }
        public int Affect { get; set; }
    }

    public static bool TickStep(
        ScrollState state,
        object? controller,
        Func<object?, double> centerHeight,
        Func<double, double, double, double, (double tdx, double tdy)> rotationComp,
        Action<double, double> scrollSideTop,
        Action<double, double> scrollSideMid,
        Action<double, double> scrollSideBottom,
        Action<double, double> scrollFloor,
        Action<double, double> scrollCeiling,
        Action<double, double> addSectorScroll,
        Func<object?, bool> markActorInScroll)
    {
        var dx = state.Dx;
        var dy = state.Dy;
        if (controller != null)
        {
            var height = centerHeight(controller);
            var delta = height - state.LastHeight;
            state.LastHeight = height;
            dx *= delta;
            dy *= delta;
        }

        if (state.Accel != 0)
        {
            state.Vdx = dx += state.Vdx;
            state.Vdy = dy += state.Vdy;
        }

        if (dx == 0 && dy == 0)
            return false;

        switch (state.Type)
        {
            case 1: // sc_side
                if ((state.Parts & 1) != 0)
                    scrollSideTop(dx, dy);
                if ((state.Parts & 2) != 0)
                    scrollSideMid(dx, dy);
                if ((state.Parts & 4) != 0)
                    scrollSideBottom(dx, dy);
                break;
            case 2: // sc_floor
                var floor = rotationComp(0, dx, dy, 0);
                scrollFloor(floor.tdx, floor.tdy);
                break;
            case 3: // sc_ceiling
                var ceil = rotationComp(0, dx, dy, 0);
                scrollCeiling(ceil.tdx, ceil.tdy);
                break;
            case 4: // sc_carry
                addSectorScroll(dx, dy);
                break;
        }

        return true;
    }

    public static void InitSectorScroller(ScrollState state, object? controller, Func<object?, double> centerHeight)
    {
        state.LastHeight = 0;
        if (controller != null)
            state.LastHeight = centerHeight(controller);
    }

    public static void ClearInterpolations(Action delRef, object?[] interpolations)
    {
        for (var i = 0; i < interpolations.Length; i++)
        {
            if (interpolations[i] != null)
            {
                delRef();
                interpolations[i] = null;
            }
        }
    }

    public static void SetWallScrollerRemove(
        IEnumerable<object> scrollers,
        Func<object, object?> getWall,
        Func<object, int> lineId,
        int id,
        int sideChoice,
        int where,
        Action<object> destroy)
    {
        foreach (var scroller in scrollers)
        {
            var wall = getWall(scroller);
            if (wall == null)
                continue;
            if (lineId(wall) == id && where == 0)
                destroy(scroller);
        }
    }

    public static void SetScrollerUpdateRates(IEnumerable<object> scrollers, int tag, int type, double dx, double dy, Func<object, int> sectorTag, Action<double, double> setRate)
    {
        foreach (var scroller in scrollers)
        {
            if (sectorTag(scroller) == tag)
                setRate(dx, dy);
        }
    }
}
