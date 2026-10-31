using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmStats1224SliceTests
{
    [Fact]
    public void Find_PrefersCaseInsensitiveExactAndRejectsTwoPrefixes()
    {
        string dump = Build();
        int cut = dump.IndexOf("del-alpine", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("find [FPS] fps\n", head, StringComparison.Ordinal);
        Assert.Contains("find [Al] null\n", head, StringComparison.Ordinal);
        Assert.Contains("find [] null\n", head, StringComparison.Ordinal);
        Assert.Contains("Unknown stat: Al\n", head, StringComparison.Ordinal);
        Assert.Contains("get fps_max\n", head, StringComparison.Ordinal);
        Assert.DoesNotContain("t=\n", head, StringComparison.Ordinal);
        Assert.Equal(FindGolden, head);
    }

    [Fact]
    public void Destroy_UnlinksEvenSlotsAndAMiddleOfThreeThrows()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("del-alpine", StringComparison.Ordinal)..];
        Assert.Contains("del-fps Alpha fps_max\n", tail, StringComparison.Ordinal);
        Assert.Contains("del-max Alpha fps_max\n", tail, StringComparison.Ordinal);
        Assert.Contains("del-alpha fps_max\n", tail, StringComparison.Ordinal);
        Assert.Contains("t=\\n\n", tail, StringComparison.Ordinal);
        Assert.Equal(DestroyGolden, tail);

        var mid = new VmStats1224();
        var aa = mid.Add("A", () => "A");
        var bb = mid.Add("B", () => "B");
        mid.Add("C", () => "C");
        Assert.Throws<NullReferenceException>(() => mid.Destroy(bb));
        Assert.Equal("C B A", Names(mid));
        mid.Destroy(aa);
        Assert.Equal("C B", Names(mid));
    }

    static string Names(VmStats1224 stats)
    {
        var names = new List<string>();
        for (VmStat1224? p = stats.Head.Node; p != null; p = p.Next.Node)
            names.Add(p.Name);
        return string.Join(" ", names);
    }

    static string Build()
    {
        var s = new VmStats1224();
        var sb = s.Log;
        void Show(string tag)
        {
            sb.Append(tag);
            for (VmStat1224? p = s.Head.Node; p != null; p = p.Next.Node)
            {
                sb.Append(' ');
                sb.Append(p.Name);
            }

            sb.Append('\n');
        }

        void Found(string q)
        {
            VmStat1224? hit = s.Find(q);
            sb.Append("find [");
            sb.Append(q);
            sb.Append("] ");
            sb.Append(hit == null ? "null" : hit.Name);
            sb.Append('\n');
        }

        var fps = s.Add("fps", () => "ab\ncd\n");
        var mx = s.Add("fps_max", () => "");
        var alpha = s.Add("Alpha", () => "Z");
        var alpine = s.Add("Alpine", () => "no");
        Show("list");
        Found("FPS");
        Found("fps_");
        Found("Al");
        Found("alpha");
        Found("ALP");
        Found("");
        Found("zz");
        s.Toggle("FPS");
        s.Enable("Al", true);
        s.Enable("Alpine", true);
        s.Command(new[] { "stat" });
        s.Command(new[] { "stat", "nope" });
        s.Command(new[] { "stat", "fps" });
        alpha.Active = true;
        mx.Active = true;
        s.Print();
        s.Destroy(alpine);
        Show("del-alpine");
        s.Destroy(fps);
        Show("del-fps");
        s.Destroy(mx);
        Show("del-max");
        s.Destroy(alpha);
        Show("del-alpha");

        s.Head.Node = null;
        var a = s.Add("A", () => "\n");
        var b = s.Add("B", () => "Z");
        var c = s.Add("C", () => "no");
        c.Active = true;
        b.Active = true;
        a.Active = true;
        s.Print();
        return sb.ToString();
    }

    const string FindGolden =
        """
        list Alpine Alpha fps_max fps
        find [FPS] fps
        find [fps_] fps_max
        find [Al] null
        find [alpha] Alpha
        find [ALP] null
        find [] null
        find [zz] null
        Unknown stat: Al
        Usage: stat <statistics>
        Available stats:
         *Alpine
          Alpha
          fps_max
         *fps
        Unknown stat: nope
        get Alpine
        D 3 x=2500 y=92 vw=320 vh=240 k=1 t=no
        get Alpha
        D 3 x=2500 y=84 vw=320 vh=240 k=1 t=Z
        get fps_max

        """;

    const string DestroyGolden =
        """
        del-alpine Alpha fps_max fps
        del-fps Alpha fps_max
        del-max Alpha fps_max
        del-alpha fps_max
        get C
        D 3 x=2500 y=92 vw=320 vh=240 k=1 t=no
        get B
        D 3 x=2500 y=84 vw=320 vh=240 k=1 t=Z
        get A
        D 3 x=2500 y=76 vw=320 vh=240 k=1 t=\n

        """;
}
