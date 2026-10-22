namespace HCDE.Playsim;

/// <summary>ACS clientside script scaffolding from <c>p_acs.cpp</c> (669–944).</summary>
public static class VmPAcsIsClientSideScript
{
    public const int ScriptfClientside = 1;

    public static bool IsClientSideScript(int scriptFlags) =>
        (scriptFlags & ScriptfClientside) != 0;

    public static bool IsClientSideScript(in ScriptPtr script) =>
        IsClientSideScript(script.Flags);

    public readonly record struct ScriptPtr(int Flags);

    public static int UaLong(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 4) return 0;
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }

    public sealed class AcsStack
    {
        public int Sp { get; set; }
        public AcsStack? Next { get; private set; }
        public static AcsStack? Head { get; private set; }

        public AcsStack()
        {
            Next = Head;
            Head = this;
        }

        public void DisposeFromList()
        {
            if (Head == this)
                Head = Next;
        }
    }

    public sealed class LevelScript
    {
        public bool ClientSide { get; init; }
        public bool UseClientSideIterator { get; init; }

        public bool PickClientSideIterator(int tid, Func<int, bool> serverIter, Func<int, bool> clientIter) =>
            ClientSide || UseClientSideIterator
                ? clientIter(tid)
                : serverIter(tid);
    }

    public static float AcsToFloat(int acsVal) => acsVal / 65536f;

    public static int DoubleToAcs(double val) => (int)(val * 65536.0);

    public static int AngleToAcs(double degrees) => (int)(degrees * 65536.0 / 360.0);

    public sealed class AcsStringPool
    {
        public int Count { get; private set; }

        public int Intern(string value)
        {
            Count++;
            return value.GetHashCode();
        }
    }

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

    public readonly record struct CallReturn(
        int ReturnAddress,
        bool DiscardResult,
        uint EntryInstrCount);

    public static ScriptState AdvanceScriptState(ScriptState current, bool pleaseRemove) =>
        pleaseRemove ? ScriptState.PleaseRemove : current;

    public static bool ShouldUseClientRandom(bool clientSide) => clientSide;

    public static int ClientSideRandom(int min, int max, bool clientSide, Func<int, int> serverRng, Func<int, int> clientRng) =>
        min + (clientSide ? clientRng(max - min + 1) : serverRng(max - min + 1));

    public static double AcsToAngle(int acsVal) => acsVal / 65536.0 * 360.0;

    public static int PitchToAcs(double pitch) => (int)(pitch * 65536.0 / 360.0);

    public sealed class LevelScriptMetadata
    {
        public ScriptState State { get; set; } = ScriptState.Running;
        public int ScriptNumber { get; set; }
        public bool BackSide { get; set; }
        public int WrapWidth { get; set; } = 320;
        public bool HandleAspect { get; set; }
        public int HudWidth { get; set; } = 320;
        public int HudHeight { get; set; } = 200;
        public int ClipRectLeft { get; set; }
        public int ClipRectTop { get; set; }
        public int ClipRectWidth { get; set; }
        public int ClipRectHeight { get; set; }

        public void StringBuilderStart(List<string> stack, ref string builder)
        {
            if (!string.IsNullOrEmpty(builder) || stack.Count > 0)
            {
                stack.Add(builder);
                builder = "";
            }
        }

        public bool StringBuilderFinish(List<string> stack, ref string builder)
        {
            if (stack.Count == 0) return false;
            builder = stack[^1];
            stack.RemoveAt(stack.Count - 1);
            return true;
        }
    }

    public static bool StackPush(AcsStack stack, int value, Span<int> buffer, ref int sp)
    {
        if (sp >= buffer.Length) return false;
        buffer[sp++] = value;
        stack.Sp = sp;
        return true;
    }

    public static bool StackPop(AcsStack stack, Span<int> buffer, ref int sp, out int value)
    {
        value = 0;
        if (sp <= 0) return false;
        value = buffer[--sp];
        stack.Sp = sp;
        return true;
    }

    public static int NormalizeWorldVarIndex(int index, int max) =>
        index < 0 || index >= max ? 0 : index;

    public static bool BehaviorArrayValid(int size, int[]? elements) =>
        size >= 0 && (size == 0 || elements != null);

    public static int FixedMul(int a, int b) => (int)((long)a * b >> 16);

    public static bool ScriptControllerIsClientSide(bool clientSideFlag, bool levelHasClientThinker) =>
        clientSideFlag && levelHasClientThinker;
}
