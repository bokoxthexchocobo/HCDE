using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmEndoom1231SliceTests
{
    [Fact]
    public void Run_ClampsTheCvarAndBlinksPastThePeriod()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("show in=-1 out=0\n", head, StringComparison.Ordinal);
        Assert.Contains("show in=4 out=2\n", head, StringComparison.Ordinal);
        Assert.Contains("run show=1 namelen=0\n", head, StringComparison.Ordinal);
        Assert.Contains("lookup \"ENDOOM\" trynormal=1 lump=-1 len=4000\n", head, StringComparison.Ordinal);
        Assert.Contains("run badlump\n", head, StringComparison.Ordinal);
        Assert.Contains("run pwadonly\n", head, StringComparison.Ordinal);
        Assert.Contains("bitmap 640x416\n", head, StringComparison.Ordinal);
        Assert.Contains("clear x=0 y=400 w=640 h=16 b=0 g=0 r=0 a=255\n", head, StringComparison.Ordinal);
        Assert.Contains("blink flag=1 saw=0\n", head, StringComparison.Ordinal);
        Assert.Contains("blink hold now=5267 last=5000\n", head, StringComparison.Ordinal);
        Assert.Contains("blinkdraw on=0 time=5268\n", head, StringComparison.Ordinal);
        Assert.Contains("ev type=3 sub=0 tail=1\n", head, StringComparison.Ordinal);
        Assert.Contains("ev type=5 sub=0 tail=3\n", head, StringComparison.Ordinal);
        Assert.Contains("run key\n", head, StringComparison.Ordinal);
        Assert.DoesNotContain("vga_to_ansi", head, StringComparison.Ordinal);
        Assert.Equal(1, Count(head, "stopmusic "));
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Console_PrintsAnsiAndGuiButtonsDismiss()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("console flag=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("console return\n", tail, StringComparison.Ordinal);
        Assert.Contains("vga_to_ansi\n", tail, StringComparison.Ordinal);
        Assert.Contains("run show=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("exit 0\n", tail, StringComparison.Ordinal);
        Assert.Contains("blink flag=1 saw=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("ev type=4 sub=3 tail=5\n", tail, StringComparison.Ordinal);
        Assert.Contains("ev type=4 sub=8 tail=6\n", tail, StringComparison.Ordinal);
        Assert.Contains("blinkdraw on=1 time=546\n", tail, StringComparison.Ordinal);
        Assert.Contains("post type=2 sub=0 head=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("ev type=2 sub=0 tail=0\n", tail, StringComparison.Ordinal);
        Assert.Contains("blink hold now=1267 last=1000\n", tail, StringComparison.Ordinal);
        Assert.Contains("blinkdraw on=0 time=18446744073709551615\n", tail, StringComparison.Ordinal);
        Assert.Contains("ev type=4 sub=13 tail=2\n", tail, StringComparison.Ordinal);
        Assert.Contains("ev type=4 sub=10 tail=3\n", tail, StringComparison.Ordinal);
        Assert.Contains("ev type=4 sub=1 tail=4\n", tail, StringComparison.Ordinal);
        Assert.Equal(2, Count(tail, "vga_to_ansi\n"));
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

        var screen = new VmEndoom1231();
        var frames = new Queue<Action<VmEndoom1231>>();
        screen.GetEvent = s =>
        {
            if (frames.Count == 0)
                throw new InvalidOperationException("empty pump");
            frames.Dequeue()(s);
        };

        const string Quit = "Press any key to quit.";
        screen.SetShow(-1);
        screen.SetShow(4);
        screen.SetShow(1);
        screen.Run();
        screen.Name = "ENDOOM";
        screen.Lump = -1;
        screen.Length = 4000;
        screen.Run();
        screen.Lump = 3;
        screen.Length = 3999;
        screen.Run();
        screen.SetShow(2);
        screen.Length = 4000;
        screen.Container = 5;
        screen.MaxIwad = 5;
        screen.Run();
        screen.SetShow(1);
        screen.QuitText = Quit;
        screen.Now = 5000;
        frames.Enqueue(s =>
        {
            s.Now = 5267;
            s.Post(3, 0);
        });
        frames.Enqueue(s =>
        {
            s.Now = 5268;
            s.Post(4, 6);
            s.Post(5, 0);
            s.Post(1, 0);
        });
        screen.Run();
        screen.Log.Add("---- edge");
        screen.ConsoleEndoomEnabled = false;
        screen.ConsoleEndoom();
        screen.ConsoleEndoomEnabled = true;
        screen.Length = 1;
        screen.ConsoleEndoom();
        screen.Length = 4000;
        screen.ConsoleEndoom();
        screen.SetShow(0);
        screen.StEndoom();
        screen.SetShow(2);
        screen.Container = 9;
        screen.MaxIwad = 5;
        screen.Screen[1] = 0x80;
        screen.Now = 10;
        frames.Enqueue(s =>
        {
            s.Now = 278;
            s.Post(4, 3);
            s.Post(4, 8);
        });
        frames.Enqueue(s => s.Now = 278);
        frames.Enqueue(s =>
        {
            s.Now = 546;
            s.Post(4, 7);
        });
        screen.Run();
        screen.EventHead = 127;
        screen.EventTail = 127;
        screen.Now = 546;
        frames.Enqueue(s => s.Post(2, 0));
        screen.Run();
        screen.Now = 1000;
        frames.Enqueue(s =>
        {
            s.Now = 1267;
            s.Post(1, 0);
        });
        screen.Run();
        screen.Now = ulong.MaxValue - 100;
        frames.Enqueue(s =>
        {
            s.Now = ulong.MaxValue;
            s.Post(4, 13);
        });
        screen.Run();
        screen.Now = 20;
        frames.Enqueue(s => s.Post(4, 10));
        screen.Run();
        screen.Now = 20;
        frames.Enqueue(s => s.Post(4, 1));
        screen.Run();
        return string.Join("\n", screen.Log) + "\n";
    }

    const string HeadGolden = """
        show in=-1 out=0
        show in=4 out=2
        show in=1 out=1
        run show=1 namelen=0
        run skip
        run show=1 namelen=6
        lookup "ENDOOM" trynormal=1 lump=-1 len=4000
        run badlump
        run show=1 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=3999
        run badlump
        show in=2 out=2
        run show=2 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=4000
        container 5 maxiwad 5
        run pwadonly
        show in=1 out=1
        run show=1 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=4000
        container 5 maxiwad 5
        stopmusic 1
        read lump=3
        bitmap 640x416
        drawtext
        clear x=0 y=400 w=640 h=16 b=0 g=0 r=0 a=255
        string TXT_QUITENDOOM [Press any key to quit.]
        drawstring x=0 y=25 [Press any key to quit.] fb=128 fg=128 fr=128 fa=255 bb=0 bg=0 br=0 ba=255
        time 5000
        blink flag=1 saw=0
        render force=1
        getevent
        post type=3 sub=0 head=1
        blink hold now=5267 last=5000
        ev type=3 sub=0 tail=1
        getevent
        post type=4 sub=6 head=2
        post type=5 sub=0 head=3
        post type=1 sub=0 head=4
        blinkdraw on=0 time=5268
        clean
        render force=1
        ev type=4 sub=6 tail=2
        ev type=5 sub=0 tail=3
        ev type=1 sub=0 tail=4
        run key

        """;

    const string TailGolden = """
        ---- edge
        console flag=0
        console flag=1 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=1
        console return
        console flag=1 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=4000
        read lump=3
        vga_to_ansi
        show in=0 out=0
        console flag=1 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=4000
        read lump=3
        vga_to_ansi
        run show=0
        run skip
        exit 0
        show in=2 out=2
        run show=2 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=4000
        container 9 maxiwad 5
        stopmusic 1
        read lump=3
        bitmap 640x416
        drawtext
        clear x=0 y=400 w=640 h=16 b=0 g=0 r=0 a=255
        string TXT_QUITENDOOM [Press any key to quit.]
        drawstring x=0 y=25 [Press any key to quit.] fb=128 fg=128 fr=128 fa=255 bb=0 bg=0 br=0 ba=255
        time 10
        blink flag=1 saw=1
        render force=1
        getevent
        post type=4 sub=3 head=5
        post type=4 sub=8 head=6
        blinkdraw on=0 time=278
        clean
        render force=1
        ev type=4 sub=3 tail=5
        ev type=4 sub=8 tail=6
        getevent
        blink hold now=278 last=278
        getevent
        post type=4 sub=7 head=7
        blinkdraw on=1 time=546
        clean
        render force=1
        ev type=4 sub=7 tail=7
        run gui
        run show=2 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=4000
        container 9 maxiwad 5
        stopmusic 1
        read lump=3
        bitmap 640x416
        drawtext
        clear x=0 y=400 w=640 h=16 b=0 g=0 r=0 a=255
        string TXT_QUITENDOOM [Press any key to quit.]
        drawstring x=0 y=25 [Press any key to quit.] fb=128 fg=128 fr=128 fa=255 bb=0 bg=0 br=0 ba=255
        time 546
        blink flag=1 saw=1
        render force=1
        getevent
        post type=2 sub=0 head=0
        blink hold now=546 last=546
        ev type=2 sub=0 tail=0
        run key
        run show=2 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=4000
        container 9 maxiwad 5
        stopmusic 1
        read lump=3
        bitmap 640x416
        drawtext
        clear x=0 y=400 w=640 h=16 b=0 g=0 r=0 a=255
        string TXT_QUITENDOOM [Press any key to quit.]
        drawstring x=0 y=25 [Press any key to quit.] fb=128 fg=128 fr=128 fa=255 bb=0 bg=0 br=0 ba=255
        time 1000
        blink flag=1 saw=1
        render force=1
        getevent
        post type=1 sub=0 head=1
        blink hold now=1267 last=1000
        ev type=1 sub=0 tail=1
        run key
        run show=2 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=4000
        container 9 maxiwad 5
        stopmusic 1
        read lump=3
        bitmap 640x416
        drawtext
        clear x=0 y=400 w=640 h=16 b=0 g=0 r=0 a=255
        string TXT_QUITENDOOM [Press any key to quit.]
        drawstring x=0 y=25 [Press any key to quit.] fb=128 fg=128 fr=128 fa=255 bb=0 bg=0 br=0 ba=255
        time 18446744073709551515
        blink flag=1 saw=1
        render force=1
        getevent
        post type=4 sub=13 head=2
        blinkdraw on=0 time=18446744073709551615
        clean
        render force=1
        ev type=4 sub=13 tail=2
        run gui
        run show=2 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=4000
        container 9 maxiwad 5
        stopmusic 1
        read lump=3
        bitmap 640x416
        drawtext
        clear x=0 y=400 w=640 h=16 b=0 g=0 r=0 a=255
        string TXT_QUITENDOOM [Press any key to quit.]
        drawstring x=0 y=25 [Press any key to quit.] fb=128 fg=128 fr=128 fa=255 bb=0 bg=0 br=0 ba=255
        time 20
        blink flag=1 saw=1
        render force=1
        getevent
        post type=4 sub=10 head=3
        blink hold now=20 last=20
        ev type=4 sub=10 tail=3
        run gui
        run show=2 namelen=6
        lookup "ENDOOM" trynormal=1 lump=3 len=4000
        container 9 maxiwad 5
        stopmusic 1
        read lump=3
        bitmap 640x416
        drawtext
        clear x=0 y=400 w=640 h=16 b=0 g=0 r=0 a=255
        string TXT_QUITENDOOM [Press any key to quit.]
        drawstring x=0 y=25 [Press any key to quit.] fb=128 fg=128 fr=128 fa=255 bb=0 bg=0 br=0 ba=255
        time 20
        blink flag=1 saw=1
        render force=1
        getevent
        post type=4 sub=1 head=4
        blink hold now=20 last=20
        ev type=4 sub=1 tail=4
        run gui

        """;
}
