namespace HCDE.Scripting;

/// <summary><c>FxAssign::Resolve</c> from <c>codegen.cpp</c> (2685–2861).</summary>
public static class ZccAssignResolve
{
    public enum AssignOutcome
    {
        Continue,
        Fail,
        RewriteToCopyCall,
        WarnPointerSwapOnly,
    }

    public static bool BoolModifyAssignInvalid(bool isModifyAssign, bool baseIsBool, bool rightIsBool) =>
        isModifyAssign && baseIsBool && !rightIsBool;

    public static bool NeedsNumericPromotion(bool baseNumeric, bool rightNumeric, bool sameValueType) =>
        baseNumeric && rightNumeric && !sameValueType;

    public static bool NativeStructToMatchingPointer(bool rightNativeStruct, bool baseRealPointer, bool samePointedType) =>
        rightNativeStruct && baseRealPointer && samePointedType;

    public static bool SameTypeOrRealPointerAlias(
        bool sameType,
        bool baseRealPointerToRight,
        bool rightRealPointerToBase) =>
        sameType || baseRealPointerToRight || rightRealPointerToBase;

public static bool SkipVectorQuaternionPath(bool baseIsVector, bool baseIsQuaternion) =>
        baseIsVector || baseIsQuaternion;

    public static AssignOutcome DynArrayAssignPolicy(
        bool isDynArray,
        int version,
        bool bothRealPointers,
        int thresholdMajor,
        int thresholdMinor,
        int thresholdPatch)
    {
        if (!isDynArray) return AssignOutcome.Continue;
        if (version >= ZccTypeCastResolve.MakeVersion(thresholdMajor, thresholdMinor, thresholdPatch))
            return AssignOutcome.RewriteToCopyCall;
        return bothRealPointers ? AssignOutcome.WarnPointerSwapOnly : AssignOutcome.Fail;
    }

    public static AssignOutcome MapAssignPolicy(
        bool isMap,
        int version,
        bool bothRealPointers,
        int thresholdMajor,
        int thresholdMinor,
        int thresholdPatch)
    {
        if (!isMap) return AssignOutcome.Continue;
        if (version >= ZccTypeCastResolve.MakeVersion(thresholdMajor, thresholdMinor, thresholdPatch))
            return AssignOutcome.RewriteToCopyCall;
        return bothRealPointers ? AssignOutcome.WarnPointerSwapOnly : AssignOutcome.Fail;
    }

    public static AssignOutcome MapIteratorAssignPolicy(bool isMapIterator) =>
        isMapIterator ? AssignOutcome.Fail : AssignOutcome.Continue;

    public static AssignOutcome StructAssignPolicy(bool isNonNativeStruct, bool bothRealPointers)
    {
        if (!isNonNativeStruct) return AssignOutcome.Continue;
        return bothRealPointers ? AssignOutcome.WarnPointerSwapOnly : AssignOutcome.Fail;
    }

    public static bool RequiresWritableAddress(bool requestOk, bool writable) => requestOk && writable;

    public static bool ModifiableValueFails(bool requestOk, bool writable) => !requestOk || !writable;

    public static int NormalizeBitWriteIndex(int rawBitWrite)
    {
        if (rawBitWrite < 0) return rawBitWrite;
        return rawBitWrite >= 0x10000 ? rawBitWrite & 0xffff : rawBitWrite;
    }

    public static bool UsesDeprecatedFlagHandler(int bitWrite) => bitWrite >= 64;

    public static AssignOutcome ClassifyContainerAssign(
        bool isDynArray,
        bool isMap,
        bool isMapIterator,
        bool isNonNativeStruct,
        int version,
        bool bothRealPointers)
    {
        var iter = MapIteratorAssignPolicy(isMapIterator);
        if (iter != AssignOutcome.Continue) return iter;

        var dyn = DynArrayAssignPolicy(isDynArray, version, bothRealPointers, 4, 11, 1);
        if (dyn != AssignOutcome.Continue) return dyn;

        var map = MapAssignPolicy(isMap, version, bothRealPointers, 4, 11, 1);
        if (map != AssignOutcome.Continue) return map;

        return StructAssignPolicy(isNonNativeStruct, bothRealPointers);
    }

    public static bool FallsThroughToTypeCast(
        bool sameTypePathMatched,
        bool nativeStructPointerPath,
        bool numericPromotionPath) =>
        !sameTypePathMatched && !nativeStructPointerPath && !numericPromotionPath;
}
