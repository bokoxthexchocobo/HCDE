using System.IO.Compression;
using System.Text;

namespace HCDE.Playsim.Tests;

public class VmErrorWindow1323SliceTests
{
    [Fact]
    public void Modal_CentersAndStripsTheClipboard()
    {
        var host = new VmErrorWindow1323();
        host.Strings["ERROR_FATAL"] = "Fatal";
        host.Strings["ERROR_ABORT"] = "Abort";
        host.Version = "4.14";
        host.GitTime = "abc";
        host.DuringLoop = window => window.Last!.OnRestart();
        bool restart = host.ExecModal("boom", "\u001cAhi\u001d", null);
        Assert.True(restart);
        Assert.True(host.Shown);
        Assert.Equal("enu", host.CanvasLanguage);
        Assert.Equal("Fatal - HCDE 4.14 (abc)", host.Caption);
        Assert.Equal(200, host.Last!.X);
        Assert.Equal(100, host.Last.Y);
        Assert.Equal(1200, host.Last.Width);
        Assert.Equal(700, host.Last.Height);
        Assert.Equal("log", host.Last.Focus);
        Assert.NotNull(host.Last.RestartButton);
        Assert.Null(host.Last.SaveReportButton);
        Assert.Equal("Ahi\u2550\nExecution could not continue.\nboom\n", host.Clipboard);
        Assert.Equal("hi\u2550 ", host.Last.Log.Lines[0].Text);
        Assert.Equal(0xCC, host.Last.Log.Lines[0].R);
        Assert.Equal(0x33, host.Last.Log.Lines[0].G);
        Assert.Equal(0x33, host.Last.Log.Lines[0].B);
        Assert.Equal(" ", host.Last.Log.Lines[1].Text);
        Assert.Equal("Abort", host.Last.Log.Lines[2].Text);
        Assert.Equal(170, host.Last.Log.Lines[2].G);
        Assert.Equal("boom", host.Last.Log.Lines[3].Text);
        Assert.Equal(255, host.Last.Log.Lines[3].R);
        Assert.Equal(255, host.Last.Log.Lines[3].G);
        Assert.Equal(170, host.Last.Log.Lines[3].B);
        Assert.Equal(3, host.Last.Log.Bar.Position);
        Assert.Equal(4, host.Last.Log.Bar.Max);

        host.Last.OnClose();
        Assert.False(host.Last.Restart);
        Assert.True(host.LoopExited);

        byte[] euro = { 0x80 };
        host.Last.SetText("x", Encoding.Latin1.GetString(euro));
        Assert.Contains("\u20AC", host.Clipboard);
    }

    [Fact]
    public void Layout_ScrollsAndSavesTheReport()
    {
        var host = new VmErrorWindow1323();
        var window = new VmErrorWindow1323.Window(host, new byte[] { 1, 2, 3 });
        Assert.Null(window.RestartButton);
        Assert.NotNull(window.SaveReportButton);
        window.ClipboardButton.PreferredWidth = 100;
        window.ClipboardButton.PreferredHeight = 30;
        window.SaveReportButton!.PreferredWidth = 80;
        window.WadDirButton.PreferredWidth = 80;
        window.ConfigDirButton.PreferredWidth = 80;
        window.SetFrame(0, 0, 800, 600);
        window.OnGeometryChanged();
        Assert.Equal(20, window.ClipboardButton.X);
        Assert.Equal(555, window.ClipboardButton.Y);
        Assert.Equal(100, window.ClipboardButton.Width);
        Assert.Equal(700, window.SaveReportButton.X);
        Assert.Equal(80, window.SaveReportButton.Width);
        Assert.Equal(610, window.ConfigDirButton.X);
        Assert.Equal(520, window.WadDirButton.X);
        Assert.Equal(535, window.Log.Height);
        window.WadDirButton.Click!();
        window.ConfigDirButton.Click!();
        Assert.True(host.OpenedWadDir);
        Assert.True(host.OpenedConfigDir);

        window.Log.SetText("tail", "one\ntwo");
        Assert.Equal(5, window.Log.Lines.Count);
        window.Log.Height = 40;
        window.Log.Bar.PreferredWidth = 16;
        window.Log.OnGeometryChanged();
        window.Log.PaintFrame();
        window.Log.Paint();
        Assert.Equal((38, 38, 38), window.Log.FrameFill);
        Assert.Equal(new[] { 4, 3, 2 }, window.Log.PaintedLines.Select(p => p.Index).ToArray());
        Assert.Equal(24, window.Log.PaintedLines[0].Top);
        Assert.Equal(800 - 16, window.Log.PaintedLines[0].Width);

        window.Log.OnKey("Home");
        Assert.Equal(0, window.Log.Bar.Position);
        window.Log.OnKey("End");
        Assert.Equal(4, window.Log.Bar.Position);
        window.Log.OnKey("Up");
        Assert.Equal(0, window.Log.Bar.Position);
        window.Log.ScrollDown(4);
        Assert.Equal(4, window.Log.Bar.Position);
        Assert.True(window.Log.OnMouseWheel(1));
        Assert.Equal(0, window.Log.Bar.Position);
        window.Log.OnKey("PageDown");
        Assert.Equal(4, window.Log.Bar.Position);

        window.Log.SetText("plain", "\u001c[Red]no\u001c+go\u001c?");
        Assert.Equal("no", window.Log.Lines[0].Pieces[0].Text);
        Assert.Equal(0xFF, window.Log.Lines[0].Pieces[0].R);
        Assert.Equal(0x55, window.Log.Lines[0].Pieces[0].G);
        Assert.Equal("go ", window.Log.Lines[0].Pieces[1].Text);
        Assert.Equal(0xEE, window.Log.Lines[0].Pieces[1].R);
        Assert.Equal(0x33, window.Log.Lines[0].Pieces[1].B);

        window.SetText("plain", "hello");
        host.DialogAccepts = false;
        window.OnSaveReport();
        Assert.Equal("CrashReport.zip", host.DialogFilename);
        Assert.Equal("zip", host.DialogExtension);
        Assert.Equal(2, host.DialogFilters.Count);
        Assert.Null(host.SavedZip);
        host.DialogAccepts = true;
        window.OnSaveReport();
        Assert.NotNull(host.SavedZip);
        using var zip = new ZipArchive(new MemoryStream(host.SavedZip!), ZipArchiveMode.Read);
        ZipArchiveEntry dump = zip.GetEntry("minidump.dmp")!;
        using (var stream = dump.Open())
        {
            var body = new MemoryStream();
            stream.CopyTo(body);
            Assert.Equal(new byte[] { 1, 2, 3 }, body.ToArray());
        }
        ZipArchiveEntry log = zip.GetEntry("log.txt")!;
        using var reader = new StreamReader(log.Open(), Encoding.UTF8);
        Assert.Contains("Execution could not continue.", reader.ReadToEnd());
    }
}
