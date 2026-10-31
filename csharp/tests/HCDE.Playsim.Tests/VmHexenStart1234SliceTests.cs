using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmHexenStart1234SliceTests
{
    [Fact]
    public void Create_FallsBackAndPaintsEachNewNotch()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("error Start screen assets missing\n", head, StringComparison.Ordinal);
        Assert.Contains("lookup NETNOTCH ns=0 lump=21\n", head, StringComparison.Ordinal);
        Assert.Contains("music hexen order=1 looping=1 force=0\n", head, StringComparison.Ordinal);
        Assert.Contains("blit bg x=0 y=0 w=640 h=480\n", head, StringComparison.Ordinal);
        Assert.Contains("blit notch x=64 y=441\n", head, StringComparison.Ordinal);
        Assert.Contains("blit notch x=80 y=441\n", head, StringComparison.Ordinal);
        Assert.Contains("blit notch x=96 y=441\n", head, StringComparison.Ordinal);
        Assert.Contains("blit notch x=112 y=441\n", head, StringComparison.Ordinal);
        Assert.Equal(1, Count(head, "countdown\n"));
        Assert.Equal(4, Count(head, "error Start screen assets missing\n"));
        Assert.DoesNotContain("net recurse", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Progress_HoldsAtTheCapAndSkipsTheNetBlit()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("music orb order=1 looping=1 force=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("music skip\n", tail, StringComparison.Ordinal);
        Assert.Contains("notch calc=0 prev=4 num=32\n", tail, StringComparison.Ordinal);
        Assert.Contains("clean full=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("sound StartupTick\n", tail, StringComparison.Ordinal);
        Assert.Contains("notch calc=32 prev=32 num=32\n", tail, StringComparison.Ordinal);
        Assert.Contains("notch hold\n", tail, StringComparison.Ordinal);
        Assert.Contains("stretch max=12000\n", tail, StringComparison.Ordinal);
        Assert.Contains("notch calc=1 prev=0 num=10\n", tail, StringComparison.Ordinal);
        Assert.Contains("stretch max=2058\n", tail, StringComparison.Ordinal);
        Assert.Contains("notch skip\n", tail, StringComparison.Ordinal);
        Assert.Contains("net old=3 count=5\n", tail, StringComparison.Ordinal);
        Assert.Contains("net recurse\n", tail, StringComparison.Ordinal);
        Assert.Contains("sound PickupWeapon\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("blit notch x=288", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("misc/netnotch", tail, StringComparison.Ordinal);
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
        void Take(VmHexenStart1234 screen) => log.AddRange(screen.Log);

        Take(VmHexenStart1234.Create(100, -2, 9, 1, 0, 2, 0, true, true, true, 640, 480, 4, 4, 16, 8, false, "hexen"));
        Take(VmHexenStart1234.Create(100, -1, -1, 3, 0, 4, 0, true, true, true, 640, 480, 4, 4, 16, 8, false, "hexen"));
        Take(VmHexenStart1234.Create(100, 10, 0, 11, 0, 12, 0, false, true, true, 640, 480, 4, 4, 16, 8, false, "hexen"));
        Take(VmHexenStart1234.Create(100, 10, 0, 11, 0, 12, 0, true, true, true, 639, 480, 4, 4, 16, 8, false, "hexen"));
        var ok = VmHexenStart1234.Create(8000, 10, 0, -1, 21, 12, 0, true, true, true, 640, 480, 4, 4, 16, 8, false, "hexen");
        ok.DoProgress(249);
        ok.DoProgress(1);
        ok.DoProgress(1);
        ok.DoProgress(748);
        ok.DoProgress(1);
        Take(ok);
        log.Add("---- edge");
        var orb = VmHexenStart1234.Create(8000, 10, 0, 11, 0, 12, 0, true, true, true, 640, 480, 4, 4, 16, 8, false, "");
        orb.NotchPos = 4;
        orb.DoProgress(1);
        Take(orb);
        var batch = VmHexenStart1234.Create(8000, 10, 0, 11, 0, 12, 0, true, true, true, 640, 480, 4, 4, 16, 8, true, "hexen");
        batch.CurPos = 8000;
        batch.NotchPos = 32;
        batch.DoProgress(1);
        Take(batch);
        var wide = VmHexenStart1234.Create(10, 10, 0, 11, 0, 12, 0, true, true, true, 640, 480, 8, 8, 48, 8, false, "hexen");
        wide.DoProgress(1);
        wide.CurPos = 11;
        wide.MaxPos = 10;
        wide.DoProgress(1);
        Take(wide);
        var net = new VmHexenStart1234();
        net.NetCurPos = 3;
        net.DoNetProgress(5);
        net.NetDone();
        Take(net);
        return string.Join("\n", log) + "\n";
    }

    const string HeadGolden = """
        lookup STARTUP ns=15 lump=-2
        lookup NETNOTCH ns=15 lump=1
        lookup NOTCH ns=15 lump=2
        error Start screen assets missing
        lookup STARTUP ns=15 lump=-1
        lookup STARTUP ns=0 lump=-1
        lookup NETNOTCH ns=15 lump=3
        lookup NOTCH ns=15 lump=4
        error Start screen assets missing
        lookup STARTUP ns=15 lump=10
        lookup NETNOTCH ns=15 lump=11
        lookup NOTCH ns=15 lump=12
        image STARTUP lump=10 false
        image NETNOTCH lump=11 false
        image NOTCH lump=12 false
        error Start screen assets missing
        lookup STARTUP ns=15 lump=10
        lookup NETNOTCH ns=15 lump=11
        lookup NOTCH ns=15 lump=12
        image STARTUP lump=10 false
        image NETNOTCH lump=11 false
        image NOTCH lump=12 false
        error Start screen assets missing
        lookup STARTUP ns=15 lump=10
        lookup NETNOTCH ns=15 lump=-1
        lookup NETNOTCH ns=0 lump=21
        lookup NOTCH ns=15 lump=12
        image STARTUP lump=10 false
        image NETNOTCH lump=21 false
        image NOTCH lump=12 false
        cache net style=0 w=4 h=4
        cache notch style=0 w=16 h=8
        cache bg style=0 w=640 h=480
        bitmap 640x480
        blit bg x=0 y=0 w=640 h=480
        music hexen order=1 looping=1 force=0
        header
        countdown
        notch calc=0 prev=0 num=32
        notch hold
        base adv=249 cur=249 max=8000 done=1
        notch calc=1 prev=0 num=32
        blit notch x=64 y=441
        clean full=1
        sound StartupTick
        base adv=1 cur=250 max=8000 done=1
        notch calc=1 prev=1 num=32
        notch hold
        base adv=1 cur=251 max=8000 done=1
        notch calc=1 prev=1 num=32
        notch hold
        base adv=748 cur=999 max=8000 done=1
        notch calc=4 prev=1 num=32
        blit notch x=80 y=441
        blit notch x=96 y=441
        blit notch x=112 y=441
        clean full=1
        sound StartupTick
        base adv=1 cur=1000 max=8000 done=1

        """;

    const string TailGolden = """
        ---- edge
        lookup STARTUP ns=15 lump=10
        lookup NETNOTCH ns=15 lump=11
        lookup NOTCH ns=15 lump=12
        image STARTUP lump=10 false
        image NETNOTCH lump=11 false
        image NOTCH lump=12 false
        cache net style=0 w=4 h=4
        cache notch style=0 w=16 h=8
        cache bg style=0 w=640 h=480
        bitmap 640x480
        blit bg x=0 y=0 w=640 h=480
        music orb order=1 looping=1 force=0
        header
        countdown
        notch calc=0 prev=4 num=32
        clean full=1
        sound StartupTick
        base adv=1 cur=1 max=8000 done=1
        lookup STARTUP ns=15 lump=10
        lookup NETNOTCH ns=15 lump=11
        lookup NOTCH ns=15 lump=12
        image STARTUP lump=10 false
        image NETNOTCH lump=11 false
        image NOTCH lump=12 false
        cache net style=0 w=4 h=4
        cache notch style=0 w=16 h=8
        cache bg style=0 w=640 h=480
        bitmap 640x480
        blit bg x=0 y=0 w=640 h=480
        music skip
        header
        countdown
        notch calc=32 prev=32 num=32
        notch hold
        stretch max=12000
        base adv=1 cur=8001 max=12000 done=1
        lookup STARTUP ns=15 lump=10
        lookup NETNOTCH ns=15 lump=11
        lookup NOTCH ns=15 lump=12
        image STARTUP lump=10 false
        image NETNOTCH lump=11 false
        image NOTCH lump=12 false
        cache net style=0 w=8 h=8
        cache notch style=0 w=48 h=8
        cache bg style=0 w=640 h=480
        bitmap 640x480
        blit bg x=0 y=0 w=640 h=480
        music hexen order=1 looping=1 force=0
        header
        countdown
        notch calc=1 prev=0 num=10
        blit notch x=64 y=441
        clean full=1
        sound StartupTick
        stretch max=2058
        base adv=1 cur=1 max=2058 done=1
        notch skip
        stretch max=2058
        base adv=1 cur=12 max=2058 done=1
        net old=3 count=5
        net recurse
        sound PickupWeapon
        netdone

        """;
}
