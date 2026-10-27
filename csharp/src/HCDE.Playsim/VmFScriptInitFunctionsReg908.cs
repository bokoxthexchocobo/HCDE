namespace HCDE.Playsim;

internal static class VmFScriptInitFunctionsReg908
{
    internal static void Register(IDictionary<string, Action<VmFScriptParser.ParserContext>> target)
    {
        target["startsectorsound"] = VmFScriptParser.SfStartSectorSound;
    }
}
