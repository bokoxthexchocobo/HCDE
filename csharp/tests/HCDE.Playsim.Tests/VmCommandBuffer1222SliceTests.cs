using System.Globalization;
using System.Text;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmCommandBuffer1222SliceTests
{
    [Fact]
    public void Edit_YanksWordsAndDecodesThePaste()
    {
        string dump = Build();
        string edit = dump[..dump.IndexOf("wide ", StringComparison.Ordinal)];
        Assert.Contains("yank=65,66 ", edit, StringComparison.Ordinal);
        Assert.Contains("yank=63,64,20,65,66 ", edit, StringComparison.Ordinal);
        Assert.Contains("20ac,51", edit, StringComparison.Ordinal);
        Assert.Contains("utf=68695859e282ac51c3a9\n", edit, StringComparison.Ordinal);
        Assert.Contains("ffffffff", edit, StringComparison.Ordinal);
        Assert.Equal(EditGolden, edit);
    }

    [Fact]
    public void Draw_SkipsAWideCellAndTheCopyLeavesColumnsZero()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("wide ", StringComparison.Ordinal)..];
        Assert.Contains("wide text=57,41 cur=1 start=1 cc=2 sc=2", tail, StringComparison.Ordinal);
        Assert.Contains("T1 8 18 20 41\n", tail, StringComparison.Ordinal);
        Assert.Contains("C2 10 18 20 b 160 100 1\n", tail, StringComparison.Ordinal);
        Assert.Contains("copy text=5a cur=1 start=0 cc=0 sc=0 yank= app=0 cols=0", tail, StringComparison.Ordinal);
        Assert.Contains("srccols=40\n", tail, StringComparison.Ordinal);
        Assert.Equal(DrawGolden, tail);
    }

    static string Build()
    {
        var sb = new StringBuilder();
        void Show(string tag, VmCommandBuffer1222 b)
        {
            foreach (string d in b.Draws)
            {
                sb.Append(tag);
                sb.Append(' ');
                sb.Append(d);
                sb.Append('\n');
            }
            b.Draws.Clear();
            sb.Append(tag);
            sb.Append(" text=");
            sb.Append(Hex(b.Text));
            sb.Append(" cur=");
            sb.Append(b.CursorPos.ToString(CultureInfo.InvariantCulture));
            sb.Append(" start=");
            sb.Append(b.StartPos.ToString(CultureInfo.InvariantCulture));
            sb.Append(" cc=");
            sb.Append(b.CursorPosCells.ToString(CultureInfo.InvariantCulture));
            sb.Append(" sc=");
            sb.Append(b.StartPosCells.ToString(CultureInfo.InvariantCulture));
            sb.Append(" yank=");
            sb.Append(Hex(b.Yank));
            sb.Append(" app=");
            sb.Append(b.AppendToYankBuffer ? "1" : "0");
            sb.Append(" cols=");
            sb.Append(b.ConCols.ToString(CultureInfo.InvariantCulture));
            sb.Append(" utf=");
            sb.Append(Hexb(b.GetText()));
            sb.Append('\n');
        }

        var b = new VmCommandBuffer1222();
        Wire(b);
        b.ConCols = 80;
        b.ScaleDivisor = 1;
        foreach (char ch in "ab cd ef")
            b.AddChar(ch);
        Show("add", b);
        b.CursorWordLeft();
        Show("wl1", b);
        b.CursorWordLeft();
        Show("wl2", b);
        b.CursorWordRight();
        Show("wr", b);
        sb.Append("endmoved=");
        sb.Append(b.CursorEnd() ? "1\n" : "0\n");
        Show("end", b);
        b.DeleteWordLeft();
        Show("dw1", b);
        b.AppendToYankBuffer = true;
        b.DeleteWordLeft();
        Show("dw2", b);
        b.AddYankBuffer();
        Show("yank", b);
        b.CursorStart();
        b.CursorRight();
        b.CursorRight();
        b.DeleteLineRight();
        Show("dlr", b);
        b.DeleteLineLeft();
        Show("dll", b);
        b.AddString(new byte[] { (byte)'h', (byte)'i' });
        Show("setwait", b);
        b.Text.Clear();
        b.CursorPos = 0;
        b.StartPos = 0;
        b.Yank.Clear();
        b.AppendToYankBuffer = false;
        b.SetString(new byte[] { (byte)'h', (byte)'i' });
        Show("set", b);
        b.AddString(new byte[] { (byte)'X', (byte)'Y', (byte)'\n', (byte)'Z' });
        Show("nl", b);
        b.AddString(Array.Empty<byte>());
        Show("empty", b);
        b.AddString(new byte[] { 0x80, (byte)'Q' });
        Show("win", b);
        b.AddString(new byte[] { 0xC3, 0xA9 });
        Show("utf", b);
        b.AddChar(-1);
        Show("neg", b);

        var wide = new VmCommandBuffer1222();
        Wire(wide);
        wide.ConCols = 3;
        wide.ScaleDivisor = 1;
        wide.AddChar('W');
        wide.AddChar('A');
        wide.CursorStart();
        wide.CursorRight();
        Show("wide", wide);
        wide.Draw(10, 20, 1, true);
        wide.Draw(10, 20, 2, true);
        Show("draw", wide);

        var src = new VmCommandBuffer1222();
        Wire(src);
        src.ConCols = 40;
        src.AddChar('Z');
        src.Yank.Add('Q');
        src.AppendToYankBuffer = true;
        src.CursorPosCells = 9;
        var copy = new VmCommandBuffer1222(src);
        Show("copy", copy);
        sb.Append("srccols=");
        sb.Append(src.ConCols.ToString(CultureInfo.InvariantCulture));
        sb.Append('\n');
        return sb.ToString();
    }

    static void Wire(VmCommandBuffer1222 b)
    {
        b.NewCharWidth = cp => cp == 'W' ? 18 : 9;
        b.CurrentCharWidth = cp =>
        {
            if (cp == 0x1c)
                return 8;
            if (cp == 0x0b)
                return 4;
            return 9;
        };
    }

    static string Hex(IReadOnlyList<int> values)
    {
        var parts = new List<string>();
        foreach (int v in values)
            parts.Add(unchecked((uint)v).ToString("x", CultureInfo.InvariantCulture));
        return string.Join(",", parts);
    }

    static string Hexb(byte[] bytes)
    {
        var o = new StringBuilder();
        foreach (byte value in bytes)
            o.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        return o.ToString();
    }

    private const string EditGolden =
"""
add text=61,62,20,63,64,20,65,66 cur=8 start=0 cc=8 sc=0 yank= app=0 cols=80 utf=6162206364206566
wl1 text=61,62,20,63,64,20,65,66 cur=6 start=0 cc=6 sc=0 yank= app=0 cols=80 utf=6162206364206566
wl2 text=61,62,20,63,64,20,65,66 cur=3 start=0 cc=3 sc=0 yank= app=0 cols=80 utf=6162206364206566
wr text=61,62,20,63,64,20,65,66 cur=5 start=0 cc=5 sc=0 yank= app=0 cols=80 utf=6162206364206566
endmoved=1
end text=61,62,20,63,64,20,65,66 cur=8 start=0 cc=8 sc=0 yank= app=0 cols=80 utf=6162206364206566
dw1 text=61,62,20,63,64,20 cur=6 start=0 cc=6 sc=0 yank=65,66 app=0 cols=80 utf=616220636420
dw2 text=61,62,20 cur=3 start=0 cc=3 sc=0 yank=63,64,20,65,66 app=1 cols=80 utf=616220
yank text=61,62,20,63,64,20,65,66 cur=8 start=0 cc=8 sc=0 yank=63,64,20,65,66 app=1 cols=80 utf=6162206364206566
dlr text=61,62 cur=2 start=0 cc=2 sc=0 yank=63,64,20,65,66,20,63,64,20,65,66 app=1 cols=80 utf=6162
dll text= cur=0 start=0 cc=0 sc=0 yank=61,62,63,64,20,65,66,20,63,64,20,65,66 app=1 cols=80 utf=
setwait text=68,69 cur=2 start=0 cc=2 sc=0 yank=61,62,63,64,20,65,66,20,63,64,20,65,66 app=1 cols=80 utf=6869
set text=68,69 cur=2 start=0 cc=2 sc=0 yank= app=0 cols=80 utf=6869
nl text=68,69,58,59 cur=4 start=0 cc=4 sc=0 yank= app=0 cols=80 utf=68695859
empty text=68,69,58,59 cur=4 start=0 cc=4 sc=0 yank= app=0 cols=80 utf=68695859
win text=68,69,58,59,20ac,51 cur=6 start=0 cc=6 sc=0 yank= app=0 cols=80 utf=68695859e282ac51
utf text=68,69,58,59,20ac,51,e9 cur=7 start=0 cc=7 sc=0 yank= app=0 cols=80 utf=68695859e282ac51c3a9
neg text=68,69,58,59,20ac,51,e9,ffffffff cur=8 start=0 cc=8 sc=0 yank= app=0 cols=80 utf=68695859e282ac51c3a9

""";

    private const string DrawGolden =
"""
wide text=57,41 cur=1 start=1 cc=2 sc=2 yank= app=0 cols=3 utf=5741
draw C1 8 10 20 1c
draw T1 8 18 20 41
draw C1 10 18 20 b
draw C2 8 10 20 1c 160 100 1
draw T2 8 18 20 41 160 100 1
draw C2 10 18 20 b 160 100 1
draw text=57,41 cur=1 start=1 cc=2 sc=2 yank= app=0 cols=3 utf=5741
copy text=5a cur=1 start=0 cc=0 sc=0 yank= app=0 cols=0 utf=5a
srccols=40

""";
}
