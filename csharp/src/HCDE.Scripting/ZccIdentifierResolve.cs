namespace HCDE.Scripting;

/// <summary><c>FxIdentifier::Resolve</c> and <c>ResolveMember</c> from <c>codegen.cpp</c> (6647–6983).</summary>
public static class ZccIdentifierResolve
{
    public const int Version411100 = 40011100;

    public enum LocalDispatch
    {
        None,
        StaticArray,
        LocalVariable,
        StackVariable,
    }

    public enum ResolvePath
    {
        Unknown,
        Local,
        SelfField,
        SelfFunctionConstant,
        ClassConstant,
        ClassFunctionConstant,
        AmbiguousSelfMember,
        GlobalConstant,
        GlobalVariable,
        Cvar,
        SpecialIdentifier,
        Error,
    }

    public static LocalDispatch ClassifyLocal(bool hasLocal, bool isStaticArray, bool hasRegType, int regTypeNil)
    {
        if (!hasLocal) return LocalDispatch.None;
        if (isStaticArray) return LocalDispatch.StaticArray;
        if (hasRegType && regTypeNil != 0) return LocalDispatch.LocalVariable;
        return LocalDispatch.StackVariable;
    }

    public static bool FieldFromConstantContext(bool inFunction) => !inFunction;

    public static bool VmFunctionConstantAllowed(int version, int threshold) => version >= threshold;

    public static bool ClassConstantLookup(bool foundInClass, bool isConst) => foundInClass && isConst;

    public static bool ClassFunctionWithoutParens(bool isFunction, int version, int threshold) =>
        isFunction && version >= threshold;

    public static bool AmbiguousSelfMember(
        bool hasFunction,
        bool selfClassDiffers,
        bool isField,
        bool hasSelfClass) =>
        hasFunction && selfClassDiffers && isField && hasSelfClass;

    public static bool FunctionUsedWithoutParens(bool isFunction) => isFunction;

    public static bool InvalidMemberInClass(bool isFunction) => !isFunction;

    public static bool NoGlobalLookup(bool noglobal) => noglobal;

    public static bool GlobalVersionAccessible(int symbolVersion, int ctxVersion) => symbolVersion <= ctxVersion;

    public static bool DeprecatedGlobalWarning(bool deprecated, bool versionOk) => deprecated && versionOk;

    public static bool UserinfoCvarForbidden(bool isUserinfo) => isUserinfo;

    public static bool MemberConstantOnContainer(bool isConst) => isConst;

    public static bool MemberFieldVersionOk(int fieldVersion, int ctxVersion) => fieldVersion <= ctxVersion;

    public static bool PrivateMemberDenied(bool isPrivate, bool sameSymbolTable) =>
        isPrivate && !sameSymbolTable;

    public static bool ProtectedMemberDenied(
        bool isProtected,
        bool hasClassCtx,
        bool hasTargetClass,
        bool descendant) =>
        isProtected && (!hasClassCtx || !hasTargetClass || !descendant);

    public static bool MemberFunctionRequiresCall(bool isFunction) => isFunction;

    public static ResolvePath PlanIdentifierResolve(
        LocalDispatch local,
        bool specialIdHook,
        bool selfField,
        bool selfFnConst,
        bool classConst,
        bool classFnConst,
        bool ambiguous,
        bool noglobal,
        bool globalConst,
        bool globalField,
        bool cvar,
        bool unknown)
    {
        if (local != LocalDispatch.None) return ResolvePath.Local;
        if (specialIdHook) return ResolvePath.SpecialIdentifier;
        if (selfField) return ResolvePath.SelfField;
        if (selfFnConst) return ResolvePath.SelfFunctionConstant;
        if (classConst) return ResolvePath.ClassConstant;
        if (classFnConst) return ResolvePath.ClassFunctionConstant;
        if (ambiguous) return ResolvePath.AmbiguousSelfMember;
        if (noglobal) return ResolvePath.Error;
        if (globalConst) return ResolvePath.GlobalConstant;
        if (globalField) return ResolvePath.GlobalVariable;
        if (cvar) return ResolvePath.Cvar;
        return unknown ? ResolvePath.Unknown : ResolvePath.Error;
    }

    public static bool ShouldDeleteOnError(ResolvePath path) =>
        path is ResolvePath.Error or ResolvePath.Unknown;

    public static bool ResolveMemberDeletesObject(bool found) => !found;

    public static int? FindDeclaringClassDepth(IReadOnlyList<int?> symbolTables, int targetTable)
    {
        for (var i = 0; i < symbolTables.Count; i++)
        {
            if (symbolTables[i] == targetTable) return i;
        }

        return null;
    }

    public static bool IsDescendantOf(bool ctxIsDescendant) => ctxIsDescendant;

    public static string DeprecationMessageSuffix(bool empty, string message) =>
        empty ? string.Empty : $", {message}";

    public static bool InternalLumpDeprecation(bool lumpIsInternal) => lumpIsInternal;

    public static bool MarkUnsafeContext(bool ambiguousSelf) => ambiguousSelf;

    public static bool StaticFunctionSeesGlobal(bool functionIsStatic, bool hasSelfClass) =>
        functionIsStatic || !hasSelfClass;

    public static bool ConstantDeclarationFieldAccess(bool inFunction, bool foundField) =>
        !inFunction && foundField;

    public static bool ResolveFoundExpression(bool hasNewExpression) => hasNewExpression;

    public static bool ChainedResolveAfterFound(bool newExpressionOk) => newExpressionOk;

    public static ResolvePath MemberResolvePath(
        bool foundOnObject,
        bool isConst,
        bool isField,
        bool isFunction,
        bool versionOk,
        bool privateOk,
        bool protectedOk)
    {
        if (!foundOnObject) return ResolvePath.Unknown;
        if (isConst) return ResolvePath.ClassConstant;
        if (isField && versionOk && privateOk && protectedOk) return ResolvePath.SelfField;
        if (isFunction) return ResolvePath.Error;
        return ResolvePath.Error;
    }

    public static bool ClassMemberFactory(bool isClass) => isClass;

    public static bool StructMemberFactory(bool isClass) => !isClass;

    public static bool ClearObjectAfterMember(bool clear) => clear;
}
