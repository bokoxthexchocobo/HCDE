namespace HCDE.Scripting;

/// <summary>Native <c>FxFunctionCall::Resolve</c> math/random builtin arity rules.</summary>
public static class ZccFunctionCallMathBuiltins
{
    public static bool TryGetRandomDefaultBounds(out int min, out int max)
    {
        min = 0;
        max = 255;
        return true;
    }

    public static bool TryValidateRandom(string methodName, int argCount)
    {
        if (argCount == 0)
            return true;
        return ZccFunctionCallArgSize.Check(argCount, 2, 2);
    }

    public static bool TryValidateRandomPick(string methodName, int argCount) =>
        ZccFunctionCallArgSize.CheckUnboundedMax(argCount, 1);

    public static bool TryValidateRandom2(int argCount) =>
        ZccFunctionCallArgSize.Check(argCount, 0, 1);

    public static bool TryValidateMinMax(string methodName, int argCount) =>
        ZccFunctionCallArgSize.CheckUnboundedMax(argCount, 2);

    public static bool TryValidateClamp(int argCount) =>
        ZccFunctionCallArgSize.Check(argCount, 3, 3);

    public static bool TryValidateAbs(int argCount) =>
        ZccFunctionCallArgSize.Check(argCount, 1, 1);

    public static bool TryFoldRandom(int min, int max, int seed, out int result)
    {
        if (max < min)
        {
            result = 0;
            return false;
        }

        var span = (uint)(max - min + 1);
        result = min + (int)(seed % span);
        return true;
    }
}
