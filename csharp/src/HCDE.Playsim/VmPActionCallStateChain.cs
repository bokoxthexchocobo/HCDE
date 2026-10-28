namespace HCDE.Playsim;

/// <summary><c>CallStateChain</c> and <c>AActor::GetZAt</c> from <c>p_actionfunctions.cpp</c> (87–229, 248–326).</summary>
public static class VmPActionCallStateChain
{
    public const int SufItem = 1;
    public const int LoopLimit = 10000;
    public const int TypeState = 1;
    public const int TypeSInt32 = 2;
    public const int TypeBool = 3;
    public const int TypeUInt32 = 4;

    public sealed class ChainState
    {
        public int UseFlags { get; set; }
        public bool HasAction { get; set; }
        public bool ActionUnsafe { get; set; }
        public bool ActionRemoved { get; set; }
        public int ReturnTypesCount { get; set; }
        public int ReturnType0 { get; set; }
        public int ReturnType1 { get; set; }
        public ChainState? Next { get; set; }
        public string Name { get; set; } = "State";
    }

    public sealed class ChainContext
    {
        public object? SavedState { get; set; }
        public object? CurrentState { get; set; }
        public Func<ChainState, ChainCallDescriptor, ChainCallResult>? VmCallAction { get; init; }
        public Action<string, string>? LogUnsafe { get; init; }
        public Action<string>? LogMissingItemFlag { get; init; }
    }

    public readonly record struct ChainCallDescriptor(int NumRet, bool WantStateRet, bool WantIntRet, bool JumpOnlyStateReturn);

    public static bool DefaultActionRetval(ChainCallDescriptor plan) =>
        plan.JumpOnlyStateReturn ? false : true;

    public readonly record struct ChainCallResult(bool Retval, ChainState? JumpState);

    public static bool CallStateChain(ChainContext ctx, ChainState? state)
    {
        var result = false;
        var counter = 0;
        ctx.SavedState = ctx.CurrentState;

        while (state != null)
        {
            if ((state.UseFlags & SufItem) == 0)
            {
                ctx.LogMissingItemFlag?.Invoke(state.Name);
                return false;
            }

            ctx.CurrentState = state;
            ChainState? nextState = null;
            var retval = true;

            if (state.HasAction && !state.ActionRemoved)
            {
                if (state.ActionUnsafe)
                {
                    ctx.LogUnsafe?.Invoke(state.Name, state.Name);
                    state.ActionRemoved = true;
                    state.HasAction = false;
                }
                else
                {
                    var plan = BuildReturnPlan(state);
                    retval = DefaultActionRetval(plan);
                    var call = ctx.VmCallAction?.Invoke(state, plan)
                        ?? new ChainCallResult(retval, null);
                    retval = call.Retval;
                    nextState = call.JumpState;
                }
            }

            if (nextState == null)
                result |= retval;

            counter++;
            if (counter >= LoopLimit)
                break;

            if (nextState == null)
                nextState = state.Next;

            if (ReferenceEquals(state, nextState))
            {
                result = false;
                break;
            }

            state = nextState;
        }

        ctx.CurrentState = ctx.SavedState;
        return result;
    }

    public static ChainCallDescriptor BuildReturnPlan(ChainState state)
    {
        if (state.ReturnTypesCount >= 2 && state.ReturnType0 == TypeState
            && (state.ReturnType1 == TypeSInt32 || state.ReturnType1 == TypeUInt32 || state.ReturnType1 == TypeBool))
            return new ChainCallDescriptor(2, true, true, false);

        if (state.ReturnTypesCount == 1 && state.ReturnType0 == TypeState)
            return new ChainCallDescriptor(1, true, false, true);

        if (state.ReturnTypesCount >= 1
            && (state.ReturnType0 == TypeSInt32 || state.ReturnType0 == TypeUInt32 || state.ReturnType0 == TypeBool))
            return new ChainCallDescriptor(1, false, true, false);

        return new ChainCallDescriptor(0, false, true, false);
    }

    public const int GzfAbsolutepos = 1;
    public const int GzfAbsoluteang = 1 << 1;
    public const int GzfCeiling = 1 << 2;
    public const int Gzf3Drestrict = 1 << 3;
    public const int GzfNoportals = 1 << 4;
    public const int GzfNo3dfloor = 1 << 5;
    public const int Ffcf3dRestrict = 1;
    public const int FfcfNoportals = 1 << 1;

    public static int BuildFloorClipFlags(int gzFlags)
    {
        var pflags = 0;
        if ((gzFlags & Gzf3Drestrict) != 0)
            pflags |= Ffcf3dRestrict;
        if ((gzFlags & GzfNoportals) != 0)
            pflags |= FfcfNoportals;
        return pflags;
    }

    public static void PrepareGetZAtPosition(
        ref double px,
        ref double py,
        ref double angleRad,
        double offsetX,
        double offsetY,
        double actorYawRad,
        int flags)
    {
        if ((flags & GzfAbsolutepos) != 0)
            return;

        if ((flags & GzfAbsoluteang) == 0)
            angleRad += actorYawRad;

        var s = Math.Sin(angleRad);
        var c = Math.Cos(angleRad);
        px = offsetX * c + offsetY * s;
        py = offsetX * s - offsetY * c;
    }

    public static bool GetZAtAbsolutePos(double px, double py, double yaw, bool absoluteAng, out double wx, out double wy)
    {
        if (absoluteAng)
        {
            wx = px;
            wy = py;
            return true;
        }

        var s = Math.Sin(yaw);
        var c = Math.Cos(yaw);
        wx = px * c + py * s;
        wy = px * s - py * c;
        return true;
    }

    public static double SelectFloorZ(bool no3d, bool noPortal, double floorPlane, double lowest, double nextLowest, double maxStep)
    {
        if (no3d && noPortal) return floorPlane;
        if (no3d) return lowest != 0 ? lowest : floorPlane;
        _ = maxStep;
        return nextLowest != 0 ? nextLowest : floorPlane;
    }

    public static double SelectCeilingZ(bool no3d, bool noPortal, double ceilingPlane, double highest, double nextHighest)
    {
        if (no3d && noPortal) return ceilingPlane;
        if (no3d) return highest != 0 ? highest : ceilingPlane;
        return nextHighest != 0 ? nextHighest : ceilingPlane;
    }

    public sealed class GetZAtResolveContext
    {
        public double PlaneZ { get; init; }
        public double PortalOnlyZ { get; init; }
        public double Full3dZ { get; init; }
    }

    public static double ResolveFloorZAt(int flags, GetZAtResolveContext samples)
    {
        var no3d = (flags & GzfNo3dfloor) != 0;
        var noPortal = (flags & GzfNoportals) != 0;
        if (no3d && noPortal)
            return samples.PlaneZ;
        if (no3d)
            return samples.PortalOnlyZ != 0 ? samples.PortalOnlyZ : samples.PlaneZ;
        return samples.Full3dZ != 0 ? samples.Full3dZ : samples.PlaneZ;
    }

    public static double ResolveCeilingZAt(int flags, GetZAtResolveContext samples)
    {
        var no3d = (flags & GzfNo3dfloor) != 0;
        var noPortal = (flags & GzfNoportals) != 0;
        if (no3d && noPortal)
            return samples.PlaneZ;
        if (no3d)
            return samples.PortalOnlyZ != 0 ? samples.PortalOnlyZ : samples.PlaneZ;
        return samples.Full3dZ != 0 ? samples.Full3dZ : samples.PlaneZ;
    }

    public static double GetZAt(
        double px,
        double py,
        double angleRad,
        double offsetX,
        double offsetY,
        double actorYawRad,
        int flags,
        bool hasSector,
        GetZAtResolveContext floorSamples,
        GetZAtResolveContext ceilingSamples)
    {
        if (!hasSector)
            return 0;

        PrepareGetZAtPosition(ref px, ref py, ref angleRad, offsetX, offsetY, actorYawRad, flags);

        if ((flags & GzfCeiling) != 0)
            return ResolveCeilingZAt(flags, ceilingSamples);

        return ResolveFloorZAt(flags, floorSamples);
    }

    public static double GetZAt(double px, double py, double actorYaw, double offsetX, double offsetY, int flags,
        bool hasSector, double floorZ, double ceilingZ, double nextLowest, double nextHighest)
    {
        var floor = new GetZAtResolveContext
        {
            PlaneZ = floorZ,
            PortalOnlyZ = nextLowest,
            Full3dZ = nextLowest,
        };
        var ceiling = new GetZAtResolveContext
        {
            PlaneZ = ceilingZ,
            PortalOnlyZ = nextHighest,
            Full3dZ = nextHighest,
        };
        return GetZAt(px, py, 0, offsetX, offsetY, actorYaw, flags, hasSector, floor, ceiling);
    }
}
