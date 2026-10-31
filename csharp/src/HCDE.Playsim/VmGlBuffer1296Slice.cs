namespace HCDE.Playsim;

/// <summary>
/// OpenGL buffer upload, attribute bind, and resize from <c>gl_buffers.cpp</c> (1-266).
/// Chunk 1296. Buffer ids, map tokens, and the persistent-buffer flag are host services.
/// The render-state reset is a counter. Resize copies the size stored by the nested
/// <c>SetData</c> call, which is the new size, and a later mappable upload leaves
/// <c>Persistent</c> set so <c>Map</c> stays a no-op.
/// </summary>
public sealed class VmGlBuffer1296
{
    public const int UsageStatic = 0;
    public const int UsageStream = 1;
    public const int UsagePersistent = 2;
    public const int UsageMappable = 3;
    public const int ArrayBuffer = 0x8892;
    public const int ElementArrayBuffer = 0x8893;
    public const int ShaderStorageBuffer = 0x90D2;
    public const int UniformBuffer = 0x8A11;
    public const int CopyReadBuffer = 0x8F36;
    public const int StaticDraw = 0x88E4;
    public const int StreamDraw = 0x88E0;
    public const int MapWrite = 0x0002;
    public const int MapPersistent = 0x0040;
    public const int MapCoherent = 0x0080;
    public const int MapInvalidateBuffer = 0x0008;
    public const int MapUnsynchronized = 0x0020;
    public const int Float = 0x1406;
    public const int UnsignedByte = 0x1401;
    public const int Int2101010Rev = 0x8D9F;
    public const int AlreadySignaled = 0x911A;
    public const int ConditionSatisfied = 0x911C;
    public const int SyncGpuCommandsComplete = 0x9117;
    public const int SyncFlushCommands = 0x1;
    public const int AttrMax = 10;
    public const int FmtFloat4 = 0;
    public const int FmtFloat3 = 1;
    public const int FmtFloat2 = 2;
    public const int FmtFloat = 3;
    public const int FmtByte4 = 4;
    public const int FmtPacked = 5;
    public const int FmtByte4UInt = 6;
    public const long WaitTimeoutNs = 1000L * 1000L * 50L;

    public sealed class Host
    {
        public bool PersistentBuffers;
        public int WaitStatus = AlreadySignaled;
        public int NextBufferId = 1;
        public int NextMap = 1;
        public int NextSync = 1;
    }

    public readonly record struct Attr(int Binding, int Location, int Format, int Offset);
    public readonly record struct AttrState(int Binding, int Format, bool Normalize, bool IntegerType, int Size, int Offset);
    public readonly record struct AttribBind(int Index, bool Enabled, int Size, int Format, bool Normalize, bool IntegerType, int Stride, long Offset);
    public readonly record struct Copy(int ReadTarget, int WriteTarget, int ReadBuffer, int Size);
    public readonly record struct SubWrite(int Offset, int Size);

    static readonly int[] GlFormats = { Float, Float, Float, Float, UnsignedByte, Int2101010Rev, UnsignedByte };
    static readonly int[] GlSizes = { 4, 3, 2, 1, 4, 4, 4 };
    static readonly bool[] GlNormalize = { false, false, false, false, true, true, false };
    static readonly bool[] GlInteger = { false, false, false, false, false, false, true };

    public int UseType;
    public int BufferId;
    public int BoundId;
    public bool Persistent;
    public bool NoMap = true;
    public int BufferSize;
    public int? MapToken;
    public int LockToken;
    public bool LockActive;
    public int ResetCount;
    public int BindingPoint;
    public int Stride;
    public int NumBindingPoints;
    public int LastDrawUsage;
    public int LastStorageFlags;
    public bool UsedBufferStorage;
    public int LastMapFlags;
    public int LastMapLength;
    public byte[] Bytes = Array.Empty<byte>();
    public int Sync;
    public int DeletedSyncs;
    public int FenceCondition;
    public long LastWaitTimeout;
    public int LastWaitFlags;
    public int LastWaitStatus;
    public bool WaitFailed;
    public int GlUnmapCalls;
    public int DestroyUnmaps;
    public bool Destroyed;
    public int RangeStart;
    public int RangeLength;
    public bool RangeBound;
    public bool BaseBound;
    public Host Services;
    public AttrState[] Attrs = new AttrState[AttrMax];
    public List<AttribBind> AttribBinds = new();
    public List<Copy> Copies = new();
    public List<int> DeletedBuffers = new();
    public List<SubWrite> SubWrites = new();

    public static VmGlBuffer1296 Vertex(Host host)
    {
        return new VmGlBuffer1296(host, ArrayBuffer);
    }

    public static VmGlBuffer1296 Index(Host host)
    {
        return new VmGlBuffer1296(host, ElementArrayBuffer);
    }

    public static VmGlBuffer1296 Data(Host host, int binding, bool ssbo)
    {
        var buffer = new VmGlBuffer1296(host, ssbo ? ShaderStorageBuffer : UniformBuffer);
        buffer.BindingPoint = binding;
        return buffer;
    }

    VmGlBuffer1296(Host host, int useType)
    {
        Services = host;
        UseType = useType;
        BufferId = host.NextBufferId++;
        BoundId = BufferId;
        NoMap = true;
    }

    public void Bind()
    {
        BoundId = BufferId;
    }

    public void SetData(int size, byte[]? data, int usage)
    {
        Bind();
        if (usage == UsageStatic)
        {
            Upload(size, data, StaticDraw);
        }
        else if (usage == UsageStream)
        {
            Upload(size, data, StreamDraw);
        }
        else if (usage == UsagePersistent)
        {
            Persistent = Services.PersistentBuffers;
            if (Persistent)
            {
                LastStorageFlags = MapWrite | MapPersistent | MapCoherent;
                UsedBufferStorage = true;
                LastDrawUsage = 0;
                Bytes = new byte[size];
                MapToken = Services.NextMap++;
                LastMapFlags = LastStorageFlags;
                LastMapLength = size;
            }
            else
            {
                Upload(size, null, StreamDraw);
                MapToken = null;
            }

            NoMap = false;
        }
        else if (usage == UsageMappable)
        {
            Upload(size, null, StaticDraw);
            MapToken = null;
        }

        BufferSize = size;
        Invalidate();
    }

    public void SetSubData(int offset, int size, byte[]? data)
    {
        Bind();
        if (data != null && size > 0)
        {
            int need = offset + size;
            if (Bytes.Length < need)
            {
                var grown = new byte[need];
                Array.Copy(Bytes, grown, Bytes.Length);
                Bytes = grown;
            }

            int count = size;
            if (count > data.Length)
                count = data.Length;
            Array.Copy(data, 0, Bytes, offset, count);
        }

        SubWrites.Add(new SubWrite(offset, size));
    }

    public void Map()
    {
        if (!Persistent && !NoMap)
        {
            Bind();
            MapToken = Services.NextMap++;
            LastMapFlags = MapWrite | MapUnsynchronized;
            LastMapLength = BufferSize;
            Invalidate();
        }
    }

    public void Unmap()
    {
        if (!Persistent && MapToken != null)
        {
            Bind();
            GlUnmap();
            Invalidate();
            MapToken = null;
        }
    }

    public int Lock(int size)
    {
        SetData(size, null, UsageMappable);
        LockToken = Services.NextMap++;
        LockActive = true;
        LastMapFlags = MapWrite | MapInvalidateBuffer | MapUnsynchronized;
        LastMapLength = size;
        return LockToken;
    }

    public void Unlock()
    {
        Bind();
        GlUnmap();
        Invalidate();
        LockActive = false;
    }

    public void Resize(int newsize)
    {
        if (newsize > BufferSize && !NoMap)
        {
            int oldBuffer = BufferId;
            Bind();
            GlUnmap();
            BufferId = Services.NextBufferId++;
            SetData(newsize, null, UsagePersistent);
            Copies.Add(new Copy(CopyReadBuffer, UseType, oldBuffer, BufferSize));
            DeletedBuffers.Add(oldBuffer);
            BufferSize = newsize;
            Invalidate();
        }
    }

    public void GPUDropSync()
    {
        if (Sync != 0)
        {
            DeletedSyncs++;
            Sync = 0;
        }

        Sync = Services.NextSync++;
        FenceCondition = SyncGpuCommandsComplete;
    }

    public void GPUWaitSync()
    {
        LastWaitTimeout = WaitTimeoutNs;
        LastWaitFlags = SyncFlushCommands;
        LastWaitStatus = Services.WaitStatus;
        WaitFailed = LastWaitStatus != AlreadySignaled && LastWaitStatus != ConditionSatisfied;
        DeletedSyncs++;
        Sync = 0;
    }

    public void SetFormat(int numBindingPoints, int numAttributes, int stride, Attr[] attrs)
    {
        Stride = stride;
        NumBindingPoints = numBindingPoints;
        for (int i = 0; i < numAttributes; i++)
        {
            int location = attrs[i].Location;
            if (location >= 0 && location < AttrMax)
            {
                int format = attrs[i].Format;
                if ((uint)format >= (uint)GlFormats.Length)
                    throw new ArgumentOutOfRangeException(nameof(attrs));
                Attrs[location] = new AttrState(
                    attrs[i].Binding,
                    GlFormats[format],
                    GlNormalize[format],
                    GlInteger[format],
                    GlSizes[format],
                    attrs[i].Offset);
            }
        }
    }

    public void BindVertices(int[]? offsets)
    {
        Bind();
        var binds = new List<AttribBind>(AttrMax);
        for (int i = 0; i < AttrMax; i++)
        {
            AttrState attr = Attrs[i];
            if (attr.Size == 0)
            {
                binds.Add(new AttribBind(i, false, 0, 0, false, false, Stride, 0));
            }
            else
            {
                long ofs = attr.Offset;
                if (offsets != null)
                    ofs = attr.Offset + (long)Stride * offsets[attr.Binding];
                binds.Add(new AttribBind(i, true, attr.Size, attr.Format, attr.Normalize, attr.IntegerType, Stride, ofs));
            }
        }

        AttribBinds = binds;
    }

    public void BindRange(int start, int length)
    {
        Bind();
        RangeStart = start;
        RangeLength = length;
        RangeBound = true;
    }

    public void BindBase()
    {
        Bind();
        BaseBound = true;
    }

    public void Destroy()
    {
        if (BufferId != 0 && !Destroyed)
        {
            BoundId = BufferId;
            GlUnmap();
            BoundId = 0;
            DeletedBuffers.Add(BufferId);
            DestroyUnmaps++;
            Destroyed = true;
            BufferId = 0;
        }
    }

    void Upload(int size, byte[]? data, int drawUsage)
    {
        LastDrawUsage = drawUsage;
        UsedBufferStorage = false;
        LastStorageFlags = 0;
        Bytes = new byte[size];
        if (data != null)
        {
            int count = size;
            if (count > data.Length)
                count = data.Length;
            Array.Copy(data, 0, Bytes, 0, count);
        }
    }

    void GlUnmap()
    {
        GlUnmapCalls++;
    }

    void Invalidate()
    {
        ResetCount++;
    }
}
