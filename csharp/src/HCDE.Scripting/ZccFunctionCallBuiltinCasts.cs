namespace HCDE.Scripting;

/// <summary>Native <c>FxFunctionCall::Resolve</c> builtin type-cast switch (<c>codegen.cpp</c>).</summary>
public static class ZccFunctionCallBuiltinCasts
{
    public static bool TryGetCastTargetType(string methodName, out VmValueType targetType)
    {
        switch (methodName.ToLowerInvariant())
        {
            case "bool":
                targetType = VmValueType.Integer;
                return true;
            case "int":
            case "uint":
                targetType = VmValueType.Integer;
                return true;
            case "float":
            case "double":
                targetType = VmValueType.Float;
                return true;
            case "name":
            case "sound":
            case "state":
            case "spriteid":
            case "textureid":
            case "translationid":
                targetType = VmValueType.String;
                return true;
            default:
                targetType = default;
                return false;
        }
    }

    public static bool TryResolveTypeCast(string methodName, int argCount, out VmValueType targetType)
    {
        if (!TryGetCastTargetType(methodName, out targetType))
            return false;
        return ZccFunctionCallArgSize.Check(argCount, 1, 1);
    }
}
