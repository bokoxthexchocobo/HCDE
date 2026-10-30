using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Reserved flat vertices, pipeline upload, and the vertex-cursor cap from <c>flatvertices.cpp</c> (<b>1–170</b>).
/// Chunk <b>1258</b>. Creating the GPU buffers is a host step. A copy reads the shadow from its first vertex
/// and writes that run at the requested index. The five-argument vertex write leaves the lightmap coordinates
/// unchanged and stores a lightmap index of -1.
/// </summary>
public sealed class VmFlatVertices1258
{
    public const uint BufferSize = 2000000;
    public const uint BufferSizeToUse = BufferSize - 500;
    public const int Reserved = 20;
    public const int VertexBytes = 32;

    public readonly List<string> Log = new();
    public readonly List<Vert> Shadow = new();
    public Gpu?[] Pipes = [];
    public Gpu? Current;
    public bool IndexAlive;
    public int IndexBytes;
    public int IndexUsage;
    public int PipelineCount;
    public uint Index;
    public uint CurIndex;
    public uint NumReserved;
    public uint MapStart;
    public bool Fatal;

    public VmFlatVertices1258(int width, int height, int pipelineNbr)
    {
        PipelineCount = pipelineNbr;
        Pipes = new Gpu?[pipelineNbr];
        for (int i = 0; i < Reserved; i++)
            Shadow.Add(new Vert());
        Put(0, 0, 0, 0, 0, 0);
        Put(1, 1, 0, 0, 0, 0);
        Put(2, 2, 0, 0, 0, 0);
        Put(3, 3, 0, 0, 0, 0);
        Put(4, 0, 0, 0, 0, 0);
        Put(5, 0, height, 0, 0, 1);
        Put(6, width, 0, 0, 1, 0);
        Put(7, width, height, 0, 1, 1);
        Put(8, -1, -1, 0, 0, 0);
        Put(9, 3, -1, 0, 2, 0);
        Put(10, -1, 3, 0, 0, 2);
        Put(11, 3, 3, 0, 2, 2);
        Put(12, -32767, 32767, -32767, 0, 0);
        Put(13, -32767, 32767, 32767, 0, 0);
        Put(14, 32767, 32767, 32767, 0, 0);
        Put(15, 32767, 32767, -32767, 0, 0);
        Put(16, -32767, -32767, -32767, 0, 0);
        Put(17, -32767, -32767, 32767, 0, 0);
        Put(18, 32767, -32767, 32767, 0, 0);
        Put(19, 32767, -32767, -32767, 0, 0);
        for (int i = 0; i < Reserved; i++)
            Log.Add($"v {i} {Shadow[i].Text()}");

        IndexAlive = true;
        IndexBytes = 4;
        IndexUsage = 0;
        Log.Add($"index {IndexBytes} {IndexUsage}");
        for (int n = 0; n < pipelineNbr; n++)
        {
            Pipes[n] = new Gpu();
            Log.Add($"vbo {n} {(long)BufferSize * VertexBytes} 2");
            Log.Add($"fmt {n} 1 3 {VertexBytes} 0 0 1 12 1 2 20 6 1");
        }

        Current = pipelineNbr > 0 ? Pipes[0] : null;
        Index = CurIndex = (uint)Reserved;
        NumReserved = (uint)Reserved;
        Log.Add($"cursor {CurIndex} {NumReserved}");
        Copy(0, Reserved);
    }

    public void OutputResized(int width, int height)
    {
        Put(4, 0, 0, 0, 0, 0);
        Put(5, 0, height, 0, 0, 1);
        Put(6, width, 0, 0, 1, 0);
        Put(7, width, height, 0, 1, 1);
        Copy(4, 4);
    }

    public (Vert? Slot, uint Index) Alloc(uint count)
    {
        uint index = CurIndex;
        CurIndex += count;
        if (index + count >= BufferSizeToUse)
        {
            Fatal = true;
            Log.Add($"fatal {index + count}");
            return (null, index);
        }
        Log.Add($"alloc {index} {count}");
        Vert? slot = Current?.SlotOrNull(index);
        return (slot, index);
    }

    public void Copy(int start, int count)
    {
        Gpu? old = Current;
        for (int n = 0; n < PipelineCount; n++)
        {
            Current = Pipes[n];
            Log.Add($"copy {n} {start} {count}");
            Map();
            if (Current != null)
            {
                for (int i = 0; i < count; i++)
                    Current.Put(start + i, Shadow[i].Clone());
            }
            Unmap();
            Log.Add($"upload {start * VertexBytes} {count * VertexBytes}");
        }
        Current = old;
        Log.Add($"bind {Which(Current)}");
    }

    public void Destroy()
    {
        for (int n = 0; n < PipelineCount; n++)
        {
            if (Pipes[n] != null)
            {
                Pipes[n] = null;
                Log.Add($"drop {n}");
            }
        }
        if (IndexAlive)
        {
            IndexAlive = false;
            Log.Add("drop index");
        }
        Current = null;
    }

    void Map()
    {
        MapStart = CurIndex;
        Log.Add($"map {MapStart}");
    }

    void Unmap()
    {
        long start = (long)MapStart * VertexBytes;
        long size = (long)(CurIndex - MapStart) * VertexBytes;
        Log.Add($"unmap {start} {size}");
    }

    void Put(int index, float x, float z, float y, float u, float v)
    {
        Shadow[index].Set(x, z, y, u, v);
    }

    int Which(Gpu? gpu)
    {
        for (int n = 0; n < PipelineCount; n++)
        {
            if (ReferenceEquals(Pipes[n], gpu))
                return n;
        }
        return -1;
    }

    public sealed class Vert
    {
        public float X, Z, Y, U, V, Lu, Lv, LIndex;

        public void Set(float x, float z, float y, float u, float v)
        {
            X = x;
            Z = z;
            Y = y;
            U = u;
            V = v;
            LIndex = -1f;
        }

        public Vert Clone()
        {
            return new Vert { X = X, Z = Z, Y = Y, U = U, V = V, Lu = Lu, Lv = Lv, LIndex = LIndex };
        }

        public string Text()
        {
            return $"{Fmt(X)} {Fmt(Z)} {Fmt(Y)} {Fmt(U)} {Fmt(V)} {Fmt(Lu)} {Fmt(Lv)} {Fmt(LIndex)}";
        }

        static string Fmt(float value) => value.ToString("F6", CultureInfo.InvariantCulture);
    }

    public sealed class Gpu
    {
        readonly Dictionary<uint, Vert> _slot = new();

        public void Put(int index, Vert value) => _slot[(uint)index] = value;

        public Vert? SlotOrNull(uint index)
        {
            return _slot.TryGetValue(index, out Vert? value) ? value : null;
        }

        public Vert Get(uint index)
        {
            Vert? value = SlotOrNull(index);
            if (value == null)
                throw new InvalidOperationException($"missing vertex {index}");
            return value;
        }
    }
}
