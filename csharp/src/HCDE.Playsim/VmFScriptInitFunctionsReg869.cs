namespace HCDE.Playsim;

/// <summary>Chunk <b>869</b> <c>NewFunction</c> rows (managed LOC on chunk <b>869</b>).</summary>
internal static class VmFScriptInitFunctionsReg869
{
    internal static void Register(IDictionary<string, Action<VmFScriptParser.ParserContext>> target)
    {
        target["ceilingheight"] = VmFScriptParser.SfCeilingHeight;
        target["moveceiling"] = VmFScriptParser.SfMoveCeiling;
        target["lightlevel"] = VmFScriptParser.SfLightLevel;
        target["floortexture"] = VmFScriptParser.SfFloorTexture;
    }
}
