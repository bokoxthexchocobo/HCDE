namespace HCDE.Playsim;

/// <summary>
/// Debugger stack frame from <c>StackFrameStateNode.cpp</c> 1-161. Chunk 1402.
/// Line lookup and the source cache stay callees. The file ends at line 161.
/// </summary>
public static class VmStackFrame1402Slice
{
    public const int BandStart = 1;
    public const int BandEnd = 161;
    public const int VarfNative = 1 << 3;
    public const string LocalName = "Local";
    public const string RegistersName = "Registers";
    public const string GlobalName = "Global";
    public const string CvarName = "CVars";

    public sealed class Function
    {
        public int VarFlags;
        public bool IsScript;
        public string? SourceFile = "";
        public string PrintableName = "";
        public Func<ulong, int>? PcToLine;
        public int[] LineInfo { get; set; } = [];
    }

    public sealed class Frame
    {
        public Function? Func;
        public Frame? Parent;
        public ulong Pc;
        public int NumRegD;
        public int NumRegF;
        public int NumRegS;
        public int NumRegA;
        public int MaxParam;
        public int NumParam;
    }

    public sealed class ScopeNode
    {
        public string Kind = "";
    }

    public sealed class Source
    {
        public string Path = "";
    }

    public sealed class ProtocolFrame
    {
        public int Id;
        public string Name = "";
        public Source? Source;
        public int? Line;
        public int? Column;
        public string? InstructionPointer;
    }

    public sealed class Node
    {
        public int Id;
        public Frame Stack { get; }
        public Frame Fake { get; } = new();
        public bool UsesFake;
        public ScopeNode? Local;
        public ScopeNode? Registers;
        public ScopeNode? Globals;
        public ScopeNode? Cvars;

        public Node(Function? function, Frame? parent)
        {
            Fake.Func = function;
            Fake.Parent = parent;
            Fake.Pc = 0;
            Fake.NumRegD = 0;
            Fake.NumRegF = 0;
            Fake.NumRegS = 0;
            Fake.NumRegA = 0;
            Fake.MaxParam = 0;
            Fake.NumParam = 0;
            Stack = Fake;
            UsesFake = true;
            Globals = new ScopeNode { Kind = GlobalName };
            Cvars = new ScopeNode { Kind = CvarName };
        }

        public Node(Frame stack)
        {
            if (stack == null)
                throw new ArgumentNullException(nameof(stack));
            Stack = stack;
            if (!IsNative(stack.Func))
            {
                if (IsScript(stack.Func))
                    Local = new ScopeNode { Kind = LocalName };
                Registers = new ScopeNode { Kind = RegistersName };
            }
            Globals = new ScopeNode { Kind = GlobalName };
            Cvars = new ScopeNode { Kind = CvarName };
        }

        public bool Serialize(ProtocolFrame frame, Func<string, Source, bool>? sourceData)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            frame.Id = Id;
            Function? function = Stack.Func;
            if (IsNative(function))
            {
                frame.Name = function!.PrintableName;
                return true;
            }
            if (IsScript(function) && function!.SourceFile != null)
            {
                if (sourceData == null)
                    throw new ArgumentNullException(nameof(sourceData));
                var source = new Source();
                if (sourceData(function.SourceFile, source))
                {
                    frame.Source = source;
                    if (Stack.Pc != 0)
                    {
                        int lineNumber = function.PcToLine == null ? 0 : function.PcToLine(Stack.Pc);
                        if (lineNumber > 0)
                        {
                            frame.Line = lineNumber;
                            frame.Column = 1;
                        }
                        else if (lineNumber == -1 && function.LineInfo.Length > 0)
                        {
                            int max = 0;
                            foreach (int line in function.LineInfo)
                            {
                                if (line > max)
                                    max = line;
                            }
                            frame.Line = max + 1;
                            frame.Column = 1;
                        }
                        frame.InstructionPointer = FormatPointer(Stack.Pc);
                    }
                }
            }
            if (function == null)
                throw new InvalidOperationException("function");
            frame.Name = function.PrintableName;
            return true;
        }

        public bool GetChildNames(List<string> names)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));
            if (IsScript(Stack.Func) && !IsNative(Stack.Func))
                names.Add(LocalName);
            names.Add(GlobalName);
            names.Add(CvarName);
            if (IsScript(Stack.Func) && !IsNative(Stack.Func))
                names.Add(RegistersName);
            return true;
        }

        public bool TryGetChild(string name, ref ScopeNode? node)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (Same(name, GlobalName))
            {
                node = Globals;
                return true;
            }
            if (Same(name, CvarName))
            {
                node = Cvars;
                return true;
            }
            if (IsNative(Stack.Func))
                return false;
            if (Same(name, RegistersName))
            {
                node = Registers;
                return true;
            }
            if (Same(name, LocalName) && IsScript(Stack.Func))
            {
                node = Local;
                return true;
            }
            return false;
        }
    }

    public static bool IsNative(Function? function)
    {
        return function != null && (function.VarFlags & VarfNative) != 0;
    }

    public static bool IsScript(Function? function)
    {
        return function != null && function.IsScript;
    }

    public static string FormatPointer(ulong address)
    {
        if (address == 0)
            return "(nil)";
        return "0x" + address.ToString("x");
    }

    public static bool Same(string left, string right)
    {
        if (left.Length != right.Length)
            return false;
        for (int i = 0; i < left.Length; i++)
        {
            if (Fold(left[i]) != Fold(right[i]))
                return false;
        }
        return true;
    }

    public static char Fold(char value)
    {
        if (value is >= 'A' and <= 'Z')
            return (char)(value + 32);
        return value;
    }
}
