using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmTab1230SliceTests
{
    [Fact]
    public void Add_SortsNamesAndCyclesForward()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("add [gi:1,gif:1,give:2,god:1,Map:1]\n", head, StringComparison.Ordinal);
        Assert.Contains("remove drop gi [", head, StringComparison.Ordinal);
        Assert.Contains("remove none [", head, StringComparison.Ordinal);
        Assert.Contains("A\\xc0:2", head, StringComparison.Ordinal);
        Assert.Contains("\\xff:1", head, StringComparison.Ordinal);
        Assert.Contains("Zzz:1", head, StringComparison.Ordinal);
        Assert.Contains("out [\\x1cDgif    ]\n", head, StringComparison.Ordinal);
        Assert.Contains("out [\\x1cNgive   ]\n", head, StringComparison.Ordinal);
        Assert.Contains("out [\\x1cGgod    ]\n", head, StringComparison.Ordinal);
        Assert.Contains("set [gif ]\n", head, StringComparison.Ordinal);
        Assert.Contains("set [g]\n", head, StringComparison.Ordinal);
        Assert.Contains("nomatch\n", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Complete_FillsThePrefixAndSkipsASecondList()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("set [  gimm]\n", tail, StringComparison.Ordinal);
        Assert.Contains("set [  gimme ]\n", tail, StringComparison.Ordinal);
        Assert.Contains("blank\n", tail, StringComparison.Ordinal);
        Assert.Contains("set [foo ]\n", tail, StringComparison.Ordinal);
        Assert.Contains("set [f]\n", tail, StringComparison.Ordinal);
        Assert.Contains("state last=1 listed=0 pos=0 start=0 size=1 line=[gif ]\n", tail, StringComparison.Ordinal);
        Assert.Contains("out [\\x1cDgif    ]\nout [\\x1cNgive   ]\nout [\\x1cGgod    ]\nout [\\x0a]\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("Completions for f:", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build()
    {
        var tab = new VmTab1230();
        tab.Remove("nope");
        tab.Define("Map");
        tab.Kind["gif"] = "cvar";
        tab.Kind["give"] = "command";
        tab.Kind["god"] = "alias";
        tab.Kind["foo"] = "command";
        tab.Kind["Map"] = "command";
        tab.ConCols = 20;
        tab.Scale = 1;
        tab.Add("give");
        tab.Add("GIVE");
        tab.Add("god");
        tab.Add("gif");
        tab.Add("gi");
        tab.Add("MAP");
        tab.Add("None");
        tab.Add("A\u00C0");
        tab.Add("a\u00C0");
        tab.Add("\u00FF");
        tab.Remove("gi");
        tab.Remove("GIVE");
        tab.Remove("NONE");
        tab.Remove("nope");
        tab.Line = "g";
        tab.Complete(true);
        tab.Complete(true);
        tab.Complete(true);
        tab.Complete(true);
        tab.Complete(true);
        tab.Line = "Zzz";
        tab.TabbedLast = false;
        tab.Complete(true);
        tab.Add("zzz");

        tab.Log.Add("---- edge");
        tab.Clear();
        tab.TabbedList = false;
        tab.TabbedLast = false;
        tab.Add("gimme");
        tab.Add("gimmick");
        tab.Line = "  gi";
        tab.Complete(true);
        tab.Complete(true);
        tab.Line = "   ";
        tab.TabbedLast = false;
        tab.Complete(true);
        tab.Line = "zzz";
        tab.TabbedLast = false;
        tab.Complete(true);
        tab.Clear();
        tab.TabbedList = false;
        tab.TabbedLast = false;
        tab.Add("foo");
        tab.Line = "f";
        tab.Complete(true);
        tab.Complete(true);
        tab.Complete(true);
        tab.Clear();
        tab.TabbedList = false;
        tab.TabbedLast = false;
        tab.NoTabList = false;
        tab.Add("gif");
        tab.Add("give");
        tab.Add("god");
        tab.Line = "g";
        tab.Complete(false);
        tab.Complete(false);
        tab.NoTabList = true;
        tab.TabbedLast = false;
        tab.TabbedList = false;
        tab.Line = "g";
        tab.Complete(true);
        tab.Clear();
        tab.NoTabList = false;
        tab.TabbedLast = false;
        tab.TabbedList = false;
        tab.ConCols = 5;
        tab.Scale = 1;
        tab.Add("gif");
        tab.Add("give");
        tab.Add("god");
        tab.Line = "g";
        tab.Complete(true);
        tab.ConCols = 21;
        tab.Scale = 2;
        tab.TabbedLast = false;
        tab.TabbedList = false;
        tab.Line = "g";
        tab.Complete(true);
        return string.Join("\n", tab.Log) + "\n";
    }

    const string HeadGolden = """
        remove empty
        add [give:1]
        add [give:2]
        add [give:2,god:1]
        add [gif:1,give:2,god:1]
        add [gi:1,gif:1,give:2,god:1]
        add [gi:1,gif:1,give:2,god:1,Map:1]
        add [gi:1,gif:1,give:2,god:1,Map:1,None:1]
        add [A\xc0:1,gi:1,gif:1,give:2,god:1,Map:1,None:1]
        add [A\xc0:2,gi:1,gif:1,give:2,god:1,Map:1,None:1]
        add [A\xc0:2,gi:1,gif:1,give:2,god:1,Map:1,None:1,\xff:1]
        remove drop gi [A\xc0:2,gif:1,give:2,god:1,Map:1,None:1,\xff:1]
        remove give count=1 [A\xc0:2,gif:1,give:1,god:1,Map:1,None:1,\xff:1]
        remove none [A\xc0:2,gif:1,give:1,god:1,Map:1,None:1,\xff:1]
        remove unknown [A\xc0:2,gif:1,give:1,god:1,Map:1,None:1,\xff:1]
        go forward
        out [\x1cHCompletions for g:\x0a]
        out [\x1cDgif    ]
        out [\x1cNgive   ]
        out [\x0a]
        out [\x1cGgod    ]
        out [\x0a]
        state last=1 listed=1 pos=0 start=0 size=1 line=[g]
        go forward
        set [gif ]
        caret
        state last=1 listed=1 pos=1 start=0 size=1 line=[gif ]
        go forward
        set [give ]
        caret
        state last=1 listed=1 pos=2 start=0 size=1 line=[give ]
        go forward
        set [god ]
        caret
        state last=1 listed=1 pos=3 start=0 size=1 line=[god ]
        go forward
        set [g]
        caret
        state last=0 listed=1 pos=4 start=0 size=1 line=[g]
        go forward
        nomatch
        add [A\xc0:2,gif:1,give:1,god:1,Map:1,None:1,Zzz:1,\xff:1]

        """;

    const string TailGolden = """
        ---- edge
        clear last=0 listed=1
        add [gimme:1]
        add [gimme:1,gimmick:1]
        go forward
        out [\x1cHCompletions for   gi:\x0a]
        out [gimme     ]
        out [gimmick   ]
        out [\x0a]
        set [  gimm]
        state last=1 listed=1 pos=-1 start=2 size=4 line=[  gimm]
        go forward
        set [  gimme ]
        caret
        state last=1 listed=1 pos=0 start=2 size=4 line=[  gimme ]
        go forward
        blank
        go forward
        nomatch
        clear last=0 listed=1
        add [foo:1]
        go forward
        set [foo ]
        caret
        state last=1 listed=1 pos=0 start=0 size=1 line=[foo ]
        go forward
        set [f]
        caret
        state last=0 listed=1 pos=1 start=0 size=1 line=[f]
        go forward
        set [foo ]
        caret
        state last=1 listed=1 pos=0 start=0 size=1 line=[foo ]
        clear last=1 listed=1
        add [gif:1]
        add [gif:1,give:1]
        add [gif:1,give:1,god:1]
        go back
        out [\x1cHCompletions for g:\x0a]
        out [\x1cDgif    ]
        out [\x1cNgive   ]
        out [\x0a]
        out [\x1cGgod    ]
        out [\x0a]
        state last=1 listed=1 pos=3 start=0 size=1 line=[g]
        go back
        set [god ]
        caret
        state last=1 listed=1 pos=2 start=0 size=1 line=[god ]
        go forward
        set [gif ]
        caret
        state last=1 listed=0 pos=0 start=0 size=1 line=[gif ]
        clear last=1 listed=0
        add [gif:1]
        add [gif:1,give:1]
        add [gif:1,give:1,god:1]
        go forward
        out [\x1cHCompletions for g:\x0a]
        out [\x1cDgif    ]
        out [\x1cNgive   ]
        out [\x1cGgod    ]
        out [\x0a]
        state last=1 listed=1 pos=-1 start=0 size=1 line=[g]
        go forward
        out [\x1cHCompletions for g:\x0a]
        out [\x1cDgif    ]
        out [\x0a]
        out [\x1cNgive   ]
        out [\x0a]
        out [\x1cGgod    ]
        out [\x0a]
        state last=1 listed=1 pos=-1 start=0 size=1 line=[g]

        """;
}
