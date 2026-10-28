namespace HCDE.Gamedata;

/// <summary>
/// <c>PActorInfo</c>, <c>AddActorInfo</c>, and <c>FState::MatchesCallerType</c> from <c>info.cpp</c> (**1–150**).
/// Chunk **1040**; follows **1039** tail at EOF **1043**. Defers **800–893** without padding.
/// </summary>
public static class VmInfoActorState1040Slice
{
    public const int ActorInfoSizeBytes = 256;
    public const int ActorInfoAlignBytes = 8;
    public const uint StateDehackedFlag = 1u << 0;

    public readonly record struct StateCallerMatchContext(
        int ImplicitArgs,
        bool RequiredArgIsObjectPointer,
        string? RequiredArgClass,
        bool SecondArgIsObjectPointer,
        string? SecondArgClass,
        uint StateFlags);

    public static bool ShouldSkipCallerTypeMatch(int implicitArgs) => implicitArgs < 1;

    public static bool ShouldUseCopyActorInfoDefault(ReadOnlySpan<byte> defBytes) => !defBytes.IsEmpty;

    public static void ConstructActorInfoAtOffset(Span<byte> baseBytes, int offset, ReadOnlySpan<byte> defaultBytes)
    {
        if (ShouldUseCopyActorInfoDefault(defaultBytes))
            defaultBytes.CopyTo(baseBytes.Slice(offset, Math.Min(defaultBytes.Length, ActorInfoSizeBytes)));
    }

    public static void DestroyActorInfoAtOffset(int offset) => _ = offset;

    public static bool ShouldRegisterActorInfoMetaField(string fieldName, bool isMeta) =>
        isMeta && fieldName == "*";

    public static bool MatchesObjectPointerKind(bool isObjectPointer) => isObjectPointer;

    public static bool MatchesActorKind(string? actorClass, string? requiredClass, bool dehackedState)
    {
        if (dehackedState)
            return true;
        if (string.IsNullOrEmpty(actorClass) || string.IsNullOrEmpty(requiredClass))
            return false;
        return actorClass.Equals(requiredClass, StringComparison.OrdinalIgnoreCase)
            || actorClass.EndsWith(':' + requiredClass, StringComparison.OrdinalIgnoreCase);
    }

    public static bool MatchesCallerType(
        StateCallerMatchContext ctx,
        string? selfClass,
        string? stateOwnerClass)
    {
        if (ShouldSkipCallerTypeMatch(ctx.ImplicitArgs))
            return true;

        if (!MatchesObjectPointerKind(ctx.RequiredArgIsObjectPointer))
            return false;

        if (!MatchesActorKind(selfClass, ctx.RequiredArgClass, (ctx.StateFlags & StateDehackedFlag) != 0))
            return false;

        if (ctx.ImplicitArgs >= 2)
        {
            if (!MatchesObjectPointerKind(ctx.SecondArgIsObjectPointer))
                return false;
            return MatchesActorKind(
                stateOwnerClass,
                ctx.SecondArgClass ?? ctx.RequiredArgClass,
                (ctx.StateFlags & StateDehackedFlag) != 0);
        }

        return true;
    }

    public static bool ShouldAbortCheckCallerOnBadPrototype(bool firstArgIsObjectPointer) =>
        !firstArgIsObjectPointer;

    public static bool CheckCallerTypePasses(
        StateCallerMatchContext ctx,
        string? selfClass,
        string? stateOwnerClass) =>
        MatchesCallerType(ctx, selfClass, stateOwnerClass);
}
