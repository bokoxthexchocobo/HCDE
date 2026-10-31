namespace HCDE.Playsim.Tests;

public class VmButtons1219SliceTests
{
    [Fact]
    public void PressKey_FillsTheLowestSlotAndMasksTheDoubleClickBit()
    {
        var button = new VmButtonStatus1219();
        var log = new List<string>();
        Assert.True(button.PressKey(0x8005, log.Add));
        Assert.Equal((ushort)5, button.Keys[0]);
        Assert.True(button.Down);
        Assert.True(button.WentDown);
        Assert.False(button.PressKey(9, log.Add));
        Assert.Equal((ushort)9, button.Keys[1]);
        Assert.False(button.PressKey(5, log.Add));
        Assert.Equal((ushort)5, button.Keys[0]);
        Assert.False(button.PressKey(0x10000, log.Add));
        Assert.Equal((ushort)0xffff, button.Keys[0]);
        Assert.Equal((ushort)0, button.Keys[1]);
        Assert.Empty(log);

        var full = new VmButtonStatus1219();
        for (int key = 1; key <= 6; key++)
            full.PressKey(key, log.Add);
        Assert.False(full.PressKey(7, log.Add));
        Assert.Equal(new[] { "More than 6 keys pressed for a single action!\n" }, log);
        Assert.Equal(new ushort[] { 1, 2, 3, 4, 5, 6 }, full.Keys);
        Assert.False(full.ReleaseKey(1));
        Assert.True(full.WentUp);
        Assert.True(full.Down);
        Assert.Equal((ushort)0, full.Keys[0]);
        Assert.False(full.ReleaseKey(6));
        Assert.False(full.ReleaseKey(2));
        Assert.False(full.ReleaseKey(3));
        Assert.False(full.ReleaseKey(4));
        Assert.True(full.ReleaseKey(5));
        Assert.False(full.Down);
        full.WentUp = false;
        Assert.False(full.ReleaseKey(8));
        Assert.False(full.WentUp);

        var map = new VmButtonMap1219();
        map.SetButtons(new[] { "use" });
        Assert.True(map.Buttons[0].PressKey(4, log.Add));
        map.ResetButtonStates();
        Assert.False(map.Buttons[0].Down);
        Assert.False(map.Buttons[0].WentUp);
        Assert.False(map.Buttons[0].WentDown);
        Assert.Equal((ushort)0, map.Buttons[0].Keys[0]);

        Assert.True(map.Buttons[0].PressKey(4, log.Add));
        map.Buttons[0].WentDown = true;
        map.Buttons[0].ReleaseLock = true;
        map.ResetButtonStates();
        Assert.True(map.Buttons[0].Down);
        Assert.False(map.Buttons[0].WentDown);
        Assert.Equal((ushort)4, map.Buttons[0].Keys[0]);
        map.ResetButtonTriggers();
        Assert.False(map.Buttons[0].WentUp);
    }

    [Fact]
    public void FindAndAxes_FoldNamesAndClampTheAxis()
    {
        var map = new VmButtonMap1219();
        string longName = new string('n', 40);
        map.SetButtons(new[] { "Fire", "FIRE", "None", longName });
        Assert.Equal(1, map.FindButtonIndex("fire"));
        Assert.Equal(1, map.FindButtonIndex("FIREBALL", 4));
        Assert.Equal(-1, map.FindButtonIndex("FIREBALL", 8));
        Assert.Equal(-1, map.FindButtonIndex("none"));
        Assert.Equal(-1, map.FindButtonIndex("NONE"));
        Assert.Equal(-1, map.FindButtonIndex(null));
        Assert.Equal(-1, map.FindButtonIndex("missing"));
        Assert.Equal(3, map.FindButtonIndex(longName));

        var lines = new List<string>();
        var seen = new List<string>();
        int count = map.ListActionCommands(null, (_, _) => throw new InvalidOperationException("wild"), lines.Add);
        Assert.Equal(8, count);
        Assert.Equal("+Fire\n-Fire\n+FIRE\n-FIRE\n+None\n-None\n+" + longName + "\n-" + longName + "\n", string.Concat(lines));

        lines.Clear();
        count = map.ListActionCommands("keep", (pattern, text) =>
        {
            seen.Add(pattern + ":" + text);
            return text[0] == '+';
        }, lines.Add);
        Assert.Equal(4, count);
        Assert.Equal("+Fire\n+FIRE\n+None\n+" + longName + "\n", string.Concat(lines));
        Assert.Contains("keep:+" + longName.Substring(0, 30), seen);
        Assert.Contains("keep:-" + longName.Substring(0, 30), seen);

        var tabs = new List<string>();
        map.AddButtonTabCommands(tabs.Add);
        Assert.Equal("+Fire", tabs[0]);
        Assert.Equal("-None", tabs[5]);

        int[] axisKeys = { 10, 11, 12, 13, 14 };
        map.GetAxes(new[] { 0.25f, -1f, float.NaN, 0f, 0.125f }, command =>
        {
            if (command == "+FIRE")
                return new[] { 10 };
            if (command == "-FIRE")
                return new[] { 14 };
            return Array.Empty<int>();
        }, axisKeys);
        Assert.False(map.Buttons[0].IsAxis);
        Assert.Equal(0f, map.Buttons[0].Axis);
        Assert.True(map.Buttons[1].IsAxis);
        Assert.Equal(0.125f, map.Buttons[1].Axis);
        map.GetAxes(new[] { 0.5f, 0f, 0f, 0.75f, 0f }, command =>
        {
            if (command == "+FIRE")
                return new[] { 10, 13 };
            return Array.Empty<int>();
        }, axisKeys);
        Assert.Equal(1f, map.Buttons[1].Axis);
    }
}
