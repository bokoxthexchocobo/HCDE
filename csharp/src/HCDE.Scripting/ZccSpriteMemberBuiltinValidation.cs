namespace HCDE.Scripting;

/// <summary>Native sprite-id member builtin compare constants (<c>codegen.cpp</c>).</summary>
public static class ZccSpriteMemberBuiltinValidation
{
    public static bool TryValidateZeroArg(int argCount, string methodName, out string? error) =>
        ZccMemberFunctionCallResolve.TryValidateZeroArgBuiltin(argCount, methodName, out error);

    public static bool TryGetEqualityConstant(string methodName, out int constant)
    {
        switch (methodName.ToLowerInvariant())
        {
            case "isempty":
                constant = 0;
                return true;
            case "isfixed":
                constant = 1;
                return true;
            case "iskeep":
                constant = 2;
                return true;
            default:
                constant = 0;
                return false;
        }
    }

    public static bool UsesGreaterOrEqualCompare(string methodName) =>
        methodName.Equals("IsValid", StringComparison.OrdinalIgnoreCase) ||
        methodName.Equals("Exists", StringComparison.OrdinalIgnoreCase);
}
