using S = HCDE.Playsim.VmKeyBind1391Slice;

namespace HCDE.Playsim.Tests;

public class VmKeyBind1391SliceTests
{
    [Fact]
    public void NamesMatchScanCodesAndTheAppleRow()
    {
        Assert.Equal(0x1CB, S.Names.Length);
        Assert.Equal(0x1CB, S.NumKeys);
        Assert.Null(S.Name(0));
        Assert.Equal("Escape", S.Name(0x01));
        Assert.Equal("Tab", S.Name(0x0F));
        Assert.Equal("Enter", S.Name(0x1C));
        Assert.Equal("'", S.Name(0x28));
        Assert.Equal("`", S.Name(0x29));
        Assert.Equal("Shift", S.Name(0x2A));
        Assert.Equal("\\", S.Name(0x2B));
        Assert.Equal("RShift", S.Name(0x36));
        Assert.Equal("Space", S.Name(0x39));
        Assert.Equal("F11", S.Name(0x57));
        Assert.Equal("F12", S.Name(0x58));
        Assert.Equal("Pause", S.Name(0xC5));
        Assert.Equal("Home", S.Name(0xC7));
        Assert.Equal("LWin", S.Name(0xDB));
        Assert.Equal("RWin", S.Name(0xDC));
        Assert.Equal("Command", S.Name(0xDB, apple: true));
        Assert.Null(S.Name(0xDC, apple: true));
        Assert.Equal("Apps", S.Name(0xDD, apple: true));
        Assert.Equal("Mouse1", S.Name(0x100));
        Assert.Equal("Joy1", S.Name(0x108));
        Assert.Equal("Joy128", S.Name(0x187));
        Assert.Equal("POV1Up", S.Name(0x188));
        Assert.Equal("MWheelUp", S.Name(0x198));
        Assert.Equal("Pad_Touchpad", S.Name(0x1CA));
        Assert.Throws<IndexOutOfRangeException>(() => S.Name(S.NumKeys));

        int named = 0;
        int appleNamed = 0;
        for (int i = 0; i < S.NumKeys; i++)
        {
            if (S.Name(i) != null)
                named++;
            if (S.Name(i, true) != null)
                appleNamed++;
        }
        Assert.Equal(356, named);
        Assert.Equal(355, appleNamed);
        Assert.Equal(250, S.DoubleClickDefault);
        Assert.Equal(1, S.CvarArchive);
        Assert.Equal(1024, S.CvarGlobalConfig);
        Assert.Equal(1025, S.DoubleClickFlags);
        Assert.Equal(35, S.TicRate);
        Assert.Equal(28u, S.SyntheticReleaseDelay);
    }

    [Fact]
    public void ExecFlipsPlusCommandsAndClearDropsTheQueuedPress()
    {
        var issued = new List<S.Issued>();
        Assert.False(S.Exec("", 1, false, false, issued));
        Assert.False(S.Exec(null, 1, true, false, issued));
        Assert.False(S.Exec("use", 1, true, false, issued));
        Assert.Empty(issued);

        const string plus = "+use";
        Assert.True(S.Exec(plus, 0x1C, true, false, issued));
        Assert.Equal("+use", plus);
        Assert.Equal("-use", issued[0].Command);
        Assert.Equal(0x1Cu, issued[0].Key);

        Assert.True(S.Exec("+use", 4, false, false, issued));
        Assert.Equal("+use", issued[1].Command);
        Assert.Equal(4u, issued[1].Key);

        Assert.True(S.Exec("slot 1", 5, false, false, issued));
        Assert.Equal("slot 1", issued[2].Command);

        Assert.True(S.Exec("+", 0x1CA, true, true, issued));
        Assert.Equal("-", issued[3].Command);
        Assert.Equal(0x1CAu | 0x8000u, issued[3].Key);
        Assert.Equal(0x8000, S.KeyDblClicked);

        var queue = new S.PressQueue();
        queue.Press[9] = true;
        queue.Release[9] = true;
        queue.Binding[9] = "+use";
        queue.Deadline[9] = 40;
        queue.DoubleClicked[9] = true;
        queue.Delayed[9] = true;
        queue.Press[10] = true;
        queue.Clear(9);
        Assert.False(queue.Press[9]);
        Assert.False(queue.Release[9]);
        Assert.Null(queue.Binding[9]);
        Assert.Equal(0u, queue.Deadline[9]);
        Assert.True(queue.DoubleClicked[9]);
        Assert.True(queue.Delayed[9]);
        Assert.True(queue.Press[10]);
        Assert.Throws<IndexOutOfRangeException>(() => queue.Clear((uint)S.NumKeys));

        S.Bindings.Slots[3] = "+forward";
        Assert.Null(S.DoubleBindings.Slots[3]);
        Assert.Null(S.AutomapBindings.Slots[3]);
        Assert.NotSame(S.Bindings, S.DoubleBindings);
        Assert.NotSame(S.Bindings, S.AutomapBindings);
        S.Bindings.Slots[3] = null;
    }
}
