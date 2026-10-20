namespace HCDE.Scripting;

/// <summary>Native <c>FxMemberIdentifier::Resolve</c> prologue and member-access gates (<c>codegen.cpp</c>).</summary>
public static class ZccMemberIdentifierResolve
{
    public const string ColorValueType = "Color";
    public const string ColorStructValueType = "ColorStruct";

    /// <summary>Maps <c>TypeColor</c> to <c>TypeColorStruct</c> before member lookup.</summary>
    public static string RemapColorMemberTarget(string objectValueType) =>
        objectValueType.Equals(ColorValueType, StringComparison.OrdinalIgnoreCase)
            ? ColorStructValueType
            : objectValueType;

    public static bool TryFindContainerType(
        string identifier,
        IReadOnlyDictionary<string, string> containerTypes,
        out string containerType)
    {
        if (containerTypes.TryGetValue(identifier, out containerType!))
            return true;
        containerType = string.Empty;
        return false;
    }

    public static bool IsStructMemberAccess(string valueType) =>
        valueType.EndsWith("Struct", StringComparison.Ordinal) ||
        valueType.Equals(ColorStructValueType, StringComparison.OrdinalIgnoreCase);

    public static bool IsPointerToContainer(string valueType) =>
        valueType.EndsWith("*", StringComparison.Ordinal);

    public static bool IsValidMemberAccessTarget(string valueType) =>
        IsStructMemberAccess(valueType) || IsPointerToContainer(valueType);

    public static string StaticContextError(string containerName, string memberName) =>
        $"Unable to access '{containerName}.{memberName}' in a static context";

    public static string NotAMemberError(string memberName, string containerName) =>
        $"{memberName} is not a member of {containerName}";

    public static string LeftSideNotStructError(string memberName) =>
        $"Left side of {memberName} is not a struct or class";
}
