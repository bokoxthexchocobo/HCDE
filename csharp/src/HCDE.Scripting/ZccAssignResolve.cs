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

    public readonly record struct AssignResolveInput(
        bool IsModifyAssign,
        bool BaseIsBool,
        bool RightIsBool,
        bool BaseNumeric,
        bool RightNumeric,
        bool SameValueType,
        bool ValueTypeIsBool,
        bool RightIsNativeStruct,
        bool BaseRealPointer,
        bool SamePointedType,
        bool SameType,
        bool BaseRealPointerToRight,
        bool RightRealPointerToBase,
        bool BaseIsArray,
        bool BaseIsVector,
        bool BaseIsQuaternion,
        bool IsDynArray,
        bool IsMap,
        bool IsMapIterator,
        bool IsNonNativeStruct,
        int Version,
        bool BothRealPointers,
        bool RequestAddressOk,
        bool Writable,
        int RawBitWrite);

    public readonly record struct AssignResolveResult(
        AssignOutcome Outcome,
        bool UseNumericPromotion,
        bool PromoteToBoolCast,
        bool PromoteToIntCast,
        bool PromoteToFloatCast,
        bool NativeStructPointerPath,
        bool SameTypePath,
        bool UseTypeCastFallback,
        bool ArrayAssignBlocked,
        int NormalizedBitWrite);

    public static AssignOutcome ArrayAssignPolicy(bool baseIsArray) =>
        baseIsArray ? AssignOutcome.Fail : AssignOutcome.Continue;

    public static AssignOutcome ResolvePointedContainerAssign(AssignResolveInput input)
    {
        if (SkipVectorQuaternionPath(input.BaseIsVector, input.BaseIsQuaternion))
            return AssignOutcome.Continue;
        return ClassifyContainerAssign(
            input.IsDynArray,
            input.IsMap,
            input.IsMapIterator,
            input.IsNonNativeStruct,
            input.Version,
            input.BothRealPointers);
    }

    public static AssignResolveResult RunAssignResolvePipeline(AssignResolveInput input)
    {
        if (BoolModifyAssignInvalid(input.IsModifyAssign, input.BaseIsBool, input.RightIsBool))
            return new AssignResolveResult(AssignOutcome.Fail, false, false, false, false, false, false, false, false, input.RawBitWrite);

        var numericPromotion = NeedsNumericPromotion(input.BaseNumeric, input.RightNumeric, input.SameValueType);
        var promoteBool = numericPromotion && input.ValueTypeIsBool;
        var promoteInt = numericPromotion && !input.ValueTypeIsBool;
        var promoteFloat = false;

        var nativeStructPath = NativeStructToMatchingPointer(
            input.RightIsNativeStruct,
            input.BaseRealPointer,
            input.SamePointedType);

        var sameTypePath = SameTypeOrRealPointerAlias(
            input.SameType,
            input.BaseRealPointerToRight,
            input.RightRealPointerToBase);

        if (sameTypePath)
        {
            var array = ArrayAssignPolicy(input.BaseIsArray);
            if (array != AssignOutcome.Continue)
                return new AssignResolveResult(array, numericPromotion, promoteBool, promoteInt, promoteFloat, nativeStructPath, true, false, true, input.RawBitWrite);

            var container = ResolvePointedContainerAssign(input);
            if (container != AssignOutcome.Continue)
                return new AssignResolveResult(container, numericPromotion, promoteBool, promoteInt, promoteFloat, nativeStructPath, true, false, false, input.RawBitWrite);
        }

        var useTypeCast = FallsThroughToTypeCast(sameTypePath, nativeStructPath, numericPromotion);
        if (ModifiableValueFails(input.RequestAddressOk, input.Writable))
            return new AssignResolveResult(AssignOutcome.Fail, numericPromotion, promoteBool, promoteInt, promoteFloat, nativeStructPath, sameTypePath, useTypeCast, false, input.RawBitWrite);

        return new AssignResolveResult(
            AssignOutcome.Continue,
            numericPromotion,
            promoteBool,
            promoteInt,
            promoteFloat,
            nativeStructPath,
            sameTypePath,
            useTypeCast,
            false,
            NormalizeBitWriteIndex(input.RawBitWrite));
    }
}
