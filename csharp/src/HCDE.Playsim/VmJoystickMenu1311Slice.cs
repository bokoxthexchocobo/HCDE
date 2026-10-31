namespace HCDE.Playsim;

/// <summary>
/// Joystick config actions and the options-menu rebuild from
/// <c>joystickmenu.cpp</c> (1-281). Chunk 1311.
/// </summary>
public sealed class VmJoystickMenu1311
{
    public const string OptionsKey = "JoystickOptions";
    public const string DefaultsKey = "JoystickOptionsDefaults";

    readonly Host _host;

    public VmJoystickMenu1311(Host host) => _host = host;

    public bool UseJoystick { get; set; } = true;

    public List<Joy> Devices { get; } = new();

    public int GetSensitivity(Joy? self, int numret, out double value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnFloat(numret, joy.Sensitivity, out value);
    }

    public int SetSensitivity(Joy? self, double sens)
    {
        Require(self).Sensitivity = (float)sens;
        return 0;
    }

    public int HasHaptics(Joy? self, int numret, out int value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnInt(numret, joy.Haptics ? 1 : 0, out value);
    }

    public int GetHapticsStrength(Joy? self, int numret, out double value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnFloat(numret, joy.HapticsStrength, out value);
    }

    public int SetHapticsStrength(Joy? self, double strength)
    {
        Require(self).HapticsStrength = (float)strength;
        return 0;
    }

    public int GetAxisScale(Joy? self, int axis, int numret, out double value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnFloat(numret, joy.Scale[axis], out value);
    }

    public int SetAxisScale(Joy? self, int axis, double sens)
    {
        Require(self).Scale[axis] = (float)sens;
        return 0;
    }

    public int GetAxisDeadZone(Joy? self, int axis, int numret, out double value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnFloat(numret, joy.DeadZone[axis], out value);
    }

    public int SetAxisDeadZone(Joy? self, int axis, double zone)
    {
        Require(self).DeadZone[axis] = (float)zone;
        return 0;
    }

    public int GetAxisDigitalThreshold(Joy? self, int axis, int numret, out double value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnFloat(numret, joy.Threshold[axis], out value);
    }

    public int SetAxisDigitalThreshold(Joy? self, int axis, double threshold)
    {
        Require(self).Threshold[axis] = (float)threshold;
        return 0;
    }

    public int GetAxisResponseCurve(Joy? self, int axis, int numret, out int value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnInt(numret, joy.Curve[axis], out value);
    }

    public int SetAxisResponseCurve(Joy? self, int axis, int curve)
    {
        Require(self).Curve[axis] = curve;
        return 0;
    }

    public int GetAxisResponseCurvePoint(Joy? self, int axis, int point, int numret, out double value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnFloat(numret, joy.CurvePoint[(axis, point)], out value);
    }

    public int SetAxisResponseCurvePoint(Joy? self, int axis, int point, double raw)
    {
        Require(self).CurvePoint[(axis, point)] = (float)raw;
        return 0;
    }

    public int GetName(Joy? self, int numret, out string value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnString(numret, joy.Name, out value);
    }

    public int GetAxisName(Joy? self, int axis, int numret, out string value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnString(numret, joy.AxisName[axis], out value);
    }

    public int GetNumAxes(Joy? self, int numret, out int value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnInt(numret, joy.AxisCount, out value);
    }

    public int GetEnabled(Joy? self, int numret, out int value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnInt(numret, joy.Enabled ? 1 : 0, out value);
    }

    public int SetEnabled(Joy? self, bool enabled)
    {
        Require(self).Enabled = enabled;
        return 0;
    }

    public int AllowsEnabledInBackground(Joy? self, int numret, out int value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnInt(numret, joy.AllowBackground ? 1 : 0, out value);
    }

    public int GetEnabledInBackground(Joy? self, int numret, out int value)
    {
        Joy joy = Require(self);
        joy.Reads++;
        return ReturnInt(numret, joy.EnabledInBackground ? 1 : 0, out value);
    }

    public int SetEnabledInBackground(Joy? self, bool enabled)
    {
        Require(self).EnabledInBackground = enabled;
        return 0;
    }

    public int Reset(Joy? self)
    {
        Require(self).Reset();
        return 0;
    }

    public void Update(Joy? selected)
    {
        if (!_host.TrySlot(DefaultsKey, out Descriptor? defaults))
            return;
        if (!_host.TrySlot(OptionsKey, out Descriptor? options))
            return;
        if (options is null)
            throw new NullReferenceException("JoystickOptions");
        if (!options.KindOption)
            return;
        if (defaults is null)
            return;

        Devices.Clear();
        foreach (Joy joy in _host.Poll())
            Devices.Add(joy);

        int itemnum = -1;
        if ((uint)itemnum >= (uint)Devices.Count)
            itemnum = Devices.Count - 1;
        if (selected != null)
        {
            for (int i = 0; (uint)i < (uint)Devices.Count; i++)
            {
                if (ReferenceEquals(Devices[i], selected))
                {
                    itemnum = i;
                    break;
                }
            }
        }

        options.Items = new List<Item>(defaults.Items);
        SetFlag(options, "ConfigureMessage", Devices.Count != 0);
        SetFlag(options, "ConnectMessage1", !UseJoystick);
        SetFlag(options, "ConnectMessage2", !UseJoystick);
        for (int ii = 0; ii < Devices.Count; ii++)
        {
            Item it = _host.Create(Devices[ii].Name, Devices[ii]);
            _host.Barrier(options, it);
            options.Items.Add(it);
            if (ii == itemnum)
                options.Selected = options.Items.Count;
        }

        if (options.Selected >= options.Items.Count)
            options.Selected = options.Items.Count - 1;

        Menu? menu = _host.Current;
        if (menu == null || !menu.IsJoystickConfig)
            return;
        if (!menu.HasJoyField)
            throw new InvalidOperationException("Variable mJoy not found in " + menu.ClassName);
        Joy? open = menu.Joy;
        if (open == null)
            return;
        for (int ii = 0; ii < Devices.Count; ii++)
        {
            if (ReferenceEquals(Devices[ii], open))
                return;
        }

        menu.Close();
    }

    static void SetFlag(Descriptor menu, string action, bool on)
    {
        foreach (Item item in menu.Items)
        {
            if (!item.Action.Equals(action, StringComparison.OrdinalIgnoreCase))
                continue;
            item.SetValue(0, on ? 1 : 0);
            return;
        }
    }

    static int ReturnFloat(int numret, float raw, out double value)
    {
        if (numret > 0)
        {
            value = raw;
            return 1;
        }

        value = 0;
        return 0;
    }

    static int ReturnInt(int numret, int raw, out int value)
    {
        if (numret > 0)
        {
            value = raw;
            return 1;
        }

        value = 0;
        return 0;
    }

    static int ReturnString(int numret, string raw, out string value)
    {
        if (numret > 0)
        {
            value = raw;
            return 1;
        }

        value = "";
        return 0;
    }

    static Joy Require(Joy? self)
    {
        if (self is null)
            throw new InvalidOperationException("In function parameter self");
        return self;
    }

    public sealed class Joy
    {
        public string Name { get; set; } = "";
        public float Sensitivity { get; set; }
        public bool Haptics { get; set; }
        public float HapticsStrength { get; set; }
        public bool Enabled { get; set; }
        public bool AllowBackground { get; set; }
        public bool EnabledInBackground { get; set; }
        public int AxisCount { get; set; }
        public Dictionary<int, float> Scale { get; } = new();
        public Dictionary<int, float> DeadZone { get; } = new();
        public Dictionary<int, float> Threshold { get; } = new();
        public Dictionary<int, int> Curve { get; } = new();
        public Dictionary<(int Axis, int Point), float> CurvePoint { get; } = new();
        public Dictionary<int, string> AxisName { get; } = new();
        public int Reads { get; set; }
        public int DefaultCalls { get; private set; }
        public List<string> Trace { get; } = new();
        public Action<Joy>? OnDefault { get; set; }

        public void Reset()
        {
            OnDefault?.Invoke(this);
            Trace.Add("default");
            DefaultCalls++;
            Enabled = true;
            Trace.Add("enabled");
            EnabledInBackground = AllowBackground;
            Trace.Add("background");
            Sensitivity = 1f;
            Trace.Add("sensitivity");
        }
    }

    public sealed class Item
    {
        public string Action { get; set; } = "";
        public string Label { get; set; } = "";
        public Joy? Joy { get; set; }
        public int ValueIndex { get; private set; } = int.MinValue;
        public int Value { get; private set; }
        public bool ValueSet { get; private set; }

        public bool SetValue(int index, int value)
        {
            ValueIndex = index;
            Value = value;
            ValueSet = true;
            return true;
        }
    }

    public sealed class Descriptor
    {
        public bool KindOption { get; set; }
        public int Selected { get; set; }
        public List<Item> Items { get; set; } = new();
    }

    public sealed class Menu
    {
        public string ClassName { get; set; } = "";
        public bool HasJoyField { get; set; } = true;
        public Joy? Joy { get; set; }
        public int CloseCalls { get; private set; }

        public bool IsJoystickConfig =>
            ClassName.Equals("JoystickConfigMenu", StringComparison.OrdinalIgnoreCase);

        public void Close() => CloseCalls++;
    }

    public sealed class Host
    {
        public Dictionary<string, Descriptor?> Slots { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<Joy> Polled { get; } = new();
        public int Polls { get; private set; }
        public List<(Descriptor Menu, Item Item, int Count)> Barriers { get; } = new();
        public Menu? Current { get; set; }

        public bool TrySlot(string key, out Descriptor? desc)
        {
            if (!Slots.TryGetValue(key, out desc))
            {
                desc = null;
                return false;
            }

            return true;
        }

        public List<Joy> Poll()
        {
            Polls++;
            return Polled;
        }

        public Item Create(string label, Joy joy) => new() { Label = label, Joy = joy };

        public void Barrier(Descriptor menu, Item item) => Barriers.Add((menu, item, menu.Items.Count));
    }
}
