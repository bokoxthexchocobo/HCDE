using S = HCDE.Playsim.VmCmdlib1393Slice;

namespace HCDE.Playsim.Tests;

public class VmCmdlib1393SliceTests
{
    [Fact]
    public void FormatGuidWritesLowerHexAndAscTimeUsesTheStaticBuffer()
    {
        var guid = new S.GuidParts(0x0A1B2C3D, 0x04E5, 0x0678, 0x9A, 0xBC, 0xDE, 0xF0, 0x11, 0x22, 0x33, 0x44);
        const string full = "{0a1b2c3d-04e5-0678-9abc-def011223344}";
        Assert.Equal(38, full.Length);
        Assert.Equal(full, S.FormatGuid(guid, 39));
        Assert.Equal(full.Substring(0, 9), S.FormatGuid(guid, 10));
        Assert.Equal("", S.FormatGuid(guid, 1));
        Assert.Equal("", S.FormatGuid(guid, 0));
        Assert.Equal(646, S.StrbinStart);
        Assert.Equal("int strbin (char *str)", S.StrbinSignature);
        Assert.Equal("%F %T", S.AscTimeFormat);

        Assert.Equal("Unknown\n", S.MyAscTime(null));
        Assert.Equal("Unknown\n", S.ReadableTime);
        var stamp = new S.LocalStamp(2026, 11, 7, 7, 3, 4);
        Assert.Equal("2026-11-07 07:03:04", S.MyAscTime(stamp));
        Assert.Equal("2026-11-07 07:03:04", S.ReadableTime);
        Assert.Equal("Unknown\n", S.MyAscTime(null));
        Assert.Equal("Unknown\n", S.ReadableTime);
    }

    [Fact]
    public void CreatePathBuildsParentsAndRemoveUsesThePlatformCall()
    {
        var made = new List<(string Path, int Mode)>();
        int Mk(string path, int mode)
        {
            made.Add((path, mode));
            return 0;
        }

        S.CreatePath("/", false, _ => false, Mk);
        Assert.Empty(made);
        S.CreatePath("a/b", false, path => path == "a", Mk);
        Assert.Equal(("a/b", S.MkdirMode), Assert.Single(made));
        made.Clear();

        S.CreatePath("a/b/c", false, _ => false, (path, mode) =>
        {
            made.Add((path, mode));
            if (path == "a/b")
                return -1;
            return 0;
        });
        Assert.Equal("a", made[0].Path);
        Assert.Equal(S.MkdirMode, made[0].Mode);
        Assert.Equal("a/b", made[1].Path);
        Assert.Equal(S.MkdirMode, made[1].Mode);
        Assert.Equal(2, made.ToArray().Length);
        made.Clear();

        S.CreatePath("a/b/", false, _ => false, Mk);
        Assert.Equal("a", made[0].Path);
        Assert.Equal("a/b", made[1].Path);
        Assert.Equal("a/b/", made[2].Path);
        made.Clear();

        S.CreatePath(@"c:\foo\bar", true, _ => throw new InvalidOperationException("exists"), Mk);
        Assert.Equal("c:", made[0].Path);
        Assert.Equal(-1, made[0].Mode);
        Assert.Equal(@"c:\foo", made[1].Path);
        Assert.Equal(-1, made[1].Mode);
        Assert.Equal(@"c:\foo\bar", made[2].Path);
        Assert.Equal(-1, made[2].Mode);
        made.Clear();

        S.CreatePath("file.txt", true, _ => false, Mk);
        Assert.Equal(("file.txt", -1), Assert.Single(made));
        Assert.Throws<ArgumentException>(() => S.CreatePath("", true, _ => false, Mk));

        S.FsCall? called = null;
        S.RemoveFile("save.dat", false, call => called = call);
        Assert.Equal("save.dat", called!.Value.Path);
        Assert.False(called.Value.Wide);
        S.RemoveFile("save.dat", true, call => called = call);
        Assert.True(called!.Value.Wide);
        int rc = S.RemoveDir("maps", false, call =>
        {
            called = call;
            return 0;
        });
        Assert.Equal(0, rc);
        Assert.False(called!.Value.Wide);
        rc = S.RemoveDir("maps", true, call =>
        {
            called = call;
            return -1;
        });
        Assert.Equal(-1, rc);
        Assert.True(called!.Value.Wide);
        Assert.Equal(493, S.MkdirMode);
    }
}
