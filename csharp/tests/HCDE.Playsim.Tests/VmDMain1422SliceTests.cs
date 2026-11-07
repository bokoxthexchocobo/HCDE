using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDMain1422SliceTests
{
    [Fact]
    public void StartupWindowAndVideo()
    {
        Assert.Equal(170, VmDMain1422Slice.BandEnd - VmDMain1422Slice.BandStart + 1);
        Assert.Equal(4018, VmDMain1422Slice.BootEnd);
        Assert.Equal(4190, VmDMain1422Slice.GlPhase);
        var hexen = new VmDMain1422Slice.State { GameType = VmDMain1422Slice.Hexen, Startup = VmDMain1422Slice.DefaultStartup };
        VmDMain1422Slice.LoadResources(hexen);
        Assert.Equal(VmDMain1422Slice.HexenStartup, hexen.Startup);
        var heretic = new VmDMain1422Slice.State { GameType = VmDMain1422Slice.Heretic };
        VmDMain1422Slice.LoadResources(heretic);
        Assert.Equal(VmDMain1422Slice.HereticStartup, heretic.Startup);
        var strife = new VmDMain1422Slice.State { GameType = VmDMain1422Slice.Strife };
        VmDMain1422Slice.LoadResources(strife);
        Assert.Equal(VmDMain1422Slice.StrifeStartup, strife.Startup);
        var doom = new VmDMain1422Slice.State { GameType = 1 };
        VmDMain1422Slice.LoadResources(doom);
        Assert.Equal(VmDMain1422Slice.DefaultStartup, doom.Startup);
        var pinned = new VmDMain1422Slice.State { GameType = VmDMain1422Slice.Hexen, Startup = VmDMain1422Slice.DoomStartup };
        VmDMain1422Slice.LoadResources(pinned);
        Assert.Equal(VmDMain1422Slice.DoomStartup, pinned.Startup);
        var mixed = new VmDMain1422Slice.State { GameType = VmDMain1422Slice.Hexen | 1 };
        VmDMain1422Slice.LoadResources(mixed);
        Assert.Equal(VmDMain1422Slice.DefaultStartup, mixed.Startup);
        Assert.Throws<NullReferenceException>(() => VmDMain1422Slice.LoadResources(new VmDMain1422Slice.State { Config = null }));
        var exec = new VmDMain1422Slice.State { HasExec = true };
        VmDMain1422Slice.LoadResources(exec);
        Assert.Equal(1, exec.ExecRuns);
        Assert.False(exec.HasExec);
        var one = new VmDMain1422Slice.State { Dedicated = true, Delayed = 1, Language = "es" };
        VmDMain1422Slice.LoadResources(one);
        Assert.Equal(1, one.DelayedCalls);
        Assert.Contains("Dedicated server ignored 1 delayed startup command.\n", one.Lines);
        Assert.Equal("es", one.LoadedLanguage);
        Assert.DoesNotContain(one.Lines, line => line.StartsWith("ST_Init", StringComparison.Ordinal));
        Assert.Equal("dedicated", one.WindowKind);
        Assert.Equal(4096, one.WindowArg);
        Assert.False(one.VideoInitialized);
        var many = new VmDMain1422Slice.State { Dedicated = true, Delayed = 2 };
        VmDMain1422Slice.LoadResources(many);
        Assert.Contains("Dedicated server ignored 2 delayed startup commands.\n", many.Lines);
        var zero = new VmDMain1422Slice.State { Dedicated = true, Delayed = 0 };
        VmDMain1422Slice.LoadResources(zero);
        Assert.Equal(1, zero.DelayedCalls);
        Assert.DoesNotContain(zero.Lines, line => line.Contains("delayed", StringComparison.Ordinal));
        var client = new VmDMain1422Slice.State { MaxProgress = 4096, PerShader = 1, CpuDump = "amd\n" };
        VmDMain1422Slice.LoadResources(client);
        Assert.Contains("startup progress window", client.Guards);
        Assert.Contains("startup video loader", client.Guards);
        Assert.Equal("instance", client.WindowKind);
        Assert.Equal(4096, client.WindowArg);
        Assert.Equal(4551, client.ScreenArg);
        Assert.True(client.VideoInitialized);
        Assert.True(client.FirstShader);
        Assert.True(client.SetMode);
        Assert.True(client.Rendered);
        Assert.Contains("ST_Init: Init startup screen.\n", client.Lines);
        Assert.Contains("amd\n", client.Lines);
        Assert.Contains("I_Init: Setting up machine state.", client.Status);
        var plus3 = new VmDMain1422Slice.State { MaxProgress = 8, PerShader = 0 };
        VmDMain1422Slice.LoadResources(plus3);
        Assert.Equal(11, plus3.ScreenArg);
        var restart = new VmDMain1422Slice.State { Restart = true, NoStartScreen = true, CpuDump = null };
        VmDMain1422Slice.LoadResources(restart);
        Assert.Equal("restart", restart.WindowKind);
        Assert.Equal(0, restart.WindowArg);
        Assert.DoesNotContain(restart.Status, line => line.StartsWith("I_Init", StringComparison.Ordinal));
        Assert.False(restart.VideoInitialized);
        Assert.Throws<NullReferenceException>(() => VmDMain1422Slice.LoadResources(new VmDMain1422Slice.State { ScreenMissing = true }));
    }

    [Fact]
    public void ResourcesCallbacksAndClasses()
    {
        var s = new VmDMain1422Slice.State
        {
            MapInfo = "doommap",
            MessageBox = "Box",
            Endoom = "END",
            Blur = 1.5,
            CompatValue = "  -15abc",
            TexturePasses = 2,
            Border = "FLAT5",
            PlayerClasses = 3,
        };
        VmDMain1422Slice.LoadResources(s);
        Assert.Equal(-15, s.CompatMode);
        Assert.Equal("doommap", s.ParsedMap);
        Assert.Equal("Box", s.CopiedBox);
        Assert.Equal("END", s.CopiedEndoom);
        Assert.Equal(1.5, s.CopiedBlur);
        Assert.Equal(1, s.MusInfo);
        Assert.Equal(1, s.SoundData);
        Assert.Equal(1, s.Reverb);
        Assert.Equal(1, s.CmdLine);
        Assert.Equal(VmDMain1422Slice.Flat, s.BorderType);
        Assert.True(s.ConbackTrue);
        Assert.Equal(0.25, s.ConbackAmount);
        Assert.False(s.GenericUi);
        Assert.Equal(new[] { "actor definitions", "key bindings" }, s.Phases);
        Assert.Equal(1, s.Actors);
        Assert.Equal(1, s.WadSettings);
        Assert.Equal(1, s.HacksPassed);
        Assert.Equal(1, s.TilesPassed);
        Assert.Equal(4, s.WindowProgress);
        Assert.Equal(4, s.ScreenProgress);
        Assert.Equal(new[]
        {
            "I_Init: Setting up machine state.",
            "S_InitData: Load sound definitions.",
            "G_ParseMapInfo: Load map definitions.",
            "Texman.Init: Init texture manager.",
            "ParseTeamInfo: Load team definitions.",
        }, s.Status);
        var empty = new VmDMain1422Slice.State { CompatValue = "", PlayerClasses = 1, TexturePasses = 0 };
        VmDMain1422Slice.LoadResources(empty);
        Assert.Equal(0, empty.CompatMode);
        Assert.Equal(2, empty.WindowProgress);
        var none = new VmDMain1422Slice.State { PlayerClasses = 0, TexturePasses = 1 };
        var fatal = Assert.Throws<VmDMain1422Slice.Fatal>(() => VmDMain1422Slice.LoadResources(none));
        Assert.Equal(VmDMain1422Slice.NoClasses, fatal.Message);
        Assert.Equal(2, none.WindowProgress);
        Assert.Equal(1, none.WadSettings);
        Assert.Equal(new[] { "actor definitions", "key bindings" }, none.Phases);
        var blank = new VmDMain1422Slice.State { NullStartScreen = true, TexturePasses = 2 };
        VmDMain1422Slice.LoadResources(blank);
        Assert.Equal(0, blank.ScreenProgress);
        Assert.Empty(blank.Phases);
        Assert.False(blank.Rendered);
        Assert.True(blank.SetMode);
        Assert.Equal(4, blank.WindowProgress);
        Assert.Throws<NullReferenceException>(() => VmDMain1422Slice.LoadResources(new VmDMain1422Slice.State { Border = null }));
        Assert.Throws<NullReferenceException>(() => VmDMain1422Slice.LoadResources(new VmDMain1422Slice.State { CpuDump = null }));
    }
}
