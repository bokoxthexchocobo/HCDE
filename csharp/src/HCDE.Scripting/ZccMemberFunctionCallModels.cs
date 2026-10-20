namespace HCDE.Scripting;

public enum ZccMemberSelfExpressionKind
{
    Unknown,
    Identifier,
    Super,
    StructMember,
    ClassMember,
    StackVariable,
}

public enum ZccMemberSelfValueKind
{
    Error,
    Numeric,
    Sound,
    TextureId,
    TranslationId,
    SpriteId,
    Vector2,
    Vector3,
    Quaternion,
    String,
    DynamicArray,
    Pointer,
}

public enum ZccMemberFunctionCallPhase
{
    Continue,
    Finished,
    Failed,
}

public enum ZccMemberBuiltinRewriteKind
{
    None,
    NumericToVector,
    SoundIsValidCompare,
    TextureCompareOrAssign,
    SpriteCompareOrAssign,
    VectorUnaryBuiltin,
    VectorPlusZ,
    QuaternionUnaryBuiltin,
    StringLength,
    DynamicArraySizeRedirect,
}

public sealed class ZccMemberFunctionCallRequest
{
    public bool HasEnclosingClass { get; init; }
    public bool HasEnclosingFunction { get; init; }
    public int ScriptMajor { get; init; }
    public int ScriptMinor { get; init; }
    public int ScriptPatch { get; init; }
    public string MethodName { get; init; } = string.Empty;
    public ZccMemberSelfExpressionKind SelfExpressionKind { get; set; }
    public ZccMemberSelfValueKind SelfValueKind { get; set; }
    public bool SelfResolvedToError { get; set; }
    public string? StaticIdentifierName { get; init; }
    public string? ResolvedContainerTypeName { get; set; }
    public bool StaticContainerIsClass { get; init; }
    public bool EnclosingClassDescendsFromStaticContainer { get; init; }
    public bool StaticContainerHasVirtualMethod { get; init; }
    public int ArgumentCount { get; init; }
    public IReadOnlyList<object?>? Arguments { get; init; }
    public bool IsReadonlyConstPointer { get; set; }
    public bool NeedsOutVarDereferenceOnSelf { get; set; }
    public bool StaticOnly { get; set; }
    public bool NoVirtual { get; set; }
    public bool ReboundSelfToEnclosingInstance { get; set; }
    public string? SuperParentTypeName { get; set; }
    public ZccMemberBuiltinRewriteKind BuiltinRewrite { get; set; }
    public string? RemappedSelfStructType { get; set; }
    public IReadOnlyList<ZccDynArrayArgumentState>? DynArrayArguments { get; set; }
    public string? DynArrayElementTypeName { get; init; }
    public bool DynArrayElementIsObjectPointer { get; init; }
    public string? DynArrayBackingTypeName { get; init; }
    public string? Error { get; set; }
}

public sealed class ZccDynArrayArgumentState
{
    public int Index { get; init; }
    public bool IsNullConstant { get; init; }
    public bool IsObjectPointer { get; init; }
    public bool AncestorMatches { get; init; }
    public bool IsDynamicArrayValue { get; init; }
    public bool ElementTypeMatches { get; init; }
    public bool CanRequestAddress { get; init; }
    public bool IsPointerValue { get; init; }
    public bool PointerTypeMatchesElement { get; init; }
    public string? ArgumentTypeName { get; init; }
    public bool NeedsOutVarDereference { get; init; }
    public bool PromotedToBackingPointer { get; set; }
    public bool MemberFieldSwappedToBacking { get; set; }
}
