namespace HCDE.Gamedata.Tests;

public class VmMapInfoMapOptionsExitEnvironmentTests
{
    [Fact]
    public void ApplyMapBackground_StoresLump()
    {
        var info = new VmMapInfoLevelInfo();
        VmMapInfoMapOptionsExitEnvironment.ApplyMapBackground(info, "BACK");
        Assert.Equal("BACK", info.MapBackground);
    }

    [Fact]
    public void MergeExitText_InsertsLookupFlag()
    {
        var table = new Dictionary<string, VmMapInfoExitMapTextEntry>(StringComparer.OrdinalIgnoreCase);
        VmMapInfoMapOptionsExitEnvironment.MergeExitText(table, "MAP02", "ENDGAME", lookup: true);
        Assert.True(table["MAP02"].LookupText);
        Assert.Equal("ENDGAME", table["MAP02"].Text);
    }

    [Fact]
    public void MergeExitMusic_UpdatesExistingRow()
    {
        var table = new Dictionary<string, VmMapInfoExitMapTextEntry> { ["MAP02"] = new() };
        VmMapInfoMapOptionsExitEnvironment.MergeExitMusic(table, "MAP02", "D_E1M1", 2);
        Assert.Equal("D_E1M1", table["MAP02"].Music);
        Assert.Equal(2, table["MAP02"].MusicOrder);
    }

    [Fact]
    public void MergeExitBackdrop_SetsPicFlag()
    {
        var table = new Dictionary<string, VmMapInfoExitMapTextEntry>();
        VmMapInfoMapOptionsExitEnvironment.MergeExitBackdrop(table, "MAP02", "FLAT01", isPic: true);
        Assert.True(table["MAP02"].BackdropIsPic);
    }

    [Fact]
    public void ResolveEnvironmentId_NumericPair()
    {
        var id = VmMapInfoMapOptionsExitEnvironment.ResolveEnvironmentId(
            hasNumericHigh: true,
            highByte: 3,
            lowByte: 5,
            namedEnvironment: null,
            _ => null);
        Assert.Equal((3 << 8) | 5, id);
    }

    [Fact]
    public void ResolveEnvironmentId_UnknownNameReturnsZero()
    {
        var id = VmMapInfoMapOptionsExitEnvironment.ResolveEnvironmentId(
            false,
            0,
            null,
            "missing",
            _ => null);
        Assert.Equal(0, id);
    }
}
