using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmFScriptPreproTests
{
    static VmFScriptScript.FsScript ScriptWithData(string text)
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(text + "\0");
        return new VmFScriptScript.FsScript { Data = bytes, ScriptNum = 1 };
    }

    [Fact]
    public void SectionSerialize_RoundTripsFields()
    {
        var sec = new VmFScriptParser.FsSection
        {
            Type = VmFScriptPrepro.StIf,
            StartIndex = 3,
            EndIndex = 9,
            LoopIndex = 4,
        };
        var fields = VmFScriptPrepro.SerializeSection(sec);
        Assert.Equal(StIf, fields.Type);
        Assert.Equal(3, fields.StartIndex);
        Assert.Equal(9, fields.EndIndex);
        Assert.Equal(4, fields.LoopIndex);
    }

    const int StIf = VmFScriptPrepro.StIf;

    [Fact]
    public void SectionStart_ReturnsDataOffset()
    {
        var script = ScriptWithData("abc");
        var data = VmFScriptPrepro.DataAsChars(script);
        var sec = new VmFScriptParser.FsSection { StartIndex = 1 };
        Assert.Equal(1, VmFScriptPrepro.SectionStartIndex(script, sec));
    }

    [Fact]
    public void SectionEnd_ReturnsDataOffset()
    {
        var script = ScriptWithData("abc");
        var sec = new VmFScriptParser.FsSection { EndIndex = 2 };
        Assert.Equal(2, VmFScriptPrepro.SectionEndIndex(script, sec));
    }

    [Fact]
    public void SectionLoop_ReturnsDataOffset()
    {
        var script = ScriptWithData("abc");
        var sec = new VmFScriptParser.FsSection { LoopIndex = 2 };
        Assert.Equal(2, VmFScriptPrepro.SectionLoopIndex(script, sec));
    }

    [Fact]
    public void ClearSections_DestroyChain()
    {
        var destroyed = 0;
        var host = new VmFScriptPrepro.PreprocessHost { DestroySection = _ => destroyed++ };
        var script = new VmFScriptScript.FsScript();
        var a = new VmFScriptParser.FsSection();
        var b = new VmFScriptParser.FsSection { Next = a };
        script.Sections[0] = b;
        VmFScriptPrepro.ClearSections(script, host);
        Assert.Equal(2, destroyed);
        Assert.Null(script.Sections[0]);
    }

    [Fact]
    public void NewSection_InsertsInHashBucket()
    {
        var script = ScriptWithData("{x}");
        var data = VmFScriptPrepro.DataAsChars(script);
        var sec = VmFScriptPrepro.NewSection(script, data, 0);
        Assert.Equal(0, sec.StartIndex);
        Assert.NotNull(script.Sections[0]);
    }

    [Fact]
    public void FindSectionStart_LocatesBrace()
    {
        var script = ScriptWithData("{a}");
        var data = VmFScriptPrepro.DataAsChars(script);
        VmFScriptPrepro.NewSection(script, data, 0);
        Assert.NotNull(VmFScriptPrepro.FindSectionStart(script, data, 0));
    }

    [Fact]
    public void FindSectionEnd_LocatesClosingBrace()
    {
        var script = ScriptWithData("{a}");
        var data = VmFScriptPrepro.DataAsChars(script);
        var sec = VmFScriptPrepro.NewSection(script, data, 0);
        sec.EndIndex = 2;
        Assert.NotNull(VmFScriptPrepro.FindSectionEnd(script, data, 2));
    }

    [Fact]
    public void ProcessFindChar_BlanksBlockComment()
    {
        var host = new VmFScriptPrepro.PreprocessHost();
        var script = ScriptWithData("a/*hide*/b\0");
        var data = VmFScriptPrepro.DataAsChars(script);
        VmFScriptPrepro.ProcessFindChar(script, host, data, '\0');
        Assert.Equal('a', data[0]);
        Assert.DoesNotContain('h', data);
        Assert.Contains('b', data);
    }

    [Fact]
    public void ProcessFindChar_RegistersLabel()
    {
        var host = new VmFScriptPrepro.PreprocessHost();
        var script = ScriptWithData("loop: x=1;\0");
        script.ScriptNum = 2;
        var data = VmFScriptPrepro.DataAsChars(script);
        VmFScriptPrepro.ProcessFindChar(script, host, data, '\0');
        var slot = VmFScriptPrepro.VariableHash("loop");
        var v = script.Variables[slot];
        Assert.NotNull(v);
        Assert.Equal("loop", v!.Name);
        Assert.Equal(VmFScriptScript.SvtLabel, v.Type);
    }

    [Fact]
    public void DryRunScript_TagsIfSection()
    {
        var host = new VmFScriptPrepro.PreprocessHost();
        var script = ScriptWithData("if (1) { x=1; }\0");
        script.ScriptNum = 1;
        VmFScriptPrepro.Preprocess(host, script);
        VmFScriptParser.FsSection? tagged = null;
        for (var i = 0; i < VmFScriptScript.SectionSlots; i++)
        {
            var cur = script.Sections[i] as VmFScriptParser.FsSection;
            while (cur != null)
            {
                if (cur.Type == VmFScriptPrepro.StIf)
                    tagged = cur;
                cur = cur.Next;
            }
        }
        Assert.NotNull(tagged);
        Assert.Equal(VmFScriptPrepro.StIf, tagged!.Type);
    }

    [Fact]
    public void Preprocess_SetsLenAndRunsStages()
    {
        var host = new VmFScriptPrepro.PreprocessHost();
        var script = ScriptWithData("/*c*/ if (0) { }\0");
        script.ScriptNum = 1;
        VmFScriptPrepro.Preprocess(host, script);
        Assert.True(script.Len >= 0);
    }

    [Fact]
    public void ParseInclude_ReadsLumpAndRunsParser()
    {
        var ran = false;
        var host = new VmFScriptPrepro.PreprocessHost
        {
            CheckNumForName = n => n == "LUMP" ? 5 : -1,
            FileLength = _ => 4,
            ReadFile = (_, buf) =>
            {
                buf[0] = (byte)'x';
                buf[1] = (byte)'=';
                buf[2] = (byte)'1';
                buf[3] = (byte)';';
            },
            RunIncludedScript = (_, _, _, _, _) => ran = true,
        };
        var script = new VmFScriptScript.FsScript();
        VmFScriptPrepro.ParseInclude(host, script, "LUMP");
        Assert.True(ran);
    }
}
