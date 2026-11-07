using S = HCDE.Playsim.VmScanner1395Slice;

namespace HCDE.Playsim.Tests;

public class VmScanner1395SliceTests
{
    [Fact]
    public void AssignVersionMatchesStrtoullThenTheUnsignedStore()
    {
        Expect("1.2.3", 1, 2, 3);
        Expect("1.2", 1, 2, 0);
        Expect("1", 1, 0, 0);
        Expect("1.2.3x", 65535, 2, 3);
        Expect("1.2x", 65535, 2, 9);
        Expect("1x", 65535, 8, 9);
        Expect("", 0, 0, 0);
        Expect(" 12", 12, 0, 0);
        Expect("65535", 65535, 0, 0);
        Expect("65536", 65535, 0, 0);
        Expect("32768", 32768, 0, 0);
        Expect("-1", 65535, 0, 0);
        Expect("1.", 1, 0, 0);
        Expect("1..", 1, 0, 0);
        Expect("1.2.", 1, 2, 0);
        Expect("99999999999999999999999", 65535, 0, 0);
        Expect("-1.2.3", 65535, 2, 3);
        Expect("+4.5.6", 4, 5, 6);
        Expect("  8.9.10x", 65535, 9, 10);
        Expect(".2", 0, 2, 0);
        Expect(" .5", 65535, 8, 9);
        Expect("-0", 0, 0, 0);
        Expect("-0.1", 0, 1, 0);
        Expect("0.0.32768", 0, 0, 4294934528);
        Expect("0.0.65535", 0, 0, 4294967295);
        Expect("32768.1.2x", 65535, 1, 2);
        Expect("1.2.3 ", 65535, 2, 3);
        Expect("\t3", 3, 0, 0);
        Expect("-", 65535, 8, 9);
        Expect("++1", 65535, 8, 9);
        Expect("1.x", 65535, 0, 9);
        Expect("   ", 65535, 8, 9);
        Expect("\v2.3", 2, 3, 0);
        Assert.Equal(171, S.OpenStringLine);
        Assert.Equal(188, S.OpenLumpLine);
        Assert.Equal(65535, S.VersionMax);
    }

    [Fact]
    public void OpenPathsCallTheLumpFileAndStringCallees()
    {
        var shared = new Dictionary<string, int>();
        S.Scanner scanner = S.Create(shared);
        Assert.False(scanner.ScriptOpen);
        Assert.Same(shared, scanner.Symbols);
        scanner.Symbols["door"] = 4;
        Assert.Equal(4, shared["door"]);

        S.Scanner own = S.Create(null);
        Assert.Empty(own.Symbols);
        own.Symbols["local"] = 1;
        Assert.Equal(1, own.Symbols["local"]);

        bool sawClosed = false;
        S.Scanner fromLump = S.CreateForLump(12, shared, (opened, lump) =>
        {
            sawClosed = opened.ScriptOpen == false && lump == 12;
            opened.ScriptOpen = true;
        });
        Assert.True(sawClosed);
        Assert.True(fromLump.ScriptOpen);
        Assert.Same(shared, fromLump.Symbols);

        int literals = 0;
        var miss = Assert.Throws<InvalidOperationException>(() =>
            S.Open(scanner, "missing.txt", (name, full) =>
            {
                Assert.Equal("missing.txt", name);
                literals = full ? 1 : 0;
                return -1;
            }, (_, _) => throw new InvalidOperationException("opened")));
        Assert.Equal(S.MissingLump + "missing.txt'\n", miss.Message);
        Assert.Equal(1, literals);

        int openedLump = -2;
        S.Open(scanner, "mapinfo.txt", (_, full) => full ? 9 : -1, (_, lump) => openedLump = lump);
        Assert.Equal(9, openedLump);

        scanner.ScriptBuffer = "old";
        Assert.False(S.OpenFile(scanner, "nope", _ => null));
        Assert.Equal(1, scanner.Closes);
        Assert.Equal(0, scanner.Prepares);
        Assert.Equal("old", scanner.ScriptBuffer);

        Assert.False(S.OpenFile(scanner, "empty", _ => new S.FileBytes(4, "")));
        Assert.Equal(2, scanner.Closes);
        Assert.Equal(0, scanner.Prepares);

        Assert.True(S.OpenFile(scanner, "/tmp/script", _ => new S.FileBytes(4, "abcdEF")));
        Assert.Equal("abcd", scanner.ScriptBuffer);
        Assert.Equal("/tmp/script", scanner.ScriptName);
        Assert.Equal(-1, scanner.LumpNum);
        Assert.Equal(1, scanner.Prepares);

        Assert.True(S.OpenFile(scanner, "zero", _ => new S.FileBytes(0, "")));
        Assert.Equal("", scanner.ScriptBuffer);
        Assert.Equal(2, scanner.Prepares);

        scanner.ScriptBuffer = "kept";
        string? copied = null;
        S.OpenMem(scanner, "mem", "abcdef", 4, (name, text) => copied = name + ":" + text);
        Assert.Equal("mem:abcd", copied);
        Assert.Equal("kept", scanner.ScriptBuffer);
        S.OpenMem(scanner, "mem", null, 0, (_, text) => copied = text);
        Assert.Equal("", copied);
        Assert.Equal(211, S.PrepareLine);
        Assert.Equal(261, S.CloseLine);
    }

    private static void Expect(string text, ushort major, ushort minor, uint revision)
    {
        var version = new S.Version(7, 8, 9);
        S.Assign(ref version, text);
        Assert.Equal(major, version.Major);
        Assert.Equal(minor, version.Minor);
        Assert.Equal(revision, version.Revision);
    }
}
