using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptLoadTests
{
    [Fact]
    public void ParseInfo_ScriptsSection_ReturnsTrueAndSetsThinker()
    {
        var level = new VmFScriptLoad.LevelLoadContext();
        var lump = "[scripts]\nprint(\"hi\");\n";
        var ignore = false;
        Assert.True(VmFScriptLoad.ParseInfo(level, Lump(lump), ref ignore));
        Assert.NotNull(level.FraggleScriptThinker);
    }

    [Fact]
    public void ParseInfo_LevelInfo_SetsLevelName()
    {
        var level = new VmFScriptLoad.LevelLoadContext();
        var lump = "[level info]\nlevelname = MyMap\n";
        var ignore = false;
        Assert.False(VmFScriptLoad.ParseInfo(level, Lump(lump), ref ignore));
        Assert.Equal("MyMap", level.LevelName);
    }

    [Fact]
    public void LoadScripts_SwapsTranslatorSlotsWhenFsExecute()
    {
        var table = new VmFScriptLoad.LineTranslationTable();
        for (var i = 0; i <= 272; i++)
            table.SimpleLineTranslations.Add(new VmFScriptLoad.LineTranslation());
        table.SimpleLineTranslations[272].Special = 1;
        table.SimpleLineTranslations[270].Special = VmFScriptLoad.FsExecute;
        var level = new VmFScriptLoad.LevelLoadContext { Translator = table, TranslatorEmpty = true, MapType = VmFScriptLoad.MaptypeDoom };
        var lump = "[scripts]\nx=1;\n";
        VmFScriptLoad.LoadScripts(level, Lump(lump), VmFScriptLoad.GameDoom);
        Assert.Equal(VmFScriptLoad.FsExecute, table.SimpleLineTranslations[270].Special);
        Assert.Equal(1, table.SimpleLineTranslations[272].Special);
    }

    [Fact]
    public void AddSpawnedThing_PushesWhenThinkerPresent()
    {
        var level = new VmFScriptLoad.LevelLoadContext { FraggleScriptThinker = new object() };
        var actor = new object();
        VmFScriptLoad.AddSpawnedThing(level, actor);
        Assert.Single(level.SpawnedThings, actor);
    }

    [Fact]
    public void ParseInfo_DrownZero_ClearsAirSupply()
    {
        var level = new VmFScriptLoad.LevelLoadContext { MapType = VmFScriptLoad.MaptypeDoom, AirSupply = 100 };
        var lump = "[scripts]\n[level info]\ndrown = 0\nx=1;\n";
        var ignore = false;
        VmFScriptLoad.ParseInfo(level, Lump(lump), ref ignore);
        Assert.Equal(0, level.AirSupply);
    }

    static VmFScriptLoad.MapLumpReader Lump(string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        return new VmFScriptLoad.MapLumpReader
        {
            LumpSize = () => bytes.Length,
            ReadLump = (_, dest) => Array.Copy(bytes, dest, bytes.Length),
        };
    }
}
