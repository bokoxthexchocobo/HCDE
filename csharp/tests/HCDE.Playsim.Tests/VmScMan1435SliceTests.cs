using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmScMan1435SliceTests
{
    static void Reset()
    {
        VmScMan1435Slice.ErrorCounter = 0;
        VmScMan1435Slice.WarnCounter = 0;
        VmScMan1435Slice.Developer = 0;
        VmScMan1435Slice.StrictErrors = false;
        VmScMan1435Slice.ErrorsOut = false;
        VmScMan1435Slice.Notes.Clear();
    }

    static VmScMan1435Slice.State Words(params string[] text)
    {
        var s = new VmScMan1435Slice.State { Name = "blk", Line = 1 };
        foreach (string word in text)
            s.Words.Add(new VmScMan1435Slice.Word { Text = word, Line = s.Words.Count + 2 });
        return s;
    }

    [Fact]
    public void SymbolsBracesAndOpen()
    {
        Assert.Equal(1126, VmScMan1435Slice.BandStart);
        Assert.Equal(1342, VmScMan1435Slice.BandEnd);
        var s = new VmScMan1435Slice.State { Open = false };
        VmScMan1435Slice.AddInt(s, "Door", -4);
        Assert.Equal(-4L, s.Symbols["door"].Number);
        Assert.Equal(VmScMan1435Slice.IntConst, s.Symbols["Door"].Type);
        VmScMan1435Slice.AddUint(s, "door", 3000000000UL);
        var sym = s.Symbols["DOOR"];
        Assert.Equal(VmScMan1435Slice.UIntConst, sym.Type);
        Assert.Equal(3000000000L, sym.Number);
        Assert.Equal(3000000000d, sym.Float);
        Assert.Single(s.Symbols);
        VmScMan1435Slice.AddFloat(s, "Pi", 1.25);
        sym = s.Symbols["pi"];
        Assert.Equal(VmScMan1435Slice.FloatConst, sym.Type);
        Assert.Equal(1L, sym.Number);
        Assert.Equal(1.25, sym.Float);
        Assert.Equal(2, s.Symbols.Count);
        Assert.Throws<NullReferenceException>(() => VmScMan1435Slice.AddInt(s, null, 1));
        Assert.Throws<VmScMan1435Slice.Fatal>(() => VmScMan1435Slice.CheckOpen(s));

        var block = Words("{", "A", "{", "B", "}", "C", "}");
        int code = VmScMan1435Slice.StartBraces(block, out int end);
        Assert.Equal(0, code);
        Assert.Equal(1, block.Index);
        Assert.Equal(7, end);
        Assert.False(VmScMan1435Slice.FoundEnd(block, end));
        Assert.True(VmScMan1435Slice.GetString(block));
        Assert.Equal("A", block.Text);
        block.Index = end;
        Assert.True(VmScMan1435Slice.FoundEnd(block, end));

        var bare = Words("{", "}");
        Assert.Equal(0, VmScMan1435Slice.StartBraces(bare, out end));
        Assert.Equal(1, bare.Index);
        Assert.Equal(2, end);
        Assert.False(VmScMan1435Slice.FoundEnd(bare, end));

        var open = Words("{", "tail");
        var missing = Assert.Throws<VmScMan1435Slice.Fatal>(() => VmScMan1435Slice.StartBraces(open, out _));
        Assert.Contains("Missing string (unexpected end of file).", missing.Message);
        Assert.Contains("line 3", missing.Message);

        var nope = Words("nope");
        nope.NoFatal = true;
        Assert.Equal(-1, VmScMan1435Slice.StartBraces(nope, out end));
        Assert.Equal(-1, end);
        Assert.Contains("'{' expected", Assert.Single(nope.Lines));
        Assert.True(VmScMan1435Slice.GetString(nope));
        Assert.Equal("nope", nope.Text);

        var shut = Words("{");
        shut.Open = false;
        Assert.Throws<VmScMan1435Slice.Fatal>(() => VmScMan1435Slice.StartBraces(shut, out _));
    }

    [Fact]
    public void ScriptPositionMessages()
    {
        Reset();
        var made = VmScMan1435Slice.At("a.zs", 4);
        Assert.Equal("a.zs", made.File);
        Assert.Equal(4, made.Line);
        var scanner = new VmScMan1435Slice.State { Name = "c.zs", Line = 2, AlreadyGot = true, AlreadyGotLine = 5 };
        var fromScan = VmScMan1435Slice.AtScanner(scanner);
        Assert.Equal("c.zs", fromScan.File);
        Assert.Equal(5, fromScan.Line);
        var copied = VmScMan1435Slice.CopyFrom(made, scanner);
        Assert.Same(made, copied);
        Assert.Equal(5, made.Line);

        VmScMan1435Slice.Report(made, VmScMan1435Slice.Warning, "hi");
        var warn = Assert.Single(VmScMan1435Slice.Notes);
        Assert.Equal(VmScMan1435Slice.PrintHigh, warn.Level);
        Assert.Equal("\u001cIScript warning, \"c.zs\" line 5:\n\u001cIhi\n", warn.Text);
        Assert.Equal(1, VmScMan1435Slice.WarnCounter);

        VmScMan1435Slice.Report(made, VmScMan1435Slice.Error, null);
        Assert.Equal(1, VmScMan1435Slice.ErrorCounter);
        Assert.Contains("Bad syntax.", VmScMan1435Slice.Notes[1].Text);
        Assert.Contains("\u001cGScript error", VmScMan1435Slice.Notes[1].Text);

        VmScMan1435Slice.Report(made, VmScMan1435Slice.OptError, "soft");
        Assert.Equal(2, VmScMan1435Slice.WarnCounter);
        VmScMan1435Slice.StrictErrors = true;
        VmScMan1435Slice.Report(made, VmScMan1435Slice.OptError, "hard");
        Assert.Equal(2, VmScMan1435Slice.ErrorCounter);

        int notes = VmScMan1435Slice.Notes.Count;
        VmScMan1435Slice.Developer = 0;
        VmScMan1435Slice.Report(made, VmScMan1435Slice.DebugError, "no");
        VmScMan1435Slice.Report(made, VmScMan1435Slice.DebugWarn, "no");
        VmScMan1435Slice.Report(made, VmScMan1435Slice.DebugMsg, "no");
        VmScMan1435Slice.Report(made, VmScMan1435Slice.DebugLog, "no");
        Assert.Equal(notes, VmScMan1435Slice.Notes.Count);

        VmScMan1435Slice.Developer = 1;
        VmScMan1435Slice.Report(made, VmScMan1435Slice.DebugError, "yes");
        Assert.Equal(3, VmScMan1435Slice.WarnCounter);
        VmScMan1435Slice.Report(made, VmScMan1435Slice.DebugWarn, "still");
        Assert.Equal(3, VmScMan1435Slice.WarnCounter);

        VmScMan1435Slice.Developer = 3;
        VmScMan1435Slice.Report(made, VmScMan1435Slice.DebugMsg, "dbg %s", "x");
        Assert.Equal(VmScMan1435Slice.PrintHigh, VmScMan1435Slice.Notes[^1].Level);
        Assert.Contains("\u001cDScript message", VmScMan1435Slice.Notes[^1].Text);
        Assert.Contains("dbg x", VmScMan1435Slice.Notes[^1].Text);
        VmScMan1435Slice.Report(made, VmScMan1435Slice.DebugLog, "log");
        Assert.Equal(VmScMan1435Slice.PrintLog, VmScMan1435Slice.Notes[^1].Level);
        Assert.StartsWith("Script message,", VmScMan1435Slice.Notes[^1].Text);
        VmScMan1435Slice.Report(made, VmScMan1435Slice.Log, "plain");
        Assert.Contains("plain", VmScMan1435Slice.Notes[^1].Text);
        VmScMan1435Slice.Report(made, VmScMan1435Slice.MessageLevel, "ok");
        Assert.Contains("\u001cDok\n", VmScMan1435Slice.Notes[^1].Text);
        int before = VmScMan1435Slice.Notes.Count;
        VmScMan1435Slice.Report(made, 99, "nope");
        Assert.Equal(before, VmScMan1435Slice.Notes.Count);

        int errors = VmScMan1435Slice.ErrorCounter;
        VmScMan1435Slice.ErrorsOut = true;
        var fatal = Assert.Throws<VmScMan1435Slice.Fatal>(() => VmScMan1435Slice.Report(made, VmScMan1435Slice.Error, "die"));
        Assert.Equal(errors, VmScMan1435Slice.ErrorCounter);
        Assert.Equal("Script error, \"c.zs\" line 5:\ndie\n", fatal.Message);
        Assert.Throws<NullReferenceException>(() => VmScMan1435Slice.At(null, 1));
    }
}
