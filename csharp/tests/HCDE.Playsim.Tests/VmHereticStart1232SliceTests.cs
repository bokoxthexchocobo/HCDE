using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmHereticStart1232SliceTests
{
    [Fact]
    public void Create_PatchesTheVersionAndPaintsAlignedNotches()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("error 'LOADING' not found\n", head, StringComparison.Ordinal);
        Assert.Contains("patch index=418 byte=51\n", head, StringComparison.Ordinal);
        Assert.Contains("bitmap 640x400\n", head, StringComparison.Ordinal);
        Assert.Contains("therm x=112 y=224 w=404 h=16 msgY=7 msgX=1 notch=0\n", head, StringComparison.Ordinal);
        Assert.Contains("notch calc=4 prev=0\n", head, StringComparison.Ordinal);
        Assert.Contains("clear x=112 y=224 w=4 h=16 b=85 g=255 r=85 a=255\n", head, StringComparison.Ordinal);
        Assert.Contains("clear x=116 y=224 w=4 h=16 b=85 g=255 r=85 a=255\n", head, StringComparison.Ordinal);
        Assert.Contains("base adv=79 cur=79 max=8000 done=1\n", head, StringComparison.Ordinal);
        Assert.Equal(2, Count(head, "error 'LOADING' not found\n"));
        Assert.Equal(1, Count(head, "countdown\n"));
        Assert.DoesNotContain("stretch ", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Progress_StretchesAfterTheNotchAndStatusUsesBytes()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("notch calc=2 prev=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("notch hold\n", tail, StringComparison.Ordinal);
        Assert.Contains("char x=17 y=7 byte=72 color=7\n", tail, StringComparison.Ordinal);
        Assert.Contains("load y=8 next=9\n", tail, StringComparison.Ordinal);
        Assert.Contains("char x=17 y=9 byte=81 color=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("char x=1 y=24 byte=90 color=31\n", tail, StringComparison.Ordinal);
        Assert.Contains("char x=3 y=24 byte=107 color=31\n", tail, StringComparison.Ordinal);
        Assert.Contains("append x=2 next=4\n", tail, StringComparison.Ordinal);
        Assert.Contains("notch calc=1 prev=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("stretch max=2452\n", tail, StringComparison.Ordinal);
        Assert.Contains("notch skip\n", tail, StringComparison.Ordinal);
        Assert.Contains("stretch max=2058\n", tail, StringComparison.Ordinal);
        Assert.Contains("base adv=1 cur=11 max=2058 done=1\n", tail, StringComparison.Ordinal);
        Assert.Equal(5, Count(tail, "render force=0\n"));
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
        void Take(VmHereticStart1232 screen) => log.AddRange(screen.Log);

        var bad = VmHereticStart1232.Create(100, -1, 4000, null);
        Take(bad);
        var shortFile = VmHereticStart1232.Create(100, 2, 3999, null);
        Take(shortFile);
        var ok = VmHereticStart1232.Create(8000, 4, 4000, null);
        ok.DoProgress(79);
        ok.DoProgress(1);
        ok.DoProgress(1);
        ok.DoProgress(77);
        ok.DoProgress(1);
        Take(ok);
        log.Add("---- edge");
        var slow = VmHereticStart1232.Create(8000, 4, 4000, null);
        slow.DoProgress(39);
        slow.DoProgress(1);
        slow.Loading("Hi", 7);
        slow.Loading("", 7);
        slow.Loading("Q", 256);
        slow.Append("Z");
        slow.Append("ok");
        Take(slow);
        var stretch = VmHereticStart1232.Create(404, 4, 4000, null);
        stretch.DoProgress(1);
        Take(stretch);
        var pegged = VmHereticStart1232.Create(10, 4, 4000, null);
        pegged.CurPos = 10;
        pegged.DoProgress(1);
        Take(pegged);
        return string.Join("\n", log) + "\n";
    }

    const string HeadGolden = """
        lookup LOADING lump=-1 len=4000
        error 'LOADING' not found
        lookup LOADING lump=2 len=3999
        error 'LOADING' not found
        lookup LOADING lump=4 len=4000
        read lump=4
        patch index=418 byte=51
        bitmap 640x400
        drawtext
        therm x=112 y=224 w=404 h=16 msgY=7 msgX=1 notch=0
        header
        countdown
        notch calc=0 prev=0
        notch hold
        base adv=79 cur=79 max=8000 done=1
        notch calc=4 prev=0
        clear x=112 y=224 w=4 h=16 b=85 g=255 r=85 a=255
        clean full=1
        base adv=1 cur=80 max=8000 done=1
        notch calc=4 prev=4
        notch hold
        base adv=1 cur=81 max=8000 done=1
        notch calc=4 prev=4
        notch hold
        base adv=77 cur=158 max=8000 done=1
        notch calc=8 prev=4
        clear x=116 y=224 w=4 h=16 b=85 g=255 r=85 a=255
        clean full=1
        base adv=1 cur=159 max=8000 done=1

        """;

    const string TailGolden = """
        ---- edge
        lookup LOADING lump=4 len=4000
        read lump=4
        patch index=418 byte=51
        bitmap 640x400
        drawtext
        therm x=112 y=224 w=404 h=16 msgY=7 msgX=1 notch=0
        header
        countdown
        notch calc=0 prev=0
        notch hold
        base adv=39 cur=39 max=8000 done=1
        notch calc=2 prev=0
        notch hold
        base adv=1 cur=40 max=8000 done=1
        char x=17 y=7 byte=72 color=7
        char x=18 y=7 byte=105 color=7
        load y=7 next=8
        clean full=1
        render force=0
        load y=8 next=9
        clean full=1
        render force=0
        char x=17 y=9 byte=81 color=0
        load y=9 next=10
        clean full=1
        render force=0
        char x=1 y=24 byte=90 color=31
        append x=1 next=2
        clean full=1
        render force=0
        char x=2 y=24 byte=111 color=31
        char x=3 y=24 byte=107 color=31
        append x=2 next=4
        clean full=1
        render force=0
        lookup LOADING lump=4 len=4000
        read lump=4
        patch index=418 byte=51
        bitmap 640x400
        drawtext
        therm x=112 y=224 w=404 h=16 msgY=7 msgX=1 notch=0
        header
        countdown
        notch calc=1 prev=0
        notch hold
        stretch max=2452
        base adv=1 cur=1 max=2452 done=1
        lookup LOADING lump=4 len=4000
        read lump=4
        patch index=418 byte=51
        bitmap 640x400
        drawtext
        therm x=112 y=224 w=404 h=16 msgY=7 msgX=1 notch=0
        header
        countdown
        notch skip
        stretch max=2058
        base adv=1 cur=11 max=2058 done=1

        """;
}
