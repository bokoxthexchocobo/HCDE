using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmEngineCmds1235SliceTests
{
    [Fact]
    public void Quit_PrintsAndStopsTheLog()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("endoom\n", head, StringComparison.Ordinal);
        Assert.Contains("throw exit code=0\n", head, StringComparison.Ordinal);
        Assert.Contains("print 2 HCDE 1.0 @ 2026-10-29\n", head, StringComparison.Ordinal);
        Assert.Contains("print 2 Commit abc\n", head, StringComparison.Ordinal);
        Assert.Contains("print 2  unknown\n", head, StringComparison.Ordinal);
        Assert.Contains("print 2 MISSING unknown\n", head, StringComparison.Ordinal);
        Assert.Contains("print 2 hi\n", head, StringComparison.Ordinal);
        Assert.Contains("exec a.cfg ok=1\n", head, StringComparison.Ordinal);
        Assert.Contains("exec b.cfg ok=0\n", head, StringComparison.Ordinal);
        Assert.Contains("print 2 Could not exec \"b.cfg\"\n", head, StringComparison.Ordinal);
        Assert.Contains("open game.log append=0\n", head, StringComparison.Ordinal);
        Assert.Contains("open game.log append=1\n", head, StringComparison.Ordinal);
        Assert.Contains("print 2 Could not start log\n", head, StringComparison.Ordinal);
        Assert.Contains("ierror boom\n", head, StringComparison.Ordinal);
        Assert.Contains("ifatal die\n", head, StringComparison.Ordinal);
        Assert.Contains("crashout fault addr=0\n", head, StringComparison.Ordinal);
        Assert.Equal(2, Count(head, "endoom\n"));
        Assert.Equal(2, Count(head, "throw exit code=0\n"));
        Assert.DoesNotContain("exec c.cfg", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Dir_ListsWildcardsAndHashesFiles()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("cwd /cwd\n", tail, StringComparison.Ordinal);
        Assert.Contains("scan bpath=/wads/ pat=*.wad recurse=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("print 200 maps <dir>\n", tail, StringComparison.Ordinal);
        Assert.Contains("scan bpath=/wads/doom.wad pat=* recurse=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("scan bpath= pat=a\\b*.txt recurse=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("scan bpath=/home/me/maps pat=* recurse=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("print 2 E1M1\n", tail, StringComparison.Ordinal);
        Assert.Contains("scan bpath=~bob pat=* recurse=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("scan bpath=/x/ pat=foo?.txt recurse=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("print 2         12 MAP01\n", tail, StringComparison.Ordinal);
        Assert.Contains("wad missing.wad num=-1\n", tail, StringComparison.Ordinal);
        Assert.Contains("print 2 missing.wad must be loaded to view its directory.\n", tail, StringComparison.Ordinal);
        Assert.Contains("print 2        100 DEMO1\n", tail, StringComparison.Ordinal);
        Assert.Contains("print 2          3 MAP02\n", tail, StringComparison.Ordinal);
        Assert.Contains("print 2 d41d8cd98f00b204e9800998ecf8427e *empty.txt\n", tail, StringComparison.Ordinal);
        Assert.Contains("read n=8192\n", tail, StringComparison.Ordinal);
        Assert.Contains("read n=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("print 2 900150983cd24fb0d6963f7d28e17f72 *abc.txt\n", tail, StringComparison.Ordinal);
        Assert.Contains("print 2 b7652d9bca37038c342cc0c492dd70f9 *wide.txt\n", tail, StringComparison.Ordinal);
        Assert.Contains("bounds index=2 len=2\n", tail, StringComparison.Ordinal);
        Assert.Contains("lang id=005f6e65\n", tail, StringComparison.Ordinal);
        Assert.Contains("print 2 hello-en\n", tail, StringComparison.Ordinal);
        Assert.Contains("lang id=00676e65\n", tail, StringComparison.Ordinal);
        Assert.Contains("lang null\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("print 2 MAP01\n", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static int Count(string hay, string needle)
    {
        int n = 0;
        int i = 0;
        while (true)
        {
            i = hay.IndexOf(needle, i, StringComparison.Ordinal);
            if (i < 0)
                return n;
            n++;
            i += needle.Length;
        }
    }

    static string Build()
    {

        var log = new List<string>();
        void Take(VmEngineCmds1235 screen) => log.AddRange(screen.Log);

        var head = new VmEngineCmds1235
        {
            Clock = "Thu Oct 29 16:51:00 2026",
            Version = "HCDE 1.0",
            GitTime = "2026-10-29",
            GitHash = "abc",
        };
        head.Strings["HELLO"] = "hi";
        head.ExecOk.Add(true);
        head.ExecOk.Add(false);
        head.LogFail.Add("nope");
        head.Quit("quit", "now");
        head.Exit("exit");
        head.GameVersion("gameversion");
        head.Print("print");
        head.Print("print", "A", "B");
        head.Print("print", "");
        head.Print("print", "MISSING");
        head.Print("print", "HELLO");
        head.Exec("exec");
        head.Exec("exec", "a.cfg", "b.cfg", "c.cfg");
        head.Logfile("logfile");
        head.Logfile("logfile", "game.log");
        head.Logfile("logfile", "game.log", "0");
        head.Logfile("logfile", "nope");
        head.Error("error");
        head.Error("error", "boom", "extra");
        head.ErrorFatal("error_fatal");
        head.ErrorFatal("error_fatal", "die");
        head.Crashout("crashout");
        Take(head);
        log.Add("---- edge");

        var tail = new VmEngineCmds1235
        {
            Cwd = "/cwd",
            Home = "/home/me",
            Clock = "Thu Oct 29 16:51:00 2026",
        };
        tail.Strings["HELLO"] = "hi";
        tail.Languages["HELLO|005f6e65"] = "hello-en";
        void Fail(string bpath, string pat) => tail.Scans[bpath + "\n" + pat] = new VmEngineCmds1235.Scan { Ok = false };
        void Ok(string bpath, string pat, params (string name, bool dir)[] rows)
        {
            var scan = new VmEngineCmds1235.Scan { Ok = true };
            foreach (var row in rows)
                scan.Rows.Add(new VmEngineCmds1235.Row { Name = row.name, Dir = row.dir });
            tail.Scans[bpath + "\n" + pat] = scan;
        }
        Fail("/cwd", "*");
        Ok("/wads/", "*.wad", ("maps", true), ("doom.wad", false));
        Fail("/wads/doom.wad", "*");
        Fail("", "a\\b*.txt");
        Ok("/home/me/maps", "*", ("E1M1", false));
        Fail("~bob", "*");
        Fail("/x/", "foo?.txt");
        tail.WadNums["doom.wad"] = 2;
        tail.Lumps.Add(new VmEngineCmds1235.Lump { Container = 0, Length = 12, Name = "MAP01" });
        tail.Lumps.Add(new VmEngineCmds1235.Lump { Container = 2, Length = 100, Name = "DEMO1" });
        tail.Lumps.Add(new VmEngineCmds1235.Lump { Container = 2, Length = 3, Name = "MAP02" });
        tail.Files["missing.txt"] = null;
        tail.Errors["missing.txt"] = "No such file or directory";
        tail.Files["empty.txt"] = Array.Empty<byte>();
        tail.Files["abc.txt"] = System.Text.Encoding.ASCII.GetBytes("abc");
        tail.Files["wide.txt"] = new byte[8193];
        Array.Fill(tail.Files["wide.txt"]!, (byte)'a');
        tail.Dir("dir");
        tail.Dir("dir", "/wads/*.wad");
        tail.Dir("dir", "/wads/doom.wad");
        tail.Dir("dir", "a\\b*.txt");
        tail.Dir("dir", "~/maps");
        tail.Dir("dir", "~bob");
        tail.Dir("dir", "/x/foo?.txt");
        tail.Wdir("wdir");
        tail.Wdir("wdir", "missing.wad");
        tail.Wdir("wdir", "doom.wad");
        tail.Wdir("wdir", "doom.wad", "extra");
        tail.Md5Sum("md5sum");
        tail.Md5Sum("md5sum", "missing.txt", "empty.txt", "abc.txt", "wide.txt");
        tail.PrintLocalized("printlocalized");
        tail.PrintLocalized("printlocalized", "HELLO");
        tail.PrintLocalized("printlocalized", "HELLO", "EN");
        tail.PrintLocalized("printlocalized", "HELLO", "E");
        tail.PrintLocalized("printlocalized", "HELLO", "EN_us");
        tail.PrintLocalized("printlocalized", "NOPE", "eng");
        tail.PrintLocalized("printlocalized", "HELLO", "");
        Take(tail);
        return string.Join("\n", log) + "\n";
    }

    const string HeadGolden = """
        endoom
        throw exit code=0
        endoom
        throw exit code=0
        print 2 HCDE 1.0 @ 2026-10-29
        print 2 Commit abc
        print 2 print <name>: Print a string from the string table
        print 2 print <name>: Print a string from the string table
        print 2  unknown
        print 2 MISSING unknown
        print 2 hi
        exec a.cfg ok=1
        exec b.cfg ok=0
        print 2 Could not exec "b.cfg"
        open game.log append=0
        print 2 Log started: Thu Oct 29 16:51:00 2026
        print 2 Log stopped: Thu Oct 29 16:51:00 2026
        fclose
        open game.log append=1
        print 2 Log started: Thu Oct 29 16:51:00 2026
        print 2 Log stopped: Thu Oct 29 16:51:00 2026
        fclose
        open nope append=0
        print 2 Could not start log
        print 2 Usage: error <error text>
        ierror boom
        print 2 Usage: error_fatal <error text>
        ifatal die
        crashout fault addr=0

        """;

    const string TailGolden = """
        ---- edge
        cwd /cwd
        scan bpath=/cwd pat=* recurse=1
        print 2 Nothing matching /cwd
        scan bpath=/wads/ pat=*.wad recurse=1
        print 2 Listing of /wads/*.wad:
        print 200 maps <dir>
        print 2 doom.wad
        scan bpath=/wads/doom.wad pat=* recurse=1
        print 2 Nothing matching /wads/doom.wad
        scan bpath= pat=a\b*.txt recurse=1
        print 2 Nothing matching a\b*.txt
        scan bpath=/home/me/maps pat=* recurse=1
        print 2 Listing of /home/me/maps:
        print 2 E1M1
        scan bpath=~bob pat=* recurse=1
        print 2 Nothing matching ~bob
        scan bpath=/x/ pat=foo?.txt recurse=1
        print 2 Nothing matching /x/foo?.txt
        print 2         12 MAP01
        print 2        100 DEMO1
        print 2          3 MAP02
        wad missing.wad num=-1
        print 2 missing.wad must be loaded to view its directory.
        wad doom.wad num=2
        print 2        100 DEMO1
        print 2          3 MAP02
        print 2         12 MAP01
        print 2        100 DEMO1
        print 2          3 MAP02
        print 2 Usage: md5sum <file> ...
        print 2 missing.txt: No such file or directory
        print 2 d41d8cd98f00b204e9800998ecf8427e *empty.txt
        read n=3
        print 2 900150983cd24fb0d6963f7d28e17f72 *abc.txt
        read n=8192
        read n=1
        print 2 b7652d9bca37038c342cc0c492dd70f9 *wide.txt
        print 2 hi
        bounds index=2 len=2
        print 2 hi
        lang id=005f6e65
        print 2 hello-en
        lang id=00676e65
        lang null
        print 2 hi

        """;
}
