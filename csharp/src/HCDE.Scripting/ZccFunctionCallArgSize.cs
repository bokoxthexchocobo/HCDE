namespace HCDE.Scripting;

/// <summary>Native <c>CheckArgSize</c> helper used by <c>FxFunctionCall::Resolve</c>.</summary>
public static class ZccFunctionCallArgSize
{
    public static bool Check(int actual, int min, int max)
    {
        if (actual < min)
            return false;
        if (max >= 0 && actual > max)
            return false;
        return true;
    }

    public static bool CheckUnboundedMax(int actual, int min) => actual >= min;
}
