using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmStrifeStart1233SliceTests
{
    [Fact]
    public void Create_RejectsABadBackgroundAndPaintsEvenNotches()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("assets w=319 h=200\n", head, StringComparison.Ordinal);
        Assert.Contains("error bad startscreen assets\n", head, StringComparison.Ordinal);
        Assert.Contains("lookup STRTPB1 ns=0 lump=20\n", head, StringComparison.Ordinal);
        Assert.Contains("image lump=13 false\nlookup STRTLZ1 ns=15 lump=14\n", head, StringComparison.Ordinal);
        Assert.Contains("assets w=320 h=200\n", head, StringComparison.Ordinal);
        Assert.Contains("blit x=60 y=156 pic=4\n", head, StringComparison.Ordinal);
        Assert.Contains("scale 2\n", head, StringComparison.Ordinal);
        Assert.Contains("base adv=86 cur=86 max=8000 done=1\n", head, StringComparison.Ordinal);
        Assert.Contains("blit x=62 y=156 pic=4\n", head, StringComparison.Ordinal);
        Assert.Contains("blit x=262 y=136 pic=1\n", head, StringComparison.Ordinal);
        Assert.Contains("notch calc=2 prev=2\n", head, StringComparison.Ordinal);
        Assert.Equal(1, Count(head, "countdown\n"));
        Assert.Equal(1, Count(head, "error bad startscreen assets\n"));
        Assert.DoesNotContain("stretch ", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Draw_IgnoresTheOldLaserAndHopsTheBot()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("notch calc=1 prev=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("notch hold\n", tail, StringComparison.Ordinal);
        Assert.Contains("stretch max=2232\n", tail, StringComparison.Ordinal);
        Assert.Contains("notch skip\n", tail, StringComparison.Ordinal);
        Assert.Contains("stretch max=2058\n", tail, StringComparison.Ordinal);
        Assert.Contains("draw old=9 new=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("blit x=61 y=156 pic=5\n", tail, StringComparison.Ordinal);
        Assert.Contains("blit x=14 y=140 pic=6\n", tail, StringComparison.Ordinal);
        Assert.Contains("blit x=68 y=156 pic=4\n", tail, StringComparison.Ordinal);
        Assert.Contains("blit x=14 y=139 pic=6\n", tail, StringComparison.Ordinal);
        Assert.Contains("blit x=66 y=156 pic=4\n", tail, StringComparison.Ordinal);
        Assert.Contains("blit x=262 y=136 pic=3\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("blit x=69 ", tail, StringComparison.Ordinal);
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
        void Take(VmStrifeStart1233 screen) => log.AddRange(screen.Log);

        int[] nsGood = { 10, -1, 12, 13, 14, 15, 16, 17 };
        int[] anyGood = { 0, 20, 0, 0, 0, 0, 0, 0 };
        bool[] hasGood = { true, true, true, false, true, true, true, true };
        int[] wGood = { 32, 32, 32, 0, 16, 16, 48, 320 };
        int[] hGood = { 64, 64, 64, 0, 16, 16, 48, 200 };
        int[] nsBad = { 1, 2, 3, 4, 5, 6, 7, 8 };
        int[] anyBad = { 0, 0, 0, 0, 0, 0, 0, 0 };
        bool[] hasBad = { true, true, true, true, true, true, true, true };
        int[] wBad = { 32, 32, 32, 0, 16, 16, 48, 319 };

        Take(VmStrifeStart1233.Create(100, nsBad, anyBad, hasBad, wBad, hGood));
        var ok = VmStrifeStart1233.Create(8000, nsGood, anyGood, hasGood, wGood, hGood);
        ok.DoProgress(86);
        ok.DoProgress(1);
        ok.DoProgress(1);
        Take(ok);
        log.Add("---- edge");
        var tail = VmStrifeStart1233.Create(8000, nsGood, anyGood, hasGood, wGood, hGood);
        tail.DoProgress(43);
        tail.DoProgress(1);
        tail.MaxPos = 184;
        tail.CurPos = 0;
        tail.NotchPos = 0;
        tail.DoProgress(1);
        tail.CurPos = 10;
        tail.MaxPos = 10;
        tail.DoProgress(1);
        tail.DrawStuff(9, 1);
        tail.DrawStuff(9, 8);
        tail.DrawStuff(0, 6);
        Take(tail);
        return string.Join("\n", log) + "\n";
    }

    const string HeadGolden = """
        lookup STRTPA1 ns=15 lump=1
        image lump=1 false
        cache style=0 w=32 h=64
        lookup STRTPB1 ns=15 lump=2
        image lump=2 false
        cache style=0 w=32 h=64
        lookup STRTPC1 ns=15 lump=3
        image lump=3 false
        cache style=0 w=32 h=64
        lookup STRTPD1 ns=15 lump=4
        image lump=4 false
        cache style=0 w=0 h=0
        lookup STRTLZ1 ns=15 lump=5
        image lump=5 false
        cache style=0 w=16 h=16
        lookup STRTLZ2 ns=15 lump=6
        image lump=6 false
        cache style=0 w=16 h=16
        lookup STRTBOT ns=15 lump=7
        image lump=7 false
        cache style=0 w=48 h=48
        lookup STARTUP0 ns=15 lump=8
        image lump=8 false
        cache style=0 w=319 h=200
        assets w=319 h=200
        error bad startscreen assets
        lookup STRTPA1 ns=15 lump=10
        image lump=10 false
        cache style=0 w=32 h=64
        lookup STRTPB1 ns=15 lump=-1
        lookup STRTPB1 ns=0 lump=20
        image lump=20 false
        cache style=0 w=32 h=64
        lookup STRTPC1 ns=15 lump=12
        image lump=12 false
        cache style=0 w=32 h=64
        lookup STRTPD1 ns=15 lump=13
        image lump=13 false
        lookup STRTLZ1 ns=15 lump=14
        image lump=14 false
        cache style=0 w=16 h=16
        lookup STRTLZ2 ns=15 lump=15
        image lump=15 false
        cache style=0 w=16 h=16
        lookup STRTBOT ns=15 lump=16
        image lump=16 false
        cache style=0 w=48 h=48
        lookup STARTUP0 ns=15 lump=17
        image lump=17 false
        cache style=0 w=320 h=200
        assets w=320 h=200
        draw old=0 new=0
        blit x=0 y=0 pic=7
        blit x=60 y=156 pic=4
        blit x=14 y=138 pic=6
        blit x=262 y=136 pic=0
        scale 2
        header
        countdown
        notch calc=0 prev=0
        notch hold
        base adv=86 cur=86 max=8000 done=1
        notch calc=2 prev=0
        draw old=0 new=2
        blit x=0 y=0 pic=7
        blit x=62 y=156 pic=4
        blit x=14 y=138 pic=6
        blit x=262 y=136 pic=1
        clean full=1
        base adv=1 cur=87 max=8000 done=1
        notch calc=2 prev=2
        notch hold
        base adv=1 cur=88 max=8000 done=1

        """;

    const string TailGolden = """
        ---- edge
        lookup STRTPA1 ns=15 lump=10
        image lump=10 false
        cache style=0 w=32 h=64
        lookup STRTPB1 ns=15 lump=-1
        lookup STRTPB1 ns=0 lump=20
        image lump=20 false
        cache style=0 w=32 h=64
        lookup STRTPC1 ns=15 lump=12
        image lump=12 false
        cache style=0 w=32 h=64
        lookup STRTPD1 ns=15 lump=13
        image lump=13 false
        lookup STRTLZ1 ns=15 lump=14
        image lump=14 false
        cache style=0 w=16 h=16
        lookup STRTLZ2 ns=15 lump=15
        image lump=15 false
        cache style=0 w=16 h=16
        lookup STRTBOT ns=15 lump=16
        image lump=16 false
        cache style=0 w=48 h=48
        lookup STARTUP0 ns=15 lump=17
        image lump=17 false
        cache style=0 w=320 h=200
        assets w=320 h=200
        draw old=0 new=0
        blit x=0 y=0 pic=7
        blit x=60 y=156 pic=4
        blit x=14 y=138 pic=6
        blit x=262 y=136 pic=0
        scale 2
        header
        countdown
        notch calc=0 prev=0
        notch hold
        base adv=43 cur=43 max=8000 done=1
        notch calc=1 prev=0
        notch hold
        base adv=1 cur=44 max=8000 done=1
        notch calc=1 prev=0
        notch hold
        stretch max=2232
        base adv=1 cur=1 max=2232 done=1
        notch skip
        stretch max=2058
        base adv=1 cur=11 max=2058 done=1
        draw old=9 new=1
        blit x=0 y=0 pic=7
        blit x=61 y=156 pic=5
        blit x=14 y=138 pic=6
        blit x=262 y=136 pic=0
        draw old=9 new=8
        blit x=0 y=0 pic=7
        blit x=68 y=156 pic=4
        blit x=14 y=140 pic=6
        blit x=262 y=136 pic=0
        draw old=0 new=6
        blit x=0 y=0 pic=7
        blit x=66 y=156 pic=4
        blit x=14 y=139 pic=6
        blit x=262 y=136 pic=3

        """;
}
