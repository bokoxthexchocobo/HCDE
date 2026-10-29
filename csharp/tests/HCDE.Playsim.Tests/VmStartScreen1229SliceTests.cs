using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmStartScreen1229SliceTests
{
    [Fact]
    public void Create_CentersASmallLogoAndPaintsTheNotch()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("blit center x=590 y=420\n", head, StringComparison.Ordinal);
        Assert.Contains("accent b=153 g=102 r=51 a=255\n", head, StringComparison.Ordinal);
        Assert.Contains("notch pos=0 prev=0 hold\n", head, StringComparison.Ordinal);
        Assert.Contains("notch pos=200 prev=0 draw\n", head, StringComparison.Ordinal);
        Assert.Contains("clear x=440 y=930 w=200 h=8 b=153 g=102 r=51 a=255\n", head, StringComparison.Ordinal);
        Assert.Contains("phase skip\n", head, StringComparison.Ordinal);
        Assert.Contains("clean 1\n", head, StringComparison.Ordinal);
        Assert.Equal(1, Count(head, "accent "));
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Create_HoldsTheBarAndDrawsTheLastConsoleLines()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("blit center x=0 y=-39\n", tail, StringComparison.Ordinal);
        Assert.Contains("clear x=440 y=930 w=400 h=8 b=0 g=0 r=0 a=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("notch pos=2 prev=400 hold\n", tail, StringComparison.Ordinal);
        Assert.Contains("blit crop x=0 y=0 w=1280 h=960\n", tail, StringComparison.Ordinal);
        Assert.Contains("blit center x=1 y=-40\n", tail, StringComparison.Ordinal);
        Assert.Contains("blit center x=-360 y=390\n", tail, StringComparison.Ordinal);
        Assert.Contains("image none\n", tail, StringComparison.Ordinal);
        Assert.Contains("overlay skip buffer\n", tail, StringComparison.Ordinal);
        Assert.Contains("draw i=0 x=1 y=0.5 [L2] fb=170 fg=170 fr=170 fa=255 bb=0 bg=0 br=0 ba=180\n", tail, StringComparison.Ordinal);
        Assert.Contains("draw i=7 x=1 y=7.5 [L9] fb=170 fg=170 fr=170 fa=255 bb=0 bg=0 br=0 ba=180\n", tail, StringComparison.Ordinal);
        Assert.Contains("accent b=51 g=34 r=17 a=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("base adv=2 cur=-4 max=-4 done=1\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("[L0]", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("[L1]", tail, StringComparison.Ordinal);
        Assert.Equal(2, Count(tail, "accent "));
        Assert.Equal(TailGolden, tail);
    }

    static int Count(string hay, string needle)
    {
        int n = 0;
        int i = 0;
        while (true)
        {
            i = hay.IndexOf(needle, i, StringComparison.Ordinal);
            if (i < 0)
                return n;
            n++;
            i += needle.Length;
        }
    }

    static string Build()
    {
        var log = new List<string>();
        void Take(VmGenericStart1229 screen)
        {
            log.AddRange(screen.Log);
        }

        VmGenericStart1229.ResetAccent();
        VmGenericStart1229.AccentBgra = unchecked((int)0xFF336699);
        var a = VmGenericStart1229.Create(8000, 4, true, 100, 40);
        a.DoProgress(1);
        a.DoProgress(4000);
        a.StartupStatus("boot", 5, true);
        a.Loading("", 2);
        a.Loading("maps", 7);
        a.Append("more");
        Take(a);

        var b = VmGenericStart1229.Create(100, -1, false, 0, 0);
        b.HasTexture = true;
        b.DoProgress(50);
        b.DoProgress(0);
        Take(b);

        log.Add("---- edge");
        VmGenericStart1229.ResetAccent();
        VmGenericStart1229.AccentBgra = 0;
        var c = VmGenericStart1229.Create(10, 9, true, 1280, 959);
        c.DoProgress(10);
        c.DoProgress(1);
        c.DoProgress(-3);
        VmGenericStart1229.AccentBgra = unchecked((int)0xFFFFFFFF);
        c.DoProgress(1);
        Take(c);

        Take(VmGenericStart1229.Create(100, 2, true, 1300, 1000));
        Take(VmGenericStart1229.Create(100, 1, true, 1279, 960));
        Take(VmGenericStart1229.Create(100, 3, true, 1, 1));
        Take(VmGenericStart1229.Create(100, 8, false, 0, 0));
        Take(VmGenericStart1229.Create(100, 5, true, 3, 5));
        Take(VmGenericStart1229.Create(50, 6, true, 2000, 100));
        Take(VmGenericStart1229.Create(20, 11, true, 1280, 960));

        var o = VmGenericStart1229.Create(4, -1, false, 0, 0);
        o.HasBuffer = false;
        o.DrawOverlay();
        o.HasBuffer = true;
        o.HasFont = false;
        o.DrawOverlay();
        o.HasFont = true;
        o.FontHeight = 10;
        o.Lines = Array.Empty<string>();
        o.LineCount = -1;
        o.DrawOverlay();
        o.Lines = null;
        o.LineCount = 4;
        o.DrawOverlay();
        o.Lines = new[] { "L0", "L1", "L2", "L3", "L4", "L5", "L6", "L7", "L8", "L9" };
        o.LineCount = -1;
        o.DrawOverlay();
        Take(o);

        VmGenericStart1229.ResetAccent();
        VmGenericStart1229.AccentBgra = 0x00112233;
        var z = VmGenericStart1229.Create(0, -1, false, 0, 0);
        z.DoProgress(1);
        z.DoProgress(1);
        Take(z);
        var n = VmGenericStart1229.Create(-4, -1, false, 0, 0);
        n.DoProgress(2);
        Take(n);
        return string.Join("\n", log) + "\n";
    }

    const string HeadGolden = """
        lookup BOOTLOGO ns=15 lump=4
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        image w=100 h=40 style=0
        blit center x=590 y=420
        countdown
        accent b=153 g=102 r=51 a=255
        base adv=1 cur=1 max=8000 done=1
        notch pos=0 prev=0 hold
        base adv=4000 cur=4001 max=8000 done=1
        notch pos=200 prev=0 draw
        clear x=440 y=930 w=200 h=8 b=153 g=102 r=51 a=255
        msg startup colors=5 center=1 [boot]
        phase skip
        msg loading colors=2 []
        phase [maps]
        msg loading colors=7 [maps]
        msg append [more]
        lookup BOOTLOGO ns=15 lump=-1
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        countdown
        base adv=50 cur=50 max=100 done=1
        notch pos=200 prev=0 draw
        clear x=440 y=930 w=200 h=8 b=153 g=102 r=51 a=255
        clean 1
        base adv=0 cur=50 max=100 done=1
        notch pos=200 prev=200 hold

        """;

    const string TailGolden = """
        ---- edge
        lookup BOOTLOGO ns=15 lump=9
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        image w=1280 h=959 style=0
        blit center x=0 y=-39
        countdown
        accent b=0 g=0 r=0 a=0
        base adv=10 cur=10 max=10 done=1
        notch pos=400 prev=0 draw
        clear x=440 y=930 w=400 h=8 b=0 g=0 r=0 a=0
        base adv=1 cur=11 max=2058 done=1
        notch pos=2 prev=400 hold
        base adv=-3 cur=8 max=2058 done=1
        notch pos=1 prev=400 hold
        base adv=1 cur=9 max=2058 done=1
        notch pos=1 prev=400 hold
        lookup BOOTLOGO ns=15 lump=2
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        image w=1300 h=1000 style=0
        blit crop x=0 y=0 w=1280 h=960
        countdown
        lookup BOOTLOGO ns=15 lump=1
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        image w=1279 h=960 style=0
        blit center x=1 y=-40
        countdown
        lookup BOOTLOGO ns=15 lump=3
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        image w=1 h=1 style=0
        blit center x=640 y=440
        countdown
        lookup BOOTLOGO ns=15 lump=8
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        image none
        countdown
        lookup BOOTLOGO ns=15 lump=5
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        image w=3 h=5 style=0
        blit center x=639 y=438
        countdown
        lookup BOOTLOGO ns=15 lump=6
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        image w=2000 h=100 style=0
        blit center x=-360 y=390
        countdown
        lookup BOOTLOGO ns=15 lump=11
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        image w=1280 h=960 style=0
        blit crop x=0 y=0 w=1280 h=960
        countdown
        lookup BOOTLOGO ns=15 lump=-1
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        countdown
        overlay skip buffer
        overlay skip font
        overlay format panel=1264 lines=0 ptr=1
        overlay no
        overlay format panel=1264 lines=4 ptr=0
        overlay no
        overlay format panel=1264 lines=10 ptr=1
        clear x=0 y=0 w=1280 h=88 b=0 g=0 r=0 a=180
        draw i=0 x=1 y=0.5 [L2] fb=170 fg=170 fr=170 fa=255 bb=0 bg=0 br=0 ba=180
        draw i=1 x=1 y=1.5 [L3] fb=170 fg=170 fr=170 fa=255 bb=0 bg=0 br=0 ba=180
        draw i=2 x=1 y=2.5 [L4] fb=170 fg=170 fr=170 fa=255 bb=0 bg=0 br=0 ba=180
        draw i=3 x=1 y=3.5 [L5] fb=170 fg=170 fr=170 fa=255 bb=0 bg=0 br=0 ba=180
        draw i=4 x=1 y=4.5 [L6] fb=170 fg=170 fr=170 fa=255 bb=0 bg=0 br=0 ba=180
        draw i=5 x=1 y=5.5 [L7] fb=170 fg=170 fr=170 fa=255 bb=0 bg=0 br=0 ba=180
        draw i=6 x=1 y=6.5 [L8] fb=170 fg=170 fr=170 fa=255 bb=0 bg=0 br=0 ba=180
        draw i=7 x=1 y=7.5 [L9] fb=170 fg=170 fr=170 fa=255 bb=0 bg=0 br=0 ba=180
        overlay yes
        lookup BOOTLOGO ns=15 lump=-1
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        countdown
        accent b=51 g=34 r=17 a=0
        base adv=1 cur=0 max=0 done=1
        base adv=1 cur=0 max=0 done=1
        lookup BOOTLOGO ns=15 lump=-1
        bitmap 1280x960
        clear x=0 y=0 w=1280 h=960 b=0 g=0 r=0 a=255
        countdown
        base adv=2 cur=-4 max=-4 done=1

        """;
}
