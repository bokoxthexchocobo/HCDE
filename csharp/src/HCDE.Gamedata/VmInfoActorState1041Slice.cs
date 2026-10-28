namespace HCDE.Gamedata;

/// <summary>
/// <c>FState::CheckCallerType</c>, <c>FState::CallAction</c>, and <c>GetSpriteIndex</c> hull from <c>info.cpp</c> (**151–300**).
/// Chunk **1041**; continues **1040** (**1–150**). Sprite lookup seam: uncredited <see cref="VmSpriteCatalog"/>.
/// </summary>
public static class VmInfoActorState1041Slice
{
    public const int StatePsprite = 1;
    public const int TypeStateReturn = 1;

    public enum CheckCallerTypeOutcome
    {
        Skip,
        Ok,
        BadPrototype,
        NullCaller,
        InvalidClass,
    }

    public readonly record struct CheckCallerTypeRequest(
        int ImplicitArgs,
        bool FirstArgIsObjectPointer,
        string ActionPrintableName,
        string ExpectedCallerClass,
        string? SelfClass,
        string? StateOwnerClass,
        uint StateFlags);

    public static CheckCallerTypeOutcome EvaluateCheckCallerType(CheckCallerTypeRequest req)
    {
        if (VmInfoActorState1040Slice.ShouldSkipCallerTypeMatch(req.ImplicitArgs))
            return CheckCallerTypeOutcome.Skip;

        if (VmInfoActorState1040Slice.ShouldAbortCheckCallerOnBadPrototype(req.FirstArgIsObjectPointer))
            return CheckCallerTypeOutcome.BadPrototype;

        var matchCtx = new VmInfoActorState1040Slice.StateCallerMatchContext(
            req.ImplicitArgs,
            req.FirstArgIsObjectPointer,
            req.ExpectedCallerClass,
            SecondArgIsObjectPointer: req.ImplicitArgs >= 2,
            SecondArgClass: req.ExpectedCallerClass,
            req.StateFlags);

        if (VmInfoActorState1040Slice.CheckCallerTypePasses(matchCtx, req.SelfClass, req.StateOwnerClass))
            return CheckCallerTypeOutcome.Ok;

        if (string.IsNullOrEmpty(req.SelfClass))
            return CheckCallerTypeOutcome.NullCaller;

        return CheckCallerTypeOutcome.InvalidClass;
    }

    public static string FormatBadPrototypeMessage(string actionName) =>
        $"Bad function prototype in function call to {actionName}";

    public static string FormatNullCallerMessage(string actionName, string expectedClass) =>
        $"{actionName} called without valid caller. {expectedClass} expected";

    public static string FormatInvalidClassMessage(string selfClass, string actionName, string expectedClass) =>
        $"Invalid class {selfClass} in function call to {actionName}. {expectedClass} expected";

    public readonly record struct StateReturnTracking(
        bool CallerProvidedStateReturn,
        bool ReturnTypeIsState,
        bool TrackStateReturnPointer);

    public static StateReturnTracking PrepareStateReturnTracking(
        bool callerProvidedStateReturn,
        bool hasReturnProto,
        int returnTypesCount,
        int firstReturnTypeTag)
    {
        if (!callerProvidedStateReturn)
            return new StateReturnTracking(false, false, false);

        var returnIsState = hasReturnProto
            && returnTypesCount > 0
            && firstReturnTypeTag == TypeStateReturn;

        return new StateReturnTracking(true, returnIsState, returnIsState);
    }

    public static bool ShouldInvokeCallAction(bool hasActionFunc) => hasActionFunc;

    public static bool ShouldUseDefaultArgsPath(int defaultArgsCount) => defaultArgsCount > 0;

    public static int ResolveVmCallParamCount(bool useDefaultArgsPath, int defaultArgsCount, int implicitArgs) =>
        useDefaultArgsPath ? defaultArgsCount : implicitArgs;

    public static void FillImplicitActionParams(
        Span<object?> target,
        int index,
        int implicitArgs,
        object? self,
        object? stateOwner,
        object? stateParamInfo)
    {
        if (implicitArgs >= 1)
            target[index] = self;
        if (implicitArgs == 3)
        {
            target[index + 1] = stateOwner;
            target[index + 2] = stateParamInfo;
        }
    }

    public static string SelectPspriteCallInfoPrefix(
        int? stateType,
        bool stateOwnerIsWeaponKind,
        bool stateOwnerDiffersFromSelf)
    {
        if (stateType != StatePsprite)
            return string.Empty;
        if (stateOwnerIsWeaponKind && stateOwnerDiffersFromSelf)
            return "weapon ";
        return "overlay ";
    }

    public static string FormatCallActionAbortStackTrace(
        string stateName,
        string? stateOwnerClass,
        string callInfoPrefix)
    {
        if (string.IsNullOrEmpty(stateOwnerClass))
            return $"Called from state {stateName}\n";

        return $"Called from {callInfoPrefix}state {stateName} in {stateOwnerClass}\n";
    }
}
