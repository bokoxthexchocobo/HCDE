namespace HCDE.Playsim.Tests;

public class VmLoadSaveMenu1310SliceTests
{
    [Fact]
    public void Compat_AllowsGzdoom4142AndRejectsTheNeighbors()
    {
        Assert.False(VmLoadSaveMenu1310.CheckGzdoom("GZDOOM", "gzdoom g4.14"));
        Assert.False(VmLoadSaveMenu1310.CheckGzdoom("GZDOOM", "GZDoom g4"));
        Assert.False(VmLoadSaveMenu1310.CheckGzdoom("GZDOOM", "GZDoom g3.1"));
        Assert.False(VmLoadSaveMenu1310.CheckGzdoom("GZDOOM", "GZDoom g4.15"));
        Assert.False(VmLoadSaveMenu1310.CheckGzdoom("GZDOOM", "GZDoom g4.14.3"));
        Assert.False(VmLoadSaveMenu1310.CheckGzdoom("GZDOOM", "GZDoom g4.14.10"));
        Assert.True(VmLoadSaveMenu1310.CheckGzdoom("unused", "GZDoom g4.14.2"));
        Assert.True(VmLoadSaveMenu1310.CheckGzdoom("unused", "GZDoom g4.14"));
        Assert.True(VmLoadSaveMenu1310.CheckGzdoom("unused", "GZDoom g4.0"));
        Assert.True(VmLoadSaveMenu1310.CheckGzdoom("unused", "GZDoom g4.13.99"));
        Assert.Equal(24, VmLoadSaveMenu1310.OldSaveStringSize);
        Assert.Equal("only", VmLoadSaveMenu1310.ExtractComment("", "only"));
        Assert.Equal("noon\nnote", VmLoadSaveMenu1310.ExtractComment("noon", "note"));
        Assert.Same(VmLoadSaveMenu1310.GetManager(), VmLoadSaveMenu1310.GetManager());
    }

    [Fact]
    public void Read_SkipsForeignSavesAndKeepsTheSortCache()
    {
        VmLoadSaveMenu1310.ResetProcess();
        var host = new VmLoadSaveMenu1310.Host { Folder = "saves", Iwad = "doom.wad" };
        Add(host, "closed", null);
        Add(host, "no-info", File("HCDE", 4560, "doom.wad", "", info: -1));
        Add(host, "bad-reader", File("HCDE", 4560, "doom.wad", "", reader: false));
        Add(host, "gz-new", File("GZDOOM", 4560, "doom.wad", "GZDoom g4.15"));
        Add(host, "gz-old", File("GZDOOM", 100, "doom.wad", "GZDoom g4.14.2", title: "oldgz", uuid: "a"));
        Add(host, "gz-high", File("GZDOOM", 9999, "doom.wad", "GZDoom g4.14", title: "gone"));
        Add(host, "new-hcde", File("HCDE", 4561, "doom.wad", ""));
        Add(host, "old-hcde", File("hcde", 4550, "DOOM.WAD", "", title: "old", uuid: "b"));
        Add(host, "foreign", File("ZDOOM", 4560, "doom.wad", ""));
        Add(host, "other-game", File("ZDOOM", 4560, "heretic.wad", "", allow: "Hcde"));
        Add(host, "ok", File("HCDE", 4556, "doom.wad", "", title: "Hi", uuid: "c"));
        Add(host, "allowed", File("ZDOOM", 4560, "doom.wad", "", allow: "hcde", title: "In"));
        var menu = new VmLoadSaveMenu1310(host);

        menu.ReadSaveStrings();

        Assert.Equal(-1, menu.LastSaved);
        Assert.Equal(-1, menu.LastAccessed);
        Assert.Null(menu.QuickSave);
        Assert.Equal(new[] { "gz-old", "gz-high", "old-hcde", "ok", "allowed" }, menu.Saves.Select(s => s.Filename));
        Assert.True(menu.Saves[0].OldVersion);
        Assert.False(menu.Saves[0].MissingWads);
        Assert.Equal("a", menu.Saves[0].Uuid);
        Assert.False(menu.Saves[1].OldVersion);
        Assert.True(menu.Saves[1].MissingWads);
        Assert.True(menu.Saves[2].OldVersion);
        Assert.False(menu.Saves[3].OldVersion);
        Assert.False(menu.Saves[3].MissingWads);
        Assert.Equal("Hi", menu.Saves[3].Title);
        Assert.Equal(new[] { "wads:gone:False", "wads:Hi:False", "wads:In:False" }, host.Calls.Where(c => c.StartsWith("wads:")).ToArray());
        Assert.Equal("scan:saves:*.zds:True", Assert.Single(host.Calls, c => c.StartsWith("scan:")));

        menu.Saves.Add(new VmLoadSaveMenu1310.Node { Filename = "kept" });
        menu.ReadSaveStrings();
        Assert.Equal(1, host.Calls.Count(c => c.StartsWith("scan:")));
        Assert.Equal("kept", menu.Saves[^1].Filename);

        menu.SortOrder = 1;
        menu.ReadSaveStrings();
        Assert.Equal(2, host.Calls.Count(c => c.StartsWith("scan:")));
        Assert.DoesNotContain(menu.Saves, s => s.Filename == "kept");
        Assert.Equal(5, menu.Saves.Count);

        menu.PerformLoad("ok", true);
        menu.PerformSave("ok", "Hi");
        Assert.Equal("path/auto07.zds", menu.BuildSaveName("auto", 7));
        Assert.Contains("load:ok:True", host.Calls);
        Assert.Contains("save:ok:Hi", host.Calls);
        Assert.Contains("name:auto07", host.Calls);
        VmLoadSaveMenu1310.ResetProcess();
    }

    static void Add(VmLoadSaveMenu1310.Host host, string name, VmLoadSaveMenu1310.SaveFile? file)
    {
        host.Entries.Add(name);
        host.Files[name] = file;
    }

    static VmLoadSaveMenu1310.SaveFile File(
        string engine,
        int version,
        string iwad,
        string software,
        string title = "",
        string uuid = "",
        string allow = "",
        bool reader = true,
        int info = 0)
    {
        var arc = new VmLoadSaveMenu1310.Arc { SaveVersion = version, HasAllow = allow.Length > 0 };
        arc.Strings["Engine"] = engine;
        arc.Strings["Game WAD"] = iwad;
        arc.Strings["Title"] = title;
        arc.Strings["Creation Time"] = "t";
        arc.Strings["GameUUID"] = uuid;
        arc.Strings["Software"] = software;
        if (allow.Length > 0)
            arc.Allow.Add(allow);
        return new VmLoadSaveMenu1310.SaveFile { Info = info, ReaderOk = reader, Arc = arc, Data = new byte[] { 1 } };
    }
}
