using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Vulkan command submit, timestamp groups, and the frame wait from <c>vk_commandbuffer.cpp</c> (1-266).
/// Chunk 1297. The device, swapchain image, and timestamp period are host services.
/// Upload-only flushes leave the draw buffer open. A finish wait publishes the running
/// command count and then a later finish with no new buffers publishes zero.
/// The destructor does not release the pool.
/// </summary>
public sealed class VmVkCommandBuffer1297
{
    public const int MaxConcurrentSubmits = 8;
    public const int MaxTimestampQueries = 100;
    public const int TopOfPipe = 0x1;
    public const int ColorAttachmentOutput = 0x400;
    public const int QueryTimestamp = 2;
    public const int QueryResult64 = 0x1;
    public const int QueryResultWait = 0x2;
    public const ulong WaitForever = ulong.MaxValue;

    public sealed class Host
    {
        public bool GraphicsTimeQueries;
        public int GraphicsFamily;
        public int PresentImageIndex = -1;
        public bool VSync;
        public bool GpuStatActive;
        public bool KeepGpuStatActive;
        public double TimestampPeriod = 1;
        public ulong[] Timestamps = Array.Empty<ulong>();
    }

    public sealed class Cmd
    {
        public int Id;
        public string Name = "";
        public bool Begun;
        public bool Ended;
        public List<int> QueryResets = new();
        public List<int> TimestampWrites = new();
    }

    public sealed class Query
    {
        public string Name = "";
        public int Start;
        public int End;
    }

    public readonly record struct Submit(
        int Index, int Count, int FirstId, int SecondId,
        int WaitSemaphore, int WaitStage, bool PresentWait, int PresentSignal,
        int SignalSemaphore, bool Finish, bool LastSubmit);

    public Host Services;
    public string PoolName = "";
    public int GraphicsFamily;
    public bool PoolCreated;
    public bool PoolReleased;
    public bool Destroyed;
    public bool QueryPool;
    public int SemaphoreCount;
    public int FenceCount;
    public int NextSubmit;
    public int NextQuery;
    public int NextCmd;
    public int EndRenderPasses;
    public int CurrentRendered;
    public int Rendered;
    public int Acquires;
    public int Presents;
    public int FpsLimits;
    public int Clocks;
    public int ClockResets;
    public int Unclocks;
    public int FenceWaits;
    public int FenceResets;
    public int LastFenceWaitCount;
    public ulong LastFenceTimeout;
    public int TransferDestroyed;
    public int DrawDestroyed;
    public int ResultCount;
    public int ResultFlags;
    public string GpuStat = "";
    public Cmd? Draw;
    public Cmd? Transfer;
    public List<Submit> Submits = new();
    public List<int> PreSubmitFenceWaits = new();
    public List<string> TransferDeleted = new();
    public List<string> DrawDeleted = new();
    public List<int> QueryResets = new();
    public List<Query> Queries = new();
    public List<int> GroupStack = new();

    public VmVkCommandBuffer1297(Host host)
    {
        Services = host;
        GraphicsFamily = host.GraphicsFamily;
        PoolName = "mCommandPool";
        PoolCreated = true;
        SemaphoreCount = MaxConcurrentSubmits;
        FenceCount = MaxConcurrentSubmits;
        if (host.GraphicsTimeQueries)
        {
            QueryPool = true;
            ResetQueries(MaxTimestampQueries);
        }
    }

    public void Destroy()
    {
        Destroyed = true;
        PoolReleased = false;
    }

    public Cmd GetTransfer()
    {
        if (Transfer == null)
        {
            Transfer = Create("VulkanRenderDevice.mTransferCommands");
        }

        return Transfer;
    }

    public Cmd GetDraw()
    {
        if (Draw == null)
        {
            Draw = Create("VulkanRenderDevice.mDrawCommands");
        }

        return Draw;
    }

    public void BeginFrame()
    {
        if (NextQuery > 0)
        {
            int used = NextQuery;
            ResetQueries(used);
            NextQuery = 0;
        }
    }

    public void Flush(bool finish, bool lastSubmit, bool uploadOnly)
    {
        if (!uploadOnly)
            EndRenderPasses++;
        if ((!uploadOnly && Draw != null) || Transfer != null)
        {
            int count = 0;
            int first = 0;
            int second = 0;
            if (Transfer != null)
            {
                Transfer.Ended = true;
                first = Transfer.Id;
                TransferDeleted.Add(Transfer.Name);
                Transfer = null;
                count++;
            }

            if (!uploadOnly && Draw != null)
            {
                Draw.Ended = true;
                if (count == 0)
                    first = Draw.Id;
                else
                    second = Draw.Id;
                DrawDeleted.Add(Draw.Name);
                Draw = null;
                count++;
            }

            SubmitBatch(first, second, count, finish, lastSubmit);
            CurrentRendered += count;
        }
    }

    public void Wait(bool finish)
    {
        Wait(finish, false);
    }

    public void Wait(bool finish, bool uploadOnly)
    {
        if (finish)
        {
            ClockResets++;
            Clocks++;
            Acquires++;
        }

        Flush(finish, true, uploadOnly);
        if (finish)
        {
            if (!Services.VSync)
                FpsLimits++;
            Presents++;
        }

        int fences = NextSubmit;
        if (fences > MaxConcurrentSubmits)
            fences = MaxConcurrentSubmits;
        if (fences > 0)
        {
            FenceWaits++;
            FenceResets++;
            LastFenceWaitCount = fences;
            LastFenceTimeout = WaitForever;
        }

        DeleteFrameObjects(uploadOnly);
        NextSubmit = 0;
        if (finish)
        {
            Unclocks++;
            Rendered = CurrentRendered;
            CurrentRendered = 0;
        }
    }

    public void DeleteFrameObjects(bool uploadOnly)
    {
        TransferDestroyed += TransferDeleted.Count;
        TransferDeleted.Clear();
        if (!uploadOnly)
        {
            DrawDestroyed += DrawDeleted.Count;
            DrawDeleted.Clear();
        }
    }

    public void PushGroup(string name)
    {
        if (!Services.GpuStatActive)
            return;
        if (NextQuery < MaxTimestampQueries && Services.GraphicsTimeQueries)
        {
            var query = new Query();
            query.Name = name;
            query.Start = NextQuery++;
            query.End = 0;
            GetDraw().TimestampWrites.Add(query.Start);
            GroupStack.Add(Queries.Count);
            Queries.Add(query);
        }
    }

    public void PopGroup()
    {
        if (!Services.GpuStatActive || GroupStack.Count == 0)
            return;
        int index = GroupStack[GroupStack.Count - 1];
        GroupStack.RemoveAt(GroupStack.Count - 1);
        if (NextQuery < MaxTimestampQueries && Services.GraphicsTimeQueries)
        {
            Queries[index].End = NextQuery++;
            GetDraw().TimestampWrites.Add(Queries[index].End);
        }
    }

    public void UpdateGpuStats()
    {
        if (NextQuery > 0)
        {
            ResultCount = NextQuery;
            ResultFlags = QueryResult64 | QueryResultWait;
        }

        double period = Services.TimestampPeriod;
        var built = new List<string>();
        for (int i = 0; i < Queries.Count; i++)
        {
            Query query = Queries[i];
            if (query.End <= query.Start)
                continue;
            long elapsed = (long)Services.Timestamps[query.End] - (long)Services.Timestamps[query.Start];
            if (elapsed < 0)
                elapsed = 0;
            double ms = elapsed * period / 1000000.0;
            built.Add(FormatMs(query.Name, ms));
        }

        GpuStat = string.Concat(built);
        Queries.Clear();
        GroupStack.Clear();
        Services.GpuStatActive = Services.KeepGpuStatActive;
        Services.KeepGpuStatActive = false;
    }

    void SubmitBatch(int first, int second, int count, bool finish, bool lastSubmit)
    {
        int index = NextSubmit % MaxConcurrentSubmits;
        if (NextSubmit >= MaxConcurrentSubmits)
            PreSubmitFenceWaits.Add(index);
        int waitSemaphore = -1;
        int waitStage = 0;
        if (NextSubmit > 0)
        {
            waitSemaphore = (NextSubmit - 1) % MaxConcurrentSubmits;
            waitStage = TopOfPipe;
        }

        bool present = finish && Services.PresentImageIndex != -1;
        int presentSignal = present ? Services.PresentImageIndex : -1;
        int signal = lastSubmit ? -1 : index;
        Submits.Add(new Submit(index, count, first, second, waitSemaphore, waitStage, present, presentSignal, signal, finish, lastSubmit));
        NextSubmit++;
    }

    void ResetQueries(int count)
    {
        Cmd draw = GetDraw();
        draw.QueryResets.Add(count);
        QueryResets.Add(count);
    }

    Cmd Create(string name)
    {
        var cmd = new Cmd();
        cmd.Id = ++NextCmd;
        cmd.Name = name;
        cmd.Begun = true;
        return cmd;
    }

    static string FormatMs(string name, double ms)
    {
        string num = ms.ToString("0.00", CultureInfo.InvariantCulture);
        if (num.Length < 4)
            num = num.PadLeft(4, '0');
        return name + "=" + num + " ms\n";
    }
}
