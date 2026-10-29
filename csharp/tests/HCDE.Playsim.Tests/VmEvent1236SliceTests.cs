using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmEvent1236SliceTests
{
    [Fact]
    public void Process_DelaysTheEatenKeyUpAndTogglesFullscreen()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("move x=6 y=-8\n", head, StringComparison.Ordinal);
        Assert.Contains("move x=0 y=0\n", head, StringComparison.Ordinal);
        Assert.Contains("console eat=1\n", head, StringComparison.Ordinal);
        Assert.Contains("fullscreen 1\n", head, StringComparison.Ordinal);
        Assert.Contains("escape want=1\n", head, StringComparison.Ordinal);
        Assert.Contains("mark key=1\n", head, StringComparison.Ordinal);
        Assert.Contains("delay key=1\n", head, StringComparison.Ordinal);
        Assert.Contains("menu eat=1\n", head, StringComparison.Ordinal);
        Assert.Contains("fullscreen 0\n", head, StringComparison.Ordinal);
        Assert.Contains("post type=2 head=12\n", head, StringComparison.Ordinal);
        Assert.Equal(2, Count(head, "device list=4\n"));
        Assert.DoesNotContain("drop device", head, StringComparison.Ordinal);
        Assert.DoesNotContain("intro\n", head, StringComparison.Ordinal);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Remove_BlanksGuiCharsAndViewsTheEvent()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("move x=-6 y=8\n", tail, StringComparison.Ordinal);
        Assert.Contains("intro\n", tail, StringComparison.Ordinal);
        Assert.Contains("dispatch eat=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("drop device head=2\n", tail, StringComparison.Ordinal);
        Assert.Contains("blank index=0 sub=1\n", tail, StringComparison.Ordinal);
        Assert.Contains("slot 1 type=4\n", tail, StringComparison.Ordinal);
        Assert.Contains("blank index=2 sub=4\n", tail, StringComparison.Ordinal);
        Assert.Contains("slot 2 type=0 sub=4\n", tail, StringComparison.Ordinal);
        Assert.Contains("assert fail\n", tail, StringComparison.Ordinal);
        Assert.Contains("input type=1 scan=353 char=97 str=61 mouse=0,0\n", tail, StringComparison.Ordinal);
        Assert.Contains("input type=1 scan=256 char=0 str= mouse=0,0\n", tail, StringComparison.Ordinal);
        Assert.Contains("mouse=6,-1\n", tail, StringComparison.Ordinal);
        Assert.Contains("str=efbfbf shift=0 alt=1 ctrl=0 mouse=0,0\n", tail, StringComparison.Ordinal);
        Assert.Contains("shift=1 alt=1 ctrl=1 mouse=0,0\n", tail, StringComparison.Ordinal);
        Assert.Contains("field InputEvent.MouseY\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("console eat", tail, StringComparison.Ordinal);
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
        void Take(VmEvent1236 screen) => log.AddRange(screen.Log);

        VmEvent1236.Ev Key(int type, short data1, short data2 = 0, short data3 = 0) => new()
        {
            Type = type, Data1 = data1, Data2 = data2, Data3 = data3
        };

        var head = new VmEvent1236 { EscapeWanted = true, Devices = 4 };
        head.ConsoleEat.AddRange(new[] { true, false, false, false, false, false });
        head.MenuEat.AddRange(new[] { false, false, false, true, false });
        head.GameEat.AddRange(new[] { false, false, false, true, false });
        head.Post(Key(VmEvent1236.EvKeyDown, 65));
        head.PostMouse(3, 4);
        head.PostMouse(0, 0);
        head.Post(new VmEvent1236.Ev { Type = VmEvent1236.EvDevice });
        head.Post(new VmEvent1236.Ev { Type = VmEvent1236.EvDevice });
        head.Post(Key(VmEvent1236.EvKeyDown, VmEvent1236.KeyEnter, 0, VmEvent1236.GkmAlt));
        head.Post(Key(VmEvent1236.EvKeyDown, VmEvent1236.KeyEscape));
        head.Post(Key(VmEvent1236.EvKeyUp, VmEvent1236.KeyEscape));
        head.Post(new VmEvent1236.Ev { Type = VmEvent1236.EvNone });
        head.Post(Key(VmEvent1236.EvKeyDown, 66));
        head.Post(new VmEvent1236.Ev { Type = VmEvent1236.EvGui, Sub = VmEvent1236.GuiKeyDown, Data1 = VmEvent1236.GkReturn, Data3 = VmEvent1236.GkmAlt });
        head.Post(Key(VmEvent1236.EvKeyUp, 66));
        head.Process();
        Take(head);
        log.Add("---- edge");

        var tail = new VmEvent1236 { GameState = VmEvent1236.GsIntro, Devices = 9, InvertX = true, InvertY = true };
        tail.GameEat.Add(false);
        tail.GameEat.Add(false);
        tail.PostMouse(3, 4);
        tail.Post(Key(VmEvent1236.EvKeyDown, 70));
        tail.Process();
        tail.DispatchEat.Add(true);
        tail.Post(Key(VmEvent1236.EvKeyDown, 9));
        tail.Ring[tail.Head].Type = VmEvent1236.EvDevice;
        tail.Post(new VmEvent1236.Ev { Type = VmEvent1236.EvDevice });
        tail.Tail = 0;
        tail.Head = 3;
        tail.Ring[0] = new VmEvent1236.Ev { Type = VmEvent1236.EvGui, Sub = VmEvent1236.GuiKeyDown };
        tail.Ring[1] = new VmEvent1236.Ev { Type = VmEvent1236.EvGui, Sub = 6, Data1 = 1, Data2 = 2 };
        tail.Ring[2] = new VmEvent1236.Ev { Type = VmEvent1236.EvGui, Sub = VmEvent1236.GuiChar, Data1 = 65 };
        tail.RemoveNextChar();
        for (int i = 0; i < 3; i++)
            tail.Log.Add("slot " + i + " type=" + tail.Ring[i].Type);
        tail.Tail = 0;
        tail.Head = 3;
        tail.Ring[0] = new VmEvent1236.Ev { Type = VmEvent1236.EvGui, Sub = VmEvent1236.GuiKeyDown };
        tail.Ring[1] = new VmEvent1236.Ev { Type = VmEvent1236.EvGui, Sub = VmEvent1236.GuiKeyDown };
        tail.Ring[2] = new VmEvent1236.Ev { Type = VmEvent1236.EvGui, Sub = VmEvent1236.GuiChar, Data1 = 0xE9 };
        tail.RemoveNextChar();
        for (int i = 0; i < 3; i++)
            tail.Log.Add("slot " + i + " type=" + tail.Ring[i].Type + " sub=" + tail.Ring[i].Sub);
        tail.Ring[0] = new VmEvent1236.Ev { Type = VmEvent1236.EvMouse };
        tail.RemoveNextChar();
        tail.Log.Add(tail.Input(Key(VmEvent1236.EvKeyDown, 353, 97)));
        tail.Log.Add(tail.Input(Key(VmEvent1236.EvKeyDown, 256)));
        tail.Log.Add(tail.Input(new VmEvent1236.Ev { Type = VmEvent1236.EvMouse, X = 6.9f, Y = -1.9f }));
        tail.Log.Add(tail.Ui(new VmEvent1236.Ev { Sub = VmEvent1236.GuiKeyDown, Data1 = 65, Data3 = 7 }));
        tail.Log.Add(tail.Ui(new VmEvent1236.Ev { Sub = VmEvent1236.GuiChar, Data1 = -1, Data2 = 1 }));
        tail.Log.Add(tail.Ui(new VmEvent1236.Ev { Sub = VmEvent1236.GuiChar, Data1 = 65 }));
        tail.Log.Add(tail.Ui(new VmEvent1236.Ev { Sub = 6, Data1 = 8, Data2 = 9, Data3 = 1 }));
        tail.ExportFields();
        Take(tail);
        return string.Join("\n", log) + "\n";
    }

    const string HeadGolden = """
        post type=1 head=1
        move x=6 y=-8
        post type=3 head=2
        move x=0 y=0
        post type=5 head=3
        post type=5 head=4
        post type=1 head=5
        post type=1 head=6
        post type=2 head=7
        post type=0 head=8
        post type=1 head=9
        post type=4 head=10
        post type=2 head=11
        console eat=1
        console eat=0
        menu eat=0
        game eat=0
        device list=4
        console eat=0
        menu eat=0
        game eat=0
        device list=4
        console eat=0
        menu eat=0
        game eat=0
        fullscreen 1
        escape want=1
        game eat=1
        mark key=1
        delay key=1
        skip none
        console eat=0
        menu eat=1
        fullscreen 0
        console eat=0
        menu eat=0
        game eat=0
        post type=2 head=12

        """;

    const string TailGolden = """
        ---- edge
        move x=-6 y=8
        post type=3 head=1
        post type=1 head=2
        intro
        game eat=0
        intro
        game eat=0
        dispatch eat=1
        drop device head=2
        blank index=0 sub=1
        slot 0 type=0
        slot 1 type=4
        slot 2 type=4
        blank index=0 sub=1
        blank index=1 sub=1
        blank index=2 sub=4
        slot 0 type=0 sub=1
        slot 1 type=0 sub=1
        slot 2 type=0 sub=4
        assert fail
        input type=1 scan=353 char=97 str=61 mouse=0,0
        input type=1 scan=256 char=0 str= mouse=0,0
        input type=3 scan=0 char=0 str= mouse=6,-1
        ui type=1 char=65 str=41 shift=1 alt=1 ctrl=1 mouse=0,0
        ui type=4 char=65535 str=efbfbf shift=0 alt=1 ctrl=0 mouse=0,0
        ui type=4 char=65 str=41 shift=0 alt=0 ctrl=0 mouse=0,0
        ui type=6 char=0 str= shift=1 alt=0 ctrl=0 mouse=8,9
        field UiEvent.Type
        field UiEvent.KeyString
        field UiEvent.KeyChar
        field UiEvent.MouseX
        field UiEvent.MouseY
        field UiEvent.IsShift
        field UiEvent.IsAlt
        field UiEvent.IsCtrl
        field InputEvent.Type
        field InputEvent.KeyScan
        field InputEvent.KeyString
        field InputEvent.KeyChar
        field InputEvent.MouseX
        field InputEvent.MouseY

        """;
}
