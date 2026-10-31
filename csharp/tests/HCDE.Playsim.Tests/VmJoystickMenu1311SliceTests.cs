namespace HCDE.Playsim.Tests;

public class VmJoystickMenu1311SliceTests
{
    [Fact]
    public void Actions_NarrowFloatsAndResetRunsTheHeaderSequence()
    {
        var slice = new VmJoystickMenu1311(new VmJoystickMenu1311.Host());
        var joy = new VmJoystickMenu1311.Joy
        {
            Name = "Pad",
            Sensitivity = 4f,
            Haptics = true,
            HapticsStrength = 0.25f,
            Enabled = false,
            AllowBackground = true,
            EnabledInBackground = false,
            AxisCount = 2,
        };
        joy.Scale[0] = 2f;
        joy.DeadZone[0] = 0.25f;
        joy.Threshold[0] = 0.5f;
        joy.Curve[0] = 1;
        joy.CurvePoint[(0, 1)] = 0.5f;
        joy.AxisName[0] = "Stick";
        joy.OnDefault = device =>
        {
            device.AllowBackground = false;
            device.Sensitivity = 9f;
            device.Enabled = false;
        };

        var missing = Assert.Throws<InvalidOperationException>(() => slice.Reset(null));
        Assert.Contains("self", missing.Message, StringComparison.Ordinal);
        Assert.Equal(0, slice.Reset(joy));
        Assert.Equal(new[] { "default", "enabled", "background", "sensitivity" }, joy.Trace);
        Assert.Equal(1, joy.DefaultCalls);
        Assert.True(joy.Enabled);
        Assert.False(joy.EnabledInBackground);
        Assert.Equal(1f, joy.Sensitivity);

        Assert.Equal(0, slice.SetSensitivity(joy, 16777217d));
        Assert.Equal(16777216f, joy.Sensitivity);
        Assert.Equal(1, slice.GetSensitivity(joy, 2, out double widened));
        Assert.Equal((double)16777216f, widened);
        int reads = joy.Reads;
        Assert.Equal(0, slice.GetSensitivity(joy, 0, out double skipped));
        Assert.Equal(0d, skipped);
        Assert.Equal(reads + 1, joy.Reads);
        Assert.Equal(0, slice.GetSensitivity(joy, -1, out _));

        Assert.Equal(0, slice.SetAxisResponseCurvePoint(joy, 1, 3, 0.1));
        Assert.Equal((float)0.1, joy.CurvePoint[(1, 3)]);
        Assert.Equal(1, slice.GetAxisResponseCurvePoint(joy, 1, 3, 1, out double point));
        Assert.Equal((double)(float)0.1, point);
        Assert.NotEqual(0.1, point);

        Assert.Equal(0, slice.SetAxisResponseCurve(joy, 0, -1));
        Assert.Equal(1, slice.GetAxisResponseCurve(joy, 0, 1, out int curve));
        Assert.Equal(-1, curve);
        Assert.Equal(0, slice.SetAxisResponseCurve(joy, 0, 7));
        Assert.Equal(7, joy.Curve[0]);
        Assert.Equal(0, slice.SetAxisScale(joy, 0, 1.5));
        Assert.Equal((float)1.5, joy.Scale[0]);
        Assert.Equal(1, slice.GetAxisScale(joy, 0, 1, out double scale));
        Assert.Equal((double)joy.Scale[0], scale);
        Assert.Equal(1, slice.HasHaptics(joy, 1, out int haptics));
        Assert.Equal(1, haptics);
        joy.Haptics = false;
        Assert.Equal(1, slice.HasHaptics(joy, 1, out haptics));
        Assert.Equal(0, haptics);
        Assert.Equal(1, slice.GetName(joy, 1, out string name));
        Assert.Equal("Pad", name);
        Assert.Equal(0, slice.GetName(joy, 0, out string blank));
        Assert.Equal("", blank);
        Assert.Equal(1, slice.GetAxisName(joy, 0, 1, out string axis));
        Assert.Equal("Stick", axis);
        Assert.Equal(1, slice.GetNumAxes(joy, 1, out int axes));
        Assert.Equal(2, axes);
        Assert.Equal(0, slice.SetEnabled(joy, false));
        Assert.False(joy.Enabled);
        Assert.Equal(1, slice.GetEnabled(joy, 1, out int enabled));
        Assert.Equal(0, enabled);
        Assert.Equal(0, slice.SetEnabledInBackground(joy, true));
        Assert.True(joy.EnabledInBackground);
        Assert.Equal(1, slice.AllowsEnabledInBackground(joy, 1, out int allow));
        Assert.Equal(0, allow);
        Assert.Equal(0, slice.SetHapticsStrength(joy, 0.1));
        Assert.Equal((float)0.1, joy.HapticsStrength);
        Assert.Equal(1, slice.GetHapticsStrength(joy, 1, out double strength));
        Assert.Equal((double)joy.HapticsStrength, strength);
        Assert.Equal(0, slice.SetAxisDeadZone(joy, 0, 0.2));
        Assert.Equal((float)0.2, joy.DeadZone[0]);
        Assert.Equal(1, slice.GetAxisDeadZone(joy, 0, 1, out double zone));
        Assert.Equal((double)joy.DeadZone[0], zone);
        Assert.Equal(0, slice.SetAxisDigitalThreshold(joy, 0, 0.3));
        Assert.Equal((float)0.3, joy.Threshold[0]);
        Assert.Equal(1, slice.GetAxisDigitalThreshold(joy, 0, 1, out double threshold));
        Assert.Equal((double)joy.Threshold[0], threshold);
    }

    [Fact]
    public void Update_RebuildsFromDefaultsAndKeepsTheDeviceList()
    {
        var host = new VmJoystickMenu1311.Host();
        var slice = new VmJoystickMenu1311(host);
        var configure = new VmJoystickMenu1311.Item { Action = "CONFIGUREMESSAGE" };
        var configureAgain = new VmJoystickMenu1311.Item { Action = "ConfigureMessage" };
        var connect1 = new VmJoystickMenu1311.Item { Action = "ConnectMessage1" };
        var connect2 = new VmJoystickMenu1311.Item { Action = "ConnectMessage2" };
        var defaults = new VmJoystickMenu1311.Descriptor
        {
            Items = new List<VmJoystickMenu1311.Item> { configure, configureAgain, connect1, connect2 },
        };
        var options = new VmJoystickMenu1311.Descriptor { KindOption = true, Selected = 4 };
        host.Slots["JOYSTICKOPTIONSDEFAULTS"] = defaults;
        host.Slots["joystickoptions"] = options;
        var alpha = new VmJoystickMenu1311.Joy { Name = "Alpha" };
        var beta = new VmJoystickMenu1311.Joy { Name = "Beta" };
        var gamma = new VmJoystickMenu1311.Joy { Name = "Gamma" };
        var orphan = new VmJoystickMenu1311.Joy { Name = "Gone" };
        host.Polled.Add(alpha);
        host.Polled.Add(beta);
        host.Polled.Add(gamma);
        host.Current = new VmJoystickMenu1311.Menu
        {
            ClassName = "joystickconfigmenu",
            Joy = orphan,
        };

        slice.Update(beta);

        Assert.Equal(4, defaults.Items.Count);
        Assert.Same(configure, options.Items[0]);
        Assert.Equal(7, options.Items.Count);
        Assert.Equal(6, options.Selected);
        Assert.Equal("Beta", options.Items[5].Label);
        Assert.Same(beta, options.Items[5].Joy);
        Assert.True(configure.ValueSet);
        Assert.Equal(0, configure.ValueIndex);
        Assert.Equal(1, configure.Value);
        Assert.False(configureAgain.ValueSet);
        Assert.Equal(0, connect1.Value);
        Assert.Equal(0, connect2.Value);
        Assert.Equal(3, host.Barriers.Count);
        Assert.Equal(4, host.Barriers[0].Count);
        Assert.Equal(5, host.Barriers[1].Count);
        Assert.Same(options, host.Barriers[2].Menu);
        Assert.Equal(1, host.Current.CloseCalls);
        Assert.Equal(3, slice.Devices.Count);
        Assert.Equal(1, host.Polls);

        host.Current = new VmJoystickMenu1311.Menu
        {
            ClassName = "JoystickConfigMenu",
            Joy = alpha,
        };
        slice.Update(null);
        Assert.Equal(0, host.Current.CloseCalls);
        Assert.Equal(6, options.Selected);

        host.Slots.Remove(VmJoystickMenu1311.DefaultsKey);
        host.Polled.Add(new VmJoystickMenu1311.Joy { Name = "Later" });
        int polls = host.Polls;
        options.Items.Add(new VmJoystickMenu1311.Item { Action = "sentinel" });
        slice.Update(alpha);
        Assert.Equal(polls, host.Polls);
        Assert.Equal(3, slice.Devices.Count);
        Assert.Equal(8, options.Items.Count);

        host.Slots[VmJoystickMenu1311.DefaultsKey] = null;
        slice.Update(alpha);
        Assert.Equal(polls, host.Polls);

        host.Slots.Remove(VmJoystickMenu1311.OptionsKey);
        host.Slots[VmJoystickMenu1311.DefaultsKey] = defaults;
        slice.Update(alpha);
        Assert.Equal(polls, host.Polls);

        host.Slots[VmJoystickMenu1311.OptionsKey] = options;
        options.KindOption = false;
        slice.Update(alpha);
        Assert.Equal(polls, host.Polls);

        options.KindOption = true;
        options.Selected = 100;
        host.Polled.Clear();
        slice.UseJoystick = false;
        host.Current = new VmJoystickMenu1311.Menu { ClassName = "OptionMenu", Joy = orphan };
        slice.Update(orphan);
        Assert.Equal(0, host.Current.CloseCalls);
        Assert.Equal(3, options.Selected);
        Assert.Equal(0, configure.Value);
        Assert.Equal(1, connect1.Value);
        Assert.Equal(1, connect2.Value);
        Assert.Empty(slice.Devices);

        defaults.Items.Clear();
        defaults.Items.Add(new VmJoystickMenu1311.Item());
        defaults.Items.Add(new VmJoystickMenu1311.Item());
        options.Selected = -3;
        slice.Update(null);
        Assert.Equal(-3, options.Selected);

        var one = new VmJoystickMenu1311.Joy { Name = "Only" };
        host.Polled.Add(one);
        slice.Update(orphan);
        Assert.Equal(2, options.Selected);
        Assert.Equal("Only", options.Items[2].Label);

        host.Current = new VmJoystickMenu1311.Menu
        {
            ClassName = "JoystickConfigMenu",
            HasJoyField = false,
            Joy = null,
        };
        var error = Assert.Throws<InvalidOperationException>(() => slice.Update(one));
        Assert.Contains("mJoy", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, host.Current.CloseCalls);
        Assert.Equal("Only", options.Items[^1].Label);

        host.Current = new VmJoystickMenu1311.Menu
        {
            ClassName = "JoystickConfigMenu",
            Joy = null,
        };
        int before = host.Current.CloseCalls;
        slice.Update(one);
        Assert.Equal(before, host.Current.CloseCalls);

        host.Slots[VmJoystickMenu1311.OptionsKey] = null;
        Assert.Throws<NullReferenceException>(() => slice.Update(one));
    }
}
