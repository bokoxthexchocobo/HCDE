namespace HCDE.Scripting;

/// <summary>Native <c>FxFunctionCall::Resolve</c> class-member branch (<c>codegen.cpp</c>).</summary>
public static class ZccFunctionCallClassResolve
{
    public static bool TryValidateArguments(IReadOnlyList<object?> arguments, out int nullIndex)
    {
        if (ZccFunctionCallValidation.HasNullArgument(arguments, out nullIndex))
            return false;
        nullIndex = -1;
        return true;
    }

    public static bool CanCallFromConstantDeclaration(bool hasEnclosingFunction) => hasEnclosingFunction;

    public static string ConstantContextCallError(string methodName) =>
        $"Unable to call function {methodName} from constant declaration";
}
