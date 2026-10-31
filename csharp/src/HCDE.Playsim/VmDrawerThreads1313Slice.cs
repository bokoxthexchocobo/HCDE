namespace HCDE.Playsim;

/// <summary>
/// Drawer threads, the debug-draw cursor, and the memcpy rows from
/// <c>r_thread.cpp</c> (1-285). Chunk 1313.
/// </summary>
public sealed class VmDrawerThreads1313
{
    public const string NumaWarning =
        "Warning: Unable to determine number of CPU cores/threads for this computer. To improve performance, please type 'r_multithreaded x' in the console, where x is the number of threads to use.\n";

    public const string TimeoutMessage = "Drawer threads did not finish within 5 seconds!";

    public static bool FirstCall { get; set; } = true;

    public static VmDrawerThreads1313? Current { get; set; }

    public static VmDrawerThreads1313 Instance()
    {
        Current ??= new VmDrawerThreads1313();
        return Current;
    }

    public int Multithreaded { get; set; } = 1;

    public int DebugDraw { get; set; }

    public int ScreenHeight { get; set; }

    public bool FinishBeforeTimeout { get; set; }

    public List<string> Messages { get; } = new();

    public List<Worker> Threads { get; } = new();

    public List<int> NumaCounts { get; } = new();

    public List<int> NumaBinds { get; } = new();

    public List<CommandQueue> Active { get; } = new();

    public List<string> LockTrace { get; } = new();

    public ulong TasksLeft { get; set; }

    public ulong DebugDrawEnd { get; set; }

    public bool Shutdown { get; set; }

    public int StartNotifies { get; set; }

    public int EndNotifies { get; set; }

    public int Joins { get; set; }

    public int StartCalls { get; set; }

    public int Spawned { get; set; }

    public void Destroy()
    {
        StopThreads();
    }

    public static void Execute(CommandQueue? commands)
    {
        if (commands == null || commands.Commands.Count == 0)
            return;

        VmDrawerThreads1313 queue = Instance();
        queue.StartThreads();
        queue.LockTrace.Add("start");
        queue.LockTrace.Add("end");
        queue.Active.Add(commands);
        queue.TasksLeft += (ulong)queue.Threads.Count;
        queue.LockTrace.Add("end-unlock");
        queue.LockTrace.Add("start-unlock");
        queue.StartNotifies++;
        queue.LockTrace.Add("notify");
    }

    public static void ResetDebugDrawPos()
    {
        VmDrawerThreads1313 queue = Instance();
        bool reached = false;
        foreach (Worker thread in queue.Threads)
        {
            ulong add = unchecked((ulong)(queue.DebugDraw * 60 * 2));
            if (unchecked(thread.DebugDrawPos + add) < queue.DebugDrawEnd)
                reached = true;
            thread.DebugDrawPos = 0;
        }

        if (!reached)
            queue.DebugDrawEnd = unchecked(queue.DebugDrawEnd + (ulong)queue.DebugDraw);
        else
            queue.DebugDrawEnd = 0;
    }

    public static void WaitForWorkers()
    {
        VmDrawerThreads1313 queue = Instance();
        if (queue.TasksLeft != 0)
        {
            if (!queue.FinishBeforeTimeout)
                throw new InvalidOperationException(TimeoutMessage);
            queue.TasksLeft = 0;
        }

        foreach (Worker thread in queue.Threads)
            thread.CurrentQueue = 0;
        foreach (CommandQueue list in queue.Active)
        {
            foreach (Command command in list.Commands)
                command.Destroy();
            list.Clear();
        }

        queue.Active.Clear();
    }

    public void WorkerMain(Worker thread)
    {
        while (true)
        {
            bool ready = thread.CurrentQueue < Active.Count || Shutdown;
            if (!ready)
                break;
            if (Shutdown)
                break;

            CommandQueue list = Active[thread.CurrentQueue];
            thread.CurrentQueue++;
            thread.NumaStartY = thread.NumaNode * ScreenHeight / thread.NumNumaNodes;
            thread.NumaEndY = (thread.NumaNode + 1) * ScreenHeight / thread.NumNumaNodes;
            if (DebugDraw != 0)
            {
                foreach (Command command in list.Commands)
                {
                    thread.DebugDrawPos++;
                    if (thread.DebugDrawPos < DebugDrawEnd)
                        command.Execute(thread);
                }
            }
            else
            {
                foreach (Command command in list.Commands)
                    command.Execute(thread);
            }

            unchecked
            {
                TasksLeft--;
            }

            if (TasksLeft == 0)
                EndNotifies++;
        }
    }

    public void StartThreads()
    {
        StartCalls++;
        int nodes = NumaCounts.Count;
        int numaThreads = 0;
        for (int i = 0; i < nodes; i++)
            numaThreads += NumaCounts[i];

        int numThreads = numaThreads;
        if (numThreads == 0)
        {
            if (FirstCall)
            {
                FirstCall = false;
                if (Multithreaded == 1)
                    Messages.Add(NumaWarning);
            }

            numThreads = 1;
        }

        if (Multithreaded == 0)
            numThreads = 1;
        else if (Multithreaded != 1)
            numThreads = Multithreaded;

        if (numThreads == Threads.Count)
            return;

        StopThreads();
        NumaBinds.Clear();
        for (int n = 0; n < numThreads; n++)
            Threads.Add(new Worker());

        if (numThreads == numaThreads)
        {
            int cur = 0;
            for (int numaNode = 0; numaNode < nodes; numaNode++)
            {
                int count = NumaCounts[numaNode];
                for (int i = 0; i < count; i++)
                {
                    Worker worker = Threads[cur];
                    cur++;
                    worker.Core = i;
                    worker.NumCores = count;
                    worker.NumaNode = numaNode;
                    worker.NumNumaNodes = nodes;
                    Spawned++;
                    NumaBinds.Add(numaNode);
                }
            }
        }
        else
        {
            for (int i = 0; i < numThreads; i++)
            {
                Worker worker = Threads[i];
                worker.Core = i;
                worker.NumCores = numThreads;
                worker.NumaNode = 0;
                worker.NumNumaNodes = 1;
                Spawned++;
                NumaBinds.Add(0);
            }
        }
    }

    public void StopThreads()
    {
        Shutdown = true;
        StartNotifies++;
        Joins += Threads.Count;
        Threads.Clear();
        Shutdown = false;
    }

    public sealed class Worker
    {
        public int Core { get; set; }

        public int NumCores { get; set; } = 1;

        public int NumaNode { get; set; }

        public int NumNumaNodes { get; set; } = 1;

        public int NumaStartY { get; set; }

        public int NumaEndY { get; set; }

        public ulong DebugDrawPos { get; set; }

        public int CurrentQueue { get; set; }

        public int SkippedByThread(int firstLine)
        {
            int clip = Math.Max(firstLine, NumaStartY);
            int coreSkip = (NumCores - (clip - Core) % NumCores) % NumCores;
            return clip + coreSkip - firstLine;
        }

        public int CountForThread(int firstLine, int count)
        {
            count = Math.Min(count, NumaEndY - firstLine);
            int skipped = SkippedByThread(firstLine);
            int rows = (count - skipped + NumCores - 1) / NumCores;
            return Math.Max(rows, 0);
        }
    }

    public abstract class Command
    {
        public int Runs { get; set; }

        public int Destroys { get; set; }

        public abstract void Execute(Worker thread);

        public void Destroy()
        {
            Destroys++;
        }
    }

    public sealed class CommandQueue
    {
        public FrameMemory? Frame { get; set; }

        public List<Command> Commands { get; } = new();

        public CommandQueue(FrameMemory? frame)
        {
            Frame = frame;
        }

        public void Clear()
        {
            Commands.Clear();
        }

        public byte[] Alloc(ulong size)
        {
            FrameMemory? frame = Frame;
            if (frame == null)
                throw new NullReferenceException("FrameMemory");
            return frame.Alloc((int)size);
        }
    }

    public sealed class FrameMemory
    {
        public int LastSize { get; private set; }

        public int Allocs { get; private set; }

        public byte[] Alloc(int size)
        {
            Allocs++;
            LastSize = size;
            return new byte[size];
        }
    }

    public sealed class BarrierCommand : Command
    {
        public ulong Count { get; set; }

        public int Notifies { get; set; }

        public int Waits { get; set; }

        public override void Execute(Worker thread)
        {
            Count++;
            Notifies++;
            if (Count < (ulong)thread.NumCores)
                Waits++;
        }
    }

    public sealed class CopyCommand : Command
    {
        public byte[] Dest { get; }

        public byte[] Src { get; }

        public int DestPitch { get; }

        public int Width { get; }

        public int Height { get; }

        public int SrcPitch { get; }

        public int PixelSize { get; }

        public int Copies { get; private set; }

        public CopyCommand(byte[] dest, int destPitch, byte[] src, int width, int height, int srcPitch, int pixelSize)
        {
            Dest = dest;
            Src = src;
            DestPitch = destPitch;
            Width = width;
            Height = height;
            SrcPitch = srcPitch;
            PixelSize = pixelSize;
        }

        public override void Execute(Worker thread)
        {
            Runs++;
            int start = thread.SkippedByThread(0);
            int count = thread.CountForThread(0, Height);
            int sstep = thread.NumCores * SrcPitch * PixelSize;
            int dstep = thread.NumCores * DestPitch * PixelSize;
            int size = Width * PixelSize;
            int destAt = start * DestPitch * PixelSize;
            int srcAt = start * SrcPitch * PixelSize;
            for (int i = 0; i < count; i++)
            {
                Buffer.BlockCopy(Src, srcAt, Dest, destAt, size);
                destAt += dstep;
                srcAt += sstep;
                Copies++;
            }
        }
    }

    public sealed class MarkCommand : Command
    {
        public override void Execute(Worker thread)
        {
            Runs++;
        }
    }
}
