namespace HCDE.Playsim;

/// <summary>
/// Null mesh, level-mesh swap, and the bottom and top acceleration structures
/// from <c>vk_raytrace.cpp</c> (1-293). Chunk 1300. Build sizes and device
/// addresses are host services. A mesh change idles only when a mesh is already
/// current. The upload wait uses finish false.
/// </summary>
public sealed class VmVkRaytrace1300
{
    public const int VertexStride = 12;
    public const int IndexStride = 4;
    public const int InstanceBytes = 64;
    public const int UsageTransferSrc = 0x1;
    public const int UsageTransferDst = 0x2;
    public const int UsageStorage = 0x20;
    public const int UsageIndex = 0x40;
    public const int UsageVertex = 0x80;
    public const int UsageShaderAddress = 0x20000;
    public const int UsageAccelRead = 0x80000;
    public const int UsageAccelStorage = 0x100000;
    public const int MemoryCpuOnly = 2;
    public const int AccessShaderRead = 0x20;
    public const int AccessTransferWrite = 0x1000;
    public const int AccessAccelRead = 0x200000;
    public const int AccessAccelWrite = 0x400000;
    public const int StageFragment = 0x80;
    public const int StageTransfer = 0x1000;
    public const int StageAccelBuild = 0x2000000;
    public const int GeometryTriangles = 0;
    public const int GeometryInstances = 2;
    public const int GeometryOpaque = 1;
    public const int InstanceCullDisable = 1;
    public const int FormatRgb32Float = 106;
    public const int IndexUint32 = 1;
    public const int AccelTop = 0;
    public const int AccelBottom = 1;
    public const int BuildFastTrace = 1;
    public const int BuildModeBuild = 0;

    public sealed class Host
    {
        public bool Raytracing;
        public int Idles;
        public int Waits;
        public bool LastWaitFinish = true;
        public bool LastWaitUpload;
        public int BlasAccelSize(int primitiveCount) { return 1000 + primitiveCount; }
        public int BlasScratchSize(int primitiveCount) { return 50 + primitiveCount; }
        public int TlasAccelSize(int instanceCount) { return 2000 + instanceCount; }
        public int TlasScratchSize(int instanceCount) { return 80 + instanceCount; }
    }

    public readonly record struct Vertex(float X, float Y, float Z);
    public readonly record struct Copy(string Src, string Dst, int SrcOffset, int DstOffset, int Size);
    public readonly record struct Barrier(int SrcAccess, int DstAccess, int SrcStage, int DstStage);

    public sealed class Mesh
    {
        public List<Vertex> Vertices = new();
        public List<int> Elements = new();
    }

    public sealed class Buffer
    {
        public int Id;
        public string Name = "";
        public int Size;
        public int Usage;
        public int Memory = -1;
        public int Address;
        public bool Alive = true;
        public bool Queued;
    }

    public sealed class Accel
    {
        public int Id;
        public string Name = "";
        public int Type;
        public int Size;
        public string BufferName = "";
        public int Address;
        public bool Alive = true;
        public bool Queued;
    }

    public sealed class Build
    {
        public string Name = "";
        public int Type;
        public int Flags;
        public int Mode;
        public int Geometry;
        public int GeometryFlags;
        public int PrimitiveCount;
        public int AccelSize;
        public int ScratchSize;
        public int MaxVertex = -1;
        public int VertexStride;
        public int IndexType = -1;
        public int VertexFormat = -1;
        public int VertexAddress;
        public int IndexAddress;
        public int InstanceAddress;
    }

    public sealed class Instance
    {
        public float[] Matrix = new float[12];
        public int Mask;
        public int Flags;
        public int BlasAddress;
    }

    public Host Services;
    public Mesh NullMesh = new();
    public Mesh? Current;
    public Buffer? VertexBuffer;
    public Buffer? IndexBuffer;
    public Buffer? TransferBuffer;
    public Buffer? BlScratch;
    public Buffer? BlStorage;
    public Accel? Blas;
    public Buffer? TlTransfer;
    public Buffer? TlScratch;
    public Buffer? TlInstance;
    public Buffer? TlStorage;
    public Accel? Tlas;
    public Instance? TopInstance;
    public int NextId = 1;
    public int NextAddress = 0x1000;
    public List<object> DeleteList = new();
    public List<string> Trace = new();
    public List<Copy> Copies = new();
    public List<Barrier> Barriers = new();
    public List<Build> Builds = new();
    public List<(string Kind, int Count)> Queries = new();

    public VmVkRaytrace1300(Host host)
    {
        Services = host;
        float[] seed = { -1, -1, -1, 1, -1, -1, 1, 1, -1, -1, -1, -1, -1, 1, -1, 1, 1, -1, -1, -1, 1, 1, -1, 1, 1, 1, 1, -1, -1, 1, -1, 1, 1, 1, 1, 1 };
        for (int i = 0; i < seed.Length; i += 3)
            NullMesh.Vertices.Add(new Vertex(seed[i], seed[i + 1], seed[i + 2]));
        for (int i = 0; i < 3 * 4; i++)
            NullMesh.Elements.Add(i);
        SetLevelMesh(null);
    }

    public void SetLevelMesh(Mesh? mesh)
    {
        mesh ??= NullMesh;
        if (ReferenceEquals(mesh, Current))
            return;
        if (Current != null)
        {
            Services.Idles++;
            Trace.Add("idle");
        }
        Reset();
        Current = mesh;
        if (Services.Raytracing)
        {
            CreateVulkanObjects();
            WaitForCommands(false, true);
        }
    }

    public void WaitForCommands(bool finish, bool uploadOnly)
    {
        Services.Waits++;
        Services.LastWaitFinish = finish;
        Services.LastWaitUpload = uploadOnly;
        Trace.Add(uploadOnly ? "wait:upload" : "wait:draw");
        foreach (object item in DeleteList)
        {
            if (item is Buffer buffer && buffer.Alive)
                buffer.Alive = false;
            else if (item is Accel accel && accel.Alive)
                accel.Alive = false;
        }
        DeleteList.Clear();
    }

    void Reset()
    {
        Retire(VertexBuffer); VertexBuffer = null;
        Retire(IndexBuffer); IndexBuffer = null;
        Retire(TransferBuffer); TransferBuffer = null;
        Retire(BlScratch); BlScratch = null;
        Retire(BlStorage); BlStorage = null;
        Retire(Blas); Blas = null;
        Retire(TlTransfer); TlTransfer = null;
        Retire(TlScratch); TlScratch = null;
        Retire(TlInstance); TlInstance = null;
        Retire(TlStorage); TlStorage = null;
        Retire(Tlas); Tlas = null;
    }

    void CreateVulkanObjects()
    {
        CreateVertexAndIndexBuffers();
        CreateBottomLevelAccelerationStructure();
        CreateTopLevelAccelerationStructure();
    }

    void CreateVertexAndIndexBuffers()
    {
        Mesh mesh = Current!;
        int vertexBytes = mesh.Vertices.Count * VertexStride;
        int indexBytes = mesh.Elements.Count * IndexStride;
        int vertexOffset = 0;
        int indexOffset = vertexOffset + vertexBytes;
        TransferBuffer = MakeBuffer("transferBuffer", vertexBytes + indexBytes, UsageTransferSrc, MemoryCpuOnly);
        VertexBuffer = MakeBuffer("vertexBuffer", vertexBytes, UsageVertex | UsageTransferDst | UsageShaderAddress | UsageAccelRead, -1);
        IndexBuffer = MakeBuffer("indexBuffer", indexBytes, UsageIndex | UsageTransferDst | UsageShaderAddress | UsageAccelRead, -1);
        Copies.Add(new Copy(TransferBuffer.Name, VertexBuffer.Name, vertexOffset, 0, VertexBuffer.Size));
        Copies.Add(new Copy(TransferBuffer.Name, IndexBuffer.Name, indexOffset, 0, IndexBuffer.Size));
        Trace.Add("copy:vertex");
        Trace.Add("copy:index");
        AddBarrier(AccessTransferWrite, AccessShaderRead, StageTransfer, StageAccelBuild);
    }

    void CreateBottomLevelAccelerationStructure()
    {
        Mesh mesh = Current!;
        int primitives = mesh.Elements.Count / 3;
        Queries.Add(("blas", primitives));
        int accelSize = Services.BlasAccelSize(primitives);
        int scratchSize = Services.BlasScratchSize(primitives);
        BlStorage = MakeBuffer("blAccelStructBuffer", accelSize, UsageAccelStorage | UsageShaderAddress, -1);
        Blas = MakeAccel("blAccelStruct", AccelBottom, accelSize, BlStorage.Name);
        BlScratch = MakeBuffer("blScratchBuffer", scratchSize, UsageShaderAddress | UsageStorage, -1);
        var build = new Build
        {
            Name = "blas",
            Type = AccelBottom,
            Flags = BuildFastTrace,
            Mode = BuildModeBuild,
            Geometry = GeometryTriangles,
            GeometryFlags = GeometryOpaque,
            PrimitiveCount = primitives,
            AccelSize = accelSize,
            ScratchSize = scratchSize,
            MaxVertex = mesh.Vertices.Count - 1,
            VertexStride = VertexStride,
            IndexType = IndexUint32,
            VertexFormat = FormatRgb32Float,
            VertexAddress = VertexBuffer!.Address,
            IndexAddress = IndexBuffer!.Address,
        };
        Builds.Add(build);
        Trace.Add("blas-build:" + primitives.ToString());
        AddBarrier(AccessAccelWrite, AccessAccelRead, StageAccelBuild, StageAccelBuild);
    }

    void CreateTopLevelAccelerationStructure()
    {
        var instance = new Instance();
        instance.Matrix[0] = 1;
        instance.Matrix[5] = 1;
        instance.Matrix[10] = 1;
        instance.Mask = 0xff;
        instance.Flags = InstanceCullDisable;
        instance.BlasAddress = Blas!.Address;
        TopInstance = instance;
        TlTransfer = MakeBuffer("tlTransferBuffer", InstanceBytes, UsageTransferSrc, MemoryCpuOnly);
        TlInstance = MakeBuffer("tlInstanceBuffer", InstanceBytes, UsageShaderAddress | UsageAccelRead | UsageTransferDst, -1);
        Copies.Add(new Copy(TlTransfer.Name, TlInstance.Name, 0, 0, TlInstance.Size));
        Trace.Add("copy:instance");
        AddBarrier(AccessTransferWrite, AccessShaderRead, StageTransfer, StageAccelBuild);
        int instances = 1;
        Queries.Add(("tlas", instances));
        int accelSize = Services.TlasAccelSize(instances);
        int scratchSize = Services.TlasScratchSize(instances);
        TlStorage = MakeBuffer("tlAccelStructBuffer", accelSize, UsageAccelStorage | UsageShaderAddress, -1);
        Tlas = MakeAccel("tlAccelStruct", AccelTop, accelSize, TlStorage.Name);
        TlScratch = MakeBuffer("tlScratchBuffer", scratchSize, UsageShaderAddress | UsageStorage, -1);
        Builds.Add(new Build
        {
            Name = "tlas",
            Type = AccelTop,
            Flags = BuildFastTrace,
            Mode = BuildModeBuild,
            Geometry = GeometryInstances,
            PrimitiveCount = 1,
            AccelSize = accelSize,
            ScratchSize = scratchSize,
            InstanceAddress = TlInstance.Address,
        });
        Trace.Add("tlas-build:1");
        AddBarrier(AccessAccelWrite, AccessShaderRead, StageAccelBuild, StageFragment);
    }

    Buffer MakeBuffer(string name, int size, int usage, int memory)
    {
        var buffer = new Buffer
        {
            Id = NextId++,
            Name = name,
            Size = size,
            Usage = usage,
            Memory = memory,
            Address = NextAddress,
        };
        NextAddress += 0x100;
        Trace.Add(name);
        return buffer;
    }

    Accel MakeAccel(string name, int type, int size, string bufferName)
    {
        var accel = new Accel
        {
            Id = NextId++,
            Name = name,
            Type = type,
            Size = size,
            BufferName = bufferName,
            Address = NextAddress,
        };
        NextAddress += 0x100;
        Trace.Add(name);
        return accel;
    }

    void Retire(Buffer? buffer)
    {
        if (buffer == null || !buffer.Alive || buffer.Queued)
            return;
        buffer.Queued = true;
        DeleteList.Add(buffer);
        Trace.Add("retire:" + buffer.Name);
    }

    void Retire(Accel? accel)
    {
        if (accel == null || !accel.Alive || accel.Queued)
            return;
        accel.Queued = true;
        DeleteList.Add(accel);
        Trace.Add("retire:" + accel.Name);
    }

    void AddBarrier(int srcAccess, int dstAccess, int srcStage, int dstStage)
    {
        Barriers.Add(new Barrier(srcAccess, dstAccess, srcStage, dstStage));
        Trace.Add("barrier");
    }
}
