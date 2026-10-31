namespace HCDE.Playsim.Tests;

public class VmDoomFont1188SliceTests
{
    static VmDoomFont1188Slice.Host Host()
    {
        VmDoomFont1188Slice.Clear();
        return new VmDoomFont1188Slice.Host { IwadNum = 3 };
    }

    [Fact]
    public void Init_BuildsAPwadSmallFontAndKeepsTheOriginal()
    {
        var host = Host();
        host.Graphics["FONTA_S"] = 9;
        VmDoomFont1188Slice.ConFont = new VmDoomFont1188Slice.Font { Name = "ConsoleFont" };
        VmDoomFont1188Slice.Init(host);
        Assert.Same(VmDoomFont1188Slice.ConFont, VmDoomFont1188Slice.SmallFont);
        Assert.Null(VmDoomFont1188Slice.SmallFont!.NameTemplate);

        host = Host();
        host.Names["FONTA_S"] = 10;
        host.Graphics["FONTA33"] = 20;
        host.Containers[20] = 4;
        var calls = new List<string>();
        host.GetFont = (name, lump) =>
        {
            calls.Add(lump == null ? name : name + " " + lump);
            return null;
        };
        VmDoomFont1188Slice.Init(host);

        Assert.Equal(223, VmDoomFont1188Slice.FontSize);
        Assert.Equal(33, VmDoomFont1188Slice.FontStart);
        Assert.Contains("SmallFont SMALLFNT", calls);
        var small = VmDoomFont1188Slice.SmallFont!;
        Assert.Equal("FONTA%02u", small.NameTemplate);
        Assert.Null(small.FileTemplate);
        Assert.Equal('[', small.Cursor);
        Assert.Equal(1, small.Base);
        Assert.Equal(-1, small.FdLump);
        Assert.False(small.IwadOnly);
        var original = VmDoomFont1188Slice.OriginalSmallFont!;
        Assert.Equal("defsmallfont", original.FileTemplate);
        Assert.True(original.IwadOnly);
        Assert.Equal('[', original.Cursor);
        Assert.Same(small, VmDoomFont1188Slice.SmallFont2);
        Assert.Equal("bigfont", VmDoomFont1188Slice.OriginalBigFont!.FileTemplate);
        Assert.Null(VmDoomFont1188Slice.OriginalBigFont.NameTemplate);
        Assert.Same(VmDoomFont1188Slice.OriginalBigFont, VmDoomFont1188Slice.BigFont);
        Assert.Contains("BigFont ZBIGFONT", calls);
    }

    [Fact]
    public void Init_UsesDoomTemplatesStrifeAndAFolderOverride()
    {
        var host = Host();
        host.Names["FONTA_S"] = 10;
        host.Graphics["FONTA33"] = 21;
        host.Containers[21] = 3;
        VmDoomFont1188Slice.Init(host);
        Assert.Equal("defsmallfont", VmDoomFont1188Slice.SmallFont!.FileTemplate);
        Assert.Equal('[', VmDoomFont1188Slice.SmallFont.Cursor);

        host = Host();
        host.Graphics["STCFN033"] = 30;
        host.Graphics["STCFN065"] = 31;
        host.Containers[31] = 9;
        host.Graphics["STBFN033"] = 40;
        host.Names["FONTB_S"] = 50;
        var big = new VmDoomFont1188Slice.Font { Name = "BigFont", Type = 0 };
        var upper = new VmDoomFont1188Slice.Font { Name = "BigUpper", Type = VmDoomFont1188Slice.FontFolder };
        var doom = new VmDoomFont1188Slice.Font { Name = "IntermissionFont_Doom" };
        var calls = new List<string>();
        host.GetFont = (name, lump) =>
        {
            calls.Add(lump == null ? name : name + " " + lump);
            if (name == "BigFont" && lump == null)
                return big;
            if (name == "BigUpper")
                return upper;
            return null;
        };
        host.FindFont = name => name == "IntermissionFont_Doom" ? doom : null;
        host.Winum0 = true;
        VmDoomFont1188Slice.Init(host);

        var small = VmDoomFont1188Slice.SmallFont!;
        Assert.Equal("STCFN%.3d", small.NameTemplate);
        Assert.Null(small.FileTemplate);
        Assert.True(small.DoomTemplate);
        Assert.Equal('\0', small.Cursor);
        Assert.Equal(VmDoomFont1188Slice.FontStart, small.Base);
        Assert.Equal("defsmallfont2", VmDoomFont1188Slice.SmallFont2!.FileTemplate);
        Assert.False(VmDoomFont1188Slice.SmallFont2.DoomTemplate);
        Assert.Equal("FONTB%02u", VmDoomFont1188Slice.OriginalBigFont!.NameTemplate);
        Assert.True(VmDoomFont1188Slice.OriginalBigFont.IwadOnly);
        Assert.True(upper.Deleted);
        Assert.Same(big, VmDoomFont1188Slice.BigUpper);
        Assert.Same(big, VmDoomFont1188Slice.BigFont);
        Assert.Same(doom, VmDoomFont1188Slice.IntermissionFont);
        Assert.DoesNotContain("BigFont ZBIGFONT", calls);

        host = Host();
        host.Graphics["STCFN033"] = 30;
        host.Winum0 = false;
        host.FindFont = name => name == "IntermissionFont_Doom" ? doom : null;
        var supplied = new VmDoomFont1188Slice.Font { Name = "BigFont" };
        host.GetFont = (name, lump) => name == "BigFont" && lump == null ? supplied : null;
        VmDoomFont1188Slice.Init(host);
        Assert.Equal("defsmallfont", VmDoomFont1188Slice.SmallFont!.FileTemplate);
        Assert.True(VmDoomFont1188Slice.SmallFont.DoomTemplate);
        Assert.Same(supplied, VmDoomFont1188Slice.IntermissionFont);
    }

    [Fact]
    public void Init_FallsBackAndRejectsAMissingConsoleFont()
    {
        var host = Host();
        var error = Assert.Throws<VmDoomFont1188Slice.FatalError>(() => VmDoomFont1188Slice.Init(host));
        Assert.Equal("Console font not found.", error.Detail);
        Assert.Null(VmDoomFont1188Slice.AlternativeSmallFont);
        Assert.Null(VmDoomFont1188Slice.AlternativeBigFont);
        Assert.Equal("bigfont", VmDoomFont1188Slice.OriginalBigFont!.FileTemplate);

        host = Host();
        var console = new VmDoomFont1188Slice.Font { Name = "ConsoleFont" };
        var small = new VmDoomFont1188Slice.Font { Name = "SmallFont", Cursor = 'x' };
        var big = new VmDoomFont1188Slice.Font { Name = "BigFont", Type = VmDoomFont1188Slice.FontFolder };
        var upper = new VmDoomFont1188Slice.Font { Name = "BigUpper", Type = VmDoomFont1188Slice.FontFolder };
        VmDoomFont1188Slice.ConFont = console;
        host.GetFont = (name, lump) =>
        {
            if (name == "SmallFont")
                return small;
            if (name == "BigFont" && lump == null)
                return big;
            if (name == "BigUpper")
                return upper;
            return null;
        };
        var inter = new VmDoomFont1188Slice.Font { Name = "IntermissionFont" };
        host.FindFont = name => name == "IntermissionFont" ? inter : null;
        host.Winum0 = true;
        VmDoomFont1188Slice.Init(host);

        Assert.Same(small, VmDoomFont1188Slice.SmallFont);
        Assert.Equal('x', small.Cursor);
        Assert.Same(console, VmDoomFont1188Slice.OriginalSmallFont);
        Assert.Same(small, VmDoomFont1188Slice.SmallFont2);
        Assert.Same(big, VmDoomFont1188Slice.BigFont);
        Assert.False(upper.Deleted);
        Assert.Same(upper, VmDoomFont1188Slice.BigUpper);
        Assert.Same(inter, VmDoomFont1188Slice.IntermissionFont);
        Assert.Same(console, VmDoomFont1188Slice.AlternativeSmallFont);
        Assert.Same(VmDoomFont1188Slice.OriginalBigFont, VmDoomFont1188Slice.AlternativeBigFont);
        Assert.Same(console, VmDoomFont1188Slice.ConFont);
    }
}
