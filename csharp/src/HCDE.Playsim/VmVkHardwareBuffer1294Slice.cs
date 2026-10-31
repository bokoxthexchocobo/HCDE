namespace HCDE.Playsim;

/// <summary>
/// Vulkan hardware-buffer upload, resize, and lock from <c>vk_hwbuffer.cpp</c> (1-253).
/// Chunk 1294. The device, command lists, and vertex-format table are host services.
/// </summary>
public sealed class VmVkHardwareBuffer1294
{
    public const int UsageStatic = 0;
    public const int UsageStream = 1;
    public const int UsagePersistent = 2;
    public const int UsageMappable = 3;
    public const int TransferSrc = 0x1;
    public const int TransferDst = 0x2;
    public const int UniformBuffer = 0x10;
    public const int StorageBuffer = 0x20;
    public const int IndexBuffer = 0x40;
    public const int VertexBuffer = 0x80;
    public const int HostVisible = 0x2;
    public const int HostCoherent = 0x4;
    public const int DeviceLocal = 0x1;

    public sealed class GpuBuffer
    {
        public int Usage;
        public int Bytes;
        public string Name = "";
        public string Memory = "";
        public bool Dedicated;
        public bool CreatedMapped;
        public byte[] Data = Array.Empty<byte>();
    }

    public readonly struct Copy
    {
        public Copy(int srcOffset, int dstOffset, int size)
        {
            SrcOffset = srcOffset;
            DstOffset = dstOffset;
            Size = size;
        }

        public readonly int SrcOffset;
        public readonly int DstOffset;
        public readonly int Size;
    }

    public bool Attached = true;
    public bool Registered = true;
    public bool Removed;
    public int BufferType;
    public int BindingPoint;
    public bool Persistent;
    public int StoredSize;
    public bool Mapped;
    public int VertexFormat = -1;
    public int FormatBindings;
    public int FormatAttributes;
    public int FormatStride;
    public int NextVertexFormat;
    public int Waits;
    public bool LastWaitFinish;
    public int DescriptorUpdates;
    public byte[] StaticUpload = Array.Empty<byte>();
    public GpuBuffer? DeviceBuffer;
    public GpuBuffer? Staging;
    public List<GpuBuffer> DrawDeleted = new();
    public List<GpuBuffer> TransferDeleted = new();
    public List<Copy> Copies = new();
    public List<uint> BoundStarts = new();

    public static VmVkHardwareBuffer1294 Vertex()
    {
        return new VmVkHardwareBuffer1294(VertexBuffer);
    }

    public static VmVkHardwareBuffer1294 Index()
    {
        return new VmVkHardwareBuffer1294(IndexBuffer);
    }

    public static VmVkHardwareBuffer1294 Data(int binding, bool ssbo, bool needResize)
    {
        int usage = ssbo ? StorageBuffer : UniformBuffer;
        if (needResize)
            usage |= TransferSrc | TransferDst;
        var buffer = new VmVkHardwareBuffer1294(usage);
        buffer.BindingPoint = binding;
        return buffer;
    }

    VmVkHardwareBuffer1294(int bufferType)
    {
        BufferType = bufferType;
    }

    public void Destroy()
    {
        if (Attached)
            Removed = true;
    }

    public void Reset()
    {
        if (!Attached)
            return;
        if (DeviceBuffer != null && Mapped)
            Mapped = false;
        if (DeviceBuffer != null)
        {
            DrawDeleted.Add(DeviceBuffer);
            DeviceBuffer = null;
        }

        if (Staging != null)
        {
            TransferDeleted.Add(Staging);
            Staging = null;
        }
    }

    public void SetData(int size, byte[]? data, int usage)
    {
        int bytes = Math.Max(size, 16);
        RetireCurrent();
        if (usage == UsageStatic || usage == UsageStream)
        {
            Persistent = false;
            string kind = usage == UsageStatic ? "Static" : "Stream";
            DeviceBuffer = Allocate(TransferDst | BufferType, bytes, "VkHardwareBuffer." + kind, "gpu-only", false, false);
            Staging = Allocate(TransferSrc, bytes, "VkHardwareBuffer.Staging." + kind, "cpu-only", false, false);
            if (data != null)
                Array.Copy(data, 0, Staging.Data, 0, size);
            CopyWhole(Staging, DeviceBuffer);
        }
        else if (usage == UsagePersistent)
        {
            Persistent = true;
            DeviceBuffer = Allocate(BufferType, bytes, "VkHardwareBuffer.Persistent", "unknown", true, true);
            Mapped = true;
            if (data != null)
                Array.Copy(data, 0, DeviceBuffer.Data, 0, size);
        }
        else if (usage == UsageMappable)
        {
            Persistent = false;
            DeviceBuffer = Allocate(BufferType, bytes, "VkHardwareBuffer.Mappable", "unknown", false, false);
            if (data != null)
                Array.Copy(data, 0, DeviceBuffer.Data, 0, size);
        }

        StoredSize = size;
    }

    public void SetSubData(int offset, int size, byte[] data)
    {
        int bytes = Math.Max(size, 16);
        if (Staging != null)
        {
            Array.Copy(data, 0, Staging.Data, offset, bytes);
            CopyRange(Staging, DeviceBuffer!, offset, offset, bytes);
        }
        else
        {
            Array.Copy(data, 0, DeviceBuffer!.Data, offset, bytes);
        }
    }

    public void Resize(int newSize)
    {
        int bytes = Math.Max(newSize, 16);
        int oldSize = StoredSize;
        GpuBuffer old = DeviceBuffer!;
        Mapped = false;
        DeviceBuffer = Allocate(BufferType, bytes, "VkHardwareBuffer.Resized", "unknown", true, true);
        StoredSize = bytes;
        CopyRange(old, DeviceBuffer, 0, 0, oldSize);
        TransferDeleted.Add(old);
        Waits++;
        LastWaitFinish = false;
        DescriptorUpdates++;
        Mapped = true;
    }

    public void Map()
    {
        if (!Persistent && DeviceBuffer != null)
            Mapped = true;
    }

    public void Unmap()
    {
        if (!Persistent)
            Mapped = false;
    }

    public byte[] Lock(int size)
    {
        int bytes = Math.Max(size, 16);
        if (DeviceBuffer == null)
        {
            StaticUpload = new byte[bytes];
            Mapped = true;
            return StaticUpload;
        }

        if (!Persistent)
            Mapped = true;
        return DeviceBuffer.Data;
    }

    public void Unlock()
    {
        if (DeviceBuffer == null)
        {
            Mapped = false;
            byte[] upload = StaticUpload;
            SetData(upload.Length, upload, UsageStatic);
            StaticUpload = Array.Empty<byte>();
        }
        else if (!Persistent)
        {
            Mapped = false;
        }
    }

    public void SetFormat(int bindings, int attributes, int stride)
    {
        FormatBindings = bindings;
        FormatAttributes = attributes;
        FormatStride = stride;
        VertexFormat = NextVertexFormat;
    }

    public void BindRange(int start, int length)
    {
        BoundStarts.Add(unchecked((uint)start));
        _ = length;
    }

    void RetireCurrent()
    {
        if (DeviceBuffer != null)
        {
            DrawDeleted.Add(DeviceBuffer);
            DeviceBuffer = null;
        }

        if (Staging != null)
        {
            TransferDeleted.Add(Staging);
            Staging = null;
        }
    }

    static GpuBuffer Allocate(int usage, int bytes, string name, string memory, bool dedicated, bool createdMapped)
    {
        return new GpuBuffer
        {
            Usage = usage,
            Bytes = bytes,
            Name = name,
            Memory = memory,
            Dedicated = dedicated,
            CreatedMapped = createdMapped,
            Data = new byte[bytes],
        };
    }

    void CopyWhole(GpuBuffer src, GpuBuffer dst)
    {
        CopyRange(src, dst, 0, 0, dst.Data.Length);
    }

    void CopyRange(GpuBuffer src, GpuBuffer dst, int srcOffset, int dstOffset, int size)
    {
        if (size > 0)
            Array.Copy(src.Data, srcOffset, dst.Data, dstOffset, size);
        Copies.Add(new Copy(srcOffset, dstOffset, size));
    }
}
