namespace HCDE.Playsim;

/// <summary>
/// ACS fixed-point conversions, clientside guards, uallong, and level-script helpers
/// from <c>p_acs.cpp</c> (471–760, 828–839).
/// </summary>
public static class VmPAcsScriptHelpers
{
    public const int LineFront = 0;
    public const int LineBack = 1;
    public const int SideFront = 0;
    public const int SideBack = 1;
    public const int GameSinglePlayer = 0;
    public const int GameNetCooperative = 1;
    public const int GameNetDeathmatch = 2;
    public const int ScriptfClientside = 1;

    public static double AcsToDouble(int acsVal) => acsVal / 65536.0;
    public static float AcsToFloat(int acsVal) => acsVal / 65536f;
    public static int DoubleToAcs(double val) => (int)(val * 65536.0);
    public static double AcsToAngle(int acsVal) => acsVal / 65536.0 * 360.0;
    public static int AngleToAcs(double degrees) => (int)(degrees / 360.0 * 65536.0);
    public static int PitchToAcs(double pitch) => (int)(pitch / 360.0 * 65536.0);

    public static int UaLong(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 4) return 0;
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }

    public static int UaLongAligned(int value) => value;

    public static bool IsClientSideScript(int flags) => (flags & ScriptfClientside) != 0;

    public sealed class Activator
    {
        public PlayerRef? Player { get; set; }
        public bool ClientSide { get; set; }
    }

    public sealed class PlayerRef
    {
        public Activator? Mo { get; set; }
    }

    public sealed class LevelRef
    {
        public Func<Activator, bool>? IsConsolePlayer { get; set; }
    }

    public static Activator? GetScriptOwner(Activator activator) =>
        activator.Player?.Mo ?? activator;

    public static bool ShouldIgnoreClientSideScript(Activator? activator, LevelRef? level)
    {
        if (activator is null || !activator.ClientSide)
            return false;
        var owner = GetScriptOwner(activator);
        if (owner?.Player is null)
            return false;
        return level?.IsConsolePlayer?.Invoke(owner) == false;
    }

    public readonly record struct CallReturn(
        int ReturnAddress,
        bool DiscardResult,
        uint EntryInstrCount);

    public enum ScriptState
    {
        Running,
        Suspended,
        Delayed,
        TagWait,
        PolyWait,
        ScriptWaitPre,
        ScriptWait,
        PleaseRemove,
        DivideBy0,
        ModulusBy0,
    }

    public sealed class LevelScript
    {
        public ScriptState State { get; set; } = ScriptState.Running;
        public bool ClientSide { get; set; }
        public List<int> LocalVars { get; } = [];
        public List<string> StringBuilderStack { get; } = [];

        public void SetState(ScriptState state) => State = state;
        public ScriptState GetState() => State;

        public void MarkLocalVarStrings(Action<IReadOnlyList<int>> mark) =>
            mark(LocalVars);

        public void LockLocalVarStrings(int level, Action<int, IReadOnlyList<int>> locker) =>
            locker(level, LocalVars);

        public void StringBuilderStart(ref string builder)
        {
            if (!string.IsNullOrEmpty(builder) || StringBuilderStack.Count > 0)
            {
                StringBuilderStack.Add(builder);
                builder = "";
            }
        }

        public void StringBuilderFinish(ref string builder)
        {
            if (StringBuilderStack.Count == 0)
            {
                builder = "";
                return;
            }

            builder = StringBuilderStack[^1];
            StringBuilderStack.RemoveAt(StringBuilderStack.Count - 1);
        }

        public bool PickActorIterator(int tid, Func<int, bool> server, Func<int, bool> client) =>
            ClientSide ? client(tid) : server(tid);
    }
}
