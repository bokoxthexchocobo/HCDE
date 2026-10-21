namespace HCDE.Playsim;

/// <summary>
/// View-roll ref guard and user field lookup validation from <c>p_actionfunctions.cpp</c>
/// (<c>SetViewRollNative</c>, <c>GetVar</c>, <c>GetArrayVar</c>).
/// </summary>
public static class VmActionUserFieldAccess
{
    public const int VarfNative = 1 << 0;
    public const int VarfPrivate = 1 << 1;
    public const int VarfProtected = 1 << 2;
    public const int VarfStatic = 1 << 3;

    public static bool SetViewRollOnRef(object? refActor) => refActor is not null;

    public static bool IsBlockedUserFieldFlag(int flags) =>
        (flags & (VarfNative | VarfPrivate | VarfProtected | VarfStatic)) != 0;

    public static bool IsUserScalarField(bool found, int flags, bool isScalar) =>
        found && !IsBlockedUserFieldFlag(flags) && isScalar;

    public static bool ShouldSetUserScalar(bool fieldOk) => fieldOk;

    public static bool IsUserArrayField(bool found, int flags, bool isArray, bool elementIsScalar) =>
        found
        && !IsBlockedUserFieldFlag(flags)
        && isArray
        && elementIsScalar;

    public static bool ArrayIndexInBounds(int pos, int elementCount) =>
        (uint)pos < (uint)elementCount;

    public static bool ShouldSetUserArrayElement(bool fieldOk, bool indexOk) =>
        fieldOk && indexOk;
}
