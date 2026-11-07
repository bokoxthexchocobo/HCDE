using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmScMan1434SliceTests
{
    static VmScMan1434Slice.Tok Tok(int type, string text, int line = 1, long big = 0, int number = 0, double value = 0)
    {
        return new VmScMan1434Slice.Tok { Type = type, Text = text, Line = line, Big = big, Number = number, Float = value };
    }

    [Fact]
    public void ValuesSignsConstantsAndBools()
    {
        Assert.Equal(933, VmScMan1434Slice.BandStart);
        Assert.Equal(1118, VmScMan1434Slice.BandEnd);
        var s = new VmScMan1434Slice.State();
        s.Tokens.Add(Tok(VmScMan1434Slice.IntConst, "4", 2, 4, 4, 4));
        Assert.True(VmScMan1434Slice.ScanValue(s, false, false));
        Assert.Equal(4, s.Number);
        Assert.Equal(4L, s.Big);
        Assert.False(s.LastEvaluate);

        s = new VmScMan1434Slice.State { Big = 8, Number = 9, Float = 7 };
        s.Tokens.Add(Tok('-', "-"));
        s.Tokens.Add(Tok(VmScMan1434Slice.FloatConst, "1.5", value: 1.5));
        Assert.True(VmScMan1434Slice.ScanValue(s, true, true));
        Assert.Equal(-8L, s.Big);
        Assert.Equal(-9, s.Number);
        Assert.Equal(-1.5, s.Float);

        s = new VmScMan1434Slice.State { Number = 9, Float = 7 };
        s.Tokens.Add(Tok(VmScMan1434Slice.FloatConst, "1.5", value: 1.5));
        Assert.False(VmScMan1434Slice.CheckValue(s, false));
        Assert.Equal(0, s.Index);
        Assert.Equal(1.5, s.Float);
        Assert.Equal(9, s.Number);
        Assert.True(s.LastEvaluate);

        s = new VmScMan1434Slice.State();
        s.Constants["TWO"] = 2;
        s.Constants["HALF"] = 0.5;
        s.Tokens.Add(Tok('+', "+"));
        s.Tokens.Add(Tok(VmScMan1434Slice.Identifier, "two", 4));
        Assert.True(VmScMan1434Slice.CheckValue(s, false, false));
        Assert.Equal(2, s.Number);
        Assert.Equal(2d, s.Float);
        Assert.False(s.LastEvaluate);

        s.Tokens.Clear();
        s.Index = 0;
        s.Tokens.Add(Tok('-', "-"));
        s.Tokens.Add(Tok(VmScMan1434Slice.Identifier, "HALF"));
        Assert.True(VmScMan1434Slice.ScanValue(s, true, false));
        Assert.Equal(0, s.Number);
        Assert.Equal(-0.5, s.Float);

        s.Tokens.Clear();
        s.Index = 0;
        s.Number = 3;
        s.Tokens.Add(Tok(VmScMan1434Slice.Identifier, "HALF"));
        Assert.False(VmScMan1434Slice.CheckValue(s, false));
        Assert.Equal(0, s.Index);
        Assert.Equal(3, s.Number);

        s.Tokens.Clear();
        s.Index = 0;
        s.Tokens.Add(Tok(VmScMan1434Slice.UIntConst, "3u", number: 3, big: 3, value: 3));
        Assert.False(VmScMan1434Slice.ScanValue(s, true, false));
        Assert.Equal(1, s.Index);
        Assert.Equal(3, s.Number);

        s = new VmScMan1434Slice.State();
        s.Tokens.Add(Tok('-', "-"));
        s.Tokens.Add(Tok('-', "-"));
        Assert.False(VmScMan1434Slice.CheckValue(s, true));
        Assert.Equal(0, s.Index);

        s = new VmScMan1434Slice.State();
        Assert.False(VmScMan1434Slice.ScanValue(s, true, false));
        Assert.True(s.End);

        s = new VmScMan1434Slice.State { Name = "nums", Line = 6 };
        s.Tokens.Add(Tok(VmScMan1434Slice.Identifier, "nope", 6));
        var missing = Assert.Throws<VmScMan1434Slice.Fatal>(() => VmScMan1434Slice.MustGetValue(s, false));
        Assert.Equal("Script error, \"nums\" line 6:\nInteger constant expected\n", missing.Message);
        Assert.True(s.ParseError);
        s.NoFatal = true;
        s.ParseError = false;
        s.Tokens.Add(Tok(VmScMan1434Slice.FloatConst, "1.5", 6, value: 1.5));
        VmScMan1434Slice.MustGetValue(s, false);
        Assert.Contains("Integer constant expected", Assert.Single(s.Lines));
        s.Lines.Clear();
        s.Tokens.Add(Tok(VmScMan1434Slice.Identifier, "nope", 6));
        VmScMan1434Slice.MustGetValue(s, true);
        Assert.Contains("Numeric constant expected", Assert.Single(s.Lines));

        var flags = new VmScMan1434Slice.State();
        flags.Tokens.Add(Tok(VmScMan1434Slice.TrueToken, "true", 3));
        Assert.True(VmScMan1434Slice.CheckBool(flags));
        Assert.Equal(1, flags.Number);
        Assert.Equal(1d, flags.Float);
        Assert.False(flags.LastEvaluate);
        flags.Tokens.Add(Tok(VmScMan1434Slice.FalseToken, "false"));
        Assert.True(VmScMan1434Slice.CheckBool(flags));
        Assert.Equal(0, flags.Number);
        flags.Tokens.Add(Tok(VmScMan1434Slice.Identifier, "maybe", 9));
        Assert.False(VmScMan1434Slice.CheckBool(flags));
        Assert.True(flags.AlreadyGot);
        Assert.Equal(9, VmScMan1434Slice.MessageLine(flags));
        var badBool = Assert.Throws<VmScMan1434Slice.Fatal>(() => VmScMan1434Slice.MustGetBool(flags));
        Assert.Contains("line 9", badBool.Message);
        Assert.Contains("Expected true or false", badBool.Message);

        var closed = new VmScMan1434Slice.State { Open = false };
        Assert.Throws<VmScMan1434Slice.Fatal>(() => VmScMan1434Slice.CheckValue(closed, true));
        closed.Tokens.Add(Tok(VmScMan1434Slice.Identifier, "x"));
        Assert.Throws<NullReferenceException>(() => VmScMan1434Slice.ScanValue(new VmScMan1434Slice.State
        {
            Tokens = { Tok(VmScMan1434Slice.Identifier, null!) },
        }, true, false));
    }

    [Fact]
    public void TokenNamesAndScriptErrors()
    {
        Assert.Equal("'+'", VmScMan1434Slice.TokenName('+'));
        Assert.Equal("Unknown(32)", VmScMan1434Slice.TokenName(32));
        Assert.Equal("identifier 'Foo'", VmScMan1434Slice.TokenName(VmScMan1434Slice.Identifier, "Foo"));
        Assert.Equal("identifier", VmScMan1434Slice.TokenName(VmScMan1434Slice.Identifier, null));
        Assert.Equal("string constant \"hi\"", VmScMan1434Slice.TokenName(VmScMan1434Slice.StringConst, "hi"));
        Assert.Equal("float constant '1.5'", VmScMan1434Slice.TokenName(VmScMan1434Slice.FloatConst, "1.5"));
        Assert.Equal("'true'", VmScMan1434Slice.TokenName(VmScMan1434Slice.TrueToken, "x"));
        Assert.Equal("'static const'", VmScMan1434Slice.TokenName(412));
        Assert.Equal("Unknown(413)", VmScMan1434Slice.TokenName(VmScMan1434Slice.LastToken));

        var s = new VmScMan1434Slice.State { Name = "map.zs", Prepend = "pre ", Line = 3, NoFatal = true };
        VmScMan1434Slice.ScriptError(s, "Unknown keyword '%s'", "door");
        Assert.Equal("\u001cGpre Script error, \"map.zs\"\u001cG line 3:\n\u001cGUnknown keyword 'door'\n", Assert.Single(s.Lines));
        Assert.True(s.ParseError);

        s.Lines.Clear();
        s.AlreadyGot = true;
        s.AlreadyGotLine = 11;
        VmScMan1434Slice.ScriptMessage(s, null);
        Assert.Contains("line 11", Assert.Single(s.Lines));
        Assert.Contains("Bad syntax.", s.Lines[0]);
        Assert.Contains("\u001cG", s.Lines[0]);

        s.NoFatal = false;
        s.AlreadyGot = false;
        s.Line = 4;
        var fatal = Assert.Throws<VmScMan1434Slice.Fatal>(() => VmScMan1434Slice.ScriptError(s, "Bad token %d", 7));
        Assert.Equal("pre Script error, \"map.zs\" line 4:\nBad token 7\n", fatal.Message);
        Assert.True(s.ParseError);
    }
}
