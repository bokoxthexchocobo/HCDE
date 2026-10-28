namespace HCDE.Gamedata;

/// <summary>MAPINFO brace/assign helpers from <c>g_mapinfo.cpp</c> (480–667).</summary>
public enum VmMapInfoFormatType { Old, New }

public static class VmMapInfoParserFormat
{
    public static VmMapInfoFormatType ResolveOpenBrace(bool sawBrace, VmMapInfoFormatType current) =>
        current switch
        {
            VmMapInfoFormatType.New => VmMapInfoFormatType.New,
            _ => sawBrace ? VmMapInfoFormatType.New : VmMapInfoFormatType.Old
        };

    public static bool IsCloseBrace(VmMapInfoFormatType format, string token) =>
        format == VmMapInfoFormatType.New && token == "}";

    public static bool UsesExplicitAssign(VmMapInfoFormatType format) =>
        format == VmMapInfoFormatType.New;

    public static void EnsureNotUnexpectedEof(VmMapInfoFormatType format, bool atEnd, string block)
    {
        if (format == VmMapInfoFormatType.New && atEnd)
            throw new InvalidOperationException($"Unexpected end of file in {block} definition");
    }
}
