namespace HCDE.Playsim.Tests;

public class VmVideoScale1251SliceTests
{
    const string HeadGolden =
"""
fill hit 0.000000
fill early 1
fill hit 0.000000
cvar mode 6 size=1
scaledw 320 mode=6 min=320
scaledh 200 mode=6 min=200
special 1 min 640 400
scaledw 640 mode=6 min=640
scaledh 400 mode=6 min=400
cvar mode 0 size=1
scaledw 1920 mode=0 min=640
scaledh 1080 mode=0 min=400
cvar mode 2 size=1
scaledw 640 mode=2 min=640
scaledh 400 mode=2 min=400
aspect 1.200000 mode=2
cvar mode 1 size=1
fill miss 0.370370
scaledw 711 mode=1 min=640
fill hit 0.370370
scaledh 400 mode=1 min=400
cvar mode 7 size=1
fill2 miss 0.370370
scaledw 1023 mode=7 min=640
fill2 hit 0.370370
scaledh 400 mode=7 min=400
cvar mode 0 size=1
cvar factor 1.500000 size=1 show=1
scaledw 2880 mode=0 min=640
scaledh 1620 mode=0 min=400
Current vid_scalefactor: 1.500000
Real resolution: 1920 x 1080
Emulated resolution: 2880 x 1620
cvar factor 1.000000 size=1 show=0
cvar crop 1 size=1
cropw 1920x1080 -> 1440x1080
scaledw 1440 mode=0 min=640
croph 1920x1080 -> 1440x1080
scaledh 1080 mode=0 min=400
cvar crop 0 size=1
cvar mode 5 size=1
cvar width 4000 size=1
cvar height 3000 size=1
cvar aspect 1.250000 size=1
aspect 1.250000 mode=5
scaledw 4000 mode=5 min=640
scaledh 3000 mode=5 min=400
linear super=1 flag=0 -> 1
menu linear 0
menu width 1920
menu aspect 1.000000

""";

    const string TailGolden =
"""
---- edge
cvar mode 0 size=1
cvar factor 1.000000 size=1 show=0
special 0 min 320 200
scaledw 320 mode=0 min=320
scaledh 200 mode=0 min=200
cvar linear 1
cvar linear 0
cvar factor 1.000000 size=1 show=0
cvar factor 1.000000 size=1 show=0
cvar factor 2.000000 size=1 show=1
scaledw 3840 mode=0 min=320
scaledh 2160 mode=0 min=200
Current vid_scalefactor: 2.000000
Real resolution: 1920 x 1080
Emulated resolution: 3840 x 2160
cvar factor 1.000000 size=1 show=0
cvar mode 0 size=1
mode clamp 0
scaledw 1920 mode=0 min=320
cvar aspect 1.000000 size=1
cvar aspect 0.200000 size=1
cvar aspect 5.000000 size=1
cvar aspect 1.000000 size=1
cvar width 320 size=1
Usage: vid_setscale <x> <y> [bool linear] [float pixel-shape]
This command will create a custom viewport scaling mode.
cvar width 800 size=1
cvar height 500 size=1
cvar linear 1
cvar aspect 1.250000 size=1
cvar mode 5 size=1
cvar factor 1.000000 size=1 show=0
cvar factor 1.000000 size=1 show=0
scaledw 800 mode=5 min=320
cvar factor 0.800000 size=1 show=1
scaledw 640 mode=5 min=320
scaledh 400 mode=5 min=200
Current vid_scalefactor: 0.800000
Real resolution: 1920 x 1080
Emulated resolution: 640 x 400
cvar factor 1.000000 size=1 show=0
scaledh 500 mode=5 min=200
cvar factor 0.800000 size=1 show=1
scaledw 640 mode=5 min=320
scaledh 400 mode=5 min=200
Current vid_scalefactor: 0.800000
Real resolution: 1920 x 1080
Emulated resolution: 640 x 400
cvar mode 1 size=1
cvar factor 1.000000 size=1 show=0
cvar mode 5 size=1
cvar factor 1.000000 size=1 show=0
cvar aspect 1.000000 size=1
fill hit 0.370370
cvar width 711 size=1
fill hit 0.370370
cvar height 400 size=1
cvar mode 0 size=1
fill hit 0.370370
cvar factor 0.370370 size=1 show=1
scaledw 711 mode=0 min=320
scaledh 400 mode=0 min=200
Current vid_scalefactor: 0.370370
Real resolution: 1920 x 1080
Emulated resolution: 711 x 400
cvar width 640 size=1
cvar height 400 size=1
cvar linear 0
cvar mode 5 size=1
cvar factor 1.000000 size=1 show=0

""";

    [Fact]
    public void Scale_CachesTheFillRatioAndRaisesTheUiMinimum()
    {
        string dump = Build(out VmVideoScale1251 vm);
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("fill hit 0.000000\n", head, StringComparison.Ordinal);
        Assert.Contains("fill early 1\n", head, StringComparison.Ordinal);
        Assert.Contains("special 1 min 640 400\n", head, StringComparison.Ordinal);
        Assert.Contains("scaledw 320 mode=6 min=320\n", head, StringComparison.Ordinal);
        Assert.Contains("scaledw 711 mode=1 min=640\n", head, StringComparison.Ordinal);
        Assert.Contains("scaledw 1023 mode=7 min=640\n", head, StringComparison.Ordinal);
        Assert.Contains("cropw 1920x1080 -> 1440x1080\n", head, StringComparison.Ordinal);
        Assert.Contains("croph 1920x1080 -> 1440x1080\n", head, StringComparison.Ordinal);
        Assert.Contains("menu width 1920\n", head, StringComparison.Ordinal);
        Assert.Contains("menu linear 0\n", head, StringComparison.Ordinal);
        Assert.Contains("aspect 1.200000 mode=2\n", head, StringComparison.Ordinal);
        _ = vm;
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Scale_ClampsTheCvarsAndAppliesTheConsoleCommands()
    {
        string dump = Build(out VmVideoScale1251 vm);
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("cvar factor 1.000000 size=1 show=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("cvar factor 2.000000 size=1 show=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("Current vid_scalefactor: 2.000000\n", tail, StringComparison.Ordinal);
        Assert.Contains("mode clamp 0\n", tail, StringComparison.Ordinal);
        Assert.Contains("Usage: vid_setscale <x> <y> [bool linear] [float pixel-shape]\n", tail, StringComparison.Ordinal);
        Assert.Contains("cvar width 800 size=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("cvar linear 1\n", tail, StringComparison.Ordinal);
        Assert.Contains("cvar linear 0\n", tail, StringComparison.Ordinal);
        Assert.Contains("cvar width 711 size=1\n", tail, StringComparison.Ordinal);
        Assert.Equal(5, vm.Mode);
        Assert.Equal(640, vm.CustomWidth);
        Assert.Equal(400, vm.CustomHeight);
        Assert.False(vm.Linear);
        Assert.Equal(1f, vm.Factor);
        Assert.Equal(1f, vm.PixelAspect);
        Assert.Equal(320u, vm.MinW);
        Assert.Equal(TailGolden, tail);
    }

    static string Build(out VmVideoScale1251 vm)
    {
        vm = new VmVideoScale1251();
        vm.Fill(0, 0);
        vm.Fill(0, 5);
        vm.Fill(0, 0);

        vm.IsSpecialUi = () => false;
        vm.SetMode(6);
        vm.ScaledWidth(1920, 1080);
        vm.ScaledHeight(1920, 1080);

        vm.IsSpecialUi = null;
        vm.ScaledWidth(1920, 1080);
        vm.ScaledHeight(1920, 1080);

        vm.SetMode(0);
        vm.ScaledWidth(1920, 1080);
        vm.ScaledHeight(1920, 1080);

        vm.SetMode(2);
        vm.ScaledWidth(1920, 1080);
        vm.ScaledHeight(1920, 1080);
        vm.PixelAspectOf();

        vm.SetMode(1);
        vm.ScaledWidth(1920, 1080);
        vm.ScaledHeight(1920, 1080);

        vm.SetMode(7);
        vm.ScaledWidth(1920, 1080);
        vm.ScaledHeight(1920, 1080);

        vm.SetMode(0);
        vm.SetFactor(1.5f);
        vm.SetFactor(1f);

        vm.VidAspect = 3;
        vm.SetCrop(true);
        vm.ScaledWidth(1920, 1080);
        vm.ScaledHeight(1920, 1080);
        vm.SetCrop(false);
        vm.VidAspect = 0;

        vm.SetMode(5);
        vm.SetCustomWidth(4000);
        vm.SetCustomHeight(3000);
        vm.SetPixelAspect(1.25f);
        vm.LinearScale();
        vm.MenuActive = 1;
        vm.MenuKind = "VideoOptions";
        vm.LinearScale();
        vm.ScaledWidth(1920, 1080);
        vm.PixelAspectOf();
        vm.MenuActive = 0;
        vm.MenuKind = null;
        vm.Log.Add("---- edge");

        vm.IsSpecialUi = () => false;
        vm.SetMode(0);
        vm.SetFactor(1f);
        vm.ScaledWidth(100, 100);
        vm.ScaledHeight(100, 100);

        vm.SizeNeeded = false;
        vm.SetLinear(true);
        vm.SetLinear(false);

        vm.SetFactor(0.04f);
        vm.SetFactor(2.1f);
        vm.SetFactor(2f);
        vm.SetFactor(1f);

        vm.SetMode(8);
        vm.Valid[4] = false;
        vm.ForceMode(4);
        vm.ScaledWidth(1920, 1080);
        vm.Valid[4] = true;

        vm.SetPixelAspect(0.1f);
        vm.SetPixelAspect(0.2f);
        vm.SetPixelAspect(5f);
        vm.SetPixelAspect(5.1f);
        vm.SetCustomWidth(10);

        vm.CmdSetScale(new[] { "vid_setscale" });
        vm.CmdSetScale(new[] { "vid_setscale", "800px", "500", "TRUE", "1.25" });
        vm.CmdToWidth(new[] { "vid_scaletowidth" });
        vm.CmdToWidth(new[] { "vid_scaletowidth", "640" });
        vm.CmdToHeight(new[] { "vid_scaletoheight", "400" });
        vm.CmdToLowest(new[] { "vid_scaletolowest", "2" });
        vm.CmdToLowest(new[] { "vid_scaletolowest", "1" });
        vm.CmdToLowest(new[] { "vid_scaletolowest", "9" });
        vm.CmdSetScale(new[] { "vid_setscale", "640", "400", "yes" });

        return string.Join('\n', vm.Log) + "\n";
    }
}
