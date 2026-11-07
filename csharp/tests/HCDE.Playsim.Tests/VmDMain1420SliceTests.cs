using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDMain1420SliceTests
{
    [Fact]
    public void TextureHacksRootsAndNames()
    {
        var slice = VmDMain1420Slice.BandEnd - VmDMain1420Slice.BandStart + 1;
        Assert.Equal(203, slice);
        Assert.Equal(3644, VmDMain1420Slice.DimLine);
        Assert.Equal(3856, VmDMain1420Slice.InitGame);
        var doom = new VmDMain1420Slice.State { GameType = VmDMain1420Slice.Doom };
        doom.Textures.Add(new VmDMain1420Slice.Tex());
        VmDMain1420Slice.PatchTextures(doom);
        Assert.Equal(0, doom.BlankLooks);
        Assert.Equal(VmDMain1420Slice.Wall, doom.Textures[0].UseType);
        var hexen = new VmDMain1420Slice.State { GameType = VmDMain1420Slice.Hexen | VmDMain1420Slice.Doom };
        VmDMain1420Slice.PatchTextures(hexen);
        Assert.Equal(0, hexen.BlankLooks);
        var miss = new VmDMain1420Slice.State { GameType = VmDMain1420Slice.Hexen, BlankId = -1 };
        miss.Textures.Add(new VmDMain1420Slice.Tex());
        VmDMain1420Slice.PatchTextures(miss);
        Assert.Equal(1, miss.BlankLooks);
        Assert.Equal(VmDMain1420Slice.Wall, miss.Textures[0].UseType);
        var hit = new VmDMain1420Slice.State { GameType = VmDMain1420Slice.Hexen, BlankId = 0 };
        hit.Textures.Add(new VmDMain1420Slice.Tex { UseType = VmDMain1420Slice.Wall });
        VmDMain1420Slice.PatchTextures(hit);
        Assert.Equal(VmDMain1420Slice.NullTex, hit.Textures[0].UseType);
        var empty = new VmDMain1420Slice.State { GameType = VmDMain1420Slice.Hexen, BlankId = 0 };
        Assert.Throws<NullReferenceException>(() => VmDMain1420Slice.PatchTextures(empty));
        var heretic = new VmDMain1420Slice.State { GameType = VmDMain1420Slice.Heretic };
        var sky = new VmDMain1420Slice.Build
        {
            Name = "SKY2",
            Width = 256,
            Height = 128,
            Parts = { new VmDMain1420Slice.Part { OriginY = -8, ImageHeight = 200 } },
        };
        VmDMain1420Slice.CheckForHacks(heretic, sky);
        Assert.Equal(200, sky.Height);
        Assert.Equal(256, sky.SizedW);
        Assert.Equal(200, sky.SizedH);
        Assert.Equal(-8, sky.Parts[0].OriginY);
        var low = new VmDMain1420Slice.Build
        {
            Name = "sky1",
            Height = 128,
            Parts = { new VmDMain1420Slice.Part { ImageHeight = 200 } },
        };
        VmDMain1420Slice.CheckForHacks(heretic, low);
        Assert.Equal(128, low.Height);
        Assert.Equal(int.MinValue, low.SizedH);
        var none = new VmDMain1420Slice.Build { Name = "SKY1", Height = 128 };
        VmDMain1420Slice.CheckForHacks(heretic, none);
        Assert.Equal(128, none.Height);
        var broken = new VmDMain1420Slice.Build
        {
            Name = "SKY1",
            Height = 128,
            Parts = { new VmDMain1420Slice.Part { Image = false, ImageHeight = 200 } },
        };
        Assert.Throws<NullReferenceException>(() => VmDMain1420Slice.CheckForHacks(heretic, broken));
        var sky1 = new VmDMain1420Slice.Build
        {
            Name = "SKY1",
            Height = 128,
            Parts = { new VmDMain1420Slice.Part { OriginY = -8 } },
        };
        VmDMain1420Slice.CheckForHacks(doom, sky1);
        Assert.Equal(0, sky1.Parts[0].OriginY);
        sky1.Parts[0].OriginY = -8;
        doom.Flags = VmDMain1420Slice.MapXx;
        VmDMain1420Slice.CheckForHacks(doom, sky1);
        Assert.Equal(-8, sky1.Parts[0].OriginY);
        doom.Flags = 0;
        var door = new VmDMain1420Slice.Build
        {
            Name = "BigDoor7",
            Height = 128,
            Parts =
            {
                new VmDMain1420Slice.Part { OriginY = -4 },
                new VmDMain1420Slice.Part { OriginY = -4 },
            },
        };
        VmDMain1420Slice.CheckForHacks(doom, door);
        Assert.Equal(0, door.Parts[0].OriginY);
        Assert.Equal(0, door.Parts[1].OriginY);
        door.Parts[0].OriginY = -4;
        door.Parts[1].OriginY = -3;
        VmDMain1420Slice.CheckForHacks(doom, door);
        Assert.Equal(-4, door.Parts[0].OriginY);
        var chex = new VmDMain1420Slice.State { GameType = 16 };
        var chexSky = new VmDMain1420Slice.Build
        {
            Name = "SKY1",
            Height = 128,
            Parts = { new VmDMain1420Slice.Part { OriginY = -8 } },
        };
        VmDMain1420Slice.CheckForHacks(chex, chexSky);
        Assert.Equal(-8, chexSky.Parts[0].OriginY);
        var wide = new VmDMain1420Slice.Tex { Width = 400, ScaleX = 1f };
        doom.StatusTex = wide;
        VmDMain1420Slice.FixWideStatusBar(doom);
        Assert.Equal(40, wide.Left[0]);
        Assert.Equal(40, wide.Left[1]);
        Assert.Equal(0, wide.Top[0]);
        Assert.Equal(0, wide.Top[1]);
        wide.Left[0] = 0;
        wide.Top[0] = 0;
        wide.ScaleX = 2f;
        VmDMain1420Slice.FixWideStatusBar(doom);
        Assert.Equal(0, wide.Left[0]);
        wide.ScaleX = 1f;
        wide.Width = 321;
        VmDMain1420Slice.FixWideStatusBar(doom);
        Assert.Equal(0, wide.Left[0]);
        wide.Width = 320;
        wide.Left[0] = 0;
        VmDMain1420Slice.FixWideStatusBar(doom);
        Assert.Equal(0, wide.Left[1]);
        doom.StatusTex = null;
        VmDMain1420Slice.FixWideStatusBar(doom);
        var names = new VmDMain1420Slice.Text();
        doom.Sprites = new List<string> { "POSS", "TROO" };
        VmDMain1420Slice.CastSprite(names, 1, doom);
        Assert.Equal("TROO", names.Value);
        VmDMain1420Slice.CastSprite(names, 2, doom);
        Assert.Equal("TNT1", names.Value);
        Assert.Throws<NullReferenceException>(() => VmDMain1420Slice.CastSprite(null!, 0, doom));
        doom.Sprites = null;
        Assert.Throws<NullReferenceException>(() => VmDMain1420Slice.CastSprite(names, 0, doom));
        var roots = new VmDMain1420Slice.State();
        var first = new VmDMain1420Slice.Obj();
        var last = new VmDMain1420Slice.Obj { Euthanize = true };
        var released = new VmDMain1420Slice.Obj { Released = true };
        var level = new VmDMain1420Slice.LevelNode();
        roots.First = first;
        roots.Last = last;
        roots.Next = released;
        roots.Levels = new List<VmDMain1420Slice.LevelNode?> { level };
        roots.InGame[0] = true;
        roots.InGame[63] = true;
        VmDMain1420Slice.MarkGameRoots(roots);
        Assert.False(first.White);
        Assert.True(first.Gray);
        Assert.Null(roots.Last);
        Assert.Same(released, roots.Next);
        Assert.Equal(1, level.Marks);
        Assert.Equal(2, roots.PlayerMarks);
        Assert.Equal(1, roots.RollbackMarks);
        Assert.Equal(new[] { first }, roots.Gray);
        roots.Levels = new List<VmDMain1420Slice.LevelNode?> { null };
        Assert.Throws<NullReferenceException>(() => VmDMain1420Slice.MarkGameRoots(roots));
        roots.Levels = null;
        Assert.Throws<NullReferenceException>(() => VmDMain1420Slice.MarkGameRoots(roots));
        var ui = new VmDMain1420Slice.State { Wipe = 7 };
        VmDMain1420Slice.ToggleFullConsole(ui);
        Assert.Equal(VmDMain1420Slice.FullConsole, ui.GameAction);
        VmDMain1420Slice.StartCutscene(ui, true);
        Assert.Equal(VmDMain1420Slice.Intro, ui.GameAction);
        VmDMain1420Slice.StartCutscene(ui, false);
        Assert.Equal(VmDMain1420Slice.Intermission, ui.GameAction);
        VmDMain1420Slice.SetTransition(ui, VmDMain1420Slice.WipeNone);
        Assert.Equal(7, ui.Wipe);
        VmDMain1420Slice.SetTransition(ui, VmDMain1420Slice.WipeBurn);
        Assert.Equal(VmDMain1420Slice.ForceBurn, ui.Wipe);
        VmDMain1420Slice.SetTransition(ui, VmDMain1420Slice.WipeFade);
        Assert.Equal(VmDMain1420Slice.ForceFade, ui.Wipe);
        VmDMain1420Slice.SetTransition(ui, VmDMain1420Slice.WipeMelt);
        Assert.Equal(VmDMain1420Slice.ForceMelt, ui.Wipe);
        VmDMain1420Slice.HudScaleChanged(ui);
        Assert.False(ui.SizeNeeded);
        ui.Status = new VmDMain1420Slice.Bar();
        ui.Primary = new VmDMain1420Slice.LevelNode { Automap = true };
        VmDMain1420Slice.HudScaleChanged(ui);
        Assert.Equal(1, ui.Status.Scales);
        Assert.True(ui.SizeNeeded);
        Assert.Equal(1, ui.Primary.UiScales);
        ui.Primary.Automap = false;
        VmDMain1420Slice.HudScaleChanged(ui);
        Assert.Equal(1, ui.Primary.UiScales);
        Assert.True(VmDMain1420Slice.FileNameCheck(ui, "music", null));
        Assert.True(VmDMain1420Slice.FileNameCheck(ui, "texture", "x"));
        Assert.Throws<NullReferenceException>(() => VmDMain1420Slice.FileNameCheck(ui, null, "x"));
        Assert.Throws<NullReferenceException>(() => VmDMain1420Slice.FileNameCheck(ui, "Textures", null));
        ui.Files["cache"] = new byte[] { (byte)'L', (byte)'Z', (byte)'4', (byte)' ' };
        Assert.False(VmDMain1420Slice.FileNameCheck(ui, "TEXTURES.cache", "cache"));
        ui.Files["short"] = new byte[] { (byte)'L', (byte)'Z' };
        Assert.True(VmDMain1420Slice.FileNameCheck(ui, "textures", "short"));
        ui.Files["lower"] = System.Text.Encoding.ASCII.GetBytes("lz4");
        Assert.True(VmDMain1420Slice.FileNameCheck(ui, "textures", "lower"));
        ui.Files["miss"] = null;
        Assert.True(VmDMain1420Slice.FileNameCheck(ui, "textures", "miss"));
        Assert.True(VmDMain1420Slice.FileNameCheck(ui, "textures", "absent"));
    }

    [Fact]
    public void FileSystemPrintfRoutes()
    {
        var s = new VmDMain1420Slice.State();
        Assert.Equal(4, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsError, "hi"));
        Assert.Equal(VmDMain1420Slice.Red + "hi", s.Prints[0]);
        s.Prints.Clear();
        Assert.Equal(2, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsError, ""));
        Assert.Equal(VmDMain1420Slice.Red, s.Prints[0]);
        s.GameDead = true;
        s.Prints.Clear();
        Assert.Equal(0, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsError, "hi"));
        Assert.Empty(s.Prints);
        Assert.Equal(2, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsWarning, "ab"));
        Assert.Empty(s.Prints);
        s.GameDead = false;
        s.Console = false;
        Assert.Equal(5, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsMessage, "plain"));
        Assert.Empty(s.Prints);
        s.Console = true;
        Assert.Equal(2, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsWarning, "ab"));
        Assert.Equal(VmDMain1420Slice.Yellow + "ab", s.Prints[^1]);
        Assert.Equal(0, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsWarning, ""));
        Assert.Equal(VmDMain1420Slice.Yellow, s.Prints[^1]);
        Assert.Equal(3, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsAttention, "xyz"));
        Assert.Equal(VmDMain1420Slice.Blue + "xyz", s.Prints[^1]);
        Assert.Equal(3, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsMessage, "%s %d", "n", 3));
        Assert.Equal("n 3", s.Prints[^1]);
        Assert.Equal(3, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsDebugWarn, "dbg"));
        Assert.Equal("n 3", s.Prints[^1]);
        s.Developer = VmDMain1420Slice.DevWarning;
        Assert.Equal(3, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsDebugWarn, "dbg"));
        Assert.Equal("dbg", s.Prints[^1]);
        Assert.Equal(4, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsDebugNotify, "note"));
        Assert.Equal("dbg", s.Prints[^1]);
        s.Developer = VmDMain1420Slice.DevNotify;
        Assert.Equal(4, VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsDebugNotify, "note"));
        Assert.Equal("note", s.Prints[^1]);
        int before = s.Prints.Count;
        Assert.Equal(3, VmDMain1420Slice.FileSystemPrintf(s, 0, "%% %u", 9u));
        Assert.Equal(before, s.Prints.Count);
        Assert.Equal("% 9", VmDMain1420Slice.Format("%% %u", 9u));
        Assert.Throws<NullReferenceException>(() => VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsMessage, null));
        Assert.Throws<NullReferenceException>(() => VmDMain1420Slice.FileSystemPrintf(s, VmDMain1420Slice.FsMessage, "%s", new object?[] { null }));
    }
}
