using S = HCDE.Playsim.VmRuntimeEvents1403Slice;

namespace HCDE.Playsim.Tests;

public class VmRuntimeEvents1403SliceTests
{
    [Fact]
    public void SubscribeReplacesTheOnlyHandlerAndAnyLiveHandleClearsIt()
    {
        var bus = new S.Bus();
        int first = 0;
        int second = 0;
        S.LogHandler older = (_, _) => first++;
        S.LogHandler newer = (level, message) =>
        {
            Assert.Equal(3, level);
            Assert.Equal("tick", message);
            second++;
        };
        Assert.Null(bus.Log.Subscribe(null));
        Assert.False(bus.Log.Active);
        Assert.Equal("subscribed runtime event Log", bus.Log.Trace[0]);
        Assert.Same(older, bus.Log.Subscribe(older));
        Assert.Same(newer, bus.Log.Subscribe(newer));
        bus.EmitLog(3, "tick");
        Assert.Equal(0, first);
        Assert.Equal(1, second);
        Assert.False(bus.Log.Unsubscribe(null));
        Assert.True(bus.Log.Active);
        Assert.True(bus.Log.Unsubscribe(older));
        Assert.False(bus.Log.Active);
        Assert.Equal("unsubscribed runtime event Log", bus.Log.Trace[^1]);
        bus.EmitLog(3, "tick");
        Assert.Equal(1, second);
        Assert.False(bus.Log.Unsubscribe(newer));
        Assert.Contains("subscribed runtime event Log", bus.Log.Trace);

        int created = 0;
        S.CreateHandler create = _ => created++;
        bus.CreateStack.Subscribe(create);
        Assert.True(bus.CreateStack.Unsubscribe(create));
        Assert.Equal(0, created);
        uint cleaned = 0;
        bus.CleanupStack.Subscribe(id => cleaned = id);
        Assert.True(bus.CleanupStack.Active);
        Assert.Equal(0u, cleaned);
    }

    [Fact]
    public void EmitCopiesTheHandlerThenSkipsAClearedSlot()
    {
        var bus = new S.Bus();
        var point = new object();
        string? seen = null;
        int calls = 0;
        bus.Breakpoint.Subscribe((breakpoint, what) =>
        {
            Assert.Same(point, breakpoint);
            seen = what;
            calls++;
            Assert.True(bus.Breakpoint.Unsubscribe(bus.Breakpoint.Subscribe((a, b) => { })));
        });
        bus.EmitBreakpoint(point, "moved");
        Assert.Equal(1, calls);
        Assert.Equal("moved", seen);
        bus.EmitBreakpoint(point, "again");
        Assert.Equal(1, calls);

        object? stack = new object();
        object? pc = new object();
        int regs = 0;
        bus.Instruction.Subscribe((s, r, count, op) =>
        {
            Assert.Same(stack, s);
            Assert.Null(r);
            Assert.Equal(2, count);
            Assert.Same(pc, op);
            regs++;
        });
        bus.EmitInstruction(stack, null, 2, pc);
        Assert.Equal(1, regs);
        int skipped = 0;
        bus.Instruction.Subscribe((_, _, _, _) => skipped++);
        bus.EmitInstruction(stack, null, 2, pc, () => Assert.True(bus.Instruction.Unsubscribe(new S.InstructionHandler((_, _, _, _) => { }))));
        Assert.Equal(0, skipped);

        bus.Exception.Subscribe((reason, message, trace) =>
        {
            Assert.Equal(4, reason);
            Assert.Equal("boom", message);
            Assert.Equal("stack", trace);
            calls++;
        });
        bus.EmitException(4, "boom", "stack");
        Assert.Equal(2, calls);

        Assert.False(bus.IsDebugServerRunning());
        bus.Debugger.Subscribe(() => false);
        Assert.False(bus.IsDebugServerRunning());
        bus.Debugger.Subscribe(() => true);
        Assert.True(bus.IsDebugServerRunning());
        bus.EmitLog(1, null, () => { });
        Assert.False(bus.IsDebugServerRunning(() => Assert.True(bus.Debugger.Unsubscribe(() => true))));
    }
}
