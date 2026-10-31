using System.Text;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmText1225SliceTests
{
    [Fact]
    public void Break_WrapsColorUtf8AndRejectsABadIndex()
    {
        string dump = Build();
        int cut = dump.IndexOf("ll [", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("w=9 t=1c5757\n", head, StringComparison.Ordinal);
        Assert.Contains("w=4 t=1c581c5941\n", head, StringComparison.Ordinal);
        Assert.Contains("w=16 t=1c5b5265645d4e657874\n", head, StringComparison.Ordinal);
        Assert.Contains("w=11 t=e282ac\n", head, StringComparison.Ordinal);
        Assert.Contains("gap-nl n=2\n", head, StringComparison.Ordinal);
        Assert.Contains("ix=-1 sw=-1 at=ff\n", head, StringComparison.Ordinal);
        Assert.Equal(BreakGolden, head);
    }

    [Fact]
    public void Ui_ParsesStrtollAndFallsBackToTheStockFont()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("ll [", StringComparison.Ordinal)..];
        Assert.Contains("ll [010]=8\n", tail, StringComparison.Ordinal);
        Assert.Contains("ll [08]=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("ll [999999999999999999999]=9223372036854775807\n", tail, StringComparison.Ordinal);
        Assert.Contains("orig gen=0 small=3 big=5 checks=4 special=0 up=73 lo=105\n", tail, StringComparison.Ordinal);
        Assert.Contains("cvar gen=1 small=1 big=1 checks=0 special=1 up=304 lo=305\n", tail, StringComparison.Ordinal);
        Assert.Contains("dot gen=0 small=3 big=5 checks=4 special=1 up=304 lo=305\n", tail, StringComparison.Ordinal);
        Assert.Contains("zero gen=0 small=1 big=1 checks=2 special=0 up=73 lo=105\n", tail, StringComparison.Ordinal);
        Assert.Equal(UiGolden, tail);
    }

    static string Build()
    {
        var sb = new StringBuilder();
        var emptyAt = new VmBrokenLines1225(new List<VmBrokenLine1225>());
        sb.Append("ternary=");
        sb.Append(VmText1225.Hex(emptyAt.TextAt(-1)));
        sb.Append('\n');

        var port = new VmText1225();
        var font = new VmTextFont1225();
        font.CharWidth = Width;
        font.Measure = text => NativeMeasure(font, text);

        void Dump(string tag, int maxw, string text, bool keep, int kern)
        {
            font.Kerning = kern;
            var lines = port.BreakLines(font, maxw, Latin(text), keep);
            sb.Append(tag);
            sb.Append(" n=");
            sb.Append(lines.Count);
            sb.Append('\n');
            foreach (VmBrokenLine1225 line in lines)
            {
                sb.Append("w=");
                sb.Append(line.Width);
                sb.Append(" t=");
                sb.Append(VmText1225.Hex(line.Text));
                sb.Append('\n');
            }
        }

        Dump("empty", 40, "", false, 0);
        Dump("nl", 40, "\n", false, 0);
        Dump("hello-nl", 400, "Hello\nNext", false, 0);
        Dump("world-nl", 400, "Hello world\nNext", false, 0);
        Dump("world-nl-nl", 400, "Hello world\n\nX", false, 0);
        Dump("lead-sp", 400, "A\n  B", false, 0);
        Dump("exact", 8, "AB", false, 0);
        Dump("kern", 8, "AB", false, 1);
        Dump("gap-nl", 400, "A \nB", false, 0);
        Dump("wide", 3, "AB", false, 0);
        Dump("color-letter", 10, "\u001cAB", false, 0);
        Dump("color-steal", 10, "A\u001cW", false, 0);
        Dump("bracket", 8, "\u001c[Red]ABCDEF", false, 0);
        Dump("two-color", 12, "\u001cX\u001cYABCD", false, 0);
        Dump("preserve-off", 400, "\u001c[Red]Hello world\nNext", false, 0);
        Dump("preserve-on", 400, "\u001c[Red]Hello world\nNext", true, 0);
        Dump("letter-nl-off", 400, "\u001cR\nZ", false, 0);
        Dump("letter-nl-on", 400, "\u001cR\nZ", true, 0);
        Dump("utf8", 6, "A\u00c3\u00a9B", false, 0);
        Dump("euro", 8, "A\u00e2\u0082\u00acB", false, 0);
        Dump("win", 80, "\u0080Z", false, 0);
        Dump("lone", 4, "\u00c3Z", false, 0);
        Dump("ff", 6, "A\u000cB", false, 0);
        Dump("vt", 6, "A\u000bB", false, 0);
        Dump("dbl-nl", 40, "A\n\nB", false, 0);
        Dump("trail", 40, "A\n   ", false, 0);
        Dump("tab", 12, "AA\tBB", false, 0);
        Dump("cr", 12, "AA\rBB", false, 0);
        Dump("spaces", 8, "A  B", false, 0);
        Dump("unclosed", 40, "\u001c[RedABC", false, 0);
        Dump("end-esc", 40, "AB\u001c", false, 0);
        Dump("only-sp", 40, "   ", false, 0);
        Dump("wrap-sp", 8, "AB C", false, 0);

        font.Kerning = 0;
        var wrapped = new VmBrokenLines1225(port.BreakLines(font, 8, Latin("\u001c[Red]ABCDEF"), false));
        sb.Append("count=");
        sb.Append(wrapped.Count());
        sb.Append('\n');
        foreach (int ix in new[] { -1, 0, 1, wrapped.Count(), 99 })
        {
            sb.Append("ix=");
            sb.Append(ix);
            sb.Append(" sw=");
            sb.Append(wrapped.WidthAt(ix));
            sb.Append(" at=");
            sb.Append(VmText1225.Hex(wrapped.TextAt(ix)));
            sb.Append('\n');
        }
        sb.Append(wrapped.Serialize(false));
        sb.Append('\n');
        sb.Append(wrapped.Serialize(true));
        sb.Append('\n');
        var kept = port.BreakPreserving(font, Latin("\u001cR\nZ"), 400);
        sb.Append("vm-keep n=");
        sb.Append(kept.Count());
        sb.Append('\n');
        foreach (VmBrokenLine1225 line in kept.Lines)
        {
            sb.Append("w=");
            sb.Append(line.Width);
            sb.Append(" t=");
            sb.Append(VmText1225.Hex(line.Text));
            sb.Append('\n');
        }

        string[] lls =
        {
            "0", "0x10", "08", "010", "  -2x", "abc", "0x", "+", "", "0xG", "0X10",
            "\t\v\f\r\n 5", "999999999999999999999", "-999999999999999999999", "0x0", "  +0",
        };
        foreach (string sample in lls)
        {
            sb.Append("ll [");
            foreach (char ch in sample)
            {
                if (ch < 32)
                    sb.Append("\\x").Append(((int)ch).ToString("x2"));
                else
                    sb.Append(ch);
            }
            sb.Append("]=");
            sb.Append(VmGenericUi1225.Strtoll(sample));
            sb.Append('\n');
        }

        sb.Append("---- ui\n");
        var neu = Font(1, true);
        Ui("cvar", true, null, "xx\u00C4\u00B0yy", Font(2, false), Font(3, true), Font(4, false), Font(5, true));
        Ui("hex", false, "0x10", "abc", Font(2, false), Font(3, true), Font(4, false), Font(5, true));
        Ui("oct", false, "010", null, Font(2, false), null, Font(4, false), null);
        Ui("oct8", false, "08", null, Font(2, false), null, Font(4, false), null);
        Ui("zero", false, "0", "need", Font(2, false), null, Font(4, false), null);
        Ui("orig", false, "abc", "need", Font(2, false), Font(3, true), Font(4, false), Font(5, true));
        Ui("both-bad", false, "", "need", Font(2, false), Font(3, false), Font(4, false), Font(5, false));
        Ui("small-ok", false, "0x0", "yes", Font(2, true), null, Font(4, false), Font(5, true));
        Ui("null-req", false, "0xG", null, Font(2, false), Font(3, true), Font(4, false), Font(5, true));
        Ui("dot", false, "0", "xx\u00C4\u00B0yy", Font(2, false), Font(3, true), Font(4, false), Font(5, true));
        Ui("dotless", false, null, "\u00C4\u00B1", Font(2, true), Font(3, false), Font(4, true), Font(5, false));
        Ui("overflow", false, "999999999999999999999", null, Font(2, false), Font(3, true), Font(4, false), Font(5, true));
        Ui("neg", false, " -3", "\u00C4\u00B0", Font(2, false), null, Font(4, false), null);
        return sb.ToString();

        static byte[] Latin(string text) => Encoding.Latin1.GetBytes(text);

        static int NativeMeasure(VmTextFont1225 face, byte[] text)
        {
            int i = 0;
            int w = 0;
            int maxw = 0;
            while (VmText1225.At(text, i) != 0)
            {
                int chr = VmText1225.ReadChar(text, ref i);
                if (chr == VmText1225.ColorEscape)
                {
                    if (VmText1225.At(text, i) == '[')
                    {
                        while (VmText1225.At(text, i) != 0 && VmText1225.At(text, i) != ']')
                            i++;
                    }
                    if (VmText1225.At(text, i) != 0)
                        i++;
                    continue;
                }
                if (chr == '\n')
                {
                    if (w > maxw)
                        maxw = w;
                    w = 0;
                }
                else
                {
                    w += face.CharWidth(chr) + face.Kerning;
                }
            }
            return maxw > w ? maxw : w;
        }

        static int Width(int code)
        {
            if (code == ' ' || code == '\t' || code == '\r')
                return 2;
            if (code == '\n')
                return 0;
            if (code == 'W')
                return 9;
            if (code == 0x20AC)
                return 11;
            if (code == 0xC3)
                return 7;
            if (code == '\v' || code == '\f')
                return 3;
            return 4;
        }

        static VmTextFont1225 Font(int id, bool complete)
        {
            return new VmTextFont1225 { Id = id, Complete = complete };
        }

        void Ui(string tag, bool cvar, string? use, string? req, VmTextFont1225? small, VmTextFont1225? originalSmall, VmTextFont1225? big, VmTextFont1225? originalBig)
        {
            int checks = 0;
            bool Print(VmTextFont1225 face, string? text)
            {
                checks++;
                if (text == null)
                    return true;
                return face.Complete;
            }
            if (small != null)
                small.CanPrint = text => Print(small, text);
            if (originalSmall != null)
                originalSmall.CanPrint = text => Print(originalSmall, text);
            if (big != null)
                big.CanPrint = text => Print(big, text);
            if (originalBig != null)
                originalBig.CanPrint = text => Print(originalBig, text);
            var ui = new VmGenericUi1225
            {
                NewSmall = neu,
                Small = small ?? neu,
                OriginalSmall = originalSmall,
                Big = big ?? neu,
                OriginalBig = originalBig,
                Lookup = name => name == "USE_GENERIC_FONT" ? use : name == "REQUIRED_CHARACTERS" ? req : null,
            };
            ui.SetUiGeneric(cvar);
            sb.Append(tag);
            sb.Append(" gen=");
            sb.Append(ui.GenericUi ? 1 : 0);
            sb.Append(" small=");
            sb.Append(ui.AlternativeSmall == null ? 0 : ui.AlternativeSmall.Id);
            sb.Append(" big=");
            sb.Append(ui.AlternativeBig == null ? 0 : ui.AlternativeBig.Id);
            sb.Append(" checks=");
            sb.Append(checks);
            sb.Append(" special=");
            sb.Append(ui.SpecialI ? 1 : 0);
            sb.Append(" up=");
            sb.Append(ui.UpperForLowerI);
            sb.Append(" lo=");
            sb.Append(ui.LowerForUpperI);
            sb.Append('\n');
        }
    }

    const string BreakGolden =
        """
        ternary=ff
        empty n=0
        nl n=1
        w=0 t=
        hello-nl n=2
        w=20 t=48656c6c6f
        w=16 t=4e657874
        world-nl n=2
        w=42 t=48656c6c6f20776f726c64
        w=16 t=4e657874
        world-nl-nl n=3
        w=42 t=48656c6c6f20776f726c64
        w=0 t=
        w=4 t=58
        lead-sp n=2
        w=4 t=41
        w=8 t=202042
        exact n=1
        w=8 t=4142
        kern n=2
        w=5 t=41
        w=5 t=42
        gap-nl n=2
        w=4 t=41
        w=4 t=42
        wide n=2
        w=4 t=41
        w=4 t=42
        color-letter n=1
        w=4 t=1c4142
        color-steal n=2
        w=4 t=411c
        w=9 t=1c5757
        bracket n=3
        w=8 t=1c5b5265645d4142
        w=8 t=1c5b5265645d4344
        w=8 t=1c5b5265645d4546
        two-color n=2
        w=4 t=1c581c5941
        w=12 t=1c58424344
        preserve-off n=2
        w=42 t=1c5b5265645d48656c6c6f20776f726c64
        w=16 t=4e657874
        preserve-on n=2
        w=42 t=1c5b5265645d48656c6c6f20776f726c64
        w=16 t=1c5b5265645d4e657874
        letter-nl-off n=2
        w=0 t=1c52
        w=4 t=5a
        letter-nl-on n=2
        w=0 t=1c52
        w=4 t=1c525a
        utf8 n=3
        w=4 t=41
        w=4 t=c3a9
        w=4 t=42
        euro n=3
        w=4 t=41
        w=11 t=e282ac
        w=4 t=42
        win n=1
        w=15 t=805a
        lone n=2
        w=7 t=c3
        w=4 t=5a
        ff n=3
        w=4 t=41
        w=3 t=0c
        w=4 t=42
        vt n=3
        w=4 t=41
        w=3 t=0b
        w=4 t=42
        dbl-nl n=3
        w=4 t=41
        w=0 t=
        w=4 t=42
        trail n=1
        w=4 t=41
        tab n=2
        w=8 t=4141
        w=8 t=4242
        cr n=2
        w=8 t=4141
        w=8 t=4242
        spaces n=2
        w=4 t=41
        w=4 t=42
        unclosed n=1
        w=0 t=1c5b526564414243
        end-esc n=1
        w=8 t=41421c
        only-sp n=0
        wrap-sp n=2
        w=8 t=4142
        w=4 t=43
        count=3
        ix=-1 sw=-1 at=ff
        ix=0 sw=8 at=1c5b5265645d4142
        ix=1 sw=8 at=1c5b5265645d4344
        ix=3 sw=-1 at=ff
        ix=99 sw=-1 at=ff
        ser-closed
        ser-open text=1c5b5265645d4142 width=8 text=1c5b5265645d4344 width=8 text=1c5b5265645d4546 width=8
        vm-keep n=2
        w=0 t=1c52
        w=4 t=1c525a

        """;

    const string UiGolden =
        """
        ll [0]=0
        ll [0x10]=16
        ll [08]=0
        ll [010]=8
        ll [  -2x]=-2
        ll [abc]=0
        ll [0x]=0
        ll [+]=0
        ll []=0
        ll [0xG]=0
        ll [0X10]=16
        ll [\x09\x0b\x0c\x0d\x0a 5]=5
        ll [999999999999999999999]=9223372036854775807
        ll [-999999999999999999999]=-9223372036854775808
        ll [0x0]=0
        ll [  +0]=0
        ---- ui
        cvar gen=1 small=1 big=1 checks=0 special=1 up=304 lo=305
        hex gen=1 small=1 big=1 checks=0 special=0 up=73 lo=105
        oct gen=1 small=1 big=1 checks=0 special=0 up=73 lo=105
        oct8 gen=0 small=2 big=4 checks=2 special=0 up=73 lo=105
        zero gen=0 small=1 big=1 checks=2 special=0 up=73 lo=105
        orig gen=0 small=3 big=5 checks=4 special=0 up=73 lo=105
        both-bad gen=0 small=1 big=1 checks=4 special=0 up=73 lo=105
        small-ok gen=0 small=2 big=5 checks=3 special=0 up=73 lo=105
        null-req gen=0 small=2 big=4 checks=2 special=0 up=73 lo=105
        dot gen=0 small=3 big=5 checks=4 special=1 up=304 lo=305
        dotless gen=0 small=2 big=4 checks=2 special=0 up=73 lo=105
        overflow gen=1 small=1 big=1 checks=0 special=0 up=73 lo=105
        neg gen=1 small=1 big=1 checks=0 special=1 up=304 lo=305

        """;
}
