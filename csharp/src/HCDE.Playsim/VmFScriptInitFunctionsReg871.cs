namespace HCDE.Playsim;

internal static class VmFScriptInitFunctionsReg871
{
    internal static void Register(IDictionary<string, Action<VmFScriptParser.ParserContext>> target)
    {
        target["linetrigger"] = VmFScriptParser.SfLineTrigger;
        target["setlineblocking"] = VmFScriptParser.SfSetLineBlocking;
        target["setlinemonsterblocking"] = VmFScriptParser.SfSetLineMonsterBlocking;
        target["setlinetexture"] = VmFScriptParser.SfSetLineTexture;
    }
}
