namespace HCDE.Playsim;

/// <summary>
/// Button map and key slots from <c>c_buttons.cpp</c> (<b>1–316</b>).
/// Chunk <b>1219</b>. The double-click bit is stripped with <c>0x7FFF</c>.
/// A name that folds to <c>None</c> cannot be found. The lowest free key slot is filled last in the downward scan.
/// </summary>
public sealed class VmButtonStatus1219
{
    public const int MaxKeys = 6;

    public readonly ushort[] Keys = new ushort[MaxKeys];
    public bool Down;
    public bool WentDown;
    public bool WentUp;
    public bool ReleaseLock;
    public bool IsAxis;
    public float Axis;

    public void ResetTriggers()
    {
        WentDown = false;
        WentUp = false;
    }

    public bool PressKey(int keynum, Action<string> print)
    {
        int open;
        keynum &= 0x7FFF;
        if (keynum == 0)
        {
            Keys[0] = 0xffff;
            for (int i = MaxKeys - 1; i > 0; --i)
                Keys[i] = 0;
        }
        else
        {
            open = -1;
            for (int i = MaxKeys - 1; i >= 0; --i)
            {
                if (Keys[i] == 0)
                    open = i;
                else if (Keys[i] == keynum)
                    return false;
            }
            if (open < 0)
            {
                print("More than " + MaxKeys + " keys pressed for a single action!\n");
                return false;
            }
            Keys[open] = (ushort)keynum;
        }
        bool wasDown = Down;
        Down = true;
        WentDown = true;
        return !wasDown;
    }

    public bool ReleaseKey(int keynum)
    {
        keynum &= 0x7FFF;
        bool wasDown = Down;
        if (keynum == 0)
        {
            for (int i = MaxKeys - 1; i >= 0; --i)
                Keys[i] = 0;
            WentUp = true;
            Down = false;
        }
        else
        {
            int numDown = 0;
            int match = -1;
            for (int i = MaxKeys - 1; i >= 0; --i)
            {
                if (Keys[i] != 0)
                {
                    ++numDown;
                    if (Keys[i] == keynum)
                        match = i;
                }
            }
            if (match < 0)
                return false;
            Keys[match] = 0;
            WentUp = true;
            if (--numDown == 0)
                Down = false;
        }
        return wasDown && !Down;
    }

    public void AddAxes(string name, float[] joyAxes, Func<string, int[]> keysForCommand, int[] axisKeys)
    {
        IsAxis = false;
        Axis = 0f;
        int[] positive = keysForCommand("+" + name);
        int[] negative = keysForCommand("-" + name);
        for (int i = 0; i < axisKeys.Length; i++)
        {
            float axisValue = joyAxes[i];
            if (!(axisValue > 0.0))
                continue;
            int keyCode = axisKeys[i];
            if (Contains(positive, keyCode))
            {
                Axis += axisValue;
                IsAxis = true;
            }
            if (Contains(negative, keyCode))
            {
                Axis -= axisValue;
                IsAxis = true;
            }
        }
        if (Axis < 0f)
            Axis = 0f;
        else if (Axis > 1f)
            Axis = 1f;
    }

    static bool Contains(int[] keys, int keyCode)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i] == keyCode)
                return true;
        }
        return false;
    }
}

public sealed class VmButtonMap1219
{
    readonly List<VmButtonStatus1219> buttons = new();
    readonly List<string> numToName = new();
    readonly Dictionary<string, int> nameToNum = new(StringComparer.Ordinal);

    public IReadOnlyList<VmButtonStatus1219> Buttons
    {
        get { return buttons; }
    }

    public void SetButtons(string?[] names)
    {
        buttons.Clear();
        numToName.Clear();
        nameToNum.Clear();
        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i] ?? throw new NullReferenceException();
            buttons.Add(new VmButtonStatus1219());
            numToName.Add(name);
            nameToNum[Lower(name)] = i;
        }
    }

    public int FindButtonIndex(string? key, int funcLen = -1)
    {
        if (key == null)
            return -1;
        if (funcLen != -1)
        {
            if (funcLen < 0 || funcLen > key.Length)
                throw new ArgumentOutOfRangeException(nameof(funcLen));
            key = key.Substring(0, funcLen);
        }
        string folded = Lower(key);
        if (folded == "none")
            return -1;
        if (!nameToNum.TryGetValue(folded, out int index))
            return -1;
        return index;
    }

    public void ResetButtonTriggers()
    {
        for (int i = 0; i < buttons.Count; i++)
            buttons[i].ResetTriggers();
    }

    public void ResetButtonStates()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            VmButtonStatus1219 button = buttons[i];
            if (!button.ReleaseLock)
                button.ReleaseKey(0);
            button.ResetTriggers();
        }
    }

    public void GetAxes(float[] joyAxes, Func<string, int[]> keysForCommand, int[] axisKeys)
    {
        for (int i = 0; i < buttons.Count; i++)
            buttons[i].AddAxes(numToName[i], joyAxes, keysForCommand, axisKeys);
    }

    public int ListActionCommands(string? pattern, Func<string, string, bool> wildcards, Action<string> print)
    {
        int count = 0;
        for (int i = 0; i < numToName.Count; i++)
        {
            string plus = Clip("+" + numToName[i]);
            string minus = Clip("-" + numToName[i]);
            if (pattern == null || wildcards(pattern, plus))
            {
                print("+" + numToName[i] + "\n");
                count++;
            }
            if (pattern == null || wildcards(pattern, minus))
            {
                print("-" + numToName[i] + "\n");
                count++;
            }
        }
        return count;
    }

    public void AddButtonTabCommands(Action<string> addTab)
    {
        for (int i = 0; i < numToName.Count; i++)
        {
            addTab("+" + numToName[i]);
            addTab("-" + numToName[i]);
        }
    }

    static string Clip(string text)
    {
        if (text.Length > 31)
            return text.Substring(0, 31);
        return text;
    }

    static string Lower(string value)
    {
        char[] chars = new char[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
            char ch = value[i];
            if (ch >= 'A' && ch <= 'Z')
                ch = (char)(ch + 32);
            chars[i] = ch;
        }
        return new string(chars);
    }
}
