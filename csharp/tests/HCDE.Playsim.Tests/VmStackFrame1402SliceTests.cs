using S = HCDE.Playsim.VmStackFrame1402Slice;

namespace HCDE.Playsim.Tests;

public class VmStackFrame1402SliceTests
{
    [Fact]
    public void NativeFrameUsesTheFakeStackAndSkipsLocalScopes()
    {
        var parent = new S.Frame();
        var function = new S.Function { VarFlags = S.VarfNative, PrintableName = "A_Native", SourceFile = "hidden.zc" };
        var node = new S.Node(function, parent);
        Assert.True(node.UsesFake);
        Assert.Same(node.Fake, node.Stack);
        Assert.Same(parent, node.Stack.Parent);
        Assert.Same(function, node.Stack.Func);
        Assert.Equal(0ul, node.Stack.Pc);
        Assert.Equal(0, node.Stack.NumRegD + node.Stack.NumRegA + node.Stack.NumParam);
        Assert.Null(node.Local);
        Assert.Null(node.Registers);
        Assert.NotSame(node.Globals, node.Cvars);
        node.Id = 9;
        var frame = new S.ProtocolFrame { Name = "old", Line = 4 };
        int calls = 0;
        Assert.True(node.Serialize(frame, (_, _) => { calls++; return true; }));
        Assert.Equal(0, calls);
        Assert.Equal(9, frame.Id);
        Assert.Equal("A_Native", frame.Name);
        Assert.Null(frame.Source);
        Assert.Equal(4, frame.Line);
        var names = new List<string> { "seed" };
        Assert.True(node.GetChildNames(names));
        Assert.Equal(new[] { "seed", "Global", "CVars" }, names);
        S.ScopeNode? child = new S.ScopeNode { Kind = "keep" };
        Assert.True(node.TryGetChild("global", ref child));
        Assert.Same(node.Globals, child);
        child = new S.ScopeNode { Kind = "keep" };
        Assert.False(node.TryGetChild("LOCAL", ref child));
        Assert.Equal("keep", child!.Kind);
        Assert.Equal("(nil)", S.FormatPointer(0));
        Assert.Equal("0xabc", S.FormatPointer(0xABC));
    }

    [Fact]
    public void ScriptFramePublishesTheLineAndKeepsRegistersForAPlainFunction()
    {
        var function = new S.Function
        {
            IsScript = true,
            PrintableName = "Tick",
            SourceFile = "a.zc",
            PcToLine = _ => 12,
            LineInfo = [3, 9, 9, 4],
        };
        var stack = new S.Frame { Func = function, Pc = 0xABC };
        var node = new S.Node(stack);
        Assert.False(node.UsesFake);
        Assert.NotNull(node.Local);
        Assert.NotNull(node.Registers);
        var frame = new S.ProtocolFrame();
        Assert.True(node.Serialize(frame, (file, source) =>
        {
            Assert.Equal("a.zc", file);
            source.Path = "maps/a.zc";
            return true;
        }));
        Assert.Equal("maps/a.zc", frame.Source!.Path);
        Assert.Equal(12, frame.Line);
        Assert.Equal(1, frame.Column);
        Assert.Equal("0xabc", frame.InstructionPointer);
        Assert.Equal("Tick", frame.Name);

        function.PcToLine = _ => 0;
        frame = new S.ProtocolFrame();
        node.Serialize(frame, (_, _) => true);
        Assert.Null(frame.Line);
        Assert.Null(frame.Column);
        Assert.Equal("0xabc", frame.InstructionPointer);

        function.PcToLine = _ => -1;
        frame = new S.ProtocolFrame();
        node.Serialize(frame, (_, _) => true);
        Assert.Equal(10, frame.Line);
        Assert.Equal(1, frame.Column);
        function.LineInfo = [-5, 0];
        frame = new S.ProtocolFrame();
        node.Serialize(frame, (_, _) => true);
        Assert.Equal(1, frame.Line);
        function.LineInfo = [];
        frame = new S.ProtocolFrame();
        node.Serialize(frame, (_, _) => true);
        Assert.Null(frame.Line);
        Assert.Equal("0xabc", frame.InstructionPointer);

        stack.Pc = 0;
        frame = new S.ProtocolFrame();
        node.Serialize(frame, (_, _) => true);
        Assert.Null(frame.InstructionPointer);
        Assert.Null(frame.Line);
        frame = new S.ProtocolFrame();
        node.Serialize(frame, (_, source) => { source.Path = "no"; return false; });
        Assert.Null(frame.Source);
        Assert.Equal("Tick", frame.Name);
        Assert.Throws<ArgumentNullException>(() => node.Serialize(new S.ProtocolFrame(), null));

        var names = new List<string>();
        node.GetChildNames(names);
        Assert.Equal(new[] { "Local", "Global", "CVars", "Registers" }, names);
        S.ScopeNode? child = null;
        Assert.True(node.TryGetChild("registers", ref child));
        Assert.Same(node.Registers, child);

        var plain = new S.Function { PrintableName = "plain", SourceFile = null };
        var other = new S.Node(new S.Frame { Func = plain });
        Assert.Null(other.Local);
        Assert.NotNull(other.Registers);
        names.Clear();
        other.GetChildNames(names);
        Assert.Equal(new[] { "Global", "CVars" }, names);
        child = new S.ScopeNode { Kind = "stay" };
        Assert.True(other.TryGetChild("Registers", ref child));
        Assert.Same(other.Registers, child);
        child = new S.ScopeNode { Kind = "stay" };
        Assert.False(other.TryGetChild("Local", ref child));
        Assert.Equal("stay", child!.Kind);
        var published = new S.ProtocolFrame();
        Assert.True(other.Serialize(published, (_, _) => throw new InvalidOperationException("cache")));
        Assert.Equal("plain", published.Name);
        Assert.Throws<ArgumentNullException>(() => new S.Node((S.Frame)null!));
    }
}
