namespace HCDE.Playsim;

/// <summary>
/// Runtime event slots from <c>RuntimeEvents.cpp</c> 1-160. Chunk 1403.
/// Each slot keeps one handler. CreateStack and CleanupStack have no emit in this file.
/// </summary>
public static class VmRuntimeEvents1403Slice
{
    public const int BandStart = 1;
    public const int BandEnd = 160;

    public delegate void InstructionHandler(object? stack, object? ret, int count, object? pc);
    public delegate void CreateHandler(object? stack);
    public delegate void CleanupHandler(uint id);
    public delegate void LogHandler(int level, string? message);
    public delegate void BreakpointHandler(object breakpoint, string what);
    public delegate void ExceptionHandler(int reason, string message, string trace);
    public delegate bool DebuggerHandler();

    public sealed class Slot<T> where T : class
    {
        readonly object gate = new();
        bool active;
        T? handler;

        public Slot(string name) => Name = name;

        public string Name { get; }
        public List<string> Trace { get; } = new();

        public bool Active
        {
            get
            {
                lock (gate)
                    return active;
            }
        }

        public T? Subscribe(T? next)
        {
            lock (gate)
            {
                handler = next;
                active = next != null;
                Trace.Add("subscribed runtime event " + Name);
            }
            return next;
        }

        public bool Unsubscribe(T? handle)
        {
            if (handle == null)
                return false;
            lock (gate)
            {
                if (!active)
                    return false;
                active = false;
                handler = null;
                Trace.Add("unsubscribed runtime event " + Name);
                return true;
            }
        }

        public bool TryCopy(out T? copied, Action? between)
        {
            bool seen;
            lock (gate)
                seen = active;
            if (!seen)
            {
                copied = null;
                return false;
            }
            between?.Invoke();
            lock (gate)
            {
                if (!active || handler == null)
                {
                    copied = null;
                    return false;
                }
                copied = handler;
                return true;
            }
        }
    }

    public sealed class Bus
    {
        public Slot<InstructionHandler> Instruction { get; } = new("InstructionExecution");
        public Slot<CreateHandler> CreateStack { get; } = new("CreateStack");
        public Slot<CleanupHandler> CleanupStack { get; } = new("CleanupStack");
        public Slot<LogHandler> Log { get; } = new("Log");
        public Slot<BreakpointHandler> Breakpoint { get; } = new("BreakpointChanged");
        public Slot<ExceptionHandler> Exception { get; } = new("ExceptionThrown");
        public Slot<DebuggerHandler> Debugger { get; } = new("DebuggerEnabled");

        public void EmitBreakpoint(object breakpoint, string what, Action? between = null)
        {
            if (!Breakpoint.TryCopy(out BreakpointHandler? handler, between) || handler == null)
                return;
            handler(breakpoint, what);
        }

        public void EmitInstruction(object? stack, object? ret, int count, object? pc, Action? between = null)
        {
            if (!Instruction.TryCopy(out InstructionHandler? handler, between) || handler == null)
                return;
            handler(stack, ret, count, pc);
        }

        public void EmitLog(int level, string? message, Action? between = null)
        {
            if (!Log.TryCopy(out LogHandler? handler, between) || handler == null)
                return;
            handler(level, message);
        }

        public void EmitException(int reason, string message, string trace, Action? between = null)
        {
            if (!Exception.TryCopy(out ExceptionHandler? handler, between) || handler == null)
                return;
            handler(reason, message, trace);
        }

        public bool IsDebugServerRunning(Action? between = null)
        {
            if (!Debugger.TryCopy(out DebuggerHandler? handler, between) || handler == null)
                return false;
            return handler();
        }
    }
}
