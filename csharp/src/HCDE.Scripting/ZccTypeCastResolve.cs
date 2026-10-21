namespace HCDE.Scripting;

/// <summary><c>FxTypeCast::Resolve</c> from <c>codegen.cpp</c> (1804–2017).</summary>
public static class ZccTypeCastResolve
{
    public static bool SpecialTypeCastHook(bool hasHook) => hasHook;

    public static bool HookReplacedCast(bool resultNotSelf) => resultNotSelf;

    public static bool RawFunctionToFnPointer(bool baseConstant, bool baseRawFn, bool targetFnPtr) =>
        baseConstant && baseRawFn && targetFnPtr;

    public static bool RawFunctionToVmFunction(bool baseConstant, bool baseRawFn, bool targetVmFn) =>
        baseConstant && baseRawFn && targetVmFn;

    public static bool TargetTypeError(bool targetError) => targetError;

    public static bool BaseTypeError(bool baseError, bool baseNull) => baseError || baseNull;

    public static bool TargetVoid(bool targetVoid) => targetVoid;

    public static bool BaseVoid(bool baseVoid) => baseVoid;

    public static bool IdenticalTypes(bool same) => same;

    public static bool NullPtrToPointer(bool baseNullPtr, bool targetPointer) =>
        baseNullPtr && targetPointer;

    public static bool NullPtrToIdTypes(bool baseNullPtr, bool targetSprite, bool targetTexture, bool targetTranslation, int version, int threshold) =>
        baseNullPtr && (targetSprite || targetTexture || targetTranslation) && version >= threshold;

    public static bool IsFloatCast(bool isFloat) => isFloat;

    public static bool TargetIsInt(bool targetInt) => targetInt;

    public static bool TargetIsBool(bool targetBool) => targetBool;

    public static bool TargetIsString(bool targetString) => targetString;

    public static bool TargetIsName(bool targetName) => targetName;

    public static bool TargetIsSound(bool targetSound) => targetSound;

    public static bool TargetIsTranslation(bool targetTranslation) => targetTranslation;

    public static bool TargetIsColor(bool targetColor) => targetColor;

    public static bool TargetIsVector2(bool targetV2) => targetV2;

    public static bool TargetIsVector3(bool targetV3) => targetV3;

    public static bool TargetIsStateLabel(bool targetState) => targetState;

    public static bool TargetIsPointer(bool targetPointer) => targetPointer;

    public static bool ExplicitCast(bool explicitCast) => explicitCast;

    public static bool NoWarn(bool noWarn) => noWarn;

    public static bool UnsignedIntTarget(bool unsignedTarget) => unsignedTarget;

    public static bool ShouldDeleteSelfAfterDelegate() => true;

    public static bool ShouldReturnBaseUnchanged(bool identical) => identical;

    public static bool ReplaceBaseWithZeroConstant() => true;

    public static int MakeVersion(int major, int minor, int patch) =>
        major * 10000 + minor * 100 + patch;

    public static bool VersionAtLeast(int version, int major, int minor, int patch) =>
        version >= MakeVersion(major, minor, patch);

    public static bool ResolveFailed() => false;

    public static bool EmitErrorOnInvalidCast() => true;

    public static bool VmFunctionFromImplementation(bool hasImpl) => hasImpl;

    public static bool PointerDecayAllowed(bool basePointer, bool targetPointer) =>
        basePointer && targetPointer;

    public static bool ClassCastRequiresExplicit(bool explicitCast) => explicitCast;

    public static bool DynamicCastPath(bool dynamic) => dynamic;

    public static bool StateLabelCast(bool targetState) => targetState;

    public static bool ColorCast(bool targetColor) => targetColor;

    public static bool VectorCast(bool targetVector) => targetVector;

    public static bool SoundCast(bool targetSound) => targetSound;

    public static bool TranslationCast(bool targetTranslation) => targetTranslation;

    public static bool NameCastRequiresExplicit(bool explicitCast) => explicitCast;

    public static bool StringCast(bool targetString) => targetString;

    public static bool BoolCast(bool targetBool) => targetBool;

    public static bool IntCast(bool targetInt) => targetInt;

    public static bool FloatCast(bool isFloat) => isFloat;

    public static bool PointerTypesNeedAddress(bool pointerCompare) => pointerCompare;
}
