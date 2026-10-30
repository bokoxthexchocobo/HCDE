namespace HCDE.Playsim.Tests;

public class VmVkCommandBuffer1297SliceTests
{
    [Fact]
    public void Flush_SubmitsTransferFirstAndFinishPublishesZero()
    {
        var host = new VmVkCommandBuffer1297.Host
        {
            GraphicsTimeQueries = true,
            GraphicsFamily = 2,
            PresentImageIndex = -1,
            VSync = true,
        };
        var mgr = new VmVkCommandBuffer1297(host);
        Assert.Equal("mCommandPool", mgr.PoolName);
        Assert.Equal(2, mgr.GraphicsFamily);
        Assert.Equal(8, mgr.SemaphoreCount);
        Assert.Equal(8, mgr.FenceCount);
        Assert.True(mgr.QueryPool);
        Assert.Equal(new[] { 100 }, mgr.QueryResets);
        Assert.NotNull(mgr.Draw);
        Assert.True(mgr.Draw!.Begun);
        Assert.Equal(100, mgr.Draw.QueryResets[0]);
        Assert.Null(mgr.Transfer);

        var transfer = mgr.GetTransfer();
        Assert.Equal("VulkanRenderDevice.mTransferCommands", transfer.Name);
        Assert.True(transfer.Begun);
        Assert.Same(transfer, mgr.GetTransfer());
        int transferId = transfer.Id;
        int drawId = mgr.Draw.Id;

        mgr.Flush(false, false, false);
        Assert.Equal(1, mgr.EndRenderPasses);
        Assert.Equal(2, mgr.CurrentRendered);
        Assert.Null(mgr.Draw);
        Assert.Null(mgr.Transfer);
        VmVkCommandBuffer1297.Submit first = Assert.Single(mgr.Submits);
        Assert.Equal(2, first.Count);
        Assert.Equal(transferId, first.FirstId);
        Assert.Equal(drawId, first.SecondId);
        Assert.Equal(-1, first.WaitSemaphore);
        Assert.Equal(0, first.SignalSemaphore);
        Assert.False(first.PresentWait);
        Assert.Equal(1, mgr.NextSubmit);
        Assert.Equal(new[] { "VulkanRenderDevice.mTransferCommands", "VulkanRenderDevice.mDrawCommands" }, mgr.TransferDeleted.Concat(mgr.DrawDeleted));

        mgr.DeleteFrameObjects(true);
        Assert.Equal(1, mgr.TransferDestroyed);
        Assert.Equal(new[] { "VulkanRenderDevice.mDrawCommands" }, mgr.DrawDeleted);
        mgr.DeleteFrameObjects(false);
        Assert.Equal(1, mgr.DrawDestroyed);

        mgr.Flush(false, false, false);
        Assert.Equal(2, mgr.EndRenderPasses);
        Assert.Single(mgr.Submits);
        Assert.Equal(2, mgr.CurrentRendered);

        var secondDraw = mgr.GetDraw();
        var upload = mgr.GetTransfer();
        int keptDraw = secondDraw.Id;
        int uploadId = upload.Id;
        mgr.Flush(false, false, true);
        Assert.Equal(2, mgr.EndRenderPasses);
        Assert.Null(mgr.Transfer);
        Assert.Equal(keptDraw, mgr.Draw!.Id);
        Assert.False(mgr.Draw.Ended);
        Assert.Equal(3, mgr.CurrentRendered);
        Assert.Equal(1, mgr.Submits[1].Count);
        Assert.Equal(uploadId, mgr.Submits[1].FirstId);
        Assert.Equal(0, mgr.Submits[1].SecondId);
        Assert.Equal(1, mgr.Submits[1].Index);
        Assert.Equal(0, mgr.Submits[1].WaitSemaphore);
        Assert.Equal(1, mgr.Submits[1].SignalSemaphore);
        Assert.False(mgr.Submits[1].LastSubmit);

        host.PresentImageIndex = 3;
        mgr.Wait(true);
        Assert.Equal(1, mgr.Acquires);
        Assert.Equal(0, mgr.FpsLimits);
        Assert.Equal(1, mgr.Presents);
        Assert.Equal(1, mgr.Unclocks);
        Assert.Equal(0, mgr.NextSubmit);
        Assert.Equal(4, mgr.Rendered);
        Assert.Equal(0, mgr.CurrentRendered);
        Assert.Equal(3, mgr.LastFenceWaitCount);
        Assert.Equal(VmVkCommandBuffer1297.WaitForever, mgr.LastFenceTimeout);
        VmVkCommandBuffer1297.Submit present = mgr.Submits[^1];
        Assert.True(present.Finish);
        Assert.True(present.LastSubmit);
        Assert.True(present.PresentWait);
        Assert.Equal(3, present.PresentSignal);
        Assert.Equal(-1, present.SignalSemaphore);
        Assert.Equal(VmVkCommandBuffer1297.TopOfPipe, present.WaitStage);
        Assert.Equal(1, present.WaitSemaphore);
        Assert.Equal(keptDraw, present.FirstId);
        Assert.Null(mgr.Draw);

        int rendered = mgr.Rendered;
        int presents = mgr.Presents;
        mgr.Wait(false);
        Assert.Equal(rendered, mgr.Rendered);
        Assert.Equal(presents, mgr.Presents);
        Assert.Equal(1, mgr.Acquires);

        host.VSync = false;
        mgr.Wait(true);
        Assert.Equal(1, mgr.FpsLimits);
        Assert.Equal(2, mgr.Presents);
        Assert.Equal(0, mgr.Rendered);
        Assert.Equal(1, mgr.FenceWaits);
        Assert.Equal(3, mgr.LastFenceWaitCount);
    }

    [Fact]
    public void Stats_SkipsAnOpenGroupAndBeginFrameResetsTheCursor()
    {
        var host = new VmVkCommandBuffer1297.Host
        {
            GraphicsTimeQueries = true,
            GpuStatActive = false,
            KeepGpuStatActive = true,
            TimestampPeriod = 1,
            PresentImageIndex = -1,
        };
        var mgr = new VmVkCommandBuffer1297(host);
        mgr.PushGroup("nope");
        mgr.PopGroup();
        Assert.Empty(mgr.Queries);
        Assert.Empty(mgr.GroupStack);

        host.GpuStatActive = true;
        mgr.PushGroup("outer");
        mgr.PushGroup("inner");
        mgr.PopGroup();
        mgr.PushGroup("wide");
        mgr.PopGroup();
        host.GraphicsTimeQueries = false;
        mgr.PopGroup();
        host.GraphicsTimeQueries = true;
        mgr.PushGroup("back");
        mgr.PopGroup();
        Assert.Equal(0, mgr.Queries[0].End);
        Assert.Equal(2, mgr.Queries[1].End);
        Assert.Equal(4, mgr.Queries[2].End);
        Assert.Equal(6, mgr.Queries[3].End);
        Assert.Equal(7, mgr.NextQuery);
        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 6 }, mgr.Draw!.TimestampWrites);

        host.Timestamps = new ulong[] { 0, 0, 2500000, 0, 12500000, 8000000, 1000000 };
        mgr.UpdateGpuStats();
        Assert.Equal("inner=2.50 ms\nwide=12.50 ms\nback=0.00 ms\n", mgr.GpuStat);
        Assert.Equal(7, mgr.ResultCount);
        Assert.Equal(VmVkCommandBuffer1297.QueryResult64 | VmVkCommandBuffer1297.QueryResultWait, mgr.ResultFlags);
        Assert.Empty(mgr.Queries);
        Assert.Empty(mgr.GroupStack);
        Assert.Equal(7, mgr.NextQuery);
        Assert.True(host.GpuStatActive);
        Assert.False(host.KeepGpuStatActive);

        mgr.BeginFrame();
        Assert.Equal(0, mgr.NextQuery);
        Assert.Equal(new[] { 100, 7 }, mgr.QueryResets);
        mgr.BeginFrame();
        Assert.Equal(new[] { 100, 7 }, mgr.QueryResets);

        mgr.UpdateGpuStats();
        Assert.Equal("", mgr.GpuStat);
        Assert.False(host.GpuStatActive);

        mgr.NextSubmit = 8;
        int before = mgr.EndRenderPasses;
        mgr.GetTransfer();
        mgr.Flush(true, false, true);
        Assert.Equal(before, mgr.EndRenderPasses);
        Assert.Equal(new[] { 0 }, mgr.PreSubmitFenceWaits);
        VmVkCommandBuffer1297.Submit wrapped = mgr.Submits[^1];
        Assert.Equal(0, wrapped.Index);
        Assert.Equal(1, wrapped.Count);
        Assert.Equal(7, wrapped.WaitSemaphore);
        Assert.Equal(VmVkCommandBuffer1297.TopOfPipe, wrapped.WaitStage);
        Assert.False(wrapped.PresentWait);
        Assert.Equal(0, wrapped.SignalSemaphore);
        Assert.False(wrapped.LastSubmit);
        Assert.Equal(9, mgr.NextSubmit);
        Assert.NotNull(mgr.Draw);
        Assert.False(mgr.Draw!.Ended);

        mgr.Destroy();
        Assert.True(mgr.Destroyed);
        Assert.False(mgr.PoolReleased);
        Assert.True(mgr.PoolCreated);
    }
}
