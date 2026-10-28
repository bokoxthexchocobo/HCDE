namespace HCDE.Playsim;

/// <summary>Chunk <b>868</b> <c>NewFunction</c> rows (managed LOC on chunk <b>868</b>, not <b>851</b>).</summary>
internal static class VmFScriptInitFunctionsReg868
{
    internal static void Register(IDictionary<string, Action<VmFScriptParser.ParserContext>> target)
    {
        target["setcamera"] = VmFScriptParser.SfSetCamera;
        target["clearcamera"] = VmFScriptParser.SfClearCamera;
        target["startsound"] = VmFScriptParser.SfStartSound;
        target["floorheight"] = VmFScriptParser.SfFloorHeight;
        target["movefloor"] = VmFScriptParser.SfMoveFloor;
    }
}
