namespace HCDE.Scripting;

/// <summary>Native <c>FxSelf::Resolve</c> and decorate check emit (<c>codegen.cpp</c>).</summary>
public static class ZccExpressionSelf
{
    public static bool TryResolve(bool hasEnclosingFunction, bool hasSelfClass, out string? error)
    {
        if (!hasEnclosingFunction || !hasSelfClass)
        {
            error = "self used outside of a member function";
            return false;
        }

        error = null;
        return true;
    }

}
