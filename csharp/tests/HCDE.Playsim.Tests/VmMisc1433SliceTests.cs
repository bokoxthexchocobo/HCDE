using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmMisc1433SliceTests
{
    [Fact]
    public void PcxRowsAndPngText()
    {
        Assert.Equal(440, VmMisc1433Slice.BandStart);
        Assert.Equal(802, VmMisc1433Slice.BandEnd);
        var pal = new byte[768];
        pal[0] = 1;
        pal[1] = 2;
        pal[2] = 3;
        var file = new List<byte>();
        VmMisc1433Slice.WritePcx(file, new byte[] { 1, 2 }, pal, VmMisc1433Slice.Pal, 2, 1, 2);
        Assert.Equal(new byte[] { 10, 5, 1, 8 }, file.GetRange(0, 4));
        Assert.Equal(1, file[8]);
        Assert.Equal(0, file[9]);
        Assert.Equal(0, file[10]);
        Assert.Equal(75, file[12]);
        Assert.Equal(1, file[65]);
        Assert.Equal(2, file[66]);
        Assert.Equal(1, file[68]);
        Assert.Equal(128, file.Count - 2 - 1 - 768);
        Assert.Equal(new byte[] { 1, 2, 12, 1, 2, 3 }, file.GetRange(128, 6));

        file.Clear();
        VmMisc1433Slice.WritePcx(file, new byte[] { 5, 5, 5 }, pal, VmMisc1433Slice.Pal, 3, 1, 3);
        Assert.Equal(4, file[66]);
        Assert.Equal(new byte[] { 0xC3, 5, 0 }, file.GetRange(128, 3));

        file.Clear();
        Assert.Throws<NullReferenceException>(() => VmMisc1433Slice.WritePcx(file, new byte[] { 0xC0 }, null, VmMisc1433Slice.Pal, 1, 1, 1));
        Assert.Equal(new byte[] { 0xC1, 0xC0, 0 }, file.GetRange(128, 3));
        Assert.Equal(131, file.Count);
        file.Clear();
        VmMisc1433Slice.WritePcx(file, new byte[] { 0xC0 }, pal, VmMisc1433Slice.Pal, 1, 1, 1);
        Assert.Equal(new byte[] { 0xC1, 0xC0, 0 }, file.GetRange(128, 3));

        var sixty = new byte[63];
        Array.Fill(sixty, (byte)1);
        file.Clear();
        VmMisc1433Slice.WritePcx(file, sixty, pal, VmMisc1433Slice.Pal, 63, 1, 63);
        Assert.Equal(new byte[] { 0xFF, 1 }, file.GetRange(128, 2));
        var sixtyFour = new byte[64];
        Array.Fill(sixtyFour, (byte)1);
        file.Clear();
        VmMisc1433Slice.WritePcx(file, sixtyFour, pal, VmMisc1433Slice.Pal, 64, 1, 64);
        Assert.Equal(new byte[] { 0xFF, 1, 0xC1, 1 }, file.GetRange(128, 4));

        file.Clear();
        VmMisc1433Slice.WritePcx(file, new byte[] { 10, 20, 30 }, null, VmMisc1433Slice.Rgb, 1, 1, 3);
        Assert.Equal(3, file[65]);
        Assert.Equal(2, file[66]);
        Assert.Equal(132, file.Count);
        Assert.Equal(new byte[] { 10, 20, 30, 0 }, file.GetRange(128, 4));
        file.Clear();
        VmMisc1433Slice.WritePcx(file, new byte[] { 30, 20, 10, 99 }, null, VmMisc1433Slice.Bgra, 1, 1, 4);
        Assert.Equal(new byte[] { 10, 20, 30, 0 }, file.GetRange(128, 4));

        file.Clear();
        VmMisc1433Slice.WritePcx(file, new byte[] { 7, 8, 9, 10, 11, 12 }, pal, VmMisc1433Slice.Pal, 1, 2, 3);
        Assert.Equal(1, file[10]);
        Assert.Equal(new byte[] { 7, 0, 10, 0 }, file.GetRange(128, 4));

        file.Clear();
        VmMisc1433Slice.WritePcx(file, new byte[] { 1 }, null, 9, 4, 2, 4);
        Assert.Equal(128, file.Count);
        Assert.Equal(3, file[65]);
        Assert.Equal(3, file[8]);
        Assert.Throws<NullReferenceException>(() => VmMisc1433Slice.WritePcx(null, new byte[] { 1 }, pal, 0, 1, 1, 1));

        var png = new VmMisc1433Slice.State { Version = "v", Error = "bad shot", Gamma = 1f };
        VmMisc1433Slice.WritePng(png, VmMisc1433Slice.Rgb, 8, 6, 32, 2f, pal);
        Assert.Equal(new[] { "create", "software:HCDE v", "finish" }, png.Png);
        Assert.Empty(png.Lines);
        Assert.Equal(2f, png.Gamma);
        Assert.Equal(8, png.Width);
        Assert.Same(pal, png.Palette);
        var broken = new VmMisc1433Slice.State { Version = "v", Error = "bad shot", AppendOk = false };
        VmMisc1433Slice.WritePng(broken, 0, 1, 1, 1, 1f, null);
        Assert.Equal(new[] { "create", "software:HCDE v" }, broken.Png);
        Assert.Equal("bad shot\n", Assert.Single(broken.Lines));
        var early = new VmMisc1433Slice.State { CreateOk = false, Error = "bad shot" };
        VmMisc1433Slice.WritePng(early, 0, 1, 1, 1, 1f, null);
        Assert.Equal("create", Assert.Single(early.Png));
        var late = new VmMisc1433Slice.State { FinishOk = false, Error = "bad shot", Version = new string('x', 200) };
        VmMisc1433Slice.WritePng(late, 0, 1, 1, 1, 1f, null);
        Assert.Equal(99, late.Png[1].Length - "software:".Length);
        Assert.StartsWith("software:HCDE ", late.Png[1]);
        Assert.Contains("finish", late.Png);
        Assert.Equal("bad shot\n", Assert.Single(late.Lines));
    }

    [Fact]
    public void ShotNamesFoldersAndConfig()
    {
        var when = new DateTime(2026, 11, 7, 14, 3, 5);
        var shot = new VmMisc1433Slice.State
        {
            Type = "png",
            ShotArg = "shots",
            ShotDir = "ignored",
            Game = "Doom",
            When = when,
            Buffer = new byte[] { 4 },
            Width = 1,
            Height = 1,
            Pitch = 1,
            Version = "v",
            Error = "bad shot",
        };
        VmMisc1433Slice.Shot(shot, null);
        string first = "shots/Screenshot_Doom_20261107_140305.png";
        Assert.Equal(first, Assert.Single(shot.Files.Keys));
        Assert.Equal("shots/", Assert.Single(shot.Created));
        Assert.Equal("Captured Screenshot_Doom_20261107_140305.png\n", Assert.Single(shot.Lines));
        Assert.Equal(new[] { "create", "software:HCDE v", "finish" }, shot.Png);

        shot.Files.Clear();
        shot.Lines.Clear();
        shot.Created.Clear();
        shot.Png.Clear();
        shot.Existing.Add(first);
        shot.LongNames = true;
        VmMisc1433Slice.Shot(shot, "");
        string second = "shots/Screenshot_Doom_20261107_140305_01.png";
        Assert.Equal(second, Assert.Single(shot.Files.Keys));
        Assert.Equal("Captured " + second + "\n", Assert.Single(shot.Lines));

        var clockless = new VmMisc1433Slice.State
        {
            Type = "png",
            Screenshots = "cap",
            Game = "Doom",
            Buffer = new byte[] { 1 },
            Quiet = true,
        };
        VmMisc1433Slice.Shot(clockless, null);
        Assert.Equal("cap/Screenshot_Doom_0000.png", Assert.Single(clockless.Files.Keys));
        Assert.Empty(clockless.Lines);

        var full = new VmMisc1433Slice.State { Type = "png", ShotArg = "full", Game = "Doom", When = when };
        for (int i = 0; i <= 9999; i++)
        {
            string name = i == 0
                ? "full/Screenshot_Doom_20261107_140305.png"
                : "full/Screenshot_Doom_20261107_140305_" + i.ToString("D2") + ".png";
            full.Existing.Add(name);
        }
        VmMisc1433Slice.Shot(full, null);
        Assert.Empty(full.Files);
        Assert.Equal("full/", Assert.Single(full.Created));
        Assert.Equal("M_ScreenShot: Delete some screenshots\n", Assert.Single(full.Lines));

        var pcx = new VmMisc1433Slice.State
        {
            Type = "Pcx",
            Buffer = new byte[] { 9 },
            Palette = new byte[768],
            Width = 1,
            Height = 1,
            Pitch = 1,
        };
        VmMisc1433Slice.Shot(pcx, "dir.old/shot");
        Assert.Empty(pcx.Created);
        var written = Assert.Single(pcx.Files);
        Assert.Equal("dir.old/shot.pcx", written.Key);
        Assert.Equal(9, written.Value[128]);
        Assert.Equal("Captured shot.pcx\n", Assert.Single(pcx.Lines));
        pcx.Files.Clear();
        pcx.Lines.Clear();
        VmMisc1433Slice.Shot(pcx, "shot.jpg");
        Assert.True(pcx.Files.ContainsKey("shot.jpg"));
        pcx.Files.Clear();
        VmMisc1433Slice.Shot(pcx, ".png");
        Assert.True(pcx.Files.ContainsKey(".png.pcx"));
        pcx.Files.Clear();
        pcx.Lines.Clear();
        VmMisc1433Slice.Shot(pcx, "C:/shots/a.png");
        Assert.Equal("Captured a.png\n", Assert.Single(pcx.Lines));

        var closed = new VmMisc1433Slice.State { Type = "png", Buffer = new byte[] { 1 }, FailOpen = true };
        VmMisc1433Slice.Shot(closed, "out");
        Assert.Empty(closed.Files);
        Assert.Equal("Could not open out.png\n", Assert.Single(closed.Lines));
        var empty = new VmMisc1433Slice.State { Type = "png", Quiet = true };
        VmMisc1433Slice.Shot(empty, "out");
        Assert.Empty(empty.Lines);
        empty.Quiet = false;
        VmMisc1433Slice.Shot(empty, "out");
        Assert.Equal("Could not create screenshot.\n", Assert.Single(empty.Lines));

        var bad = new VmMisc1433Slice.State
        {
            Type = "png",
            Buffer = new byte[] { 1 },
            CreateOk = false,
            Error = "bad shot",
        };
        VmMisc1433Slice.Shot(bad, "out");
        Assert.Equal(new[] { "bad shot\n", "Captured out.png\n" }, bad.Lines);

        var home = new VmMisc1433Slice.State
        {
            Type = "png",
            ShotArg = "~",
            Home = "/home/me",
            Game = "Doom",
            When = when,
            Buffer = new byte[] { 1 },
            Quiet = true,
        };
        VmMisc1433Slice.Shot(home, null);
        Assert.Equal("/home/me/", Assert.Single(home.Created));
        Assert.StartsWith("/home/me/Screenshot_Doom_", Assert.Single(home.Files.Keys));

        var command = new VmMisc1433Slice.State();
        VmMisc1433Slice.Command(command, new[] { "screenshot" });
        VmMisc1433Slice.Command(command, new[] { "screenshot", "pic", "extra" });
        Assert.Null(command.Requested[0]);
        Assert.Equal("pic", command.Requested[1]);

        var folder = new VmMisc1433Slice.State { ShotArg = "pics" };
        VmMisc1433Slice.OpenShots(folder);
        Assert.Equal("pics/", folder.Opened);
        Assert.Equal("pics/", Assert.Single(folder.Created));
        folder.ShotArg = "pics/";
        folder.Created.Clear();
        VmMisc1433Slice.OpenShots(folder);
        Assert.Equal("pics/", folder.Opened);

        var wads = new VmMisc1433Slice.State { AppData = "save" };
        VmMisc1433Slice.OpenWads(wads);
        Assert.Equal("save/", wads.Opened);
        wads.AppData = null;
        VmMisc1433Slice.OpenWads(wads);
        Assert.Equal("", wads.Opened);

        var kept = new VmMisc1433Slice.State { ConfigMissing = true, WriteOk = false };
        Assert.Equal(1, VmMisc1433Slice.SaveConfig(kept));
        Assert.Equal(1, VmMisc1433Slice.SaveAction(kept));
        kept.ConfigMissing = false;
        Assert.Equal(0, VmMisc1433Slice.SaveConfig(kept));
        Assert.Equal(0, VmMisc1433Slice.SaveAction(kept));
        Assert.Throws<NullReferenceException>(() => VmMisc1433Slice.Shot(new VmMisc1433Slice.State { Type = null }, "x"));
    }
}
