namespace HCDE.Scripting;

/// <summary>Native <c>FxMemberIdentifier::Resolve</c> basic type-name branch (<c>codegen.cpp</c>).</summary>
public static class ZccMemberTypeNames
{
    public static bool TryResolveBasicTypeName(string name, out VmValueType valueType)
    {
        switch (name.ToLowerInvariant())
        {
            case "byte":
            case "uint8":
            case "ushort":
            case "uint16":
            case "uint":
            case "uint32":
                valueType = VmValueType.Integer;
                return true;
            case "sbyte":
            case "int8":
            case "short":
            case "int16":
            case "int":
                valueType = VmValueType.Integer;
                return true;
            case "float":
                valueType = VmValueType.Float;
                return true;
            case "double":
                valueType = VmValueType.Float;
                return true;
            default:
                valueType = default;
                return false;
        }
    }
}
