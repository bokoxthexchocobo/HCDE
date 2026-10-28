namespace HCDE.Scripting;

/// <summary>Native <c>FxMemberFunctionCall::Resolve</c> dynarray <c>Size</c> member redirect.</summary>
public static class ZccDynArraySizeMember
{
    public static bool IsSizeMember(string methodName) =>
        methodName.Equals("Size", StringComparison.OrdinalIgnoreCase);
}
