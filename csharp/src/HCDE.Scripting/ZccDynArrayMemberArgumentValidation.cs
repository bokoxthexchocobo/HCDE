namespace HCDE.Scripting;

/// <summary>Native dynarray <c>Push</c>/<c>Insert</c> object-element type checks in <c>FxMemberFunctionCall::Resolve</c>.</summary>
public static class ZccDynArrayMemberArgumentValidation
{
    public static bool IsObjectPushArgument(string methodName, int argumentIndex) =>
        (methodName.Equals("Push", StringComparison.OrdinalIgnoreCase) && argumentIndex == 0) ||
        (methodName.Equals("Insert", StringComparison.OrdinalIgnoreCase) && argumentIndex == 1);

    public static bool ValidateObjectElementArgument(
        bool isObjectElementArray,
        bool argumentIsNullConstant,
        bool argumentIsObjectPointer,
        bool ancestorMatches,
        out string? error)
    {
        if (!isObjectElementArray)
        {
            error = null;
            return true;
        }

        if (argumentIsNullConstant)
        {
            error = null;
            return true;
        }

        if (!argumentIsObjectPointer || !ancestorMatches)
        {
            error = "Type mismatch in function argument";
            return false;
        }

        error = null;
        return true;
    }
}
