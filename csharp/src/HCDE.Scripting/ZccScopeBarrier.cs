namespace HCDE.Scripting;

/// <summary>Native <c>FScopeBarrier</c> call validation subset from <c>FxFunctionCall::Resolve</c>.</summary>
public static class ZccScopeBarrier
{
    public const int FlagPrivate = 1 << 0;
    public const int FlagProtected = 1 << 1;
    public const int FlagVirtualScope = 1 << 2;
    public const int FlagVirtual = 1 << 3;

    public static bool IsPrivateToOtherClass(int memberFlags, bool memberSymbolBelongsToDefiningClass) =>
        (memberFlags & FlagPrivate) != 0 && !memberSymbolBelongsToDefiningClass;

    public static bool IsProtectedFromCaller(
        int memberFlags,
        bool memberSymbolBelongsToDefiningClass,
        bool callerIsDescendantOfMemberClass) =>
        (memberFlags & FlagProtected) != 0 &&
        !memberSymbolBelongsToDefiningClass &&
        !callerIsDescendantOfMemberClass;

    public static bool TryValidateMemberCall(
        int memberFlags,
        bool memberSymbolBelongsToDefiningClass,
        bool callerIsDescendantOfMemberClass,
        string memberName,
        out string? error)
    {
        if (IsPrivateToOtherClass(memberFlags, memberSymbolBelongsToDefiningClass))
        {
            error = $"{memberName} is declared private and not accessible";
            return false;
        }

        if (IsProtectedFromCaller(memberFlags, memberSymbolBelongsToDefiningClass, callerIsDescendantOfMemberClass))
        {
            error = $"{memberName} is declared protected and not accessible";
            return false;
        }

        error = null;
        return true;
    }

    public static bool IsCallableFromContext(int outerFlags, int innerFlags)
    {
        var outerVirtual = (outerFlags & (FlagVirtualScope | FlagVirtual)) == (FlagVirtualScope | FlagVirtual);
        if (outerVirtual)
            return true;
        return (outerFlags & FlagPrivate) == 0 || (innerFlags & FlagPrivate) == 0;
    }
}
