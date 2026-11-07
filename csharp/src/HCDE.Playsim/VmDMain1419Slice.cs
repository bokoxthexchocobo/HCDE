using System.Globalization;
using System.Text;

namespace HCDE.Playsim;
/// <summary>
/// Capture, mouse dispatch, viewport, and crash text from d_main.cpp 3321-3642.
/// DoomButtons ends at line 3316. System_M_Dim is declared at line 3644.
/// </summary>
public static class VmDMain1419Slice
{
    public const int BandStart = 3321;
    public const int BandEnd = 3642;
    public const int ButtonsEnd = 3316;
    public const int DimLine = 3644;
    public const int DebugPort = 19021;
    public const int CenterView = 36;
    public const int Doom = 1;
    public const int Strife = 8;
    public const int Shareware = 2;
    public const int MenuOff = 0;
    public const int MenuOn = 1;
    public const int MenuWait = 2;
    public const int MenuNoPause = 3;
    public const int ConsoleUp = 0;
    public const int ConsoleDown = 1;
    public const int ConsoleFalling = 2;
    public const int Level = 0;
    public const int DemoScreen = 3;
    public const int TitleLevel = 7;
    public const int Cutscene = 9;
    public const int MouseEvent = 3;
    public const int Mlook = 21;
    public const int Strafe = 26;
    public const int ChanBody = 4;
    public const int ChanUi = 32;
    public const int ChanRumble = 0x40000;
    public const int ChanNoRumble = 0x80000;
    public const float AttenNone = 0f;
    public static readonly string?[] GameNames =
    {
        null, "Doom", "Heretic", null, "Hexen", null, null, null, "Strife",
        null, null, null, null, null, null, null, "Chex",
    };
    public sealed class Ev
    {
        public int Type;
        public int X;
        public int Y;
    }
    public sealed class LevelInfo
    {
        public string? Lookup;
        public string Name = "";
        public bool Automap;
    }
    public sealed class Scene
    {
        public int Left;
        public int Top;
        public int Width;
        public int Height;
        public int BlockWidth;
    }
    public sealed class CrashBuf
    {
        public char[] Data = Array.Empty<char>();
        public int Cursor;
        public int? End;
        public bool Null;
        public int Fail;
    }
    public sealed class State
    {
        public int Capture = 1;
        public bool VmDebug;
        public bool DebugServer;
        public int DebugStops;
        public List<string> Lines = new();
        public bool Lookspring = true;
        public List<int> Net = new();
        public int GameType = Doom;
        public int Flags;
        public int Menu;
        public int Console;
        public bool Chat;
        public bool HasLevel = true;
        public bool UiProcessors;
        public int GameState = Level;
        public bool Paused;
        public bool Demo;
        public bool Handled;
        public bool FreeLook;
        public bool LookStrafe;
        public bool NetGame;
        public int MoveDebug;
        public float Pitch = 1f;
        public float Yaw = 1f;
        public int GameTic;
        public int ClientTic;
        public HashSet<int> Buttons = new();
        public List<(int Kind, int Amount)> View = new();
        public List<string> Traces = new();
        public List<Ev> Events = new();
        public bool RequireMouse;
        public bool Haptics;
        public List<(int Channel, int Flags, string Name, float Volume, float Atten)> Sounds = new();
        public bool GenericUi;
        public int LogFont;
        public int DlgFont;
        public bool Multiplayer;
        public bool MenuObject;
        public bool Conversation;
        public bool Hardware = true;
        public bool IntelMipmap;
        public bool StatusBar;
        public bool Automap;
        public List<string> Screen = new();
        public int Blocks = 10;
        public int ScreenW = 320;
        public int ScreenH = 200;
        public int ViewX;
        public int ViewY;
        public int ViewW;
        public int ViewH;
        public string? ViewMap;
        public string LevelName = "";
        public double X;
        public double Y;
        public double Z;
        public double Angle;
        public double PitchDeg;
        public ulong Fps;
        public List<string> Names = new();
        public bool HudToggled;
        public int HudToggles;
        public List<LevelInfo> Levels = new();
        public int Titles;
        public string Version = "";
        public string Hash = "";
        public List<string>? Args;
        public List<string> Wads = new();
        public string? Map;
        public bool ViewActive;
        public double ViewPosX;
        public double ViewPosY;
        public double ViewPosZ;
        public double ViewYaw;
        public string CrashSuffix = "";
        public List<(int Remaining, string Line)> CrashCalls = new();
    }
    public static int ClampCapture(int self)
    {
        if (self < 0)
            return 0;
        if (self > 2)
            return 2;
        return self;
    }
    public static void VmDebugChanged(State state, bool value)
    {
        state.VmDebug = value;
        if (value == false)
        {
            if (state.DebugServer)
            {
                state.DebugStops++;
                state.DebugServer = false;
            }
        }
        else
        {
            state.Lines.Add("You must restart HCDE for this change to take effect.\n");
            state.Lines.Add("Note that enabling the debug server will disable JIT compilation.\n");
        }
    }
    public static void MlookRelease(State state)
    {
        if (state.Lookspring)
            state.Net.Add(CenterView);
    }
    public static bool ValidFilter(State state, string? text)
    {
        if (state.GameType == Strife && (state.Flags & Shareware) != 0 && Same(text, "strifeteaser"))
            return true;
        return state.GameType == 0 || Same(text, NameOf(state.GameType));
    }
    public static bool WantGuiCapture(State state)
    {
        bool want;
        if (state.Menu == MenuOff)
            want = state.Console == ConsoleDown || state.Console == ConsoleFalling || state.Chat;
        else
            want = state.Menu == MenuOn || state.Menu == MenuNoPause;
        if (!want && Processors(state))
            want = true;
        return want;
    }
    public static bool WantLeftButton(State state) =>
        state.GameState == DemoScreen || state.GameState == TitleLevel;
    public static bool Dispatch(State state, Ev ev)
    {
        state.Events.Add(ev);
        if (ev.Type == MouseEvent && state.Menu == MenuOff && state.Console != ConsoleDown && state.Console != ConsoleFalling && !state.Handled && !state.Paused)
        {
            int rawX = ev.X;
            int rawY = ev.Y;
            if (state.Buttons.Contains(Mlook) || state.FreeLook)
            {
                int look = (int)(ev.Y * state.Pitch * 16.0);
                if (state.NetGame && state.MoveDebug > 0 && look != 0)
                {
                    state.Traces.Add(Format(
                        "HCDE direct-mouse pitch gametic=%d clienttic=%d raw=(%d,%d) look=%d m_pitch=%.4f freelook=%d mlook=%d",
                        state.GameTic, state.ClientTic, rawX, rawY, look, state.Pitch, state.FreeLook ? 1 : 0, state.Buttons.Contains(Mlook) ? 1 : 0));
                }
                state.View.Add((1, look));
                ev.Y = 0;
            }
            if (!state.Buttons.Contains(Strafe) && !state.LookStrafe)
            {
                int turn = (int)(ev.X * state.Yaw * 16.0);
                if (state.NetGame && state.MoveDebug > 0 && turn != 0)
                {
                    state.Traces.Add(Format(
                        "HCDE direct-mouse yaw gametic=%d clienttic=%d raw=(%d,%d) turn=%d m_yaw=%.4f strafe=%d lookstrafe=%d",
                        state.GameTic, state.ClientTic, rawX, rawY, turn, state.Yaw, state.Buttons.Contains(Strafe) ? 1 : 0, state.LookStrafe ? 1 : 0));
                }
                state.View.Add((0, turn));
                ev.X = 0;
            }
            if (ev.X == 0 && ev.Y == 0)
                return true;
        }
        return false;
    }
    public static bool NetGame(State state) => state.NetGame;
    public static bool WantNativeMouse(State state)
    {
        if (!state.HasLevel)
            throw new NullReferenceException();
        return state.RequireMouse;
    }
    public static bool CaptureInGame(State state)
    {
        if (state.Demo || state.Paused)
            return false;
        switch (state.Capture)
        {
            default:
            case 0:
                return state.GameState == Level;
            case 1:
                return state.GameState == Level || state.GameState == Cutscene;
            case 2:
                return true;
        }
    }
    public static void PlayStartupSound(State state, string name)
    {
        int flags = ChanUi | (state.Haptics ? ChanRumble : ChanNoRumble);
        state.Sounds.Add((ChanBody, flags, name, 1f, AttenNone));
    }
    public static bool SpecialUi(State state) =>
        state.GenericUi || state.LogFont != 0 || state.DlgFont != 0 || state.Console != ConsoleUp || state.Multiplayer
        || (state.Menu == MenuOn && state.MenuObject && !state.Conversation);
    public static bool DisableTextureFilter(State state) => !state.Hardware;
    public static bool DisableAniso(State state) => state.IntelMipmap;
    public static void ScreenSizeChanged(State state)
    {
        if (state.StatusBar)
            state.Screen.Add("statusbar");
        state.Screen.Add("crosshair");
        if (state.HasLevel && state.Automap)
            state.Screen.Add("automap");
    }
    public static Scene SceneRect(State state)
    {
        int height;
        int width;
        if (state.Blocks >= 10)
        {
            height = state.ScreenH;
            width = state.ScreenW;
        }
        else
        {
            height = (state.Blocks * state.ScreenH / 10) & ~7;
            width = state.Blocks * state.ScreenW / 10;
        }
        return new Scene
        {
            Left = state.ViewX,
            Top = state.ScreenH - (height + state.ViewY - ((height - state.ViewH) / 2)),
            Width = state.ViewW,
            Height = height,
            BlockWidth = width,
        };
    }
    public static string Location(State state)
    {
        if (state.ViewMap == null)
            return "";
        return Format(
            "Map %s: \"%s\",\nx = %1.4f, y = %1.4f, z = %1.4f, angle = %1.4f, pitch = %1.4f\n%llu fps\n\n",
            state.ViewMap, state.LevelName, state.X, state.Y, state.Z, state.Angle, state.PitchDeg, state.Fps);
    }
    public static string PlayerName(State state, int node) => state.Names[node];
    public static void ConsoleToggled(State state, int console)
    {
        if (console == ConsoleFalling && state.HudToggled)
            state.HudToggles++;
    }
    public static void LanguageChanged(State state, string? lang)
    {
        _ = lang;
        foreach (LevelInfo level in state.Levels)
        {
            if (level.Lookup != null)
                level.Name = level.Lookup;
        }
        state.Titles++;
    }
    public static void AppendCrashText(CrashBuf buf, string? format, params object[] args)
    {
        if (buf.Null || buf.End == null)
            return;
        int end = buf.End.Value;
        if (buf.Cursor >= end)
        {
            buf.Data[end] = '\0';
            buf.Cursor = end;
            return;
        }
        int remaining = end - buf.Cursor;
        string text = buf.Fail > 0 ? "" : Format(format, args);
        int result = buf.Fail > 0 ? -1 : text.Length;
        if (buf.Fail > 0)
            buf.Fail--;
        if (result < 0)
        {
            buf.Data[buf.Cursor] = '\0';
            return;
        }
        int count = remaining;
        int keep = count == 0 ? 0 : Math.Min(text.Length, count - 1);
        for (int i = 0; i < keep; i++)
            buf.Data[buf.Cursor + i] = text[i];
        if (count > 0)
            buf.Data[buf.Cursor + keep] = '\0';
        if ((uint)result >= (uint)remaining)
        {
            buf.Cursor = end;
            buf.Data[buf.Cursor] = '\0';
            return;
        }
        buf.Cursor += result;
    }
    public static void CrashInfo(State state, char[]? buffer, int len, string? linefeed)
    {
        if (buffer == null || len == 0)
            return;
        buffer[0] = '\0';
        if (len < 3)
            return;
        var buf = new CrashBuf { Data = buffer, End = len - 2 };
        string lf = linefeed ?? "\n";
        AppendCrashText(buf, "HCDE version %s (%s)", state.Version, state.Hash);
        AppendCrashText(buf, "%sCommand line:", lf);
        if (state.Args != null)
        {
            foreach (string arg in state.Args)
                AppendCrashText(buf, " %s", arg);
        }
        for (int i = 0; i < state.Wads.Count; i++)
            AppendCrashText(buf, "%sWad %d: %s", lf, i, state.Wads[i]);
        if (state.GameState != Level && state.GameState != TitleLevel)
            AppendCrashText(buf, "%s%sNot in a level.", lf, lf);
        else
        {
            AppendCrashText(buf, "%s%sCurrent map: %s", lf, lf, state.Map ?? "<unknown>");
            if (!state.ViewActive)
                AppendCrashText(buf, "%s%sView not active.", lf, lf);
            else
            {
                AppendCrashText(buf, "%s%sviewx = %f", lf, lf, state.ViewPosX);
                AppendCrashText(buf, "%sviewy = %f", lf, state.ViewPosY);
                AppendCrashText(buf, "%sviewz = %f", lf, state.ViewPosZ);
                AppendCrashText(buf, "%sviewangle = %f", lf, state.ViewYaw);
            }
        }
        state.CrashCalls.Add((buf.End.Value - buf.Cursor, lf));
        if (state.CrashSuffix.Length > 0)
            AppendCrashText(buf, "%s", state.CrashSuffix);
        AppendCrashText(buf, "%s", lf);
        if (buf.Cursor < buf.Data.Length)
            buf.Data[buf.Cursor] = '\0';
    }
    public static string Text(char[] data)
    {
        int n = 0;
        while (n < data.Length && data[n] != '\0')
            n++;
        return new string(data, 0, n);
    }
    static bool Processors(State state)
    {
        if (!state.HasLevel)
            throw new NullReferenceException();
        return state.UiProcessors;
    }
    static string? NameOf(int game) => game >= 0 && game < GameNames.Length ? GameNames[game] : null;
    static bool Same(string? left, string? right)
    {
        if (left == null || right == null)
            throw new NullReferenceException();
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }
    static string Format(string? format, params object[] args)
    {
        if (format == null)
            return "";
        var built = new StringBuilder();
        int arg = 0;
        for (int i = 0; i < format.Length; i++)
        {
            if (format[i] != '%')
            {
                built.Append(format[i]);
                continue;
            }
            i++;
            if (i < format.Length && format[i] == '%')
            {
                built.Append('%');
                continue;
            }
            if (i < format.Length && format[i] == '1')
                i++;
            if (i < format.Length && format[i] == '.')
            {
                i++;
                int prec = format[i] - '0';
                i++;
                double value = Convert.ToDouble(args[arg++], CultureInfo.InvariantCulture);
                built.Append(value.ToString("0." + new string('0', prec), CultureInfo.InvariantCulture));
                continue;
            }
            char spec = format[i];
            if (spec == 's')
                built.Append(args[arg++]);
            else if (spec == 'd')
                built.Append(Convert.ToInt32(args[arg++], CultureInfo.InvariantCulture));
            else if (spec == 'f')
                built.Append(Convert.ToDouble(args[arg++], CultureInfo.InvariantCulture).ToString("0.000000", CultureInfo.InvariantCulture));
            else if (spec == 'l')
            {
                while (format[i] != 'u')
                    i++;
                built.Append(Convert.ToUInt64(args[arg++], CultureInfo.InvariantCulture));
            }
        }
        return built.ToString();
    }
}
