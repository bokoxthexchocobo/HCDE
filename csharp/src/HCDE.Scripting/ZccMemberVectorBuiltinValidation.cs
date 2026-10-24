namespace HCDE.Scripting;

/// <summary>Native <c>FxMemberFunctionCall::Resolve</c> vector/quaternion unary builtins.</summary>
public static class ZccMemberVectorBuiltinValidation
{
    public static bool TryValidateUnaryBuiltin(int argCount, string methodName, out string? error)
    {
        if (argCount > 0)
        {
            error = $"Too many parameters in call to {methodName}";
            return false;
        }

        error = null;
        return true;
    }

    public static bool TryValidatePlusZ(int argCount, out string? error)
    {
        if (argCount != 1)
        {
            error = "Incorrect number of parameters in call to PlusZ";
            return false;
        }

        error = null;
        return true;
    }
}
