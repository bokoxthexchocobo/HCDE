namespace HCDE.Playsim;

/// <summary><c>FSlide::BounceTraverse</c> and <c>FSlide::BounceWall</c> from <c>p_map.cpp</c> (3511–3707).</summary>
public static class VmPMapBounceTraverse
{
    public const int MlBlockeverything = 1;
    public const int MlTwosided = 1;
    public const int BounceWalls = 1;
    public const int BounceCeilings = 2;
    public const int BounceFloors = 4;
    public const int BounceNotOnSky = 1;
    public const int BounceKeepAngle = 64;
    public const int BounceUseBounceState = 128;
    public const int MfMissile = 1;
    public const int LineHorizon = 1;
    public const double DefaultBestSlideFrac = 1.01;

    public static bool BlockEverythingLine(int flags) => (flags & MlBlockeverything) != 0;

    public static bool TwoSidedLine(int flags) => (flags & MlTwosided) != 0;

    public static bool OneSidedBlocks(bool twoSided, bool backsector) => !twoSided || !backsector;

    public static bool OpeningTooLow(double range, double height) => range < height;

    public static bool TooHigh(double top, double actorTop) => top < actorTop;

    public static bool TooLow(double bottom, double actorZ) => bottom > actorZ;

    public static bool CloserSlideFrac(double frac, double best) => frac < best;

    public static bool BounceWallsEnabled(int bounceFlags) => (bounceFlags & BounceWalls) != 0;

    public static bool AnyPlaneBounceEnabled(int bounceFlags) =>
        (bounceFlags & (BounceWalls | BounceCeilings | BounceFloors)) != 0;

    public static bool MissileSpecialHitStop(int hit) => hit == 1;

    public static bool MissileSpecialHitBlock(int hit) => hit == 0;

    public static bool HorizonDestroys(bool horizon, bool notOnSky, bool hitSky) =>
        horizon || (notOnSky && hitSky);

    public static bool BounceCountDepleted(int count) => count > 0 && count - 1 == 0;

    public static bool KeepAngle(int bounceFlags) => (bounceFlags & BounceKeepAngle) != 0;

    public static bool UseBounceState(int bounceFlags) => (bounceFlags & BounceUseBounceState) != 0;

    public static bool ShouldContinuePastBackSide(bool oneSidedBlocks, bool pointOnBackSide) =>
        oneSidedBlocks && pointOnBackSide;

    public static bool LineBlocksMovement(
        double openRange,
        double height,
        double openTop,
        double actorTop,
        double openBottom,
        double actorZ) =>
        !OpeningTooLow(openRange, height)
        && !TooHigh(openTop, actorTop)
        && !TooLow(openBottom, actorZ);

    public readonly record struct BounceTraverseContext(
        bool IsLine,
        int LineFlags,
        bool TwoSided,
        bool Backsector,
        bool PointOnBackSide,
        double OpenRange,
        double Height,
        double OpenTop,
        double ActorTop,
        double OpenBottom,
        double ActorZ,
        double Frac,
        double BestFrac);

    public static bool EvaluateTraverseContinue(BounceTraverseContext ctx)
    {
        if (!ctx.IsLine)
            return true;
        if (BlockEverythingLine(ctx.LineFlags))
            return false;
        if (OneSidedBlocks(ctx.TwoSided, ctx.Backsector))
        {
            if (ShouldContinuePastBackSide(true, ctx.PointOnBackSide))
                return true;
            return false;
        }
        if (OpeningTooLow(ctx.OpenRange, ctx.Height))
            return false;
        if (TooHigh(ctx.OpenTop, ctx.ActorTop))
            return false;
        if (TooLow(ctx.OpenBottom, ctx.ActorZ))
            return false;
        return true;
    }

    public sealed class TraverseInterceptStep
    {
        public bool IsLine { get; set; }
        public int LineFlags { get; set; }
        public bool Backsector { get; set; }
        public bool PointOnBackSide { get; set; }
        public double Frac { get; set; }
        public double OpenRange { get; set; }
        public double OpenTop { get; set; }
        public double OpenBottom { get; set; }
        public object? Line { get; set; }
    }

    public sealed class BounceTraverseState
    {
        public double BestSlideFrac { get; set; } = DefaultBestSlideFrac;
        public double SecondSlideFrac { get; set; }
        public object? BestSlideLine { get; set; }
        public object? SecondSlideLine { get; set; }
        public double ActorHeight { get; set; }
        public double ActorTop { get; set; }
        public double ActorZ { get; set; }
    }

    public static bool TryUpdateBlockingSlide(BounceTraverseState state, TraverseInterceptStep step, ref bool stopTraverse)
    {
        if (!CloserSlideFrac(step.Frac, state.BestSlideFrac))
        {
            stopTraverse = true;
            return false;
        }
        state.SecondSlideFrac = state.BestSlideFrac;
        state.SecondSlideLine = state.BestSlideLine;
        state.BestSlideFrac = step.Frac;
        state.BestSlideLine = step.Line;
        stopTraverse = true;
        return true;
    }

    public static bool RunBounceTraverseInterceptStep(
        BounceTraverseState state,
        TraverseInterceptStep step,
        ref bool stopLoop)
    {
        if (ShouldSkipNonLineIntercept(step.IsLine))
            return true;
        var ctx = new BounceTraverseContext(
            true,
            step.LineFlags,
            TwoSidedLine(step.LineFlags),
            step.Backsector,
            step.PointOnBackSide,
            step.OpenRange,
            state.ActorHeight,
            step.OpenTop,
            state.ActorTop,
            step.OpenBottom,
            state.ActorZ,
            step.Frac,
            state.BestSlideFrac);
        if (EvaluateTraverseContinue(ctx))
            return true;
        TryUpdateBlockingSlide(state, step, ref stopLoop);
        return false;
    }

    public static bool RunBounceTraverseLoop(BounceTraverseState state, IReadOnlyList<TraverseInterceptStep> steps)
    {
        foreach (var step in steps)
        {
            var stop = false;
            if (!RunBounceTraverseInterceptStep(state, step, ref stop))
                return false;
            if (stop)
                return false;
        }
        return true;
    }

    public readonly record struct BounceWallContext(
        int BounceFlags,
        bool Missile,
        int SpecialHit,
        bool Horizon,
        bool HitSky,
        int BounceCount,
        bool KeepAngleFlag,
        double MoveLen,
        bool UseBounceState);

    public static bool EvaluateBounceWallDestroy(BounceWallContext ctx)
    {
        if (!BounceWallsEnabled(ctx.BounceFlags))
            return false;
        if (ctx.Missile && MissileSpecialHitStop(ctx.SpecialHit))
            return true;
        if (ctx.Missile && MissileSpecialHitBlock(ctx.SpecialHit))
            return false;
        if (HorizonDestroys(ctx.Horizon, (ctx.BounceFlags & BounceNotOnSky) != 0, ctx.HitSky))
            return true;
        if (BounceCountDepleted(ctx.BounceCount))
            return true;
        return false;
    }

    public static bool EvaluateBounceWallReflect(BounceWallContext ctx)
    {
        if (ctx.MoveLen < 1)
            return true;
        return UseBounceState(ctx.BounceFlags);
    }

    public static bool ProjectileHitLinedef(bool hit, int bouncecount) => hit && bouncecount > 0;

    public static bool ExplodeMissileOnHit(bool missile, bool hit) => missile && hit;

    public static double ReflectAngle(double lineAngle, double moveAngle, int side) =>
        side == 1 ? (lineAngle + 180) * 2 - moveAngle : lineAngle * 2 - moveAngle;

    public static bool BoxOnWrongSide(int boxSide) => boxSide == -1;

    public static double MinMoveLen(double len) => len < 1 ? 2 : len;

    public static (double X, double Y) ComputeLeadingCorner(double posX, double posY, double radius, double velX, double velY)
    {
        var leadX = velX > 0 ? posX + radius : posX - radius;
        var leadY = velY > 0 ? posY + radius : posY - radius;
        return (leadX, leadY);
    }

    public static bool PreferFloorBounce(double floorDist, double ceilDist) => floorDist <= ceilDist;

    public static bool RunMissileSpecialBounceGate(bool missile, int specialHit, out bool stop, out bool destroy)
    {
        stop = false;
        destroy = false;
        if (!missile)
            return true;
        if (MissileSpecialHitStop(specialHit))
        {
            destroy = true;
            return false;
        }
        if (MissileSpecialHitBlock(specialHit))
        {
            stop = true;
            return false;
        }
        return true;
    }

    public readonly record struct BounceVelocityContext(
        bool MissileHitLinedef,
        int BounceCount,
        bool Missile,
        int SpecialHit,
        bool Horizon,
        bool SkyHit,
        int BounceFlags,
        double MoveLen,
        int Side,
        double LineAngle,
        double MoveAngle,
        int BoxSide);

    public static bool RunBounceWallReflectPipeline(BounceVelocityContext ctx, out double reflectedAngle, out double moveLen)
    {
        reflectedAngle = ReflectAngle(ctx.LineAngle, ctx.MoveAngle, ctx.Side);
        moveLen = MinMoveLen(ctx.MoveLen);
        if (EvaluateBounceWallDestroy(new BounceWallContext(
            ctx.BounceFlags, ctx.Missile, ctx.SpecialHit, ctx.Horizon, ctx.SkyHit, ctx.BounceCount, false, ctx.MoveLen, false)))
            return false;
        if (ProjectileHitLinedef(ctx.MissileHitLinedef, ctx.BounceCount))
            return false;
        if (BoxOnWrongSide(ctx.BoxSide))
        { }
        return EvaluateBounceWallReflect(new BounceWallContext(
            ctx.BounceFlags, ctx.Missile, ctx.SpecialHit, false, false, ctx.BounceCount, KeepAngle(ctx.BounceFlags), moveLen, true));
    }

    public static bool EvaluateBounceWallComplete(BounceVelocityContext ctx)
    {
        if (EvaluateBounceWallDestroy(new BounceWallContext(
            ctx.BounceFlags, ctx.Missile, ctx.SpecialHit, ctx.Horizon, ctx.SkyHit, ctx.BounceCount, false, ctx.MoveLen, false)))
            return true;
        if (ProjectileHitLinedef(ctx.MissileHitLinedef, ctx.BounceCount))
            return true;
        _ = ReflectAngle(ctx.LineAngle, ctx.MoveAngle, ctx.Side);
        if (BoxOnWrongSide(ctx.BoxSide))
        { }
        _ = MinMoveLen(ctx.MoveLen);
        return EvaluateBounceWallReflect(new BounceWallContext(
            ctx.BounceFlags, ctx.Missile, ctx.SpecialHit, false, false, ctx.BounceCount, false, ctx.MoveLen, true));
    }

    public sealed class BounceWallSession
    {
        public int BounceFlags { get; set; }
        public bool Missile { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double Radius { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public object? BlockingLine { get; set; }
        public BounceTraverseState Traverse { get; } = new();
        public IReadOnlyList<TraverseInterceptStep> Intercepts { get; set; } = Array.Empty<TraverseInterceptStep>();
        public Func<bool>? FloorBounceMissile { get; init; }
        public Func<bool>? CeilingBounceMissile { get; init; }
        public Action? DestroyActor { get; init; }
        public Action? ExplodeMissile { get; init; }
        public Action? CallDie { get; init; }
        public Action<double, double, double>? ApplyVelocity { get; init; }
        public Action? SetBounceWallState { get; init; }
        public int SpecialHit { get; set; }
        public bool HorizonLine { get; set; }
        public bool HitSky { get; set; }
        public int BounceCount { get; set; }
        public bool ProjectileHitLine { get; set; }
        public int BoxSide { get; set; } = 0;
        public double LineAngle { get; set; }
        public double MoveAngle { get; set; }
        public int LineSide { get; set; }
        public double MoveLen { get; set; }
    }

    public static bool RunFloorCeilingBounceFallback(BounceWallSession session, double floorDist, double ceilDist)
    {
        if (PreferFloorBounce(floorDist, ceilDist))
            return session.FloorBounceMissile?.Invoke() ?? false;
        return session.CeilingBounceMissile?.Invoke() ?? false;
    }

    public static bool RunBounceWallPipeline(BounceWallSession session)
    {
        if (!AnyPlaneBounceEnabled(session.BounceFlags))
            return false;
        session.Traverse.BestSlideLine = session.BlockingLine;
        var lead = ComputeLeadingCorner(session.PosX, session.PosY, session.Radius, session.VelX, session.VelY);
        var traverseOk = RunBounceTraverseLoop(session.Traverse, session.Intercepts);
        if (traverseOk && session.BlockingLine == null)
            return RunFloorCeilingBounceFallback(session, 0, 1);
        if (!BounceWallsEnabled(session.BounceFlags))
            return false;
        if (!RunMissileSpecialBounceGate(session.Missile, session.SpecialHit, out _, out var destroy) && destroy)
        {
            session.DestroyActor?.Invoke();
            return true;
        }
        if (!RunMissileSpecialBounceGate(session.Missile, session.SpecialHit, out var stop, out _) && stop)
            return false;
        if (HorizonDestroys(session.HorizonLine, (session.BounceFlags & BounceNotOnSky) != 0, session.HitSky))
        {
            session.DestroyActor?.Invoke();
            return true;
        }
        if (ProjectileHitLinedef(session.ProjectileHitLine, session.BounceCount))
        {
            if (ExplodeMissileOnHit(session.Missile, true))
                session.ExplodeMissile?.Invoke();
            else
                session.CallDie?.Invoke();
            return true;
        }
        if (BounceCountDepleted(session.BounceCount))
        {
            if (session.Missile)
                session.ExplodeMissile?.Invoke();
            else
                session.CallDie?.Invoke();
            return true;
        }
        var velCtx = new BounceVelocityContext(
            session.ProjectileHitLine,
            session.BounceCount,
            session.Missile,
            session.SpecialHit,
            session.HorizonLine,
            session.HitSky,
            session.BounceFlags,
            session.MoveLen,
            session.LineSide,
            session.LineAngle,
            session.MoveAngle,
            session.BoxSide);
        if (!RunBounceWallReflectPipeline(velCtx, out var angle, out var len))
            return false;
        if (!KeepAngle(session.BounceFlags))
            session.MoveAngle = angle;
        session.ApplyVelocity?.Invoke(angle, len, session.Radius);
        if (UseBounceState(session.BounceFlags))
            session.SetBounceWallState?.Invoke();
        return true;
    }

    public static bool BounceWall(BounceWallSession session) => RunBounceWallPipeline(session);
}
