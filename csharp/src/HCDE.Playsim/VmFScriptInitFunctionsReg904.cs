namespace HCDE.Playsim;

internal static class VmFScriptInitFunctionsReg904
{
    internal static void Register(IDictionary<string, Action<VmFScriptParser.ParserContext>> target)
    {
        target["include"] = VmFScriptParser.SfInclude;
        target["exitlevel"] = VmFScriptParser.SfExitLevel;
        target["input"] = VmFScriptParser.SfInput;
        target["tip"] = VmFScriptParser.SfTip;
        target["timedtip"] = VmFScriptParser.SfTimedTip;
        target["playertip"] = VmFScriptParser.SfPlayerTip;
    }
}
