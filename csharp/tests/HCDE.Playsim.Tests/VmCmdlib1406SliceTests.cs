using S = HCDE.Playsim.VmCmdlib1406Slice;

namespace HCDE.Playsim.Tests;

public class VmCmdlib1406SliceTests
{
    [Fact]
    public void CopyStringKeepsTheTerminatorAndProbesReadTheModeAfterStat()
    {
        Assert.Equal(new[] { '\0' }, S.CopyString(null));
        Assert.Equal(new[] { '\0' }, S.CopyString(""));
        Assert.Equal(new[] { 'a', 'b', '\0' }, S.CopyString("ab"));
        Assert.Equal(new[] { 'a', 'b', '\0' }, S.CopyString("ab\0cd"));
        Assert.Equal(92, S.BandStart);
        Assert.Equal(241, S.BandEnd);
        Assert.Equal(0x4000, S.IfDir);
        Assert.Equal(4, S.ReadMode);
        Assert.Equal(242, S.GetFileInfoStart);

        int stats = 0;
        int wides = 0;
        Assert.False(S.DirEntryExists(null, true, false, _ => throw new InvalidOperationException("wide"), _ =>
        {
            stats++;
            return new S.StatResult(true, S.IfDir);
        }, out bool isDirectory));
        Assert.False(isDirectory);
        Assert.Equal(0, stats);

        Assert.False(S.DirEntryExists("", true, true, _ =>
        {
            wides++;
            return "";
        }, _ => throw new InvalidOperationException("stat"), out isDirectory));
        Assert.False(isDirectory);
        Assert.Equal(0, wides);

        Assert.True(S.DirEntryExists("dir", false, false, _ => throw new InvalidOperationException("wide"), query =>
        {
            Assert.Equal("dir", query.Path);
            Assert.False(query.Wide);
            return new S.StatResult(true, S.IfDir | 0x1ED);
        }, out isDirectory));
        Assert.False(isDirectory);

        Assert.True(S.FileExists("note.txt", false, null, query =>
        {
            Assert.Equal("note.txt", query.Path);
            Assert.False(query.Wide);
            return new S.StatResult(true, 0x81A4);
        }));
        Assert.False(S.DirExists("note.txt", false, null, _ => new S.StatResult(true, 0x81A4)));
        Assert.False(S.FileExists("dir", false, null, _ => new S.StatResult(true, S.IfDir | 0x1ED)));
        Assert.True(S.DirExists("dir", false, null, _ => new S.StatResult(true, S.IfDir | 0x1ED)));

        Assert.False(S.DirEntryExists("gone", true, false, null, _ => new S.StatResult(false, S.IfDir), out isDirectory));
        Assert.True(isDirectory);
        Assert.False(S.FileExists("gone", false, null, _ => new S.StatResult(false, S.IfDir)));
        Assert.False(S.DirExists("gone", false, null, _ => new S.StatResult(false, 0)));

        Assert.True(S.DirEntryExists("café", true, true, path =>
        {
            Assert.Equal("café", path);
            return "WIDE";
        }, query =>
        {
            Assert.Equal("WIDE", query.Path);
            Assert.True(query.Wide);
            return new S.StatResult(true, 0x81A4);
        }, out isDirectory));
        Assert.False(isDirectory);

        S.AccessQuery? seen = null;
        Assert.True(S.FileReadable(null, false, _ => throw new InvalidOperationException("wide"), query =>
        {
            seen = query;
            return 0;
        }));
        Assert.Null(seen!.Value.Path);
        Assert.Equal(S.ReadMode, seen.Value.Mode);
        Assert.False(seen.Value.Wide);
        Assert.False(S.FileReadable("locked", false, null, _ => -1));

        Assert.True(S.FileReadable(null, true, path =>
        {
            Assert.Null(path);
            return "";
        }, query =>
        {
            Assert.Equal("", query.Path);
            Assert.Equal(S.ReadMode, query.Mode);
            Assert.True(query.Wide);
            return 0;
        }));
        Assert.False(S.FileReadable("", false, null, query =>
        {
            Assert.Equal("", query.Path);
            return 1;
        }));
    }

    [Fact]
    public void RecursiveSearchChecksTheBaseThenVisibleDirectories()
    {
        var scanned = new List<S.ScanQuery>();
        var probed = new List<string>();
        S.StatResult Stat(S.StatQuery query)
        {
            probed.Add(query.Path);
            return new S.StatResult(query.Path == "base/a.txt", 0x81A4);
        }

        Assert.Equal("base/a.txt", S.RecursiveFileExists("base", "a.txt", false, null, Stat, query =>
        {
            scanned.Add(query);
            throw new InvalidOperationException("scan");
        }));
        Assert.Empty(scanned);
        Assert.Equal("base/a.txt", Assert.Single(probed));
        probed.Clear();

        Assert.Equal("", S.RecursiveFileExists("base", "a.txt", false, null, _ => new S.StatResult(false, 0), query =>
        {
            scanned.Add(query);
            return new S.ScanResult(false, Array.Empty<S.Listed>());
        }));
        S.ScanQuery miss = Assert.Single(scanned);
        Assert.Equal("base", miss.Path);
        Assert.Equal("*", miss.Match);
        Assert.False(miss.NoSubdir);
        Assert.False(miss.ReadHidden);
        scanned.Clear();

        var entries = new[]
        {
            new S.Listed("hid", true, true, false),
            new S.Listed("sys", true, false, true),
            new S.Listed("note", false, false, false),
            new S.Listed("sub", true, false, false),
            new S.Listed("later", true, false, false),
        };
        probed.Clear();
        Assert.Equal("sub/a.txt", S.RecursiveFileExists("base", "a.txt", false, null, query =>
        {
            probed.Add(query.Path);
            bool found = query.Path == "sub/a.txt";
            return new S.StatResult(found, found ? 0x81A4 : S.IfDir);
        }, query =>
        {
            scanned.Add(query);
            return new S.ScanResult(true, entries);
        }));
        Assert.Equal(new[] { "base/a.txt", "sub/a.txt" }, probed);
        Assert.Equal("base", Assert.Single(scanned).Path);

        probed.Clear();
        scanned.Clear();
        Assert.Equal("later/a.txt", S.RecursiveFileExists("base", "a.txt", true, path => "W:" + path, query =>
        {
            probed.Add(query.Path);
            Assert.True(query.Wide);
            bool found = query.Path == "W:later/a.txt";
            int mode = query.Path == "W:sub/a.txt" ? S.IfDir : 0x81A4;
            return new S.StatResult(found || query.Path == "W:sub/a.txt", mode);
        }, query =>
        {
            Assert.Equal("base", query.Path);
            Assert.Equal("*", query.Match);
            Assert.False(query.NoSubdir);
            Assert.False(query.ReadHidden);
            return new S.ScanResult(true, entries);
        }));
        Assert.Equal(new[] { "W:base/a.txt", "W:sub/a.txt", "W:later/a.txt" }, probed);

        Assert.Equal("", S.RecursiveFileExists("base", "a.txt", false, null, _ => new S.StatResult(false, 0), _ =>
            new S.ScanResult(true, Array.Empty<S.Listed>())));
        Assert.Throws<ArgumentNullException>(() => S.RecursiveFileExists(null!, "a.txt", false, null, _ => new S.StatResult(false, 0), _ =>
            new S.ScanResult(false, Array.Empty<S.Listed>())));
        Assert.Throws<ArgumentNullException>(() => S.RecursiveFileExists("base", "a.txt", false, null, _ => new S.StatResult(false, 0), null!));
    }
}
