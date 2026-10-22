namespace HCDE.Playsim;

/// <summary><c>CallStateChain</c> and adjacent action helpers from <c>p_actionfunctions.cpp</c> (87–790).</summary>
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
        public bool HasDefaultArgs { get; set; }
        public int ImplicitArgs { get; set; }
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

    public readonly record struct ChainCallDescriptor(int NumRet, bool WantStateRet, bool WantIntRet);

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
                    var call = ctx.VmCallAction?.Invoke(state, plan) ?? new ChainCallResult(true, null);
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
            return new ChainCallDescriptor(2, true, true);

        if (state.ReturnTypesCount == 1 && state.ReturnType0 == TypeState)
            return new ChainCallDescriptor(1, true, false);

        if (state.ReturnTypesCount >= 1
            && (state.ReturnType0 == TypeSInt32 || state.ReturnType0 == TypeUInt32 || state.ReturnType0 == TypeBool))
            return new ChainCallDescriptor(1, false, true);

        return new ChainCallDescriptor(0, false, true);
    }

    public static double GetZAtFloor(bool hasSector, double floorPlaneZ, double nextLowestZ, bool no3dFloor, bool noPortals)
    {
        if (!hasSector) return 0;
        if (no3dFloor && noPortals) return floorPlaneZ;
        return nextLowestZ != 0 ? nextLowestZ : floorPlaneZ;
    }

    public static double GetCrouchFactor(bool hasPlayer, double crouchFactor) =>
        hasPlayer ? crouchFactor : 1;

    public static bool SeekerMissileShouldStick(bool lookFlag, bool targetIsPlayer, bool tracerIsPlayer) =>
        !lookFlag && targetIsPlayer && !tracerIsPlayer;

    public static int DecorateInternalInt(int value) => value;

    public static bool DecorateInternalBool(bool value) => value;

    public static double DecorateInternalFloat(double value) => value;

    public const int GzfAbsolutepos = 1;
    public const int GzfAbsoluteang = 1 << 1;
    public const int GzfCeiling = 1 << 2;
    public const int Gzf3Drestrict = 1 << 3;
    public const int GzfNoportals = 1 << 4;
    public const int GzfNo3dfloor = 1 << 5;

    public static double GetZAt(double px, double py, double actorYaw, double offsetX, double offsetY, int flags,
        bool hasSector, double floorZ, double ceilingZ, double nextLowest, double nextHighest)
    {
        if (!hasSector) return 0;
        var absolutePos = (flags & GzfAbsolutepos) != 0;
        var absoluteAng = (flags & GzfAbsoluteang) != 0;
        if (!absolutePos)
        {
            var ang = absoluteAng ? 0 : actorYaw;
            var s = Math.Sin(ang);
            var c = Math.Cos(ang);
            _ = px * c + py * s;
            _ = px * s - py * c;
        }

        if ((flags & GzfCeiling) != 0)
        {
            if ((flags & GzfNo3dfloor) != 0 && (flags & GzfNoportals) != 0)
                return ceilingZ;
            return nextHighest != 0 ? nextHighest : ceilingZ;
        }

        if ((flags & GzfNo3dfloor) != 0 && (flags & GzfNoportals) != 0)
            return floorZ;
        if ((flags & GzfNo3dfloor) != 0)
            return nextLowest != 0 ? nextLowest : floorZ;
        return nextLowest != 0 ? nextLowest : floorZ;
    }

    public static double GetCVarFloat(bool found, double value) => found ? value : 0;

    public static string GetCVarString(bool found, string? value) => found ? value ?? "" : "";

    public static int GetPlayerInput(bool hasPlayer, int inputNum) => hasPlayer ? inputNum : 0;

    public static int CountProximity(bool hasMobj, int count) => hasMobj ? count : 0;

    public static bool RearrangePointer(int ptrTarget, int ptrMaster, int ptrTracer, int targetVal, int masterVal, int tracerVal,
        out int newTarget, out int newMaster, out int newTracer)
    {
        newTarget = ptrTarget switch
        {
            1 => masterVal,
            2 => tracerVal,
            3 => 0,
            _ => targetVal,
        };
        newMaster = ptrMaster switch
        {
            1 => targetVal,
            2 => tracerVal,
            3 => 0,
            _ => masterVal,
        };
        newTracer = ptrTracer switch
        {
            1 => targetVal,
            2 => masterVal,
            3 => 0,
            _ => tracerVal,
        };
        return true;
    }

    public static bool TransferPointer(bool recipientNull, object? source, object? recipient, bool sameRef) =>
        !recipientNull && source != null && !sameRef;

    public static bool CopyFriendlinessAllowed(bool isPlayer, bool hasSource) => !isPlayer && hasSource;

    public static bool PlaySoundExLooping(bool looping, bool alreadyPlaying) => looping && !alreadyPlaying;

    public static bool StopSoundExValidChannel(int channelIndex) => channelIndex > 0 && channelIndex <= 7;

    public const int SmfLook = 1;
    public const int SmfPrecise = 2;
    public const int SmfCurspeed = 4;

    public static bool SeekerMissileLook(int flags, int chanceRoll, int chance) =>
        (flags & SmfLook) != 0 && chanceRoll < chance;

    public static int SeekerThreshold(int ang1) => Math.Max(ang1, 0);

    public static int SeekerTurnMax(int ang2) => Math.Max(ang2, 0);

    public static bool SeekerMissileFailed(bool seekOk, int flags) =>
        !seekOk && (flags & SmfLook) != 0;

    public static bool ChainUsesDefaultArgs(ChainState state) =>
        state.HasDefaultArgs && state.ImplicitArgs is >= 1 and <= 3;

    public static int ReserveActionParams(int currentSize, int reserveCount) => currentSize + reserveCount;

    public static bool ActionParamsClamp(int index, int size) => index >= 0 && index <= size;

    public static bool UnsafeActionRemoved(ChainState state) => state.ActionRemoved || !state.HasAction;

    public static bool StateChainSucceeded(bool aggregateResult, int counter) =>
        aggregateResult && counter < LoopLimit;

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

    public static int PickCeilingOrFloorBranch(int flags) => (flags & GzfCeiling) != 0 ? 1 : 0;

    public static bool PortalFlagsIncludeNoPortals(int flags) => (flags & GzfNoportals) != 0;

    public static bool PortalFlagsIncludeNo3dFloor(int flags) => (flags & GzfNo3dfloor) != 0;

    public static bool ThreeDRestrict(int flags) => (flags & Gzf3Drestrict) != 0;

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

    public static bool ValidateSeekerArgs(int ang1, int ang2, int chance, int distance) =>
        ang1 >= 0 && ang2 >= 0 && chance >= 0 && distance >= 0;

    public static bool AssignTracerOnLook(bool look, bool tracerNull, int roll, int chance, bool foundRough) =>
        look && tracerNull && roll < chance && foundRough;

    public static bool ClearTracerAfterFailedSeek(bool seekResult, int flags) =>
        !seekResult && (flags & SmfLook) != 0;

    public static bool ChainJumpCountsAsSuccess(bool hasJumpState, bool retval) => !hasJumpState && retval;

    public static int ImplicitArgCount(int implicitArgs) => implicitArgs switch
    {
        1 => 1,
        3 => 3,
        _ => 0,
    };

    public static bool ShouldInjectStateParamInfo(int implicitArgs) => implicitArgs == 3;

    public static bool DefaultArgsReserve(int defaultCount, int implicitArgs, out int total)
    {
        total = defaultCount + Math.Max(0, implicitArgs - defaultCount);
        return defaultCount > 0;
    }

    public static bool InventoryChainDefineNative(bool selfActor, bool affectee, bool hasState) =>
        selfActor && affectee && hasState;

    public static double GetZAtCeilingPath(int flags, bool hasSector, double ceilingPlane, double nextHighest)
    {
        if (!hasSector) return 0;
        return SelectCeilingZ(PortalFlagsIncludeNo3dFloor(flags), PortalFlagsIncludeNoPortals(flags),
            ceilingPlane, 0, nextHighest);
    }

    public static double GetZAtFloorPath(int flags, bool hasSector, double floorPlane, double nextLowest, double maxStep)
    {
        if (!hasSector) return 0;
        return SelectFloorZ(PortalFlagsIncludeNo3dFloor(flags), PortalFlagsIncludeNoPortals(flags),
            floorPlane, 0, nextLowest, maxStep);
    }

    public static bool PlaySoundAttenuationSwitch(int attenuationRaw, out float attenuation)
    {
        attenuation = attenuationRaw switch
        {
            -1 => 0.5f,
            0 => 1f,
            1 or 2 => 0f,
            _ => 1f,
        };
        return true;
    }

    public static bool SoundChannelAutoRange(int channel) => channel >= 0;

    public static bool LoopingSoundAlreadyPlaying(bool looping, bool playing) => looping && playing;

    public static bool StopSoundChannelInRange(int channel) => channel > 0 && channel <= 7;

    public static bool SeekerUsesPreciseFlag(int flags) => (flags & SmfPrecise) != 0;

    public static bool SeekerUsesCurSpeedFlag(int flags) => (flags & SmfCurspeed) != 0;

    public static bool ChainResultBool(int result) => result != 0;

    public static int ActionReturnCount(bool numretRequested, int numret) =>
        numretRequested ? numret : 0;

    public static bool RearrangeTargetFromMaster(int ptrTarget, int masterVal, int tracerVal, out int newTarget)
    {
        newTarget = ptrTarget switch
        {
            1 => masterVal,
            2 => tracerVal,
            3 => 0,
            _ => masterVal,
        };
        return ptrTarget is 1 or 2 or 3;
    }

    public static bool RearrangeMasterFromTarget(int ptrMaster, int targetVal, int tracerVal, out int newMaster)
    {
        newMaster = ptrMaster switch
        {
            1 => targetVal,
            2 => tracerVal,
            3 => 0,
            _ => targetVal,
        };
        return ptrMaster is 1 or 2 or 3;
    }

    public static bool RearrangeTracerFromTarget(int ptrTracer, int targetVal, int masterVal, out int newTracer)
    {
        newTracer = ptrTracer switch
        {
            1 => targetVal,
            2 => masterVal,
            3 => 0,
            _ => masterVal,
        };
        return ptrTracer is 1 or 2 or 3;
    }
}
