namespace HCDE.Scripting;

/// <summary>Native <c>FxFunctionCall::Resolve</c> argument validation (<c>codegen.cpp</c>).</summary>
public static class ZccFunctionCallValidation
{
    public static bool HasNullArgument(IReadOnlyList<object?> arguments, out int index)
    {
        for (var i = 0; i < arguments.Count; i++)
        {
            if (arguments[i] is null)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }
}
