namespace HCDE.Playsim.Tests;

public class VmVkHardwareBuffer1294SliceTests
{
    [Fact]
    public void SetData_PadsSixteenAndResizeCopiesTheStoredSize()
    {
        var buffer = VmVkHardwareBuffer1294.Vertex();
        Assert.Equal(VmVkHardwareBuffer1294.VertexBuffer, buffer.BufferType);
        Assert.True(buffer.Registered);
        buffer.SetData(4, new byte[] { 1, 2, 3, 4 }, VmVkHardwareBuffer1294.UsageStatic);
        Assert.False(buffer.Persistent);
        Assert.Equal(4, buffer.StoredSize);
        Assert.Equal(16, buffer.DeviceBuffer!.Data.Length);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, buffer.DeviceBuffer.Data.Take(4).ToArray());
        Assert.Equal(0, buffer.DeviceBuffer.Data[4]);
        Assert.Equal("VkHardwareBuffer.Static", buffer.DeviceBuffer.Name);
        Assert.Equal("gpu-only", buffer.DeviceBuffer.Memory);
        Assert.Equal(VmVkHardwareBuffer1294.TransferDst | VmVkHardwareBuffer1294.VertexBuffer, buffer.DeviceBuffer.Usage);
        Assert.Equal("VkHardwareBuffer.Staging.Static", buffer.Staging!.Name);
        Assert.Equal("cpu-only", buffer.Staging.Memory);
        Assert.Equal(16, buffer.Copies[0].Size);

        buffer.SetData(0, null, VmVkHardwareBuffer1294.UsageStream);
        Assert.Single(buffer.DrawDeleted);
        Assert.Single(buffer.TransferDeleted);
        Assert.Equal("VkHardwareBuffer.Stream", buffer.DeviceBuffer!.Name);
        Assert.Equal(0, buffer.StoredSize);
        Assert.Equal(16, buffer.DeviceBuffer.Data.Length);
        Assert.All(buffer.DeviceBuffer.Data, b => Assert.Equal(0, b));

        buffer.SetData(0, null, VmVkHardwareBuffer1294.UsagePersistent);
        Assert.True(buffer.Persistent);
        Assert.Null(buffer.Staging);
        Assert.True(buffer.Mapped);
        Assert.True(buffer.DeviceBuffer!.Dedicated);
        Assert.True(buffer.DeviceBuffer.CreatedMapped);
        Assert.Equal("unknown", buffer.DeviceBuffer.Memory);
        buffer.Unmap();
        Assert.True(buffer.Mapped);
        buffer.Map();
        Assert.True(buffer.Mapped);

        buffer.Resize(8);
        Assert.Equal(16, buffer.StoredSize);
        Assert.Equal("VkHardwareBuffer.Resized", buffer.DeviceBuffer!.Name);
        Assert.Equal(0, buffer.Copies[^1].Size);
        Assert.False(buffer.LastWaitFinish);
        Assert.Equal(1, buffer.Waits);
        Assert.Equal(1, buffer.DescriptorUpdates);
        Assert.True(buffer.Mapped);
        Assert.Equal(3, buffer.TransferDeleted.Count);

        buffer.NextVertexFormat = 12;
        buffer.SetFormat(2, 3, 20);
        Assert.Equal(12, buffer.VertexFormat);
        Assert.Equal(2, buffer.FormatBindings);
        Assert.Equal(20, buffer.FormatStride);
        buffer.BindRange(5, 99);
        Assert.Equal(new uint[] { 5 }, buffer.BoundStarts);
    }

    [Fact]
    public void Lock_UploadsAStaticBufferAndSubDataPadsTheWrite()
    {
        var data = VmVkHardwareBuffer1294.Data(3, true, true);
        Assert.Equal(3, data.BindingPoint);
        Assert.Equal(
            VmVkHardwareBuffer1294.StorageBuffer | VmVkHardwareBuffer1294.TransferSrc | VmVkHardwareBuffer1294.TransferDst,
            data.BufferType);
        var index = VmVkHardwareBuffer1294.Index();
        Assert.Equal(VmVkHardwareBuffer1294.IndexBuffer, index.BufferType);

        var payload = new byte[32];
        payload[0] = 4;
        data.SetData(32, payload, VmVkHardwareBuffer1294.UsageMappable);
        Assert.False(data.Persistent);
        Assert.Null(data.Staging);
        Assert.Equal("VkHardwareBuffer.Mappable", data.DeviceBuffer!.Name);
        Assert.False(data.DeviceBuffer.Dedicated);
        var patch = new byte[16];
        for (int i = 0; i < patch.Length; i++)
            patch[i] = 7;
        data.SetSubData(4, 8, patch);
        Assert.Equal(7, data.DeviceBuffer.Data[4]);
        Assert.Equal(7, data.DeviceBuffer.Data[19]);
        Assert.Equal(4, data.DeviceBuffer.Data[0]);
        Assert.Empty(data.Copies);
        Assert.Equal(32, data.StoredSize);

        var fresh = VmVkHardwareBuffer1294.Vertex();
        var locked = fresh.Lock(4);
        Assert.Equal(16, locked.Length);
        locked[0] = 9;
        fresh.Unlock();
        Assert.False(fresh.Mapped);
        Assert.Empty(fresh.StaticUpload);
        Assert.Equal(16, fresh.StoredSize);
        Assert.Equal(9, fresh.DeviceBuffer!.Data[0]);
        Assert.Equal("VkHardwareBuffer.Static", fresh.DeviceBuffer.Name);
        Assert.False(fresh.Persistent);

        fresh.Map();
        Assert.True(fresh.Mapped);
        fresh.Unmap();
        Assert.False(fresh.Mapped);
        int stored = fresh.StoredSize;
        fresh.Reset();
        Assert.Null(fresh.DeviceBuffer);
        Assert.Null(fresh.Staging);
        Assert.False(fresh.Mapped);
        Assert.Equal(stored, fresh.StoredSize);
        Assert.Single(fresh.DrawDeleted);
        fresh.Attached = false;
        fresh.SetData(4, new byte[] { 1, 2, 3, 4 }, VmVkHardwareBuffer1294.UsageStatic);
        fresh.Reset();
        Assert.NotNull(fresh.DeviceBuffer);
        fresh.Destroy();
        Assert.False(fresh.Removed);
        fresh.Attached = true;
        fresh.Destroy();
        Assert.True(fresh.Removed);
    }
}
