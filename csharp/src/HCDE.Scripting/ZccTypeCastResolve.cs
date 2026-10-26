namespace HCDE.Scripting;

/// <summary><c>FxTypeCast::Resolve</c> from <c>codegen.cpp</c> (1804–2017).</summary>
public static class ZccTypeCastResolve
{
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

    public enum ResolveCastPath
    {
        Error,
        SameType,
        NullPtrToPointer,
        NullPtrToIdConstant,
        CompatiblePointers,
        ClassDescendant,
        NativeStructWritable,
        CompatibleVectorsPassThrough,
        Unsupported,
    }

    /// <summary>Resolve branches before Fx* cast nodes (codegen.cpp 1831–2000).</summary>
    public static ResolveCastPath ClassifyResolveCast(
        bool targetError,
        bool baseError,
        bool baseVoid,
        bool sameType,
        bool baseNullPtr,
        bool targetPointer,
        bool targetSprite,
        bool targetTexture,
        bool targetTranslation,
        int version,
        bool pointersCompatible,
        bool baseIsClass,
        bool targetIsClass,
        bool classDescendant,
        bool nativeStruct,
        bool targetRealPointer,
        bool samePointedNativeStruct,
        bool writable,
        bool targetConst,
        bool vectorMatch)
    {
        if (targetError || baseError || baseVoid) return ResolveCastPath.Error;
        if (sameType) return ResolveCastPath.SameType;
        if (baseNullPtr && targetPointer) return ResolveCastPath.NullPtrToPointer;
        if (baseNullPtr && (targetSprite || targetTexture || targetTranslation)
            && version >= MakeVersion(4, 14, 1))
            return ResolveCastPath.NullPtrToIdConstant;
        if (pointersCompatible) return ResolveCastPath.CompatiblePointers;
        if (baseIsClass && targetIsClass && classDescendant) return ResolveCastPath.ClassDescendant;
        if (nativeStruct && targetRealPointer && samePointedNativeStruct
            && (writable || targetConst || version < MakeVersion(4, 12, 0)))
            return ResolveCastPath.NativeStructWritable;
        if (vectorMatch) return ResolveCastPath.CompatibleVectorsPassThrough;
        return ResolveCastPath.Unsupported;
    }

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

    public static bool ShouldConvertRawToFunctionPointer(bool baseConstant, bool baseRawFunction, bool targetFunctionPointer) =>
        baseConstant && baseRawFunction && targetFunctionPointer;

    public static bool ShouldConvertRawToVmFunction(bool baseConstant, bool baseRawFunction, bool targetVmFunction) =>
        baseConstant && baseRawFunction && targetVmFunction;

    public static bool InvalidCastTargetOrBase(bool targetError, bool baseError, bool baseTypeNull) =>
        targetError || baseError || baseTypeNull;

    public static bool TargetIsVoid(bool targetVoid) => targetVoid;

    public static bool BaseIsVoid(bool baseVoid) => baseVoid;

    public enum CastDelegateKind
    {
        None,
        Float,
        Int,
        Bool,
        String,
        Name,
        Sound,
        Translation,
        Color,
        ClassPointer,
        Font,
    }

    public static CastDelegateKind SelectSecondaryCastDelegate(
        bool targetFloat,
        bool targetInt,
        bool targetBool,
        bool targetString,
        bool targetName,
        bool targetSound,
        bool targetTranslation,
        bool targetColor,
        bool targetClassPointer,
        bool targetFont)
    {
        if (targetFloat) return CastDelegateKind.Float;
        if (targetInt) return CastDelegateKind.Int;
        if (targetBool) return CastDelegateKind.Bool;
        if (targetString) return CastDelegateKind.String;
        if (targetName) return CastDelegateKind.Name;
        if (targetSound) return CastDelegateKind.Sound;
        if (targetTranslation) return CastDelegateKind.Translation;
        if (targetColor) return CastDelegateKind.Color;
        if (targetClassPointer) return CastDelegateKind.ClassPointer;
        if (targetFont) return CastDelegateKind.Font;
        return CastDelegateKind.None;
    }

    public static bool PromoteIntegerToSpriteId(bool targetSprite, bool baseInteger) =>
        targetSprite && baseInteger;

    public static bool PromoteIntegerToTextureId(bool targetTexture, bool baseInteger) =>
        targetTexture && baseInteger;

    public static bool NativeStructReadonlyBlocksWritable(bool writable, bool targetConst, int version) =>
        !writable && !targetConst && version >= MakeVersion(4, 12, 0);

    public enum TypeCastResolveStage
    {
        FailAndDelete,
        BaseReturn,
        NullIdConstant,
        DelegateToCastNode,
        UnsupportedError,
    }

    public readonly record struct TypeCastResolveInput(
        bool BaseConstant,
        bool BaseRawFunction,
        bool FnNull,
        int VariantFlags,
        bool TargetFunctionPointer,
        bool TargetVmFunction,
        bool TargetError,
        bool BaseError,
        bool BaseTypeNull,
        bool TargetVoid,
        bool BaseVoid,
        bool SameType,
        bool BaseNullPtr,
        bool TargetPointer,
        bool TargetSprite,
        bool TargetTexture,
        bool TargetTranslation,
        int Version,
        bool PointersCompatible,
        bool BaseIsClass,
        bool TargetIsClass,
        bool ClassDescendant,
        bool NativeStruct,
        bool TargetRealPointer,
        bool SamePointedNativeStruct,
        bool Writable,
        bool TargetConst,
        bool VectorMatch,
        bool TargetFloat,
        bool TargetInt,
        bool TargetBool,
        bool TargetString,
        bool TargetName,
        bool TargetSound,
        bool TargetColor,
        bool TargetClassPointer,
        bool TargetFont,
        bool BaseInteger,
        bool TargetSpriteId,
        bool TargetTextureId);

    public static (TypeCastResolveStage Stage, CastDelegateKind Delegate, ResolveCastPath ClassifyPath)
        RunTypeCastResolvePipeline(TypeCastResolveInput input)
    {
        if (ShouldConvertRawToFunctionPointer(input.BaseConstant, input.BaseRawFunction, input.TargetFunctionPointer))
        {
            var raw = PlanRawFunctionPointer(input.FnNull, input.VariantFlags);
            if (raw == RawFunctionPointerPlan.RejectVirtualOrAction)
                return (TypeCastResolveStage.FailAndDelete, CastDelegateKind.None, ResolveCastPath.Error);
            return (TypeCastResolveStage.BaseReturn, CastDelegateKind.None, ResolveCastPath.SameType);
        }

        if (ShouldConvertRawToVmFunction(input.BaseConstant, input.BaseRawFunction, input.TargetVmFunction))
            return (TypeCastResolveStage.BaseReturn, CastDelegateKind.None, ResolveCastPath.SameType);

        if (InvalidCastTargetOrBase(input.TargetError, input.BaseError, input.BaseTypeNull))
            return (TypeCastResolveStage.FailAndDelete, CastDelegateKind.None, ResolveCastPath.Error);
        if (TargetIsVoid(input.TargetVoid) || BaseIsVoid(input.BaseVoid))
            return (TypeCastResolveStage.UnsupportedError, CastDelegateKind.None, ResolveCastPath.Error);
        if (input.SameType)
            return (TypeCastResolveStage.BaseReturn, CastDelegateKind.None, ResolveCastPath.SameType);
        if (NullPtrToPointer(input.BaseNullPtr, input.TargetPointer))
            return (TypeCastResolveStage.BaseReturn, CastDelegateKind.None, ResolveCastPath.NullPtrToPointer);
        if (NullPtrToIdTypes(
                input.BaseNullPtr,
                input.TargetSprite,
                input.TargetTexture,
                input.TargetTranslation,
                input.Version,
                MakeVersion(4, 14, 1)))
            return (TypeCastResolveStage.NullIdConstant, CastDelegateKind.None, ResolveCastPath.NullPtrToIdConstant);

        var secondary = SelectSecondaryCastDelegate(
            input.TargetFloat,
            input.TargetInt,
            input.TargetBool,
            input.TargetString,
            input.TargetName,
            input.TargetSound,
            input.TargetTranslation,
            input.TargetColor,
            input.TargetClassPointer,
            input.TargetFont);
        if (secondary != CastDelegateKind.None)
            return (TypeCastResolveStage.DelegateToCastNode, secondary, ResolveCastPath.Unsupported);

        if (PromoteIntegerToSpriteId(input.TargetSpriteId, input.BaseInteger))
            return (TypeCastResolveStage.BaseReturn, CastDelegateKind.None, ResolveCastPath.SameType);
        if (PromoteIntegerToTextureId(input.TargetTextureId, input.BaseInteger))
            return (TypeCastResolveStage.BaseReturn, CastDelegateKind.None, ResolveCastPath.SameType);

        var path = ClassifyResolveCast(
            input.TargetError,
            input.BaseError,
            input.BaseVoid,
            input.SameType,
            input.BaseNullPtr,
            input.TargetPointer,
            input.TargetSprite,
            input.TargetTexture,
            input.TargetTranslation,
            input.Version,
            input.PointersCompatible,
            input.BaseIsClass,
            input.TargetIsClass,
            input.ClassDescendant,
            input.NativeStruct,
            input.TargetRealPointer,
            input.SamePointedNativeStruct,
            input.Writable,
            input.TargetConst,
            input.VectorMatch);

        if (path == ResolveCastPath.NativeStructWritable
            && NativeStructReadonlyBlocksWritable(input.Writable, input.TargetConst, input.Version))
            return (TypeCastResolveStage.FailAndDelete, CastDelegateKind.None, path);

        return path switch
        {
            ResolveCastPath.Error => (TypeCastResolveStage.FailAndDelete, CastDelegateKind.None, path),
            ResolveCastPath.SameType or ResolveCastPath.NullPtrToPointer or ResolveCastPath.NullPtrToIdConstant
                or ResolveCastPath.CompatiblePointers or ResolveCastPath.ClassDescendant
                or ResolveCastPath.NativeStructWritable or ResolveCastPath.CompatibleVectorsPassThrough
                => (TypeCastResolveStage.BaseReturn, CastDelegateKind.None, path),
            _ => (TypeCastResolveStage.UnsupportedError, CastDelegateKind.None, path),
        };
    }

    public static PointerDesc DescribeObjectPointer(bool isConst, bool sourceDescendant, bool destDescendant, bool samePointed) =>
        new(PointerKind.Object, isConst, sourceDescendant, destDescendant, samePointed, false, false, false, false, false);

    public static bool ObjectPointersCompatibleForCast(PointerDesc dest, PointerDesc source, bool forCompare) =>
        CompatiblePointerTypes(dest, source, forCompare);
}
