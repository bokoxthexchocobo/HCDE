namespace HCDE.Scripting;

/// <summary><c>FxTypeCast::Resolve</c> from <c>codegen.cpp</c> (1804–2017).</summary>
public static class ZccTypeCastResolve
{
    public static bool TrySpecialTypeCast(bool hasHook, bool hookReturnedReplacement) =>
        hasHook && hookReturnedReplacement;

    public static bool RawFunctionToFnPointer(bool baseConstant, bool baseRawFn, bool targetFnPtr) =>
        baseConstant && baseRawFn && targetFnPtr;

    public static bool RawFunctionToVmFunction(bool baseConstant, bool baseRawFn, bool targetVmFn) =>
        baseConstant && baseRawFn && targetVmFn;

    public static bool InvalidCastTypes(bool targetError, bool baseError, bool baseNull) =>
        targetError || baseError || baseNull;

    public static bool NullPtrToPointer(bool baseNullPtr, bool targetPointer) =>
        baseNullPtr && targetPointer;

    public static bool NullPtrToIdTypes(bool baseNullPtr, bool targetSprite, bool targetTexture, bool targetTranslation, int version, int threshold) =>
        baseNullPtr && (targetSprite || targetTexture || targetTranslation) && version >= threshold;

    public static int MakeVersion(int major, int minor, int patch) =>
        major * 10000 + minor * 100 + patch;

    public static bool VersionAtLeast(int version, int major, int minor, int patch) =>
        version >= MakeVersion(major, minor, patch);

    public static bool ClassDescendantAllowsCast(bool baseIsClass, bool targetIsClass, bool descendant) =>
        baseIsClass && targetIsClass && descendant;

    public static bool NativeStructToWritablePointer(bool baseNativeStruct, bool targetRealPointer, bool samePointedType, bool writable, bool targetConst, int version) =>
        baseNativeStruct && targetRealPointer && samePointedType
        && (writable || targetConst || version < MakeVersion(4, 12, 0));

    public static bool CompatibleVectors(bool baseV2, bool targetV2, bool baseV3, bool targetV3, bool baseV4, bool targetV4, bool baseQuat, bool targetQuat) =>
        (baseV2 && targetV2) || (baseV3 && targetV3) || (baseV4 && targetV4) || (baseQuat && targetQuat);

    public static bool IntegerPromotesToSpriteId(bool targetSprite, bool baseInteger) => targetSprite && baseInteger;

    public static bool IntegerPromotesToTextureId(bool targetTexture, bool baseInteger) => targetTexture && baseInteger;

    public const int VarfVirtual = 1 << 0;
    public const int VarfAction = 1 << 1;
    public const int VarfMethod = 1 << 2;

    public enum RawFunctionPointerPlan
    {
        RejectVirtualOrAction,
        PlainFunction,
        MethodFunction,
        NullFunction,
    }

    /// <summary>Mirrors <c>convertRawFunctionToFunctionPointer</c> (codegen.cpp 1775–1802).</summary>
    public static RawFunctionPointerPlan PlanRawFunctionPointer(bool fnNull, int variantFlags)
    {
        if (fnNull) return RawFunctionPointerPlan.NullFunction;
        var kind = variantFlags & (VarfVirtual | VarfAction | VarfMethod);
        if (kind == 0) return RawFunctionPointerPlan.PlainFunction;
        if (kind == VarfMethod) return RawFunctionPointerPlan.MethodFunction;
        return RawFunctionPointerPlan.RejectVirtualOrAction;
    }

    public static bool MethodPointerClearsThisArg(int variantFlags) =>
        (variantFlags & (VarfVirtual | VarfAction | VarfMethod)) == VarfMethod;

    public static bool RejectsVirtualActionPointer(RawFunctionPointerPlan plan) =>
        plan == RawFunctionPointerPlan.RejectVirtualOrAction;

    public enum PointerKind
    {
        Other,
        Null,
        VoidPtr,
        Object,
        Class,
        Function,
        Real,
    }

    public readonly record struct PointerDesc(
        PointerKind Kind,
        bool IsConst,
        bool SourceDescendantOfDest,
        bool DestDescendantOfSource,
        bool SamePointedType,
        bool DestIsVoidFn,
        bool SourceFnVoid,
        bool FnProtosMatch,
        bool FnArgFlagsMatch,
        bool FnScopeBarrierOk);

    /// <summary>Managed mirror of <c>AreCompatiblePointerTypes</c> (codegen.cpp 267–308).</summary>
    public static bool CompatiblePointerTypes(PointerDesc dest, PointerDesc source, bool forCompare)
    {
        if (dest.Kind == PointerKind.Other || source.Kind == PointerKind.Other) return false;
        if (source.Kind == PointerKind.Null || dest.Kind == PointerKind.VoidPtr) return true;
        if (!forCompare && source.IsConst && !dest.IsConst) return false;
        if (source.SamePointedType && dest.SamePointedType) return true;
        if (source.Kind == PointerKind.Object && dest.Kind == PointerKind.Object)
        {
            if (forCompare && dest.DestDescendantOfSource) return true;
            return source.SourceDescendantOfDest;
        }
        if (source.Kind == PointerKind.Class && dest.Kind == PointerKind.Class)
        {
            if (forCompare && dest.DestDescendantOfSource) return true;
            return source.SourceDescendantOfDest;
        }
        if (source.Kind == PointerKind.Function && dest.Kind == PointerKind.Function)
        {
            if (source.SourceFnVoid) return false;
            return dest.DestIsVoidFn
                || (source.FnProtosMatch && source.FnArgFlagsMatch && source.FnScopeBarrierOk);
        }
        if (source.Kind == PointerKind.Real && dest.Kind == PointerKind.Real)
            return source.SamePointedType;
        return false;
    }
}
