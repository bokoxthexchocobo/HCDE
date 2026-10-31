using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmRuntimeState1340SliceTests
{
    [Fact]
    public void PathsIdsAndChildWindow()
    {
        var state = new VmRuntimeState1340(new VmRuntimeState1340.IdProvider());
        Assert.Equal("1", state.GetPathById(1));
        Assert.False(state.ResolveStateById(1, out _));
        Assert.True(state.ResolveStateByPath("1", out VmRuntimeState1340.StateNode? root));
        Assert.Same(state.Root, root);
        Assert.Equal(1, root!.GetId());
        Assert.True(state.ResolveStateByPath("+1", out VmRuntimeState1340.StateNode? plus));
        Assert.Same(state.Root, plus);
        Assert.True(state.ResolveStateByPath("  2abc", out VmRuntimeState1340.StateNode? stack2));
        var other = Assert.IsType<VmRuntimeState1340.StackNode>(stack2);
        Assert.NotSame(state.Root, other);
        Assert.Equal(2u, other.StackId);
        Assert.Equal(2, other.GetId());
        Assert.True(state.ResolveStateByPath("-1", out VmRuntimeState1340.StateNode? negative));
        Assert.Equal(-1, negative!.GetId());
        Assert.Equal(uint.MaxValue, Assert.IsType<VmRuntimeState1340.StackNode>(negative).StackId);
        Assert.Throws<FormatException>(() => state.ResolveStateByPath("abc", out _));
        Assert.Throws<FormatException>(() => state.ResolveStateByPath("", out _));
        Assert.Throws<OverflowException>(() => state.ResolveStateByPath("2147483648", out _));
        Assert.False(state.ResolveStateByPath("1.", out _));

        var health = new VmRuntimeState1340.StructuredNode();
        var armor = new VmRuntimeState1340.StateNode();
        state.Root.Kids.Add(("Health", health));
        state.Root.Kids.Add(("Armor", armor));
        Assert.True(state.ResolveStateByPath("1.HeAlTh", out VmRuntimeState1340.StateNode? found));
        Assert.Same(health, found);
        Assert.Equal(1000, found!.GetId());
        Assert.Equal("1.health", state.GetPathById(1000));
        Assert.Equal("1.armor", state.GetPathById(1001));
        Assert.True(state.ResolveStateByPath("1.health", out VmRuntimeState1340.StateNode? again));
        Assert.Equal(1000, again!.GetId());
        Assert.True(state.ResolveStateById(1000, out VmRuntimeState1340.StateNode? byId));
        Assert.Same(health, byId);

        var locals = new VmRuntimeState1340.StructuredNode();
        var hidden = new VmRuntimeState1340.StateNode();
        locals.Kids.Add(("Health", hidden));
        state.Root.Virtuals.Add(("Locals", locals));
        state.Root.Virtuals.Add((string.Empty, null));
        Assert.False(state.ResolveStateByPath("1.hidden", out _));
        Assert.True(state.ResolveStateByPath("1.Health", out VmRuntimeState1340.StateNode? direct, false));
        Assert.Same(health, direct);

        var eval = new VmRuntimeState1340(new VmRuntimeState1340.IdProvider());
        var box = new VmRuntimeState1340.StructuredNode();
        var inner = new VmRuntimeState1340.StateNode();
        box.Kids.Add(("Health", inner));
        eval.Root.Virtuals.Add(("Nope", null));
        eval.Root.Virtuals.Add(("Locals", box));
        Assert.False(eval.ResolveStateByPath("1.Health", out _, false));
        Assert.True(eval.ResolveStateByPath("1.Health", out VmRuntimeState1340.StateNode? via, true));
        Assert.Same(inner, via);
        Assert.Equal(1000, via!.GetId());
        Assert.Equal("1.locals.health", eval.GetPathById(1000));

        state.Root.ListedNames = new List<string> { "a", "missing", "b", "c" };
        var a = new VmRuntimeState1340.StructuredNode();
        var b = new VmRuntimeState1340.StateNode();
        var c = new VmRuntimeState1340.StateNode();
        var leaf = new VmRuntimeState1340.StateNode();
        a.Kids.Add(("leaf", leaf));
        state.Root.Kids.Add(("a", a));
        state.Root.Kids.Add(("b", b));
        state.Root.Kids.Add(("c", c));
        var window = new List<VmRuntimeState1340.StateNode>();
        Assert.True(state.ResolveChildrenByParentPath("1", window, 0, 2));
        Assert.Equal(new[] { a, b }, window);
        window.Clear();
        Assert.True(state.ResolveChildrenByParentPath("1", window, 0, 0));
        Assert.Equal(new[] { a, b, c }, window);
        var sentinel = new VmRuntimeState1340.StateNode();
        var kept = new List<VmRuntimeState1340.StateNode> { sentinel };
        Assert.False(state.ResolveChildrenByParentPath("1.missing", kept));
        Assert.Same(sentinel, Assert.Single(kept));
        Assert.True(state.ResolveStateByPath("1.a", out VmRuntimeState1340.StateNode? aNode));
        Assert.Same(a, aNode);
        var byParent = new List<VmRuntimeState1340.StateNode>();
        Assert.True(state.ResolveChildrenByParentId((uint)aNode!.GetId(), byParent));
        Assert.Same(leaf, Assert.Single(byParent));

        VmRuntimeState1340.FrameStack? saved = VmRuntimeState1340.GlobalVmStack;
        var stack = new VmRuntimeState1340.FrameStack { Top = new VmRuntimeState1340.Frame() };
        VmRuntimeState1340.GlobalVmStack = stack;
        var oldRoot = state.Root;
        state.Reset();
        Assert.NotSame(oldRoot, state.Root);
        Assert.Same(stack, VmRuntimeState1340.GlobalVmStack);
        Assert.Equal("", state.GetPathById(1000));
        state.Root.Kids.Add(("Health", new VmRuntimeState1340.StateNode()));
        Assert.True(state.ResolveStateByPath("1.Health", out VmRuntimeState1340.StateNode? afterReset));
        Assert.NotEqual(1000, afterReset!.GetId());
        VmRuntimeState1340.GlobalVmStack = saved;
    }

    [Fact]
    public void VariableNodesAndFrameWalk()
    {
        var value = new VmRuntimeState1340.VmValue(7);
        var owner = new VmRuntimeState1340.PClass("Player");
        var frame = new VmRuntimeState1340.Frame();
        var scalar = new VmRuntimeState1340.PType { Scalar = true };
        VmRuntimeState1340.VariableNode? basic = VmRuntimeState1340.CreateNodeForVariable("hp", value, scalar, frame, owner);
        Assert.Equal(VmRuntimeState1340.VariableKind.Value, basic!.Kind);
        Assert.Same(owner, basic.OwningClass);
        Assert.Null(basic.Frame);
        Assert.Equal(7, basic.Value.I);

        var pointed = new VmRuntimeState1340.PType { Scalar = true };
        var ptr = new VmRuntimeState1340.PType { Pointer = true, PointerFlag = true, Pointed = pointed };
        Assert.Equal(VmRuntimeState1340.VariableKind.Value, VmRuntimeState1340.CreateNodeForVariable("p", value, ptr)!.Kind);

        var array = new VmRuntimeState1340.PType { DynArray = true };
        var arrayPtr = new VmRuntimeState1340.PType { Pointer = true, Pointed = array };
        Assert.Equal(VmRuntimeState1340.VariableKind.Array, VmRuntimeState1340.CreateNodeForVariable("arr", value, arrayPtr)!.Kind);

        VmRuntimeState1340.VariableNode? stateNode = VmRuntimeState1340.CreateNodeForVariable("st", value, VmRuntimeState1340.PType.TypeState, null, owner);
        Assert.Equal(VmRuntimeState1340.VariableKind.State, stateNode!.Kind);
        Assert.Same(owner, stateNode.OwningClass);
        Assert.Null(stateNode.Type);

        var obj = new VmRuntimeState1340.PType { ObjectPointer = true, Pointer = true, Pointed = new VmRuntimeState1340.PType() };
        Assert.Equal(VmRuntimeState1340.VariableKind.Object, VmRuntimeState1340.CreateNodeForVariable("actor", value, obj)!.Kind);
        var cls = new VmRuntimeState1340.PType { Class = true };
        Assert.Equal(VmRuntimeState1340.VariableKind.Object, VmRuntimeState1340.CreateNodeForVariable("cls", value, cls)!.Kind);

        var structType = new VmRuntimeState1340.PType { Struct = true };
        VmRuntimeState1340.VariableNode? structNode = VmRuntimeState1340.CreateNodeForVariable("pos", value, structType, frame);
        Assert.Equal(VmRuntimeState1340.VariableKind.Struct, structNode!.Kind);
        Assert.Same(frame, structNode.Frame);
        var structPtr = new VmRuntimeState1340.PType { Pointer = true, Pointed = structType };
        Assert.Equal(VmRuntimeState1340.VariableKind.Struct, VmRuntimeState1340.CreateNodeForVariable("sp", value, structPtr, frame)!.Kind);

        var map = new VmRuntimeState1340.PType { Container = true };
        Assert.Equal(VmRuntimeState1340.VariableKind.Struct, VmRuntimeState1340.CreateNodeForVariable("map", value, map, frame)!.Kind);
        var mapPtr = new VmRuntimeState1340.PType { Pointer = true, Pointed = map };
        Assert.Equal(VmRuntimeState1340.VariableKind.Value, VmRuntimeState1340.CreateNodeForVariable("mp", value, mapPtr)!.Kind);
        Assert.Null(VmRuntimeState1340.CreateNodeForVariable("none", value, new VmRuntimeState1340.PType()));

        VmRuntimeState1340.FrameStack? saved = VmRuntimeState1340.GlobalVmStack;
        try
        {
            VmRuntimeState1340.GlobalVmStack = null;
            var frames = new List<VmRuntimeState1340.Frame> { frame };
            Assert.False(VmRuntimeState1340.GetStackFrames(4, frames));
            Assert.Same(frame, Assert.Single(frames));
            Assert.Null(VmRuntimeState1340.GetFrame(4, 0));
            Assert.Null(VmRuntimeState1340.GetStack(9));

            var parent = new VmRuntimeState1340.Frame();
            var top = new VmRuntimeState1340.Frame { Parent = parent };
            var empty = new VmRuntimeState1340.FrameStack();
            VmRuntimeState1340.GlobalVmStack = empty;
            frames.Clear();
            Assert.True(VmRuntimeState1340.GetStackFrames(1, frames));
            Assert.Empty(frames);
            Assert.Null(VmRuntimeState1340.GetFrame(3, 0));

            VmRuntimeState1340.GlobalVmStack = new VmRuntimeState1340.FrameStack { Top = top };
            Assert.Same(VmRuntimeState1340.GlobalVmStack, VmRuntimeState1340.GetStack(99));
            frames.Clear();
            Assert.True(VmRuntimeState1340.GetStackFrames(2, frames));
            Assert.Equal(new[] { top, parent }, frames);
            Assert.Same(top, VmRuntimeState1340.GetFrame(8, 0));
            Assert.Same(parent, VmRuntimeState1340.GetFrame(8, 1));
            Assert.Null(VmRuntimeState1340.GetFrame(8, 2));
        }
        finally
        {
            VmRuntimeState1340.GlobalVmStack = saved;
        }
    }
}
