using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmNotify1226SliceTests
{
    [Fact]
    public void Add_PopsALiveDuplicateAndStillSwallows()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- draw\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("scale 0\n", head, StringComparison.Ordinal);
        Assert.Contains("shift 2 n=2\n", head, StringComparison.Ordinal);
        Assert.Contains("shift -1 n=2\n", head, StringComparison.Ordinal);
        Assert.Contains("add lvl=1 font=2 w=33 time=1.5 lines=4 text=hi\n", head, StringComparison.Ordinal);
        Assert.Contains("stack n=2 pop=1 last=hi\n", head, StringComparison.Ordinal);
        Assert.Contains("swallow\n", head, StringComparison.Ordinal);
        Assert.Equal(AddGolden, head);
    }

    [Fact]
    public void Draw_FadesPulsesAndCentersTheStackSuffix()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- draw\n", StringComparison.Ordinal)..];
        Assert.Contains("D 99 0 21 106 66 1.0000000000 full\n", tail, StringComparison.Ordinal);
        Assert.Contains("D 11 0 29 106 66 1.0000000000 high\n", tail, StringComparison.Ordinal);
        Assert.Contains("D 5 0 13 106 66 0.3500000000 p0\n", tail, StringComparison.Ordinal);
        Assert.Contains("S 2 45 9 100 50 1.0000000000  (x3)\n", tail, StringComparison.Ordinal);
        Assert.Contains("top 4\n", tail, StringComparison.Ordinal);
        Assert.Equal(DrawGolden, tail);
    }

    static string Build()
    {
        var b = new VmNotifyBuffer1226();
        int swallow = 0;
        b.ProcessNotify = (level, text) => swallow;
        b.ScaleOf = generic => generic ? 2 : 3;
        b.WidthOf = text => text.Length * 3;
        b.SetScaleText(-3);
        b.SetScaleText(4);
        void Note(string t, int timeout, int ticker, int level)
        {
            b.Text.Add(new VmNotifyText1226 { Text = t, TimeOut = timeout, Ticker = ticker, PrintLevel = level });
        }
        Note("A", 10, 0, 0);
        Note("B", 10, 0, 0);
        Note("C", 10, 0, 0);
        Note("D", 10, 0, 0);
        b.SetNotifyLines(2);
        b.SetNotifyLines(-1);
        b.SetNotifyLines(0);

        b.HasStatusBar = true;
        Note("z", 1, 0, 0);
        b.CountedIdentical = 9;
        b.LastNotify = "z";
        b.Clear();
        b.HasStatusBar = false;
        b.HasFlush = false;
        Note("q", 1, 0, 0);
        b.Clear();

        b.ShowMessages = false;
        b.AddString(1, "a");
        b.ShowMessages = true;
        b.AddString(1, "");
        b.GameState = VmNotifyBuffer1226.StateDemo;
        b.AddString(1, "a");
        b.GameState = VmNotifyBuffer1226.StateFullConsole;
        b.AddString(1, "a");
        b.GameState = 0;
        b.NotifyLines = 0;
        b.AddString(1, "a");
        b.NotifyLines = 4;
        b.DisplayWidth = 100;
        b.NotifyTime = 1.5;
        b.GenericUi = false;
        b.AddString(1024 | 1, "hi");
        b.AddString(200, "lo");

        b.StackIdent = true;
        Note("old", 10, 0, 0);
        b.LastNotify = "hi";
        b.AddString(1, "hi");
        b.LastNotify = "hi";
        Note("hi", 10, 10, 0);
        b.AddString(1, "hi");
        b.AddString(1, "ab");
        b.LastNotify = "Ab";
        b.AddString(1, "ab");

        b.HasStatusBar = true;
        b.HasProcess = true;
        swallow = 1;
        Note("same", 10, 0, 0);
        b.LastNotify = "same";
        b.CountedIdentical = 1;
        b.AddString(4, "same");
        swallow = 0;
        b.HasStatusBar = false;
        b.AddString(4, "same");

        b.Log.Add("---- draw");
        b.Text.Clear();
        b.Top = 10;
        b.TopGoal = 2;
        b.FontDisplacement = 3;
        b.FontHeight = 8;
        b.PrintColors[4] = 99;
        b.PrintColors[5] = 77;
        b.StackIdent = false;
        b.CenterNotify = false;
        b.PulseText = false;
        b.GenericUi = false;
        b.CountedIdentical = 1;
        b.DrawerWidth = 320;
        b.DrawerHeight = 200;
        Note("idle", 0, 0, 0);
        Note("fade", 5, 0, 0);
        Note("full", 6, 0, 4);
        Note("dead", 3, 3, 0);
        Note("high", 8, 1, 6);
        b.Draw();

        b.Text.Clear();
        b.PulseText = true;
        b.MsTime = 0;
        Note("p0", 3, 0, 1);
        b.Draw();
        b.Text.Clear();
        b.MsTime = 1000;
        Note("p1", 3, 0, 1);
        b.Draw();

        b.Text.Clear();
        b.PulseText = false;
        b.StackIdent = true;
        b.CountedIdentical = 3;
        b.CenterNotify = true;
        b.GenericUi = true;
        b.DrawerWidth = 200;
        b.DrawerHeight = 100;
        b.Top = 1;
        b.TopGoal = 9;
        b.FontDisplacement = 0;
        Note("AB", 9, 0, 2);
        Note("CD", 9, 0, 2);
        b.Draw();

        b.Text.Clear();
        b.Top = 10;
        b.TopGoal = 4;
        Note("gone", 2, 2, 0);
        b.Draw();
        b.Text.Clear();
        b.Top = 10;
        b.TopGoal = 4;
        Note("stay", 4, 1, 0);
        b.Draw();
        return string.Join("\n", b.Log) + "\n";
    }

    const string AddGolden =
        """
        scale 0
        scale 4
        shift 2 n=2
        shift -1 n=2
        shift 0 n=0
        clear flush=1 n=0 ident=0
        clear flush=0 n=0 ident=0
        skip
        skip
        skip
        skip
        skip
        stack n=1 pop=0 last=hi
        add lvl=1 font=2 w=33 time=1.5 lines=4 text=hi
        stack n=1 pop=0 last=lo
        add lvl=200 font=2 w=33 time=1.5 lines=4 text=lo
        stack n=2 pop=1 last=hi
        add lvl=1 font=2 w=33 time=1.5 lines=4 text=hi
        stack n=1 pop=0 last=hi
        add lvl=1 font=2 w=33 time=1.5 lines=4 text=hi
        stack n=1 pop=0 last=ab
        add lvl=1 font=2 w=33 time=1.5 lines=4 text=ab
        stack n=1 pop=0 last=ab
        add lvl=1 font=2 w=33 time=1.5 lines=4 text=ab
        stack n=2 pop=1 last=same
        swallow
        stack n=1 pop=0 last=same
        add lvl=4 font=2 w=33 time=1.5 lines=4 text=same

        """;

    const string DrawGolden =
        """
        ---- draw
        idle 0
        D 11 0 13 106 66 0.8333333333 fade
        D 99 0 21 106 66 1.0000000000 full
        expire 3
        D 11 0 29 106 66 1.0000000000 high
        keep 10
        D 5 0 13 106 66 0.3500000000 p0
        keep 10
        D 5 0 13 106 66 0.2683968334 p1
        keep 10
        D 2 47 1 100 50 1.0000000000 AB
        D 2 39 9 100 50 1.0000000000 CD
        S 2 45 9 100 50 1.0000000000  (x3)
        keep 1
        expire 0
        top 4
        D 11 36 10 100 50 0.5000000000 stay
        S 11 48 10 100 50 0.5000000000  (x3)
        keep 10

        """;
}
