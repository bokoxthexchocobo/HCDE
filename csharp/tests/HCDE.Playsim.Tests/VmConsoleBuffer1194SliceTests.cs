namespace HCDE.Playsim.Tests;

public class VmConsoleBuffer1194SliceTests
{
    [Fact]
    public void AddText_ColorsContinuesAndReplacesLines()
    {
        VmConsoleBuffer1194Slice.ClearHost();
        var buffer = new VmConsoleBuffer1194Slice.Buffer();
        Assert.Equal(VmConsoleBuffer1194Slice.NewLine, buffer.AddType);
        Assert.Equal(-1, buffer.LastDisplayWidth);
        Assert.True(buffer.WasCleared);
        Assert.Equal(new[] { 0 }, buffer.BrokenStart);

        buffer.AddText(VmConsoleBuffer1194Slice.PrintHigh, "crit\n");
        buffer.AddText(VmConsoleBuffer1194Slice.PrintLow, "low\n");
        buffer.AddText(VmConsoleBuffer1194Slice.PrintMedium, "gold\n");
        buffer.AddText(VmConsoleBuffer1194Slice.PrintChat, "chat\n");
        buffer.AddText(VmConsoleBuffer1194Slice.PrintTeamChat, "team\n");
        buffer.AddText(VmConsoleBuffer1194Slice.PrintLog, "log\n");
        buffer.AddText(VmConsoleBuffer1194Slice.PrintBold, "bold\n");
        buffer.AddText(6, "other\n");
        Assert.Equal(
            new[]
            {
                "\u001cBcrit",
                "\u001cLlow",
                "\u001cFgold",
                "\u001cDchat",
                "\u001cDteam",
                "\u001cBlog",
                "\u001cDbold",
                "\u001cBother"
            },
            buffer.Text);
        Assert.Equal(VmConsoleBuffer1194Slice.NewLine, buffer.AddType);

        buffer.AddText(VmConsoleBuffer1194Slice.PrintHigh, "hel");
        Assert.Equal(VmConsoleBuffer1194Slice.AppendLine, buffer.AddType);
        buffer.AddText(VmConsoleBuffer1194Slice.PrintBold, "lo\n");
        Assert.Equal("\u001cBhello", buffer.Text[^1]);
        Assert.Equal(VmConsoleBuffer1194Slice.NewLine, buffer.AddType);

        buffer.AddText(VmConsoleBuffer1194Slice.PrintHigh, "old\r");
        buffer.AddText(VmConsoleBuffer1194Slice.PrintLow, "new");
        Assert.Equal("\u001cLnew", buffer.Text[^1]);
        Assert.Equal(VmConsoleBuffer1194Slice.AppendLine, buffer.AddType);
        Assert.DoesNotContain("\u001cBold", buffer.Text);

        var beforeEmpty = buffer.Text.Count;
        buffer.AddText(VmConsoleBuffer1194Slice.PrintHigh, "");
        Assert.Equal(beforeEmpty, buffer.Text.Count);
        Assert.Equal("\u001cBhello", buffer.Text[^2]);
        Assert.Equal("\u001cLnew", buffer.Text[^1]);
        Assert.Equal(VmConsoleBuffer1194Slice.AppendLine, buffer.AddType);
    }

    [Fact]
    public void FormatText_RebuildsTheDirtyTailAndDropsOldLines()
    {
        VmConsoleBuffer1194Slice.ClearHost();
        var buffer = new VmConsoleBuffer1194Slice.Buffer();
        var calls = new List<string>();
        VmConsoleBuffer1194Slice.BreakLines = (font, width, text) =>
        {
            calls.Add(font.Name + " " + width + " " + text);
            if (text.Contains("wide", StringComparison.Ordinal))
            {
                return new List<VmConsoleBuffer1194Slice.BrokenLine>
                {
                    new() { Width = 2, Text = text + "#0" },
                    new() { Width = 3, Text = text + "#1" }
                };
            }

            return new List<VmConsoleBuffer1194Slice.BrokenLine>
            {
                new() { Width = width, Text = text }
            };
        };
        buffer.AddText(VmConsoleBuffer1194Slice.PrintHigh, "one\n");
        buffer.AddText(VmConsoleBuffer1194Slice.PrintHigh, "wide");
        var font = new VmConsoleBuffer1194Slice.Font { Name = "console" };
        buffer.FormatText(font, 80);
        Assert.Equal(new[] { "console 80 \u001cBone", "console 80 \u001cBwide" }, calls);
        Assert.Equal(3, buffer.TextLines);
        Assert.Equal(new[] { 0, 1, 3 }, buffer.BrokenStart);
        Assert.Equal("\u001cBone", buffer.Lines[0].Text);
        Assert.Equal(80, buffer.Lines[0].Width);
        Assert.Equal("\u001cBwide#1", buffer.Lines[2].Text);
        Assert.False(buffer.WasCleared);
        Assert.False(buffer.LastLineNeedsUpdate);
        Assert.Same(font, buffer.LastFont);

        calls.Clear();
        buffer.FormatText(font, 80);
        Assert.Empty(calls);
        Assert.Equal(new[] { 0, 1, 3 }, buffer.BrokenStart);

        buffer.AddText(VmConsoleBuffer1194Slice.PrintHigh, "r");
        calls.Clear();
        buffer.FormatText(font, 80);
        Assert.Equal(new[] { "console 80 \u001cBwider" }, calls);
        Assert.Equal(3, buffer.TextLines);
        Assert.Equal("\u001cBwider#0", buffer.Lines[1].Text);
        Assert.Equal("\u001cBwider#1", buffer.Lines[2].Text);
        Assert.Equal(2, buffer.BrokenParagraphs.Count);

        calls.Clear();
        var other = new VmConsoleBuffer1194Slice.Font { Name = "small" };
        buffer.FormatText(other, 40);
        Assert.Equal(2, calls.Count);
        Assert.Equal(40, buffer.Lines[0].Width);
        Assert.Equal("small", calls[0].Split(' ')[0]);

        buffer.ResizeBuffer(10);
        Assert.False(buffer.WasCleared);
        buffer.ResizeBuffer(1);
        Assert.True(buffer.WasCleared);
        Assert.Equal(new[] { "\u001cBwider" }, buffer.Text);
        calls.Clear();
        buffer.FormatText(other, 40);
        Assert.Equal(new[] { "small 40 \u001cBwider" }, calls);
        Assert.Equal(new[] { 0, 2 }, buffer.BrokenStart);
        Assert.Equal(2, buffer.TextLines);

        buffer.Clear();
        Assert.Empty(buffer.Text);
        Assert.True(buffer.WasCleared);
        buffer.FormatText(other, 40);
        Assert.Equal(0, buffer.TextLines);
        Assert.Equal(new[] { 0 }, buffer.BrokenStart);
    }
}
