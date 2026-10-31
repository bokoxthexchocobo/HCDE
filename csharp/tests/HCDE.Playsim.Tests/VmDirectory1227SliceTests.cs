using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDirectory1227SliceTests
{
    [Fact]
    public void Add_SkipsBackupsAndKeepsTheRelativePath()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- read\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("bak [foo.orig]\n", head, StringComparison.Ordinal);
        Assert.Contains("bak [my.original]\n", head, StringComparison.Ordinal);
        Assert.Contains("bak [foo.cachebar]\n", head, StringComparison.Ordinal);
        Assert.Contains("reject [secret.wad] [/Game/Secret.wad]\n", head, StringComparison.Ordinal);
        Assert.Contains("msg 2 /Game/huge.bin is larger than 2GB and will be ignored\n", head, StringComparison.Ordinal);
        Assert.Contains("add pos=3 rel=[backup.orig/file.txt] norm=[backup.orig/file.txt] len=6 comp=6 flags=2 id=-1 method=0 ns=0\n", head, StringComparison.Ordinal);
        Assert.Contains("dir [Maps]\n", head, StringComparison.Ordinal);
        Assert.Contains("order=[a\\xc4\\xb0z\\xff.txt,backup.orig/file.txt,cache,edge.bin,gone.dat,maps/map.wad,notes.txt]\n", head, StringComparison.Ordinal);
        Assert.Contains("check keep=1 nosubdir=1 lumps=0\n", head, StringComparison.Ordinal);
        Assert.Contains("ctor file=[/Game\\x5c/] nosubdir=1\n", head, StringComparison.Ordinal);
        Assert.Equal(AddGolden, head);
    }

    [Fact]
    public void Read_JoinsTheBaseAfterTheNameSort()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- read\n", StringComparison.Ordinal)..];
        Assert.Contains("reader entry=0 type=2 path=[/Game/A\\xc4\\xb0Z\\xff.txt] open=1 cached=1 data=51\n", tail, StringComparison.Ordinal);
        Assert.Contains("reader entry=5 type=1 path=[/Game/Maps/Map.WAD] open=1 cached=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("reader entry=4 type=1 path=[/Game/gone.dat] open=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("reader entry=4294967295 type=0 empty\n", tail, StringComparison.Ordinal);
        Assert.Contains("kept entries=7 lumps=0 hasbase=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("reader entry=0 type=1 path=[/elsewhere/filenope] open=1 cached=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("base=[]\n", tail, StringComparison.Ordinal);
        Assert.Contains("reader entry=0 type=1 path=[/data/maps/maps/a.txt] open=1 cached=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("add pos=0 rel=[Keep.Me] norm=[keep.me] len=4 comp=4 flags=2 id=-1 method=0 ns=0\n", tail, StringComparison.Ordinal);
        Assert.Equal(ReadGolden, tail);
    }

    const string AddGolden = """
        ctor file=[missing/] nosubdir=1
        scan dir=[missing/] match=[*]
        msg 1 Could not scan 'missing/': No such file or directory
        hash no_reader=1 lumps=0
        post lumps=0
        order=[]
        open ok=1 lumps=0
        check keep=1 nosubdir=1 lumps=0
        ctor file=[/empty/] nosubdir=0
        scan dir=[/empty/] match=[*]
        alloc n=0
        hash no_reader=1 lumps=0
        post lumps=0
        order=[]
        open ok=1 lumps=0
        check keep=1 nosubdir=0 lumps=0
        ctor file=[/Game\x5c/] nosubdir=1
        scan dir=[/Game\x5c/] match=[*]
        alloc n=0
        hash no_reader=1 lumps=0
        post lumps=0
        order=[]
        open ok=1 lumps=0
        check keep=1 nosubdir=1 lumps=0
        ctor file=[/Game/] nosubdir=0
        scan dir=[/Game/] match=[*]
        alloc n=15
        base=[/Game/]
        dir [Maps]
        add pos=0 rel=[Maps/Map.WAD] norm=[maps/map.wad] len=12 comp=12 flags=2 id=-1 method=0 ns=0
        bak [foo.orig]
        bak [my.original]
        bak [x.bak]
        bak [foo.cachebar]
        add pos=1 rel=[notes.txt] norm=[notes.txt] len=4 comp=4 flags=2 id=-1 method=0 ns=0
        reject [secret.wad] [/Game/Secret.wad]
        msg 2 /Game/huge.bin is larger than 2GB and will be ignored
        add pos=2 rel=[edge.bin] norm=[edge.bin] len=2147483647 comp=2147483647 flags=2 id=-1 method=0 ns=0
        add pos=3 rel=[backup.orig/file.txt] norm=[backup.orig/file.txt] len=6 comp=6 flags=2 id=-1 method=0 ns=0
        add pos=4 rel=[A\xc4\xb0Z\xff.txt] norm=[a\xc4\xb0z\xff.txt] len=8 comp=8 flags=2 id=-1 method=0 ns=0
        add pos=5 rel=[cache] norm=[cache] len=2 comp=2 flags=2 id=-1 method=0 ns=0
        bak [.cache]
        add pos=6 rel=[gone.dat] norm=[gone.dat] len=3 comp=3 flags=2 id=-1 method=0 ns=0
        hash no_reader=1 lumps=7
        post lumps=7
        order=[a\xc4\xb0z\xff.txt,backup.orig/file.txt,cache,edge.bin,gone.dat,maps/map.wad,notes.txt]
        open ok=1 lumps=7
        check keep=1 nosubdir=0 lumps=7

        """;

    const string ReadGolden = """
        ---- read
        reader entry=0 type=2 path=[/Game/A\xc4\xb0Z\xff.txt] open=1 cached=1 data=51
        reader entry=6 type=2 path=[/Game/notes.txt] open=1 cached=1 data=4e4f5445
        reader entry=5 type=0 path=[/Game/Maps/Map.WAD] open=1 cached=0
        reader entry=5 type=1 path=[/Game/Maps/Map.WAD] open=1 cached=0
        reader entry=5 type=1 path=[/Game/Maps/Map.WAD] open=1 cached=0
        reader entry=4 type=1 path=[/Game/gone.dat] open=0
        reader entry=3 type=1 path=[/Game/edge.bin] open=1 cached=0
        reader entry=7 type=2 empty
        reader entry=4294967295 type=0 empty
        scan dir=[/Game/] match=[*]
        msg 1 Could not scan '/Game/': Permission denied
        hash no_reader=1 lumps=0
        post lumps=0
        order=[]
        open ok=1 lumps=0
        kept entries=7 lumps=0 hasbase=1
        ---- rfind
        ctor file=[/elsewhere/] nosubdir=0
        scan dir=[/elsewhere/] match=[*]
        alloc n=1
        base=[/elsewhere/file]
        add pos=0 rel=[nope] norm=[nope] len=3 comp=3 flags=2 id=-1 method=0 ns=0
        hash no_reader=1 lumps=1
        post lumps=1
        order=[nope]
        open ok=1 lumps=1
        check keep=1 nosubdir=0 lumps=1
        reader entry=0 type=1 path=[/elsewhere/filenope] open=1 cached=0
        ctor file=[data/] nosubdir=0
        scan dir=[data/] match=[*]
        alloc n=1
        base=[/data/maps/]
        add pos=0 rel=[maps/a.txt] norm=[maps/a.txt] len=5 comp=5 flags=2 id=-1 method=0 ns=0
        hash no_reader=1 lumps=1
        post lumps=1
        order=[maps/a.txt]
        open ok=1 lumps=1
        check keep=1 nosubdir=0 lumps=1
        reader entry=0 type=1 path=[/data/maps/maps/a.txt] open=1 cached=0
        ctor file=[root/] nosubdir=0
        scan dir=[root/] match=[*]
        alloc n=1
        base=[]
        add pos=0 rel=[a.txt] norm=[a.txt] len=1 comp=1 flags=2 id=-1 method=0 ns=0
        hash no_reader=1 lumps=1
        post lumps=1
        order=[a.txt]
        open ok=1 lumps=1
        check keep=1 nosubdir=0 lumps=1
        reader entry=0 type=1 path=[a.txt] open=1 cached=0
        ctor file=[blank/] nosubdir=0
        scan dir=[blank/] match=[*]
        alloc n=1
        base=[/game/a]
        add pos=0 rel=[] norm=[] len=1 comp=1 flags=2 id=-1 method=0 ns=0
        hash no_reader=1 lumps=1
        post lumps=1
        order=[]
        open ok=1 lumps=1
        check keep=1 nosubdir=0 lumps=1
        reader entry=0 type=1 path=[/game/a] open=1 cached=0
        ctor file=[/plain/] nosubdir=0
        scan dir=[/plain/] match=[*]
        alloc n=1
        base=[/plain/]
        add pos=0 rel=[Keep.Me] norm=[keep.me] len=4 comp=4 flags=2 id=-1 method=0 ns=0
        hash no_reader=1 lumps=1
        post lumps=1
        order=[keep.me]
        open ok=1 lumps=1
        check keep=1 nosubdir=0 lumps=1
        reader entry=0 type=1 path=[/plain/Keep.Me] open=1 cached=0
        ctor file=[/plain2/] nosubdir=0
        scan dir=[/plain2/] match=[*]
        alloc n=1
        base=[/plain2/]
        add pos=0 rel=[Also.Me] norm=[also.me] len=5 comp=5 flags=2 id=-1 method=0 ns=0
        hash no_reader=1 lumps=1
        post lumps=1
        order=[also.me]
        open ok=1 lumps=1
        check keep=1 nosubdir=0 lumps=1
        reader entry=0 type=1 path=[/plain2/Also.Me] open=1 cached=0

        """;

    static string Fold(string rel)
    {
        var chars = rel.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (c >= 'A' && c <= 'Z')
                chars[i] = (char)(c + 32);
        }
        return new string(chars);
    }

    static VmDirScanEntry1227 Ent(string name, string path, string rel, ulong len, bool dir = false) =>
        new() { FileName = name, FilePath = path, FilePathRel = rel, Length = len, IsDirectory = dir };

    public static string Build()
    {
        var lines = new List<string>();
        void Flush(VmDirectory1227? dir)
        {
            foreach (string line in dir!.Log)
                lines.Add(line);
            dir.Log.Clear();
        }

        var hooks = new VmDirHooks1227
        {
            Normalize = Fold,
            AfterPost = dir =>
            {
                var live = new List<VmDirLump1227>();
                for (int i = 0; i < dir.NumLumps; i++)
                    live.Add(dir.Entries[i]);
                live.Sort((a, b) => string.CompareOrdinal(a.FileName, b.FileName));
                for (int i = 0; i < live.Count; i++)
                    dir.Entries[i] = live[i];
                linesOrder(dir, live);
            },
            OpenBytes = OpenBytes,
        };
        void linesOrder(VmDirectory1227 dir, List<VmDirLump1227> live)
        {
            dir.Log.Add("order=[" + string.Join(",", live.ConvertAll(e => VmDirectory1227.Esc(e.FileName))) + "]");
        }

        hooks.ScanErrno = 2;
        hooks.Scan = (dir, match) => null;
        Flush(VmDirectory1227.Check("missing", true, null, hooks));

        hooks.ScanErrno = 0;
        hooks.Scan = (dir, match) => new List<VmDirScanEntry1227>();
        Flush(VmDirectory1227.Check("/empty", false, null, hooks));

        hooks.FullPath = path => path == "win" ? "/Game\\" : path;
        Flush(VmDirectory1227.Check("win", true, null, hooks));
        hooks.FullPath = static path => path;

        string high = "A\u00C4\u00B0Z\u00FF.txt";
        hooks.Scan = (dir, match) => new List<VmDirScanEntry1227>
        {
            Ent("Maps", "/Game/Maps", "Maps", 0, true),
            Ent("Map.WAD", "/Game/Maps/Map.WAD", "Maps/Map.WAD", 12),
            Ent("FOO.ORIG", "/Game/FOO.ORIG", "FOO.ORIG", 1),
            Ent("my.original", "/Game/my.original", "my.original", 1),
            Ent("X.BAK", "/Game/X.BAK", "X.BAK", 1),
            Ent("foo.cachebar", "/Game/foo.cachebar", "foo.cachebar", 1),
            Ent("notes.txt", "/Game/notes.txt", "notes.txt", 4),
            Ent("Secret.wad", "/Game/Secret.wad", "Secret.wad", 9),
            Ent("huge.bin", "/Game/huge.bin", "huge.bin", 0x80000000UL),
            Ent("edge.bin", "/Game/edge.bin", "edge.bin", 0x7fffffffUL),
            Ent("file.txt", "/Game/backup.orig/file.txt", "backup.orig/file.txt", 6),
            Ent(high, "/Game/" + high, high, 8),
            Ent("cache", "/Game/cache", "cache", 2),
            Ent(".cache", "/Game/.cache", ".cache", 2),
            Ent("gone.dat", "/Game/gone.dat", "gone.dat", 3),
        };
        var richFilter = new VmDirFilter1227 { FileNameCheck = (name, path) => name != "secret.wad" };
        VmDirectory1227 rich = VmDirectory1227.Check("/Game", false, richFilter, hooks)!;
        Flush(rich);
        lines.Add("---- read");
        int Find(string name)
        {
            for (int i = 0; i < rich.NumLumps; i++)
                if (rich.Entries[i].FileName == name)
                    return i;
            return -1;
        }
        uint U(int i) => (uint)i;
        rich.GetEntryReader(U(Find(Fold(high))), 2, 0);
        rich.GetEntryReader(U(Find("notes.txt")), 2, 7);
        rich.GetEntryReader(U(Find("maps/map.wad")), 0, 0);
        rich.GetEntryReader(U(Find("maps/map.wad")), 1, 0);
        rich.GetEntryReader(U(Find("maps/map.wad")), 1, 99);
        rich.GetEntryReader(U(Find("gone.dat")), 1, 3);
        rich.GetEntryReader(U(Find("edge.bin")), 1, 0);
        rich.GetEntryReader(7, 2, 0);
        rich.GetEntryReader(uint.MaxValue, 0, 5);
        Flush(rich);
        hooks.ScanErrno = 13;
        hooks.Scan = (dir, match) => null;
        rich.Open(richFilter);
        Flush(rich);
        lines.Add("kept entries=" + rich.Entries.Count + " lumps=" + rich.NumLumps + " hasbase=" + (rich.HasBase ? 1 : 0));
        lines.Add("---- rfind");

        void One(string filename, VmDirFilter1227? filter, VmDirScanEntry1227 entry)
        {
            hooks.Scan = (dir, match) => new List<VmDirScanEntry1227> { entry };
            VmDirectory1227 d = VmDirectory1227.Check(filename, false, filter, hooks)!;
            d.GetEntryReader(0, 1, 0);
            Flush(d);
        }
        One("/elsewhere", null, Ent("nope", "/elsewhere/file", "nope", 3));
        One("data", null, Ent("a.txt", "/data/maps/maps/a.txt", "maps/a.txt", 5));
        One("root", null, Ent("a.txt", "a.txt", "a.txt", 1));
        One("blank", null, Ent("", "/game/a", "", 1));
        One("/plain", null, Ent("Keep.Me", "/plain/Keep.Me", "Keep.Me", 4));
        One("/plain2", new VmDirFilter1227(), Ent("Also.Me", "/plain2/Also.Me", "Also.Me", 5));
        return string.Join("\n", lines) + "\n";
    }

    static byte[]? OpenBytes(string path)
    {
        if (path.EndsWith("gone.dat", StringComparison.Ordinal))
            return null;
        if (path.EndsWith("notes.txt", StringComparison.Ordinal))
            return "NOTE"u8.ToArray();
        string high = "A\u00C4\u00B0Z\u00FF.txt";
        if (path.EndsWith(high, StringComparison.Ordinal))
            return "Q"u8.ToArray();
        return "OK"u8.ToArray();
    }
}
