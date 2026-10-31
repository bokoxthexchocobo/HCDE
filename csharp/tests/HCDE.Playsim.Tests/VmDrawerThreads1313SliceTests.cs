namespace HCDE.Playsim.Tests;

public class VmDrawerThreads1313SliceTests
{
    public VmDrawerThreads1313SliceTests()
    {
        VmDrawerThreads1313.FirstCall = true;
        VmDrawerThreads1313.Current = null;
    }

    [Fact]
    public void Start_DistributesNumaAndWarnsOnceWhenTheCountIsUnknown()
    {
        var queue = new VmDrawerThreads1313();
        VmDrawerThreads1313.Current = queue;
        queue.NumaCounts.Add(1);
        queue.NumaCounts.Add(3);
        queue.StartThreads();
        Assert.Equal(4, queue.Threads.Count);
        Assert.Equal(0, queue.Threads[0].Core);
        Assert.Equal(1, queue.Threads[0].NumCores);
        Assert.Equal(0, queue.Threads[0].NumaNode);
        Assert.Equal(2, queue.Threads[0].NumNumaNodes);
        Assert.Equal(0, queue.Threads[1].Core);
        Assert.Equal(3, queue.Threads[1].NumCores);
        Assert.Equal(1, queue.Threads[1].NumaNode);
        Assert.Equal(2, queue.Threads[3].Core);
        Assert.Equal(3, queue.Threads[3].NumCores);
        Assert.Equal(new[] { 0, 1, 1, 1 }, queue.NumaBinds);
        Assert.Equal(4, queue.Spawned);
        Assert.Empty(queue.Messages);
        Assert.True(VmDrawerThreads1313.FirstCall);
        Assert.Equal(1, queue.StartNotifies);
        Assert.Equal(0, queue.Joins);
        Assert.False(queue.Shutdown);

        queue.NumaCounts.Clear();
        queue.NumaCounts.Add(4);
        queue.StartThreads();
        Assert.Equal(1, queue.Threads[0].NumCores);
        Assert.Equal(new[] { 0, 1, 1, 1 }, queue.NumaBinds);
        Assert.Equal(1, queue.StartNotifies);
        Assert.Equal(4, queue.Spawned);
        Assert.Equal(2, queue.StartCalls);

        queue.Multithreaded = 3;
        queue.StartThreads();
        Assert.Equal(3, queue.Threads.Count);
        Assert.Equal(0, queue.Threads[0].Core);
        Assert.Equal(1, queue.Threads[1].Core);
        Assert.Equal(2, queue.Threads[2].Core);
        Assert.Equal(3, queue.Threads[0].NumCores);
        Assert.Equal(0, queue.Threads[2].NumaNode);
        Assert.Equal(1, queue.Threads[2].NumNumaNodes);
        Assert.Equal(new[] { 0, 0, 0 }, queue.NumaBinds);
        Assert.Equal(4, queue.Joins);
        Assert.Equal(2, queue.StartNotifies);
        Assert.Equal(7, queue.Spawned);

        queue.Multithreaded = 0;
        queue.NumaCounts.Clear();
        queue.NumaCounts.Add(2);
        queue.NumaCounts.Add(2);
        queue.StartThreads();
        Assert.Single(queue.Threads);
        Assert.Equal(0, queue.Threads[0].Core);
        Assert.Equal(1, queue.Threads[0].NumCores);
        Assert.Equal(0, queue.Threads[0].NumaNode);
        Assert.Equal(1, queue.Threads[0].NumNumaNodes);
        Assert.Equal(new[] { 0 }, queue.NumaBinds);

        var unknown = new VmDrawerThreads1313();
        VmDrawerThreads1313.FirstCall = true;
        unknown.StartThreads();
        Assert.Equal(VmDrawerThreads1313.NumaWarning, Assert.Single(unknown.Messages));
        Assert.False(VmDrawerThreads1313.FirstCall);
        Assert.Single(unknown.Threads);
        Assert.Equal(1, unknown.Threads[0].NumCores);
        unknown.StartThreads();
        Assert.Single(unknown.Messages);
        Assert.Equal(1, unknown.Spawned);

        var forced = new VmDrawerThreads1313 { Multithreaded = 4 };
        VmDrawerThreads1313.FirstCall = true;
        forced.StartThreads();
        Assert.Empty(forced.Messages);
        Assert.False(VmDrawerThreads1313.FirstCall);
        Assert.Equal(4, forced.Threads.Count);
        Assert.Equal(4, forced.Threads[3].NumCores);
        Assert.Equal(0, forced.Threads[3].NumNumaNodes == 1 ? forced.Threads[3].NumaNode : -1);

        var run = new VmDrawerThreads1313();
        VmDrawerThreads1313.Current = run;
        run.NumaCounts.Add(2);
        run.NumaCounts.Add(2);
        var empty = new VmDrawerThreads1313.CommandQueue(new VmDrawerThreads1313.FrameMemory());
        VmDrawerThreads1313.Execute(null);
        VmDrawerThreads1313.Execute(empty);
        Assert.Equal(0, run.StartCalls);
        Assert.Empty(run.LockTrace);

        var work = new VmDrawerThreads1313.CommandQueue(empty.Frame);
        work.Commands.Add(new VmDrawerThreads1313.MarkCommand());
        VmDrawerThreads1313.Execute(work);
        Assert.Equal(4ul, run.TasksLeft);
        Assert.Single(run.Active);
        Assert.Equal(new[] { "start", "end", "end-unlock", "start-unlock", "notify" }, run.LockTrace);
        Assert.Equal(2, run.StartNotifies);
        Assert.Equal(1, run.StartCalls);
        Assert.Equal(new[] { 0, 0, 1, 1 }, run.NumaBinds);

        var more = new VmDrawerThreads1313.CommandQueue(empty.Frame);
        more.Commands.Add(new VmDrawerThreads1313.MarkCommand());
        VmDrawerThreads1313.Execute(more);
        Assert.Equal(8ul, run.TasksLeft);
        Assert.Equal(2, run.Active.Count);
        Assert.Equal(3, run.StartNotifies);
        Assert.Equal(4, run.Threads.Count);
        Assert.Equal(0, run.Joins);

        run.Destroy();
        Assert.Empty(run.Threads);
        Assert.Equal(4, run.Joins);
        Assert.False(run.Shutdown);
        Assert.Equal(4, run.StartNotifies);
    }

    [Fact]
    public void Worker_StepsTheCursorAndCopiesRows()
    {
        var queue = new VmDrawerThreads1313 { ScreenHeight = 5 };
        VmDrawerThreads1313.Current = queue;
        queue.NumaCounts.Add(1);
        queue.NumaCounts.Add(1);
        queue.StartThreads();
        var first = new VmDrawerThreads1313.MarkCommand();
        var second = new VmDrawerThreads1313.MarkCommand();
        var list = new VmDrawerThreads1313.CommandQueue(new VmDrawerThreads1313.FrameMemory());
        list.Commands.Add(first);
        list.Commands.Add(second);
        queue.Active.Add(list);
        queue.TasksLeft = 2;
        queue.WorkerMain(queue.Threads[0]);
        Assert.Equal(1, first.Runs);
        Assert.Equal(1, second.Runs);
        Assert.Equal(0, queue.Threads[0].NumaStartY);
        Assert.Equal(2, queue.Threads[0].NumaEndY);
        Assert.Equal(1, queue.Threads[0].CurrentQueue);
        Assert.Equal(1ul, queue.TasksLeft);
        Assert.Equal(0, queue.EndNotifies);

        queue.WorkerMain(queue.Threads[1]);
        Assert.Equal(2, queue.Threads[1].NumaStartY);
        Assert.Equal(5, queue.Threads[1].NumaEndY);
        Assert.Equal(2, first.Runs);
        Assert.Equal(0ul, queue.TasksLeft);
        Assert.Equal(1, queue.EndNotifies);

        var blocked = new VmDrawerThreads1313.MarkCommand();
        var held = new VmDrawerThreads1313.CommandQueue(list.Frame);
        held.Commands.Add(blocked);
        queue.Active.Add(held);
        queue.Shutdown = true;
        queue.Threads[0].CurrentQueue = 0;
        queue.TasksLeft = 1;
        queue.WorkerMain(queue.Threads[0]);
        Assert.Equal(0, blocked.Runs);
        Assert.Equal(0, queue.Threads[0].CurrentQueue);
        Assert.Equal(1ul, queue.TasksLeft);
        queue.Shutdown = false;

        var cursor = new VmDrawerThreads1313();
        VmDrawerThreads1313.Current = cursor;
        var kept = new VmDrawerThreads1313.MarkCommand();
        var skipped = new VmDrawerThreads1313.MarkCommand();
        var drawn = new VmDrawerThreads1313.CommandQueue(list.Frame);
        drawn.Commands.Add(kept);
        drawn.Commands.Add(skipped);
        cursor.Active.Add(drawn);
        cursor.DebugDraw = 1;
        cursor.DebugDrawEnd = 2;
        cursor.TasksLeft = 1;
        var drawer = new VmDrawerThreads1313.Worker();
        cursor.WorkerMain(drawer);
        Assert.Equal(1, kept.Runs);
        Assert.Equal(0, skipped.Runs);
        Assert.Equal(2ul, drawer.DebugDrawPos);
        Assert.Equal(0ul, cursor.TasksLeft);
        Assert.Equal(1, cursor.EndNotifies);

        var negative = new VmDrawerThreads1313 { DebugDraw = -1 };
        var quiet = new VmDrawerThreads1313.MarkCommand();
        var quietList = new VmDrawerThreads1313.CommandQueue(list.Frame);
        quietList.Commands.Add(quiet);
        negative.Active.Add(quietList);
        negative.TasksLeft = 1;
        var negativeWorker = new VmDrawerThreads1313.Worker();
        negative.WorkerMain(negativeWorker);
        Assert.Equal(0, quiet.Runs);
        Assert.Equal(1ul, negativeWorker.DebugDrawPos);
        Assert.Equal(0ul, negative.TasksLeft);

        var wrap = new VmDrawerThreads1313();
        var wrapped = new VmDrawerThreads1313.MarkCommand();
        var wrapList = new VmDrawerThreads1313.CommandQueue(list.Frame);
        wrapList.Commands.Add(wrapped);
        wrap.Active.Add(wrapList);
        wrap.WorkerMain(new VmDrawerThreads1313.Worker());
        Assert.Equal(1, wrapped.Runs);
        Assert.Equal(ulong.MaxValue, wrap.TasksLeft);
        Assert.Equal(0, wrap.EndNotifies);

        var both = new VmDrawerThreads1313();
        var left = new VmDrawerThreads1313.MarkCommand();
        var right = new VmDrawerThreads1313.MarkCommand();
        var leftList = new VmDrawerThreads1313.CommandQueue(list.Frame);
        var rightList = new VmDrawerThreads1313.CommandQueue(list.Frame);
        leftList.Commands.Add(left);
        rightList.Commands.Add(right);
        both.Active.Add(leftList);
        both.Active.Add(rightList);
        both.TasksLeft = 2;
        both.WorkerMain(new VmDrawerThreads1313.Worker());
        Assert.Equal(1, left.Runs);
        Assert.Equal(1, right.Runs);
        Assert.Equal(0ul, both.TasksLeft);
        Assert.Equal(1, both.EndNotifies);
        Assert.Single(leftList.Commands);
        Assert.Single(rightList.Commands);

        VmDrawerThreads1313.Current = cursor;
        cursor.Threads.Add(new VmDrawerThreads1313.Worker { DebugDrawPos = 4 });
        cursor.DebugDraw = 0;
        cursor.DebugDrawEnd = 5;
        VmDrawerThreads1313.ResetDebugDrawPos();
        Assert.Equal(0ul, cursor.DebugDrawEnd);
        Assert.Equal(0ul, cursor.Threads[0].DebugDrawPos);

        cursor.Threads.Clear();
        cursor.DebugDraw = 5;
        cursor.DebugDrawEnd = 0;
        VmDrawerThreads1313.ResetDebugDrawPos();
        Assert.Equal(5ul, cursor.DebugDrawEnd);

        cursor.Threads.Add(new VmDrawerThreads1313.Worker());
        cursor.DebugDraw = 1;
        VmDrawerThreads1313.ResetDebugDrawPos();
        Assert.Equal(6ul, cursor.DebugDrawEnd);
        Assert.Equal(0ul, cursor.Threads[0].DebugDrawPos);

        cursor.Threads.Clear();
        cursor.Threads.Add(new VmDrawerThreads1313.Worker { DebugDrawPos = 1000 });
        cursor.Threads.Add(new VmDrawerThreads1313.Worker());
        cursor.DebugDrawEnd = 200;
        VmDrawerThreads1313.ResetDebugDrawPos();
        Assert.Equal(0ul, cursor.DebugDrawEnd);
        Assert.Equal(0ul, cursor.Threads[0].DebugDrawPos);
        Assert.Equal(0ul, cursor.Threads[1].DebugDrawPos);

        var waiting = new VmDrawerThreads1313();
        VmDrawerThreads1313.Current = waiting;
        waiting.Threads.Add(new VmDrawerThreads1313.Worker { CurrentQueue = 7 });
        var pending = new VmDrawerThreads1313.MarkCommand();
        var other = new VmDrawerThreads1313.MarkCommand();
        var pendingList = new VmDrawerThreads1313.CommandQueue(list.Frame);
        pendingList.Commands.Add(pending);
        pendingList.Commands.Add(other);
        waiting.Active.Add(pendingList);
        waiting.TasksLeft = 4;
        var fatal = Assert.Throws<InvalidOperationException>(() => VmDrawerThreads1313.WaitForWorkers());
        Assert.Contains(VmDrawerThreads1313.TimeoutMessage, fatal.Message, StringComparison.Ordinal);
        Assert.Equal(4ul, waiting.TasksLeft);
        Assert.Equal(0, pending.Destroys);
        Assert.Single(waiting.Active);

        waiting.TasksLeft = 0;
        VmDrawerThreads1313.WaitForWorkers();
        Assert.Equal(0, waiting.Threads[0].CurrentQueue);
        Assert.Equal(1, pending.Destroys);
        Assert.Equal(1, other.Destroys);
        Assert.Empty(pendingList.Commands);
        Assert.Empty(waiting.Active);

        waiting.TasksLeft = 4;
        waiting.FinishBeforeTimeout = true;
        waiting.Active.Add(pendingList);
        pendingList.Commands.Add(pending);
        VmDrawerThreads1313.WaitForWorkers();
        Assert.Equal(0ul, waiting.TasksLeft);
        Assert.Equal(2, pending.Destroys);
        Assert.Empty(waiting.Active);

        var memory = new VmDrawerThreads1313.FrameMemory();
        var allocator = new VmDrawerThreads1313.CommandQueue(memory);
        byte[] block = allocator.Alloc(7);
        Assert.Equal(7, block.Length);
        Assert.Equal(1, memory.Allocs);
        Assert.Equal(7, memory.LastSize);
        var missing = new VmDrawerThreads1313.CommandQueue(null);
        Assert.Throws<NullReferenceException>(() => missing.Alloc(1));

        var worker = new VmDrawerThreads1313.Worker { Core = 1, NumCores = 4, NumaStartY = 0, NumaEndY = 10 };
        Assert.Equal(1, worker.SkippedByThread(0));
        Assert.Equal(3, worker.CountForThread(0, 10));
        worker.NumaStartY = 10;
        worker.NumaEndY = 15;
        worker.Core = 0;
        Assert.Equal(12, worker.SkippedByThread(0));
        Assert.Equal(1, worker.CountForThread(0, 20));
        worker.Core = 1;
        worker.NumaStartY = 0;
        worker.NumaEndY = 10;

        byte[] src = new byte[128];
        byte[] dest = new byte[128];
        src[8] = 1;
        src[9] = 2;
        src[40] = 3;
        src[41] = 4;
        src[72] = 5;
        src[73] = 6;
        var copy = new VmDrawerThreads1313.CopyCommand(dest, 8, src, 2, 10, 8, 1);
        copy.Execute(worker);
        Assert.Equal(1, copy.Runs);
        Assert.Equal(3, copy.Copies);
        Assert.Equal(1, dest[8]);
        Assert.Equal(2, dest[9]);
        Assert.Equal(3, dest[40]);
        Assert.Equal(4, dest[41]);
        Assert.Equal(5, dest[72]);
        Assert.Equal(6, dest[73]);
        Assert.Equal(0, dest[0]);

        byte[] untouched = new byte[32];
        var none = new VmDrawerThreads1313.CopyCommand(untouched, 8, src, 2, 1, 8, 1);
        none.Execute(worker);
        Assert.Equal(0, none.Copies);
        Assert.Equal(1, none.Runs);
        Assert.Equal(0, untouched[8]);

        var barrier = new VmDrawerThreads1313.BarrierCommand();
        var cores = new VmDrawerThreads1313.Worker { NumCores = 4 };
        barrier.Execute(cores);
        barrier.Execute(cores);
        barrier.Execute(cores);
        Assert.Equal(3, barrier.Waits);
        Assert.Equal(3ul, barrier.Count);
        barrier.Execute(cores);
        Assert.Equal(3, barrier.Waits);
        Assert.Equal(4ul, barrier.Count);
        Assert.Equal(4, barrier.Notifies);
        var open = new VmDrawerThreads1313.BarrierCommand();
        open.Execute(new VmDrawerThreads1313.Worker { NumCores = 0 });
        Assert.Equal(0, open.Waits);
        Assert.Equal(1ul, open.Count);
    }
}
