namespace HCDE.Scripting;

/// <summary>Native <c>FxMemberFunctionCall::Resolve</c> prologue (<c>codegen.cpp</c>).</summary>
public static class ZccMemberFunctionCallResolve
{
    public const int VarFlagOut = 1 << 4;

    public static bool RequiresEnclosingClass(bool hasEnclosingClass, out string? error)
    {
        if (hasEnclosingClass)
        {
            error = null;
            return true;
        }

        error = "Expression is not constant";
        return false;
    }

    public static bool TryMapStaticTypeName(string identifier, out string containerType)
    {
        switch (identifier.ToLowerInvariant())
        {
            case "string":
                containerType = "StringStruct";
                return true;
            case "quat":
            case "fquat":
                containerType = "QuaternionStruct";
                return true;
            default:
                containerType = string.Empty;
                return false;
        }
    }

    public static string ConstantContextCallError(string methodName) =>
        $"Unable to call {methodName} from constant declaration";

    public static bool TryValidateSoundIsValid(int argCount, out string? error)
    {
        if (argCount > 0)
        {
            error = "Too many parameters in call to IsValid";
            return false;
        }

        error = null;
        return true;
    }

    public static bool ShouldMarkReadonlyConstPointer(bool isConstPointer, int major, int minor, int patch) =>
        isConstPointer && PackScriptVersion(major, minor, patch) >= PackScriptVersion(4, 12, 0);

    public static int PackScriptVersion(int major, int minor, int patch) =>
        major * 1_000_000 + minor * 1_000 + patch;
}
