namespace HCDE.Playsim;

internal static class VmFScriptInitFunctionsReg873
{
    internal static void Register(IDictionary<string, Action<VmFScriptParser.ParserContext>> target)
    {
        target["exitlevel"] = VmFScriptParser.SfExitLevel;
        target["input"] = VmFScriptParser.SfInput;
        target["tip"] = VmFScriptParser.SfTip;
        target["timedtip"] = VmFScriptParser.SfTimedTip;
        target["playertip"] = VmFScriptParser.SfPlayerTip;
    }
}
