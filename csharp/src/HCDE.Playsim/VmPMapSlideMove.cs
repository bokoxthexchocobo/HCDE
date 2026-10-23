using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Faithful port of <c>FSlide::HitSlideLine</c>, <c>SlideTraverse</c>, <c>SlideMove</c> (<c>p_map.cpp</c> 3036–3366).</summary>
public static class VmPMapSlideMove
{
    public const int Mf8Nofrictionbounce = 1;
    public const int CfPredicting = 1;
    public const double OrigFriction = 1;

    public readonly record struct Vec2(double X, double Y)
    {
        public double LengthSquared() => X * X + Y * Y;
        public double Length() => Math.Sqrt(LengthSquared());
        public Vec2 Scale(double s) => new(X * s, Y * s);
        public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    }

    public sealed class SlideLine
    {
        public required string Id { get; init; }
        public double DeltaX { get; init; }
        public double DeltaY { get; init; }
    }

    public sealed class LineOpening
    {
        public double Range { get; set; } = 999;
        public double Top { get; set; } = 999;
        public double Bottom { get; set; } = -999;
    }

    public sealed class Intercept
    {
        public SlideLine? Line { get; init; }
        public double Frac { get; init; }
        public double HitX { get; init; }
        public double HitY { get; init; }
    }

    public sealed class SlideActor
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Top { get; set; }
        public double Radius { get; set; }
        public double Height { get; set; }
        public double MaxStepHeight { get; set; }
        public double FloorZ { get; set; }
        public int Flags8 { get; set; }
        public bool HasPlayer { get; set; }
        public bool PlayerIsMo { get; set; }
        public int PlayerHealth { get; set; }
        public int PlayerCheats { get; set; }
        public int ReactionTime { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double PlayerVelX { get; set; }
        public double PlayerVelY { get; set; }
    }

    public sealed class SlideMoveState
    {
        public required SlideActor Actor { get; init; }
        public Vec2 TryDelta { get; set; }
        public int NumSteps { get; set; } = 1;
        public bool VarFriction { get; set; } = true;
        public Vec2 TmMove { get; set; }
        public double BestSlideFrac { get; set; } = 1.01;
        public SlideLine? BestSlideLine { get; set; }
        public int HitCount { get; set; } = 3;
        public object? Friction { get; init; }
        public Action? PlayGrunt { get; init; }
        /// <summary>Lines considered by <see cref="SlideTraverse"/> (test / headless maps).</summary>
        public IReadOnlyList<SlideLine> SlideLines { get; init; } = Array.Empty<SlideLine>();
        public bool BlockAllSlideLines { get; init; }
        public AuthoritySimulation? Sim { get; init; }
        public Actor? MapActor { get; init; }
    }

    public static void HitSlideLine(SlideMoveState st, SlideLine ld)
    {
        var mo = st.Actor;
        var friction = st.Friction;
        var icyfloor = (mo.Flags8 & Mf8Nofrictionbounce) == 0 && st.TmMove.LengthSquared() > 16 &&
                       st.VarFriction && mo.Z <= mo.FloorZ && friction != null;

        if (ld.DeltaX == 0)
        {
            if (icyfloor && Math.Abs(st.TmMove.X) > Math.Abs(st.TmMove.Y))
            {
                st.TmMove = new Vec2(-st.TmMove.X / 2, st.TmMove.Y / 2);
                if (mo.HasPlayer && mo.PlayerIsMo && mo.PlayerHealth > 0 && (mo.PlayerCheats & CfPredicting) == 0)
                    st.PlayGrunt?.Invoke();
            }
            else st.TmMove = new Vec2(0, st.TmMove.Y);
            return;
        }

        if (ld.DeltaY == 0)
        {
            if (icyfloor && Math.Abs(st.TmMove.Y) > Math.Abs(st.TmMove.X))
            {
                st.TmMove = new Vec2(st.TmMove.X / 2, -st.TmMove.Y / 2);
                if (mo.HasPlayer && mo.PlayerIsMo && mo.PlayerHealth > 0 && (mo.PlayerCheats & CfPredicting) == 0)
                    st.PlayGrunt?.Invoke();
            }
            else st.TmMove = new Vec2(st.TmMove.X, 0);
            return;
        }

        var pos = (mo.X, mo.Y);
        var side = PointOnLineSide(pos, ld);
        var lineangle = Math.Atan2(ld.DeltaY, ld.DeltaX) + (side == 1 ? Math.PI : 0);
        var moveangle = Math.Atan2(st.TmMove.Y, st.TmMove.X) + 2 * Math.PI;
        var deltaangle = Math.Abs(moveangle - lineangle);
        if (deltaangle > Math.PI) deltaangle = 2 * Math.PI - deltaangle;
        var movelen = st.TmMove.Length();
        if (icyfloor && deltaangle > Math.PI / 4 && deltaangle < 3 * Math.PI / 4)
        {
            movelen /= 2;
            if (mo.HasPlayer && mo.PlayerIsMo && mo.PlayerHealth > 0 && (mo.PlayerCheats & CfPredicting) == 0)
                st.PlayGrunt?.Invoke();
            st.TmMove = new Vec2(Math.Cos(lineangle) * movelen, Math.Sin(lineangle) * movelen);
            return;
        }

        var (dllX, dllY, dllDx, dllDy) = (0d, 0d, ld.DeltaX, ld.DeltaY);
        var dlv = (x: pos.Item1, y: pos.Item2, dx: dllDy, dy: -dllDx);
        var inter1 = VmPMapUtlLineOpening.InterceptVector(
            new VmPMapUtlLineOpening.Divline(dlv.x, dlv.y, dlv.dx, dlv.dy),
            new VmPMapUtlLineOpening.Divline(dllX, dllY, dllDx, dllDy));
        var inter2 = VmPMapUtlLineOpening.InterceptVector(
            new VmPMapUtlLineOpening.Divline(dlv.x, dlv.y, st.TmMove.X, st.TmMove.Y),
            new VmPMapUtlLineOpening.Divline(dllX, dllY, dllDx, dllDy));
        var inter3 = VmPMapUtlLineOpening.InterceptVector(
            new VmPMapUtlLineOpening.Divline(dlv.x, dlv.y, dlv.dx, dlv.dy),
            new VmPMapUtlLineOpening.Divline(dlv.x, dlv.y, st.TmMove.X, st.TmMove.Y));
        st.TmMove = inter3 != 0
            ? new Vec2((inter2 - inter1) * dllDx / inter3, (inter2 - inter1) * dllDy / inter3)
            : new Vec2(0, 0);
    }

    public static void SlideTraverse(SlideMoveState st, Vec2 start, Vec2 end)
    {
        var mo = st.Actor;
        foreach (var intercept in CollectSlideIntercepts(st, start, end))
        {
            var li = intercept.Line;
            if (li == null) continue;
            if (st.BlockAllSlideLines || IsSlideLineBlocked(st, mo, li)) goto isblocking;
            var open = SlideLineOpening(st, mo, li, intercept.HitX, intercept.HitY);
            if (open.Range < mo.Height || open.Top < mo.Top || open.Bottom - mo.Z > mo.MaxStepHeight)
                goto isblocking;
            if (mo.Z < open.Bottom)
            {
                var savedz = mo.Z;
                mo.Z = open.Bottom;
                var good = TestSlideMobjZ(st, mo);
                mo.Z = savedz;
                if (!good) goto isblocking;
            }

            continue;
        isblocking:
            if (intercept.Frac < st.BestSlideFrac)
            {
                st.BestSlideFrac = intercept.Frac;
                st.BestSlideLine = li;
            }

            return;
        }
    }

    public static void SlideMove(SlideMoveState st)
    {
        var mo = st.Actor;
        var tryp = st.TryDelta;
        if (mo.HasPlayer && mo.PlayerIsMo && mo.ReactionTime > 0) return;
        st.HitCount = 3;
    retry:
        if (--st.HitCount == 0) { Stairstep(st, tryp); return; }
        double leadX = tryp.X > 0 ? mo.X + mo.Radius : mo.X - mo.Radius;
        double trailX = tryp.X > 0 ? mo.X - mo.Radius : mo.X + mo.Radius;
        double leadY = tryp.Y > 0 ? mo.Y + mo.Radius : mo.Y - mo.Radius;
        double trailY = tryp.Y > 0 ? mo.Y - mo.Radius : mo.Y + mo.Radius;
        var lead = new Vec2(leadX, leadY);
        st.BestSlideFrac = 1.01;
        SlideTraverse(st, lead, lead + tryp);
        SlideTraverse(st, new Vec2(trailX, leadY), tryp + new Vec2(trailX, leadY));
        SlideTraverse(st, new Vec2(leadX, trailY), tryp + new Vec2(leadX, trailY));
        if (st.BestSlideFrac > 1) { Stairstep(st, tryp); return; }
        st.BestSlideFrac -= 1.0 / 32;
        if (st.BestSlideFrac > 0)
        {
            var newpos = tryp.Scale(st.BestSlideFrac);
            var startvel = new Vec2(mo.VelX, mo.VelY);
            if (!SlideTryMove(st, mo, mo.X + newpos.X, mo.Y + newpos.Y)) { Stairstep(st, tryp); return; }
            if (new Vec2(mo.VelX, mo.VelY) != startvel) return;
        }

        st.BestSlideFrac = 1 - (st.BestSlideFrac + 1.0 / 32);
        if (st.BestSlideFrac <= 0) return;
        if (st.BestSlideFrac > 1) st.BestSlideFrac = 1;
        tryp = st.TmMove = tryp.Scale(st.BestSlideFrac);
        if (st.BestSlideLine != null) HitSlideLine(st, st.BestSlideLine);
        mo.VelX = st.TmMove.X * st.NumSteps;
        mo.VelY = st.TmMove.Y * st.NumSteps;
        if (mo.HasPlayer && mo.PlayerIsMo)
        {
            if (Math.Abs(mo.PlayerVelX) > Math.Abs(mo.VelX)) mo.PlayerVelX = mo.VelX;
            if (Math.Abs(mo.PlayerVelY) > Math.Abs(mo.VelY)) mo.PlayerVelY = mo.VelY;
        }

        if (!SlideTryMove(st, mo, mo.X + st.TmMove.X, mo.Y + st.TmMove.Y)) goto retry;
    }

    private static void Stairstep(SlideMoveState st, Vec2 tryp)
    {
        var mo = st.Actor;
        var move = new Vec2(0, tryp.Y);
        if (!SlideTryMove(st, mo, mo.X + move.X, mo.Y + move.Y))
        {
            move = new Vec2(tryp.X, 0);
            SlideTryMove(st, mo, mo.X + move.X, mo.Y + move.Y);
        }
    }

    static int PointOnLineSide((double X, double Y) pos, SlideLine line) =>
        (line.DeltaX * (pos.Y - 0) - line.DeltaY * (pos.X - 0)) >= 0 ? 1 : 0;

    static IEnumerable<Intercept> CollectSlideIntercepts(SlideMoveState st, Vec2 start, Vec2 end)
    {
        if (st.SlideLines.Count == 0)
            yield break;
        foreach (var line in st.SlideLines)
        {
            var trace = new VmPMapUtlLineOpening.Divline(start.X, start.Y, end.X - start.X, end.Y - start.Y);
            var div = new VmPMapUtlLineOpening.Divline(0, 0, line.DeltaX, line.DeltaY);
            var frac = VmPMapUtlLineOpening.InterceptVector(trace, div);
            if (frac < 0 || frac > 1)
                continue;
            yield return new Intercept
            {
                Line = line,
                Frac = frac,
                HitX = start.X + (end.X - start.X) * frac,
                HitY = start.Y + (end.Y - start.Y) * frac,
            };
        }
    }

    static bool IsSlideLineBlocked(SlideMoveState st, SlideActor mo, SlideLine line) =>
        st.Sim is { } sim && st.MapActor is { } actor
            && ActorPhysics.SlideLineBlocks(sim, actor, ResolveSlideLine(sim, line));

    static LineOpening SlideLineOpening(SlideMoveState st, SlideActor mo, SlideLine line, double hitX, double hitY) =>
        new() { Range = 999, Top = 999, Bottom = -999 };

    static bool TestSlideMobjZ(SlideMoveState _, SlideActor __) => true;

    static bool SlideTryMove(SlideMoveState st, SlideActor mo, double x, double y)
    {
        if (st.Sim is not { } sim || st.MapActor is not { } actor)
            return false;
        return ActorPhysics.TryMove(sim, actor, x, y, out _);
    }

    static LevelLine ResolveSlideLine(AuthoritySimulation sim, SlideLine line)
    {
        foreach (var levelLine in sim.Level.Lines)
        {
            if (levelLine.Index.ToString() == line.Id || $"{levelLine.X1},{levelLine.Y1}" == line.Id)
                return levelLine;
        }

        return new LevelLine
        {
            X1 = 0,
            Y1 = 0,
            X2 = line.DeltaX,
            Y2 = line.DeltaY,
            SideFront = 0,
            Flags = LevelLine.BlockingFlag,
        };
    }
}
