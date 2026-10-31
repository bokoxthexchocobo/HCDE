namespace HCDE.Playsim;

/// <summary>
/// Key-name table, double-click threshold, queued-press clear, and binding
/// execute from <c>c_bind.cpp</c> 1-182. Chunk 1391.
/// <c>C_TickQueuedInputs</c> starts at line 183 and stays outside this band.
/// The default name table is the non-Apple row this tree compiles. The Apple
/// row differs at 0xDB and 0xDC.
/// </summary>
public static class VmKeyBind1391Slice
{
    public const int NumKeys = 0x1CB;
    public const int KeyDblClicked = 0x8000;
    public const int TicRate = 35;
    public const uint SyntheticReleaseDelay = 1000u / TicRate;
    public const int DoubleClickDefault = 250;
    public const int CvarArchive = 1;
    public const int CvarGlobalConfig = 1 << 10;
    public const int DoubleClickFlags = CvarArchive | CvarGlobalConfig;

    public static readonly string?[] Names =
    {
        null, "Escape", "1", "2", "3", "4", "5", "6",
        "7", "8", "9", "0", "-", "=", "Backspace", "Tab",
        "Q", "W", "E", "R", "T", "Y", "U", "I",
        "O", "P", "[", "]", "Enter", "Ctrl", "A", "S",
        "D", "F", "G", "H", "J", "K", "L", ";",
        "'", "`", "Shift", "\\", "Z", "X", "C", "V",
        "B", "N", "M", ",", ".", "/", "RShift", "KP*",
        "Alt", "Space", "CapsLock", "F1", "F2", "F3", "F4", "F5",
        "F6", "F7", "F8", "F9", "F10", "NumLock", "Scroll", "KP7",
        "KP8", "KP9", "KP-", "KP4", "KP5", "KP6", "KP+", "KP1",
        "KP2", "KP3", "KP0", "KP.", null, null, "OEM102", "F11",
        "F12", null, null, null, null, null, null, null,
        null, null, null, null, "F13", "F14", "F15", "F16",
        "F17", "F18", "F19", "F20", "F21", "F22", "F23", "F24",
        "Kana", null, null, "Abnt_C1", null, null, null, null,
        null, "Convert", null, "NoConvert", null, "Yen", "Abnt_C2", null,
        null, null, null, null, null, null, null, null,
        null, null, null, null, null, "KP=", null, null,
        "Circumflex", "@", ":", "_", "Kanji", "Stop", "Ax", "Unlabeled",
        null, "PrevTrack", null, null, "KP-Enter", "RCtrl", null, null,
        "Mute", "Calculator", "Play", null, "Stop", null, null, null,
        null, null, null, null, null, null, "VolDown", null,
        "VolUp", null, "WebHome", "KP,", null, "KP/", null, "SysRq",
        "RAlt", null, null, null, null, null, null, null,
        null, null, null, null, null, "Pause", null, "Home",
        "UpArrow", "PgUp", null, "LeftArrow", null, "RightArrow", null, "End",
        "DownArrow", "PgDn", "Ins", "Del", null, null, null, null,
        null, null, null, "LWin", "RWin", "Apps", "Power", "Sleep",
        null, null, null, "Wake", null, "Search", "Favorites", "Refresh",
        "WebStop", "WebForward", "WebBack", "MyComputer", "Mail", "MediaSelect", null, null,
        null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null,
        "Mouse1", "Mouse2", "Mouse3", "Mouse4", "Mouse5", "Mouse6", "Mouse7", "Mouse8",
        "Joy1", "Joy2", "Joy3", "Joy4", "Joy5", "Joy6", "Joy7", "Joy8",
        "Joy9", "Joy10", "Joy11", "Joy12", "Joy13", "Joy14", "Joy15", "Joy16",
        "Joy17", "Joy18", "Joy19", "Joy20", "Joy21", "Joy22", "Joy23", "Joy24",
        "Joy25", "Joy26", "Joy27", "Joy28", "Joy29", "Joy30", "Joy31", "Joy32",
        "Joy33", "Joy34", "Joy35", "Joy36", "Joy37", "Joy38", "Joy39", "Joy40",
        "Joy41", "Joy42", "Joy43", "Joy44", "Joy45", "Joy46", "Joy47", "Joy48",
        "Joy49", "Joy50", "Joy51", "Joy52", "Joy53", "Joy54", "Joy55", "Joy56",
        "Joy57", "Joy58", "Joy59", "Joy60", "Joy61", "Joy62", "Joy63", "Joy64",
        "Joy65", "Joy66", "Joy67", "Joy68", "Joy69", "Joy70", "Joy71", "Joy72",
        "Joy73", "Joy74", "Joy75", "Joy76", "Joy77", "Joy78", "Joy79", "Joy80",
        "Joy81", "Joy82", "Joy83", "Joy84", "Joy85", "Joy86", "Joy87", "Joy88",
        "Joy89", "Joy90", "Joy91", "Joy92", "Joy93", "Joy94", "Joy95", "Joy96",
        "Joy97", "Joy98", "Joy99", "Joy100", "Joy101", "Joy102", "Joy103", "Joy104",
        "Joy105", "Joy106", "Joy107", "Joy108", "Joy109", "Joy110", "Joy111", "Joy112",
        "Joy113", "Joy114", "Joy115", "Joy116", "Joy117", "Joy118", "Joy119", "Joy120",
        "Joy121", "Joy122", "Joy123", "Joy124", "Joy125", "Joy126", "Joy127", "Joy128",
        "POV1Up", "POV1Right", "POV1Down", "POV1Left", "POV2Up", "POV2Right", "POV2Down", "POV2Left",
        "POV3Up", "POV3Right", "POV3Down", "POV3Left", "POV4Up", "POV4Right", "POV4Down", "POV4Left",
        "MWheelUp", "MWheelDown", "MWheelRight", "MWheelLeft", "Axis1Plus", "Axis1Minus", "Axis2Plus", "Axis2Minus",
        "Axis3Plus", "Axis3Minus", "Axis4Plus", "Axis4Minus", "Axis5Plus", "Axis5Minus", "Axis6Plus", "Axis6Minus",
        "Axis7Plus", "Axis7Minus", "Axis8Plus", "Axis8Minus", "LStickRight", "LStickLeft", "LStickDown", "LStickUp",
        "RStickRight", "RStickLeft", "RStickDown", "RStickUp", "DPadUp", "DPadDown", "DPadLeft", "DPadRight",
        "Pad_Start", "Pad_Back", "LThumb", "RThumb", "LShoulder", "RShoulder", "LTrigger", "RTrigger",
        "Pad_A", "Pad_B", "Pad_X", "Pad_Y", "Paddle_1", "Paddle_2", "Paddle_3", "Paddle_4",
        "Guide", "Pad_Misc", "Pad_Touchpad"
    };

    public static string? Name(int key, bool apple = false)
    {
        if ((uint)key >= NumKeys)
            throw new IndexOutOfRangeException(nameof(key));
        if (apple && key == 0xDB)
            return "Command";
        if (apple && key == 0xDC)
            return null;
        return Names[key];
    }

    public sealed class BindTable
    {
        public readonly string?[] Slots = new string?[NumKeys];
    }

    public static readonly BindTable Bindings = new();
    public static readonly BindTable DoubleBindings = new();
    public static readonly BindTable AutomapBindings = new();

    public sealed class PressQueue
    {
        public readonly bool[] Press = new bool[NumKeys];
        public readonly bool[] Release = new bool[NumKeys];
        public readonly string?[] Binding = new string?[NumKeys];
        public readonly uint[] Deadline = new uint[NumKeys];
        public readonly bool[] DoubleClicked = new bool[NumKeys];
        public readonly bool[] Delayed = new bool[NumKeys];

        public void Clear(uint key)
        {
            if (key >= NumKeys)
                throw new IndexOutOfRangeException(nameof(key));
            Press[key] = false;
            Release[key] = false;
            Binding[key] = null;
            Deadline[key] = 0;
        }
    }

    public readonly record struct Issued(string Command, uint Key);

    public static bool Exec(string? binding, uint key, bool keyup, bool doubleClick, List<Issued> issued)
    {
        if (string.IsNullOrEmpty(binding) || (keyup && binding[0] != '+'))
            return false;
        char[] chars = binding.ToCharArray();
        if (keyup)
            chars[0] = '-';
        uint code = doubleClick ? (key | KeyDblClicked) : key;
        issued.Add(new Issued(new string(chars), code));
        return true;
    }
}
