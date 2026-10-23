namespace HCDE.Playsim;

internal static class VmFScriptInitFunctionsReg870
{
    internal static void Register(IDictionary<string, Action<VmFScriptParser.ParserContext>> target)
    {
        target["sectorcolormap"] = VmFScriptParser.SfSectorColormap;
        target["ceilingtexture"] = VmFScriptParser.SfCeilingTexture;
        target["opendoor"] = VmFScriptParser.SfOpenDoor;
        target["closedoor"] = VmFScriptParser.SfCloseDoor;
    }
}
