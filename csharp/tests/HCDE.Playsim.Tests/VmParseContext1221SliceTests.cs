using System.Globalization;
using System.Text;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmParseContext1221SliceTests
{
    [Fact]
    public void GetToken_LexesNumbersCommentsAndTheShiftBugs()
    {
        string dump = Build();
        string tokens = dump[..dump.IndexOf("CASE lump-root\n", StringComparison.Ordinal)];
        Assert.Contains("9 1 num -2147483648\n", tokens, StringComparison.Ordinal);
        Assert.Contains("9 1 num -1\n", tokens, StringComparison.Ordinal);
        Assert.Contains("flt 3f8eb851eb851eb8\n", tokens, StringComparison.Ordinal);
        Assert.Contains("dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd_\"\n0 0 zero -\n13 0 sym \"Z\"\n", tokens, StringComparison.Ordinal);
        Assert.Contains("0 5 zero -\n13 5 sym \"x\"\n23 5 op -\n0 5 zero -\n0 5 zero -\n13 5 sym \"y\"\n", tokens, StringComparison.Ordinal);
        Assert.Contains("9 0 num 34\n", tokens, StringComparison.Ordinal);
        Assert.Equal(TokenGolden, tokens);
    }

    [Fact]
    public void ParseLump_ResolvesIncludesAndKeepsTheCallerLine()
    {
        string dump = Build();
        string lumps = dump[dump.IndexOf("CASE lump-root\n", StringComparison.Ordinal)..];
        Assert.Contains("AT foo\n", lumps, StringComparison.Ordinal);
        Assert.Contains("AT \u0005\n", lumps, StringComparison.Ordinal);
        Assert.Contains("FILE glue\nLINE 0\n", lumps, StringComparison.Ordinal);
        Assert.Contains("LINE 4\n", lumps, StringComparison.Ordinal);
        Assert.Contains("(null), line 0: Lump 'badparent' not found\\n", lumps, StringComparison.Ordinal);
        Assert.Contains("Include: String parameter expected\\n", lumps, StringComparison.Ordinal);
        Assert.Equal(LumpGolden, lumps);
    }

    static string Build()
    {
        var ctx = new VmParseContext1221();
        var sb = new StringBuilder();
        var lumpList = new List<KeyValuePair<string, string>>
        {
            new("root", "1\n#include \"child\"\n+\n"),
            new("child", "2 3\n"),
            new("bad", "#include 5"),
            new("glue", "#includeFOO"),
            new("hash", "1 #no 2"),
            new("def", "#define Foo\n"),
            new("empty", ""),
            new("cased", "#InClude \"child\""),
            new("wrap", "#include \"bad\"\nfoo"),
            new("wrapnum", "#include \"bad\"\n5"),
        };

        void ResetLex()
        {
            ctx.Symbols.Clear();
            ctx.SourceLine = 0;
            ctx.Prints.Clear();
            ctx.SetSourceFile(null);
            for (int i = 0; i < ctx.TokenTrans.Length; i++)
                ctx.TokenTrans[i] = i;
            ctx.FindToken = (string tok, out int type) =>
            {
                if (tok == "tag")
                {
                    type = 21;
                    return true;
                }
                type = 0;
                return false;
            };
            ctx.FindLineSpecial = tok =>
            {
                if (tok == "Door_Open")
                    return 12;
                if (tok == "NegSpecial")
                    return -5;
                return 0;
            };
        }

        string Esc(string s)
        {
            var o = new StringBuilder();
            o.Append('"');
            foreach (char ch in s)
            {
                if (ch == '\\' || ch == '"')
                {
                    o.Append('\\');
                    o.Append(ch);
                }
                else if (ch == '\n') o.Append("\\n");
                else if (ch == '\r') o.Append("\\r");
                else if (ch == '\t') o.Append("\\t");
                else if (ch == '\v') o.Append("\\v");
                else if (ch == '\f') o.Append("\\f");
                else o.Append(ch);
            }
            o.Append('"');
            return o.ToString();
        }

        void DumpOne(int ret, VmParseToken1221 tok)
        {
            sb.Append(ret.ToString(CultureInfo.InvariantCulture));
            sb.Append(' ');
            sb.Append(ctx.SourceLine.ToString(CultureInfo.InvariantCulture));
            sb.Append(' ');
            if (ret == 0)
                sb.Append("zero -\n");
            else if (ret == ctx.TokenTrans[VmParseContext1221.Num])
            {
                sb.Append("num ");
                sb.Append(tok.Val.ToString(CultureInfo.InvariantCulture));
                sb.Append('\n');
            }
            else if (ret == ctx.TokenTrans[VmParseContext1221.FloatVal])
            {
                ulong bits = BitConverter.DoubleToUInt64Bits(tok.FVal);
                sb.Append("flt ");
                sb.Append(bits.ToString("x16", CultureInfo.InvariantCulture));
                sb.Append('\n');
            }
            else if (ret == ctx.TokenTrans[VmParseContext1221.Sym])
            {
                sb.Append("sym ");
                sb.Append(Esc(tok.Sym));
                sb.Append('\n');
            }
            else if (ret == ctx.TokenTrans[VmParseContext1221.StringToken])
            {
                sb.Append("str ");
                sb.Append(Esc(tok.String));
                sb.Append('\n');
            }
            else
                sb.Append("op -\n");
        }

        void RunTokens(string name, string src)
        {
            sb.Append("CASE ");
            sb.Append(name);
            sb.Append('\n');
            var cur = new VmParseCursor1221(src);
            for (int n = 0; n < 400; n++)
            {
                var tok = new VmParseToken1221();
                int before = cur.Pos;
                int ret = ctx.GetToken(cur, tok);
                DumpOne(ret, tok);
                if (ret == 0 && cur.Pos >= src.Length)
                    break;
                if (cur.Pos == before && ret == 0)
                    break;
            }
            sb.Append("END\n");
        }

        void FinishLump(string name)
        {
            sb.Append("CASE ");
            sb.Append(name);
            sb.Append('\n');
            sb.Append("FILE ");
            sb.Append(ctx.SourceFile ?? "null");
            sb.Append('\n');
            sb.Append("LINE ");
            sb.Append(ctx.SourceLine.ToString(CultureInfo.InvariantCulture));
            sb.Append('\n');
            sb.Append("PRINTS ");
            sb.Append(Esc(string.Concat(ctx.Prints)));
            sb.Append('\n');
            sb.Append("END\n");
        }

        ResetLex();
        ctx.AddSym("Max", 9);
        ctx.AddSym("Max", 4);
        ctx.AddSym("Other", 3);
        ctx.AddSym(new string('Q', 100), 33);
        RunTokens("mix",
            "\n\t\r\v\f 5 0x10 007 08 1.5 1. 1.5e2 1.5e 2147483648 4294967295\n" +
            "tag Max max Door_Open NegSpecial foo\n" +
            "/* c\n */ // line\n" +
            "\"ab\ncd\" \"\" |= <<x <<= <Z >>y >>= >Y\n" +
            "#includeFOO #Define #no +\n" +
            "^&-*%(),{}=;:[] / . _\n" +
            "/* unterminated");
        ResetLex();
        RunTokens("nums",
            "0x80000000 0x 0xG 0X10 0x 10 0x\n10 00x10 " +
            "9223372036854775807 999999999999999999999 " +
            "1.5e-2 1.5e+ 01.50 0.5 1.25e-1");
        ResetLex();
        RunTokens("idents", new string('a', 79) + " " + new string('b', 80) + " x" + new string('c', 78) + "_Zz");
        ResetLex();
        RunTokens("us79", new string('d', 78) + "_Z");
        ResetLex();
        RunTokens("unclosed", "\"abc");
        ResetLex();
        for (int i = 0; i < ctx.TokenTrans.Length; i++)
            ctx.TokenTrans[i] = 1000 + i;
        RunTokens("trans", "+ / 5 \"z\" #include");
        ResetLex();
        ctx.AddSym(new string('Q', 100), 34);
        RunTokens("longsym", new string('Q', 79));

        ctx.CheckNumForFullName = (name, full) =>
        {
            if (!full)
                return -1;
            for (int i = 0; i < lumpList.Count; i++)
            {
                if (lumpList[i].Key == name)
                    return i;
            }
            return -1;
        };
        ctx.GetStringFromLump = no => lumpList[no].Value;
        ctx.OnParse = (tok, token) =>
        {
            sb.Append("CB ");
            DumpOne(tok, token);
            sb.Append("AT ");
            sb.Append(ctx.SourceFile ?? "null");
            sb.Append('\n');
        };

        void RunLump(string caseName, string lump, string endName)
        {
            ResetLex();
            sb.Append("CASE ");
            sb.Append(caseName);
            sb.Append('\n');
            ctx.ParseLump(lump);
            FinishLump(endName);
        }

        RunLump("lump-root", "root", "lump-root-end");
        RunLump("lump-bad", "badparent", "lump-missing-badparent");
        ResetLex();
        sb.Append("CASE lump-miss\n");
        ctx.SourceLine = 4;
        ctx.SetSourceFile("here");
        ctx.ParseLump("nope");
        FinishLump("lump-miss-end");
        RunLump("lump-glue", "glue", "lump-glue-end");
        RunLump("lump-hash", "hash", "lump-hash-end");
        RunLump("lump-def", "def", "lump-def-end");
        RunLump("lump-empty", "empty", "lump-empty-end");
        RunLump("lump-cased", "cased", "lump-cased-end");
        RunLump("lump-wrap", "wrap", "lump-wrap-end");
        RunLump("lump-wrapnum", "wrapnum", "lump-wrapnum-end");

        ResetLex();
        ctx.SetSourceFile("f");
        ctx.SourceLine = 3;
        sb.Append("CASE err-file\n");
        sb.Append(ctx.PrintError("boom").ToString(CultureInfo.InvariantCulture));
        sb.Append('\n');
        sb.Append("PRINTS ");
        sb.Append(Esc(string.Concat(ctx.Prints)));
        sb.Append('\n');
        sb.Append("END\n");
        ResetLex();
        ctx.SourceLine = 9;
        sb.Append("CASE err-nofile\n");
        sb.Append(ctx.PrintError("only").ToString(CultureInfo.InvariantCulture));
        sb.Append('\n');
        sb.Append("PRINTS ");
        sb.Append(Esc(string.Concat(ctx.Prints)));
        sb.Append('\n');
        sb.Append("END\n");
        return sb.ToString();
    }

    private const string TokenGolden =
"""
CASE mix
9 1 num 5
9 1 num 16
9 1 num 7
9 1 num 8
10 1 flt 3ff8000000000000
10 1 flt 3ff0000000000000
10 1 flt 4062c00000000000
10 1 flt 3ff8000000000000
13 1 sym "e"
9 1 num -2147483648
9 1 num -1
21 2 op -
9 2 num 9
13 2 sym "max"
9 2 num 12
9 2 num -5
13 2 sym "foo"
25 5 str "ab\ncd"
25 5 str ""
20 5 op -
0 5 zero -
13 5 sym "x"
23 5 op -
0 5 zero -
0 5 zero -
13 5 sym "y"
24 5 op -
0 5 zero -
26 6 op -
13 6 sym "FOO"
27 6 op -
0 6 zero -
13 6 sym "no"
5 6 op -
2 7 op -
3 7 op -
4 7 op -
6 7 op -
8 7 op -
11 7 op -
12 7 op -
16 7 op -
15 7 op -
14 7 op -
17 7 op -
22 7 op -
21 7 op -
18 7 op -
19 7 op -
7 7 op -
0 7 zero -
0 7 zero -
0 8 zero -
END
CASE nums
9 0 num -2147483648
9 0 num 0
13 0 sym "xG"
9 0 num 16
9 0 num 16
9 0 num 16
9 0 num 0
13 0 sym "x10"
9 0 num -1
9 0 num -1
10 0 flt 3f8eb851eb851eb8
10 0 flt 3ff8000000000000
13 0 sym "e"
5 0 op -
10 0 flt 3ff8000000000000
10 0 flt 3fe0000000000000
10 0 flt 3fc0000000000000
0 0 zero -
END
CASE idents
13 0 sym "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
13 0 sym "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"
13 0 sym "xcccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc"
0 0 zero -
13 0 sym "Zz"
0 0 zero -
END
CASE us79
13 0 sym "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd_"
0 0 zero -
13 0 sym "Z"
0 0 zero -
END
CASE unclosed
25 0 str "abc"
0 0 zero -
END
CASE trans
1005 0 op -
1007 0 op -
1009 0 num 5
1025 0 str "z"
1026 0 op -
0 0 zero -
END
CASE longsym
9 0 num 34
0 0 zero -
END

""";

    private const string LumpGolden =
"""
CASE lump-root
CB 9 0 num 1
AT root
CB 9 0 num 2
AT child
CB 9 0 num 3
AT child
CB 5 2 op -
AT root
CASE lump-root-end
FILE null
LINE 0
PRINTS ""
END
CASE lump-bad
CASE lump-missing-badparent
FILE null
LINE 0
PRINTS "(null), line 0: Lump 'badparent' not found\n"
END
CASE lump-miss
CASE lump-miss-end
FILE here
LINE 4
PRINTS "here, line 4: Lump 'nope' not found\n"
END
CASE lump-glue
CASE lump-glue-end
FILE glue
LINE 0
PRINTS "glue, line 0: Include: String parameter expected\n"
END
CASE lump-hash
CB 9 0 num 1
AT hash
CASE lump-hash-end
FILE null
LINE 0
PRINTS ""
END
CASE lump-def
CB 27 0 op -
AT def
CB 13 0 sym "Foo"
AT def
CASE lump-def-end
FILE null
LINE 0
PRINTS ""
END
CASE lump-empty
CASE lump-empty-end
FILE null
LINE 0
PRINTS ""
END
CASE lump-cased
CB 9 0 num 2
AT child
CB 9 0 num 3
AT child
CASE lump-cased-end
FILE null
LINE 0
PRINTS ""
END
CASE lump-wrap
CB 13 1 sym "foo"
AT foo
CASE lump-wrap-end
FILE null
LINE 0
PRINTS "bad, line 0: Include: String parameter expected\n"
END
CASE lump-wrapnum
CB 9 1 num 5
AT 
CASE lump-wrapnum-end
FILE null
LINE 0
PRINTS "bad, line 0: Include: String parameter expected\n"
END
CASE err-file
0
PRINTS "f, line 3: boom\n"
END
CASE err-nofile
0
PRINTS "only\n"
END

""";
}
