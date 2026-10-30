using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// OpenGL debug groups, the message log, and the breakpoint callback from
/// <c>gl_debug.cpp</c> (1-365). Chunk 1305.
/// </summary>
public sealed class VmGlDebug1305
{
    public const int DebugFlag = 128;
    public const int GlQueryResult = 0x8866;
    public const int GlTimeElapsed = 0x88BF;
    public const int GlDebugOutputSynchronous = 0x8242;
    public const int GlDontCare = 0x1100;
    public const int GlMaxDebugMessageLength = 0x9143;
    public const int GlSeverityHigh = 0x9146;
    public const int GlSeverityMedium = 0x9147;
    public const int GlSeverityLow = 0x9148;
    public const int GlSeverityNotification = 0x826B;
    public const int GlSourceApi = 0x8246;
    public const int GlSourceWindowSystem = 0x8247;
    public const int GlSourceShaderCompiler = 0x8248;
    public const int GlSourceThirdParty = 0x8249;
    public const int GlSourceApplication = 0x824A;
    public const int GlSourceOther = 0x824B;
    public const int GlTypeError = 0x824C;
    public const int GlTypeDeprecated = 0x824D;
    public const int GlTypeUndefined = 0x824E;
    public const int GlTypePortability = 0x824F;
    public const int GlTypePerformance = 0x8250;
    public const int GlTypeOther = 0x8251;
    public const int GlTypeMarker = 0x8268;
    public const int GlTypePushGroup = 0x8269;
    public const int GlTypePopGroup = 0x826A;
    public const int MaxPrintedMessages = 50;

    public static readonly List<(string Name, uint Handle)> TimeQueries = new();
    public static int MessagesPrinted;
    public static readonly HashSet<string> Seen = new(StringComparer.Ordinal);

    public static void ResetProcess()
    {
        TimeQueries.Clear();
        MessagesPrinted = 0;
        Seen.Clear();
    }

    public uint CurrentLevel;
    public bool BreakpointMode;
    readonly Host _host;

    public VmGlDebug1305(Host host)
    {
        _host = host;
    }

    public static bool HasDebugApi(Host host)
    {
        return (host.Flags & DebugFlag) != 0;
    }

    public static void OnGlDebugLevelChanged(Host host)
    {
        if (!HasDebugApi(host))
            host.Print("No OpenGL debug support detected.\n");
    }

    public void Update()
    {
        _host.GpuStatOutput = "";
        foreach ((string name, uint handle) in TimeQueries)
        {
            uint elapsed = _host.GetQuery(handle, GlQueryResult);
            _host.DeleteQuery(handle);
            _host.GpuStatOutput += FormatElapsed(name, elapsed);
        }
        TimeQueries.Clear();
        _host.GpuStatActive = _host.KeepGpuStatActive;
        _host.KeepGpuStatActive = false;
        if (!HasDebugApi(_host))
            return;
        SetupBreakpointMode();
        UpdateLoggingLevel();
        OutputMessageLog();
    }

    public static void LabelObject(Host host, int type, uint handle, string name)
    {
        if (HasDebugApi(host) && host.DebugLevel != 0)
            host.Label(type, handle, -1, name);
    }

    public static void LabelObjectPtr(Host host, nint ptr, string name)
    {
        if (HasDebugApi(host) && host.DebugLevel != 0)
            host.LabelPtr(ptr, -1, name);
    }

    public static void PushGroup(Host host, string name)
    {
        if (HasDebugApi(host) && host.DebugLevel != 0)
            host.Push(GlSourceApplication, 0, name.Length, name);
        if (host.GpuStatActive)
        {
            uint handle = host.GenQuery();
            host.BeginQuery(GlTimeElapsed, handle);
            TimeQueries.Add((name, handle));
        }
    }

    public static void PopGroup(Host host)
    {
        if (HasDebugApi(host) && host.DebugLevel != 0)
            host.Pop();
        if (host.GpuStatActive)
            host.EndQuery(GlTimeElapsed);
    }

    public void SetupBreakpointMode()
    {
        if (BreakpointMode != _host.Breakpoint)
        {
            if (_host.Breakpoint)
            {
                _host.SetCallback(true);
                _host.Enable(GlDebugOutputSynchronous);
            }
            else
            {
                _host.SetCallback(false);
                _host.Disable(GlDebugOutputSynchronous);
            }
            BreakpointMode = _host.Breakpoint;
        }
    }

    public void UpdateLoggingLevel()
    {
        uint level = unchecked((uint)_host.DebugLevel);
        if (level != CurrentLevel)
        {
            Control(GlSeverityHigh, level > 0);
            Control(GlSeverityMedium, level > 1);
            Control(GlSeverityLow, level > 2);
            Control(GlSeverityNotification, level > 3);
            CurrentLevel = level;
        }
    }

    public bool IsFiltered(int severity)
    {
        int severityLevel = 0;
        switch (severity)
        {
            case GlSeverityHigh: severityLevel = 1; break;
            case GlSeverityMedium: severityLevel = 2; break;
            case GlSeverityLow: severityLevel = 3; break;
            case GlSeverityNotification: severityLevel = 4; break;
        }
        return severityLevel > _host.DebugLevel;
    }

    public void OutputMessageLog()
    {
        if (CurrentLevel <= 0)
            return;
        int maxLength = _host.GetInteger(GlMaxDebugMessageLength);
        const int maxMessages = 50;
        int messageLogSize = maxMessages * maxLength;
        while (true)
        {
            IReadOnlyList<DebugMsg> batch = _host.GetDebugMessageLog(maxMessages, messageLogSize);
            if (batch.Count <= 0)
                break;
            string chars = string.Concat(batch.Select(message => message.Text));
            int offset = 0;
            for (int i = 0; i < batch.Count; i++)
            {
                DebugMsg message = batch[i];
                if (!IsFiltered(message.Severity))
                {
                    string slice = offset < chars.Length ? chars[offset..] : "";
                    PrintMessage(_host, message.Source, message.Type, message.Id, message.Severity, message.Length, slice);
                }
                offset += message.Length;
            }
        }
    }

    public static void PrintMessage(Host host, int source, int type, uint id, int severity, int length, string message)
    {
        _ = id;
        if (type == GlTypePushGroup || type == GlTypePopGroup)
            return;
        if (MessagesPrinted > MaxPrintedMessages)
            return;
        string text = CString(message, length);
        if (!Seen.Add(text))
            return;
        MessagesPrinted++;
        if (MessagesPrinted == MaxPrintedMessages)
            host.Print("Max OpenGL debug messages reached. Suppressing further output.\n");
        else if (MessagesPrinted < MaxPrintedMessages)
        {
            string sourceText = SourceToString(source);
            string typeText = TypeToString(type);
            string severityText = SeverityToString(severity);
            if (type != GlTypeOther)
                host.Print("[" + sourceText + "] " + severityText + ", " + typeText + ": " + text + "\n");
            else
                host.Print("[" + sourceText + "] " + severityText + ": " + text + "\n");
        }
    }

    public void DebugCallback(int source, int type, uint id, int severity, int length, string message)
    {
        if (IsFiltered(severity))
            return;
        PrintMessage(_host, source, type, id, severity, length, message);
        if (severity != GlSeverityNotification)
            throw new InvalidOperationException("assert severity == GL_DEBUG_SEVERITY_NOTIFICATION");
    }

    public static string SourceToString(int source)
    {
        switch (source)
        {
            case GlSourceApi: return "api";
            case GlSourceWindowSystem: return "window system";
            case GlSourceShaderCompiler: return "shader compiler";
            case GlSourceThirdParty: return "third party";
            case GlSourceApplication: return "application";
            case GlSourceOther: return "other";
            default: return source.ToString(CultureInfo.InvariantCulture);
        }
    }

    public static string TypeToString(int type)
    {
        switch (type)
        {
            case GlTypeError: return "error";
            case GlTypeDeprecated: return "deprecated";
            case GlTypeUndefined: return "undefined";
            case GlTypePortability: return "portability";
            case GlTypePerformance: return "performance";
            case GlTypeMarker: return "marker";
            case GlTypePushGroup: return "push group";
            case GlTypePopGroup: return "pop group";
            case GlTypeOther: return "other";
            default: return type.ToString(CultureInfo.InvariantCulture);
        }
    }

    public static string SeverityToString(int severity)
    {
        switch (severity)
        {
            case GlSeverityLow: return "low severity";
            case GlSeverityMedium: return "medium severity";
            case GlSeverityHigh: return "high severity";
            case GlSeverityNotification: return "notification";
            default: return severity.ToString(CultureInfo.InvariantCulture);
        }
    }

    public static string FormatElapsed(string name, uint nanoseconds)
    {
        float milliseconds = nanoseconds / 1000000f;
        int cents = (int)Math.Round((double)milliseconds * 100.0, MidpointRounding.AwayFromZero);
        if (cents < 0)
            cents = 0;
        string number = (cents / 100).ToString(CultureInfo.InvariantCulture)
            + "."
            + (cents % 100).ToString("00", CultureInfo.InvariantCulture);
        if (number.Length < 4)
            number = number.PadLeft(4, '0');
        return name + "=" + number + " ms\n";
    }

    static string CString(string message, int length)
    {
        if (length <= 0 || message.Length == 0)
            return "";
        int count = Math.Min(length, message.Length);
        int nul = message.IndexOf('\0', 0, count);
        return nul >= 0 ? message[..nul] : message[..count];
    }

    void Control(int severity, bool enabled)
    {
        _host.Control(GlDontCare, GlDontCare, severity, 0, enabled);
    }

    public readonly record struct DebugMsg(int Source, int Type, uint Id, int Severity, int Length, string Text);

    public sealed class Host
    {
        public int Flags;
        public int DebugLevel;
        public bool Breakpoint;
        public bool GpuStatActive;
        public bool KeepGpuStatActive;
        public string GpuStatOutput = "";
        public int MaxDebugMessageLength;
        public uint NextQuery = 1;
        public bool Callback;
        public readonly Dictionary<uint, uint> Results = new();
        public readonly List<DebugMsg> Pending = new();
        public readonly List<string> Steps = new();
        public readonly List<string> Prints = new();

        public void Print(string line) => Prints.Add(line);

        public void Label(int type, uint handle, int length, string name)
        {
            Steps.Add("label:" + type + ":" + handle + ":" + length + ":" + name);
        }

        public void LabelPtr(nint ptr, int length, string name)
        {
            Steps.Add("ptr:" + ptr + ":" + length + ":" + name);
        }

        public void Push(int source, uint id, int length, string name)
        {
            Steps.Add("push:" + source + ":" + id + ":" + length + ":" + name);
        }

        public void Pop() => Steps.Add("pop");

        public void SetCallback(bool enabled)
        {
            Callback = enabled;
            Steps.Add(enabled ? "callback:this" : "callback:null");
        }

        public void Enable(int cap) => Steps.Add("enable:" + cap);
        public void Disable(int cap) => Steps.Add("disable:" + cap);

        public void Control(int source, int type, int severity, int count, bool enabled)
        {
            Steps.Add("control:" + source + ":" + type + ":" + severity + ":" + count + ":" + (enabled ? "1" : "0"));
        }

        public uint GenQuery()
        {
            uint id = NextQuery++;
            Steps.Add("gen:" + id);
            return id;
        }

        public void BeginQuery(int target, uint id) => Steps.Add("begin:" + target + ":" + id);
        public void EndQuery(int target) => Steps.Add("end:" + target);

        public uint GetQuery(uint id, int pname)
        {
            Steps.Add("result:" + id + ":" + pname);
            return Results.TryGetValue(id, out uint value) ? value : 0;
        }

        public void DeleteQuery(uint id) => Steps.Add("delete:" + id);

        public int GetInteger(int pname)
        {
            Steps.Add("get-int:" + pname);
            return MaxDebugMessageLength;
        }

        public IReadOnlyList<DebugMsg> GetDebugMessageLog(int count, int bufSize)
        {
            Steps.Add("get-log:" + count + ":" + bufSize);
            var batch = new List<DebugMsg>();
            int used = 0;
            while (batch.Count < count && Pending.Count > 0)
            {
                DebugMsg next = Pending[0];
                if (next.Length > bufSize - used)
                    break;
                Pending.RemoveAt(0);
                batch.Add(next);
                used += next.Length;
            }
            return batch;
        }
    }
}
