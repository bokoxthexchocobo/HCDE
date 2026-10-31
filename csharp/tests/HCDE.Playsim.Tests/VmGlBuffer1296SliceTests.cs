namespace HCDE.Playsim.Tests;

public class VmGlBuffer1296SliceTests
{
    [Fact]
    public void Resize_CopiesTheNewSizeAndLeavesPersistentSticky()
    {
        var host = new VmGlBuffer1296.Host { PersistentBuffers = true };
        var buffer = VmGlBuffer1296.Vertex(host);
        Assert.Equal(VmGlBuffer1296.ArrayBuffer, buffer.UseType);
        Assert.True(buffer.NoMap);
        Assert.Equal(1, buffer.BufferId);

        buffer.SetData(8, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, VmGlBuffer1296.UsageStatic);
        Assert.True(buffer.NoMap);
        Assert.False(buffer.Persistent);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, buffer.Bytes);
        Assert.Equal(VmGlBuffer1296.StaticDraw, buffer.LastDrawUsage);
        buffer.Resize(40);
        Assert.Empty(buffer.Copies);
        Assert.Equal(8, buffer.BufferSize);
        Assert.Equal(1, buffer.BufferId);

        buffer.SetData(8, new byte[] { 9, 9, 9, 9, 9, 9, 9, 9 }, VmGlBuffer1296.UsagePersistent);
        Assert.False(buffer.NoMap);
        Assert.True(buffer.Persistent);
        Assert.True(buffer.UsedBufferStorage);
        Assert.Equal(VmGlBuffer1296.MapWrite | VmGlBuffer1296.MapPersistent | VmGlBuffer1296.MapCoherent, buffer.LastStorageFlags);
        Assert.Equal(8, buffer.LastMapLength);
        Assert.NotNull(buffer.MapToken);
        Assert.Equal(new byte[8], buffer.Bytes);

        buffer.SetData(8, new byte[] { 3, 3, 3, 3, 3, 3, 3, 3 }, VmGlBuffer1296.UsageMappable);
        Assert.False(buffer.NoMap);
        Assert.True(buffer.Persistent);
        Assert.Null(buffer.MapToken);
        Assert.False(buffer.UsedBufferStorage);
        Assert.Equal(VmGlBuffer1296.StaticDraw, buffer.LastDrawUsage);
        Assert.Equal(new byte[8], buffer.Bytes);
        buffer.Map();
        buffer.Unmap();
        Assert.Null(buffer.MapToken);
        Assert.Equal(0, buffer.GlUnmapCalls);

        host.PersistentBuffers = false;
        buffer.Resize(20);
        VmGlBuffer1296.Copy copy = Assert.Single(buffer.Copies);
        Assert.Equal(20, copy.Size);
        Assert.Equal(1, copy.ReadBuffer);
        Assert.Equal(VmGlBuffer1296.CopyReadBuffer, copy.ReadTarget);
        Assert.Equal(VmGlBuffer1296.ArrayBuffer, copy.WriteTarget);
        Assert.Equal(new[] { 1 }, buffer.DeletedBuffers);
        Assert.Equal(2, buffer.BufferId);
        Assert.False(buffer.Persistent);
        Assert.Null(buffer.MapToken);
        Assert.False(buffer.NoMap);
        Assert.Equal(20, buffer.BufferSize);
        Assert.Equal(VmGlBuffer1296.StreamDraw, buffer.LastDrawUsage);
        Assert.False(buffer.UsedBufferStorage);
        Assert.Equal(20, buffer.Bytes.Length);
        Assert.Equal(5, buffer.ResetCount);
        Assert.Equal(1, buffer.GlUnmapCalls);
        buffer.Resize(20);
        buffer.Resize(4);
        Assert.Single(buffer.Copies);
    }

    [Fact]
    public void Format_WalksEverySlotAndLockLeavesTheMapMember()
    {
        var host = new VmGlBuffer1296.Host();
        var vertex = VmGlBuffer1296.Vertex(host);
        var attrs = new[]
        {
            new VmGlBuffer1296.Attr(0, 0, VmGlBuffer1296.FmtFloat3, 0),
            new VmGlBuffer1296.Attr(1, 2, VmGlBuffer1296.FmtByte4, 12),
            new VmGlBuffer1296.Attr(0, 4, VmGlBuffer1296.FmtPacked, 16),
            new VmGlBuffer1296.Attr(0, 9, VmGlBuffer1296.FmtByte4UInt, 20),
            new VmGlBuffer1296.Attr(0, 10, VmGlBuffer1296.FmtFloat4, 0),
            new VmGlBuffer1296.Attr(0, -1, VmGlBuffer1296.FmtFloat4, 4),
        };
        vertex.SetFormat(2, attrs.Length, 16, attrs);
        Assert.Equal(16, vertex.Stride);
        Assert.Equal(2, vertex.NumBindingPoints);
        Assert.Equal(0, vertex.Attrs[1].Size);
        Assert.Equal(VmGlBuffer1296.Float, vertex.Attrs[0].Format);
        Assert.Equal(3, vertex.Attrs[0].Size);
        Assert.False(vertex.Attrs[0].Normalize);
        Assert.Equal(VmGlBuffer1296.UnsignedByte, vertex.Attrs[2].Format);
        Assert.True(vertex.Attrs[2].Normalize);
        Assert.False(vertex.Attrs[2].IntegerType);
        Assert.Equal(VmGlBuffer1296.Int2101010Rev, vertex.Attrs[4].Format);
        Assert.True(vertex.Attrs[4].Normalize);
        Assert.True(vertex.Attrs[9].IntegerType);
        Assert.False(vertex.Attrs[9].Normalize);

        vertex.BindVertices(null);
        Assert.Equal(10, vertex.AttribBinds.Count);
        Assert.False(vertex.AttribBinds[1].Enabled);
        Assert.False(vertex.AttribBinds[3].Enabled);
        Assert.Equal(0L, vertex.AttribBinds[0].Offset);
        Assert.Equal(12L, vertex.AttribBinds[2].Offset);
        Assert.Equal(16L, vertex.AttribBinds[4].Offset);
        Assert.True(vertex.AttribBinds[4].Normalize);
        Assert.True(vertex.AttribBinds[9].Enabled);
        Assert.True(vertex.AttribBinds[9].IntegerType);
        Assert.Equal(20L, vertex.AttribBinds[9].Offset);
        Assert.Equal(16, vertex.AttribBinds[0].Stride);

        vertex.BindVertices(new[] { 5, 7 });
        Assert.Equal(80L, vertex.AttribBinds[0].Offset);
        Assert.Equal(124L, vertex.AttribBinds[2].Offset);
        Assert.Equal(96L, vertex.AttribBinds[4].Offset);
        Assert.Equal(100L, vertex.AttribBinds[9].Offset);

        int token = vertex.Lock(6);
        Assert.Null(vertex.MapToken);
        Assert.True(vertex.LockActive);
        Assert.Equal(token, vertex.LockToken);
        Assert.Equal(6, vertex.BufferSize);
        Assert.True(vertex.NoMap);
        Assert.Equal(VmGlBuffer1296.MapWrite | VmGlBuffer1296.MapInvalidateBuffer | VmGlBuffer1296.MapUnsynchronized, vertex.LastMapFlags);
        Assert.Equal(6, vertex.LastMapLength);
        vertex.Unlock();
        Assert.Null(vertex.MapToken);
        Assert.False(vertex.LockActive);
        Assert.Equal(token, vertex.LockToken);
        Assert.Equal(2, vertex.ResetCount);

        vertex.GPUDropSync();
        Assert.Equal(1, vertex.Sync);
        Assert.Equal(VmGlBuffer1296.SyncGpuCommandsComplete, vertex.FenceCondition);
        vertex.GPUDropSync();
        Assert.Equal(1, vertex.DeletedSyncs);
        Assert.Equal(2, vertex.Sync);
        host.WaitStatus = VmGlBuffer1296.ConditionSatisfied;
        vertex.GPUWaitSync();
        Assert.False(vertex.WaitFailed);
        Assert.Equal(0, vertex.Sync);
        Assert.Equal(VmGlBuffer1296.WaitTimeoutNs, vertex.LastWaitTimeout);
        Assert.Equal(VmGlBuffer1296.SyncFlushCommands, vertex.LastWaitFlags);
        Assert.Equal(2, vertex.DeletedSyncs);
        vertex.GPUDropSync();
        host.WaitStatus = 7;
        vertex.GPUWaitSync();
        Assert.True(vertex.WaitFailed);
        Assert.Equal(0, vertex.Sync);
        Assert.Equal(7, vertex.LastWaitStatus);

        var data = VmGlBuffer1296.Data(host, 4, true);
        Assert.Equal(VmGlBuffer1296.ShaderStorageBuffer, data.UseType);
        Assert.Equal(4, data.BindingPoint);
        data.BindRange(8, 16);
        Assert.Equal(8, data.RangeStart);
        Assert.Equal(16, data.RangeLength);
        Assert.True(data.RangeBound);
        data.BindBase();
        Assert.True(data.BaseBound);
        var index = VmGlBuffer1296.Index(host);
        Assert.Equal(VmGlBuffer1296.ElementArrayBuffer, index.UseType);
        var uniform = VmGlBuffer1296.Data(host, 1, false);
        Assert.Equal(VmGlBuffer1296.UniformBuffer, uniform.UseType);

        vertex.SetData(4, new byte[] { 1, 2, 3, 4 }, VmGlBuffer1296.UsageStream);
        vertex.SetSubData(1, 2, new byte[] { 8, 9 });
        Assert.Equal(new byte[] { 1, 8, 9, 4 }, vertex.Bytes);
        Assert.Equal(VmGlBuffer1296.StreamDraw, vertex.LastDrawUsage);
        Assert.Equal(1, vertex.SubWrites[0].Offset);
        Assert.Equal(2, vertex.SubWrites[0].Size);

        int id = vertex.BufferId;
        vertex.Destroy();
        Assert.Equal(0, vertex.BoundId);
        Assert.Equal(new[] { id }, vertex.DeletedBuffers);
        Assert.Equal(1, vertex.DestroyUnmaps);
        Assert.True(vertex.Destroyed);
        vertex.Destroy();
        Assert.Equal(1, vertex.DestroyUnmaps);
    }
}
