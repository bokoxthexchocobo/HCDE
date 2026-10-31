namespace HCDE.Playsim.Tests;

public class VmTPreproScan1184SliceTests
{
    [Fact]
    public void Scan_BlanksCommentsHidesQuotedBracesAndStoresLabels()
    {
        var block = ScriptOf(3, "a/*xy*/b");
        VmTPreproScan1184Slice.ProcessFindChar(block, 0, '\0');
        Assert.Equal("a      b", new string(block.Data));

        var line = ScriptOf(3, "a//z\nb");
        VmTPreproScan1184Slice.ProcessFindChar(line, 0, '\0');
        Assert.Equal("a   \nb", new string(line.Data));

        var quoted = ScriptOf(3, "{ \"a}b\" }");
        var end = VmTPreproScan1184Slice.ProcessFindChar(quoted, 0, '\0');
        Assert.Null(end);
        var section = VmTPreproSections1183Slice.FindSectionStart(quoted.Sections, 0);
        Assert.NotNull(section);
        Assert.Equal(0, section!.Type);
        Assert.Equal(0, section.StartIndex);
        Assert.Equal(8, section.EndIndex);
        Assert.Equal('}', quoted.Data[4]);

        var label = ScriptOf(3, " label:");
        VmTPreproScan1184Slice.ProcessFindChar(label, 0, '\0');
        Assert.Equal("label", label.Labels[0].Name);
        Assert.Equal(VmTPreproScan1184Slice.SvtLabel, label.Labels[0].Type);
        Assert.Equal(0, label.Labels[0].IntValue);
        Assert.Empty(label.Bold);

        var empty = ScriptOf(3, " :");
        VmTPreproScan1184Slice.ProcessFindChar(empty, 0, '\0');
        Assert.Equal("", empty.Labels[0].Name);
        Assert.Equal(0, empty.Labels[0].IntValue);
        Assert.Equal("Script 3: ':' encountrered in incorrect position!\n", empty.Bold[0]);

        var global = ScriptOf(-1, " label:");
        VmTPreproScan1184Slice.ProcessFindChar(global, 0, '\0');
        Assert.Empty(global.Labels);
    }

    [Fact]
    public void Braces_NestAndFatalErrorsStopTheScan()
    {
        var nested = ScriptOf(4, "{ { } }");
        VmTPreproScan1184Slice.ProcessFindChar(nested, 0, '\0');
        var outer = VmTPreproSections1183Slice.FindSectionStart(nested.Sections, 0);
        var inner = VmTPreproSections1183Slice.FindSectionStart(nested.Sections, 2);
        Assert.NotNull(outer);
        Assert.NotNull(inner);
        Assert.Equal(6, outer!.EndIndex);
        Assert.Equal(4, inner!.EndIndex);
        Assert.Equal(0, outer.Type);
        Assert.Equal(2, nested.Sections.WriteBarrierCount);

        var open = ScriptOf(8, "{");
        var missing = Assert.Throws<VmTPreproScan1184Slice.ScriptFault>(() =>
            VmTPreproScan1184Slice.ProcessFindChar(open, 0, '\0'));
        Assert.Equal("Script 8: section error: no ending brace\n", missing.Detail);

        var comment = ScriptOf(1, "/*");
        var terminated = Assert.Throws<VmTPreproScan1184Slice.ScriptFault>(() =>
            VmTPreproScan1184Slice.ProcessFindChar(comment, 0, '\0'));
        Assert.Equal("script terminated inside comment\n", terminated.Detail);
    }

    [Fact]
    public void DryRun_TypesSectionsAndIncludeBlanksTheLump()
    {
        var section = new VmTPreproSections1183Slice.Section();
        var script = ScriptOf(9, "aaaa\naaa");
        script.Len = script.Data.Length;
        var step = 0;
        VmTPreproScan1184Slice.DryRun(script, (_, rover, frame) =>
        {
            step++;
            frame.Section = section;
            frame.NumTokens = 1;
            frame.TokenType0 = VmTPreproScan1184Slice.TokenFunction;
            if (step == 1)
            {
                frame.NumTokens = 0;
                return rover + 1;
            }

            if (step == 2)
            {
                frame.Token0 = "IF";
                return rover + 1;
            }

            if (step == 3)
            {
                frame.TokenType0 = 0;
                frame.Token0 = "if";
                return rover + 1;
            }

            if (step == 4)
            {
                frame.Token0 = "elseif";
                return rover + 1;
            }

            if (step == 5)
            {
                frame.Token0 = "else";
                return rover + 1;
            }

            if (step == 6)
            {
                frame.Token0 = "while";
                frame.LineStart = 11;
                return rover + 1;
            }

            if (step == 7)
            {
                frame.Token0 = "for";
                frame.LineStart = 4;
                return rover + 1;
            }

            frame.LineStart = 5;
            throw new VmTPreproScan1184Slice.ScriptFault("boom");
        });
        Assert.Equal(VmTPreproScan1184Slice.StLoop, section.Type);
        Assert.Equal(4, section.LoopIndex);
        Assert.Equal("Script 9, line 1: boom", script.Errors[0]);

        var pre = ScriptOf(2, "//z\n{ }\0");
        VmTPreproSections1183Slice.Section? seen = null;
        VmTPreproScan1184Slice.Preprocess(pre, (_, rover, frame) =>
        {
            seen = VmTPreproSections1183Slice.FindSectionStart(pre.Sections, 4);
            frame.NumTokens = 1;
            frame.TokenType0 = VmTPreproScan1184Slice.TokenFunction;
            frame.Token0 = "if";
            frame.Section = seen;
            return pre.Len;
        });
        Assert.Equal(pre.Data.Length - 1, pre.Len);
        Assert.Equal(' ', pre.Data[0]);
        Assert.NotNull(seen);
        Assert.Equal(VmTPreproScan1184Slice.StIf, seen!.Type);
        Assert.Equal(6, seen.EndIndex);

        var files = new VmTPreproScan1184Slice.Files();
        var read = false;
        VmTPreproScan1184Slice.ParseInclude(pre, files, "MISSING");
        Assert.Equal("include lump 'MISSING' not found!\n", pre.Fatals[0]);
        Assert.Empty(files.Runs);

        files.CheckNumForName = name => name == "INC" ? 4 : -1;
        files.FileLength = _ => 5;
        files.ReadFile = (_, buffer) =>
        {
            read = true;
            buffer[0] = (byte)'a';
            buffer[1] = (byte)'/';
            buffer[2] = (byte)'/';
            buffer[3] = (byte)'b';
            buffer[4] = (byte)'\n';
        };
        VmTPreproScan1184Slice.ParseInclude(pre, files, "INC");
        Assert.True(read);
        Assert.Equal("a   \n", new string(files.Runs[0].Data, 0, 5));
        Assert.Equal(0, files.Runs[0].Start);
        Assert.Equal(5, files.Runs[0].End);
        Assert.Equal(' ', pre.Data[0]);
        Assert.Equal('{', pre.Data[4]);
    }

    static VmTPreproScan1184Slice.Script ScriptOf(int scriptNum, string text)
    {
        return new VmTPreproScan1184Slice.Script
        {
            ScriptNum = scriptNum,
            Data = text.ToCharArray(),
        };
    }
}
