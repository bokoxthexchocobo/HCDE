namespace HCDE.Playsim;

/// <summary><c>DScroller</c> and BOOM scroll specials from <c>mapthinkers/a_scroll.cpp</c>.</summary>
public static class VmAScroll
{
    public const int ScSide = 1;
    public const int ScFloor = 2;
    public const int ScCeiling = 3;
    public const int ScCarry = 4;
    public const int ScCarryCeiling = 5;
    public const int ScwTop = 1;
    public const int ScwMid = 2;
    public const int ScwBottom = 4;
    public const int ScwAll = ScwTop | ScwMid | ScwBottom;
    public const int ScrollPlayers = 1;
    public const int ScrollMonsters = 2;
    public const int ScrollStaticObjects = 4;
    public const int ScrollAll = ScrollPlayers | ScrollMonsters | ScrollStaticObjects;
    public const int Ml3dmidtex = 1 << 10;
    public const int MfNoclip = 1 << 12;
    public const int MfSpawnceiling = 1 << 8;
    public const int MfNogravity = 1 << 9;
    public const int Mf3Ismonster = 1 << 4;
    public const int Mf8Inscrollsec = 1;

    public static int ScrollPosAnd(int one, int two) => one & two;

    public static int NormalizeWallScrollWhere(int where) => ScrollPosAnd(where, ScwAll);

    public static int ApplySerializeAffectDefault(bool reading, int affect) =>
        reading && affect == 0 ? ScrollAll : affect;

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

    public static bool ShouldScrollMidTexture(bool backsectorNull, int lineFlags) =>
        backsectorNull || (lineFlags & Ml3dmidtex) == 0;

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
        public int Affect { get; set; } = ScrollAll;
    }

    public readonly record struct TouchingThing(bool Player, bool Monster, bool StaticObject);

    public static bool CarryAffectsActor(TouchingThing thing, int affect)
    {
        if (thing.Player)
            return (affect & ScrollPlayers) != 0;
        if (thing.Monster)
            return (affect & ScrollMonsters) != 0;
        return (affect & ScrollStaticObjects) != 0;
    }

    public static bool CeilingCarryEligible(int flags, int flags3, bool topOnCeilingPlane) =>
        (flags & MfNoclip) == 0
        && (flags & MfSpawnceiling) != 0
        && (flags & MfNogravity) != 0
        && topOnCeilingPlane;

    public static (double dx, double dy) ApplyControllerDelta(
        ScrollState state,
        object? controller,
        Func<object?, double> centerHeight)
    {
        var dx = state.Dx;
        var dy = state.Dy;
        if (controller == null)
            return (dx, dy);
        var height = centerHeight(controller);
        var delta = height - state.LastHeight;
        state.LastHeight = height;
        return (dx * delta, dy * delta);
    }

    public static (double dx, double dy) ApplyAcceleration(ScrollState state, double dx, double dy)
    {
        if (state.Accel == 0)
            return (dx, dy);
        state.Vdx = dx += state.Vdx;
        state.Vdy = dy += state.Vdy;
        return (dx, dy);
    }

    public static bool RunScrollerTick(
        ScrollState state,
        object? controller,
        Func<object?, double> centerHeight,
        Func<double, double, double, double, (double tdx, double tdy)> rotationComp,
        bool backsectorNull,
        int lineFlags,
        Action<double, double> scrollSideTop,
        Action<double, double> scrollSideMid,
        Action<double, double> scrollSideBottom,
        Action<double, double> scrollFloor,
        Action<double, double> scrollCeiling,
        Action<double, double> addSectorScroll,
        IEnumerable<TouchingThing> touchingThings,
        Func<TouchingThing, int, int, int, bool> ceilingCarryCandidate,
        Action<TouchingThing, double, double>? setCeilingCarryVel,
        Action<TouchingThing>? markInScrollSec)
    {
        var (dx, dy) = ApplyControllerDelta(state, controller, centerHeight);
        (dx, dy) = ApplyAcceleration(state, dx, dy);
        if (dx == 0 && dy == 0)
            return false;

        switch (state.Type)
        {
            case ScSide:
                if ((state.Parts & ScwTop) != 0)
                    scrollSideTop(dx, dy);
                if ((state.Parts & ScwMid) != 0 && ShouldScrollMidTexture(backsectorNull, lineFlags))
                    scrollSideMid(dx, dy);
                if ((state.Parts & ScwBottom) != 0)
                    scrollSideBottom(dx, dy);
                break;
            case ScFloor:
                var floor = rotationComp(0, dx, dy, 0);
                scrollFloor(floor.tdx, floor.tdy);
                break;
            case ScCeiling:
                var ceil = rotationComp(0, dx, dy, 0);
                scrollCeiling(ceil.tdx, ceil.tdy);
                break;
            case ScCarry:
                addSectorScroll(dx, dy);
                foreach (var thing in touchingThings)
                {
                    if (!CarryAffectsActor(thing, state.Affect))
                        continue;
                    markInScrollSec?.Invoke(thing);
                }

                break;
            case ScCarryCeiling:
                foreach (var thing in touchingThings)
                {
                    if (!ceilingCarryCandidate(thing, state.Affect, ScrollPlayers, ScrollMonsters))
                        continue;
                    if (!CarryAffectsActor(thing, state.Affect))
                        continue;
                    setCeilingCarryVel?.Invoke(thing, state.Dx, state.Dy);
                }

                break;
        }

        return true;
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
        var (dx, dy) = ApplyControllerDelta(state, controller, centerHeight);
        (dx, dy) = ApplyAcceleration(state, dx, dy);
        if (dx == 0 && dy == 0)
            return false;

        switch (state.Type)
        {
            case ScSide:
                if ((state.Parts & ScwTop) != 0)
                    scrollSideTop(dx, dy);
                if ((state.Parts & ScwMid) != 0)
                    scrollSideMid(dx, dy);
                if ((state.Parts & ScwBottom) != 0)
                    scrollSideBottom(dx, dy);
                break;
            case ScFloor:
                var floor = rotationComp(0, dx, dy, 0);
                scrollFloor(floor.tdx, floor.tdy);
                break;
            case ScCeiling:
                var ceil = rotationComp(0, dx, dy, 0);
                scrollCeiling(ceil.tdx, ceil.tdy);
                break;
            case ScCarry:
                addSectorScroll(dx, dy);
                markActorInScroll?.Invoke(null);
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

    public static void ConstructSectorScroller(
        ScrollState state,
        int type,
        double dx,
        double dy,
        int accel,
        int parts,
        int affect,
        object? controller,
        Func<object?, double> centerHeight,
        Action<int>? registerSectorScroller,
        Action? wallNoAutodecals,
        Action<int>? setInterpolation)
    {
        state.Type = type;
        state.Dx = dx;
        state.Dy = dy;
        state.Accel = accel;
        state.Parts = parts;
        state.Vdx = 0;
        state.Vdy = 0;
        state.Affect = affect;
        InitSectorScroller(state, controller, centerHeight);
        switch (type)
        {
            case ScCarry:
                registerSectorScroller?.Invoke(0);
                break;
            case ScSide:
                wallNoAutodecals?.Invoke();
                if ((parts & ScwTop) != 0)
                    setInterpolation?.Invoke(0);
                if ((parts & ScwMid) != 0)
                    setInterpolation?.Invoke(1);
                if ((parts & ScwBottom) != 0)
                    setInterpolation?.Invoke(2);
                break;
            case ScFloor:
                setInterpolation?.Invoke(0);
                break;
            case ScCeiling:
                setInterpolation?.Invoke(0);
                break;
        }
    }

    public static void OnDestroyScroller(Action delRef, object?[] interpolations) =>
        ClearInterpolations(delRef, interpolations);

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

    public static void SetWallScroller(
        double dx,
        double dy,
        int where,
        int id,
        int sideChoice,
        IEnumerable<object> scrollers,
        Func<object, object?> getWall,
        Func<object, int> lineId,
        Func<object, int> scrollParts,
        Func<object, object?> getWallFromScroller,
        Action<object, double, double> setRate,
        Action<double, double, int, int, object?> createScroller,
        Func<object, object, bool> wallMatches)
    {
        where = NormalizeWallScrollWhere(where);
        if (where == 0)
            return;

        if (dx == 0 && dy == 0)
        {
            foreach (var scroller in scrollers)
            {
                var wall = getWallFromScroller(scroller);
                if (wall != null && lineId(wall) == id && scrollParts(scroller) == where)
                    setRate(scroller, 0, 0);
            }

            return;
        }

        var collected = new List<object>();
        foreach (var scroll in scrollers)
        {
            var wall = getWallFromScroller(scroll);
            if (wall == null)
                continue;
            if (lineId(wall) == id && scrollParts(scroll) == where)
            {
                setRate(scroll, dx, dy);
                collected.Add(scroll);
            }
        }

        foreach (var lineWall in EnumerateLineSides(id, sideChoice))
        {
            if (lineWall == null)
                continue;
            if (collected.Exists(s => wallMatches(getWallFromScroller(s)!, lineWall)))
                continue;
            createScroller(dx, dy, sideChoice, where, lineWall);
        }
    }

    public static IEnumerable<object?> EnumerateLineSides(int id, int sideChoice)
    {
        yield return null;
    }

    public static int SetScrollerPass(
        IEnumerable<object> scrollers,
        int tag,
        int type,
        double dx,
        double dy,
        Func<object, int> scrollerType,
        Func<object, int> sectorTag,
        Action<object, double, double> setRate,
        Action<int, int, double, double> createForSector)
    {
        var updated = 0;
        foreach (var scroller in scrollers)
        {
            if (scrollerType(scroller) != type)
                continue;
            if (sectorTag(scroller) != tag)
                continue;
            updated++;
            setRate(scroller, dx, dy);
        }

        if (updated > 0 || (dx == 0 && dy == 0))
            return updated;

        createForSector(tag, type, dx, dy);
        return updated;
    }

    public static void WallConstructRates(
        double dx,
        double dy,
        double deltaX,
        double deltaY,
        out double x,
        out double y)
    {
        var ax = Math.Abs(deltaX);
        var ay = Math.Abs(deltaY);
        var d = ax;
        if (ay > ax)
        {
            d = ay;
            ay = ax;
            ax = d;
        }

        d = ax / Math.Sin(Math.Atan2(ay, ax) + Math.PI / 2);
        x = -(dy * deltaY + dx * deltaX) / d;
        y = -(dx * deltaY - dy * deltaX) / d;
    }

    public readonly record struct ScrollSerializeFields(
        int Type,
        double Dx,
        double Dy,
        object? Sector,
        object? Side,
        object? Controller,
        double LastHeight,
        double Vdx,
        double Vdy,
        int Accel,
        int Affect,
        int Parts);
}
