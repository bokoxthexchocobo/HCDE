namespace HCDE.Playsim;

/// <summary>
/// Voxel palette texture, slab faces, and the frame draw from
/// <c>models_voxel.cpp</c> (1-398). Chunk 1324.
/// </summary>
public sealed class VmVoxelModel1324
{
    public const int Normal = 0;
    public const int Luminance = 1;
    public const int TextureOverride = 10;
    public const int HitFlat = 2;
    public const int ErrVoxel = -2;

    public PalEntry[]? MatchPalette { get; set; }
    public byte[]? IndexMap { get; set; }
    public PalEntry[] BaseColors { get; } = new PalEntry[256];
    public int NextTextureId { get; set; } = 1;
    public int LastTextureType { get; private set; } = -1;
    public bool LastTextureNameWasNull { get; private set; }

    public readonly record struct PalEntry(byte R, byte G, byte B, byte A);

    public readonly record struct VertexKey(float X, float Y, float Z, float U, float V);

    public struct Vertex
    {
        public float X;
        public float Y;
        public float Z;
        public float U;
        public float V;
        public uint PackedNormal;
    }

    public static byte Expand6(byte c) => (byte)((c << 2) | (c >> 4));

    public byte Pick(byte r, byte g, byte b)
    {
        if (MatchPalette == null)
        {
            return 1;
        }

        int start = IndexMap == null ? 1 : 0;
        return (byte)BestColor(MatchPalette, r, g, b, start, 255, IndexMap);
    }

    public static int BestColor(PalEntry[] pal, int r, int g, int b, int first, int num, byte[]? indexMap)
    {
        int bestColor = first;
        int bestDist = 257 * 257 + 257 * 257 + 257 * 257;
        for (int color = first; color < num; color++)
        {
            int co = indexMap != null ? indexMap[color] : color;
            int x = r - pal[co].R;
            int y = g - pal[co].G;
            int z = b - pal[co].B;
            int dist = x * x + y * y + z * z;
            if (dist < bestDist)
            {
                if (dist == 0)
                {
                    return co;
                }

                bestDist = dist;
                bestColor = co;
            }
        }

        return bestColor;
    }

    public int AddGameTexture(VoxelTexture image, string? name, int type)
    {
        _ = image;
        LastTextureNameWasNull = name == null;
        LastTextureType = type;
        int id = NextTextureId;
        NextTextureId++;
        return id;
    }

    public static void FlipSquare(byte[] block, int x)
    {
        for (int i = 0; i < x; i++)
        {
            int corner = x * i + i;
            int count = x - i;
            for (int j = 0; j < count; j++)
            {
                int a = corner + j;
                int b = corner + j * x;
                (block[a], block[b]) = (block[b], block[a]);
            }
        }
    }

    public sealed class Voxel
    {
        public byte[]? Palette { get; set; }
        public bool Deleted { get; set; }
        public Mip[] Mips { get; } = { new Mip() };
    }

    public sealed class Mip
    {
        public int SizeX { get; set; }
        public int SizeY { get; set; }
        public double PivotX { get; set; }
        public double PivotY { get; set; }
        public double PivotZ { get; set; }
        public int[] OffsetX { get; set; } = Array.Empty<int>();
        public short[] OffsetXY { get; set; } = Array.Empty<short>();
        public byte[] SlabData { get; set; } = Array.Empty<byte>();
    }

    public sealed class Bitmap
    {
        public Bitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Pitch = width * 4;
            Data = new byte[Pitch * height];
        }

        public int Width { get; }
        public int Height { get; }
        public int Pitch { get; }
        public byte[] Data { get; }

        public void CopyPaletted(int originX, int originY, byte[] patch, int srcWidth, int srcHeight, int stepX, int stepY, PalEntry[] palette)
        {
            for (int y = 0; y < srcHeight; y++)
            {
                int pos = (originY + y) * Pitch + originX * 4;
                for (int x = 0; x < srcWidth; x++, pos += 4)
                {
                    int v = patch[y * stepY + x * stepX];
                    byte a = palette[v].A;
                    if (a != 0)
                    {
                        Data[pos] = palette[v].B;
                        Data[pos + 1] = palette[v].G;
                        Data[pos + 2] = palette[v].R;
                        Data[pos + 3] = a;
                    }
                }
            }
        }
    }

    public sealed class VoxelTexture
    {
        public VoxelTexture(Voxel voxel, VmVoxelModel1324 host)
        {
            Source = voxel;
            Host = host;
            Width = 16;
            Height = 16;
        }

        public Voxel Source { get; }
        public VmVoxelModel1324 Host { get; }
        public int Width { get; }
        public int Height { get; }

        public byte[] CreatePalettedPixels(int conversion, int frame)
        {
            _ = frame;
            var pixels = new byte[256];
            byte[]? pp = Source.Palette;
            if (pp != null)
            {
                int o = 0;
                for (int i = 0; i < 256; i++, o += 3)
                {
                    byte r = Expand6(pp[o]);
                    byte g = Expand6(pp[o + 1]);
                    byte b = Expand6(pp[o + 2]);
                    pixels[i] = conversion == Luminance ? r : Host.Pick(r, g, b);
                }
            }
            else
            {
                for (int i = 0; i < 256; i++)
                {
                    pixels[i] = (byte)i;
                }
            }

            FlipSquare(pixels, Width);
            return pixels;
        }

        public int CopyPixels(Bitmap bmp, int conversion, int frame)
        {
            _ = conversion;
            _ = frame;
            var pe = new PalEntry[256];
            var bitmap = new byte[256];
            byte[]? pp = Source.Palette;
            if (pp != null)
            {
                int o = 0;
                for (int i = 0; i < 256; i++, o += 3)
                {
                    bitmap[i] = (byte)i;
                    pe[i] = new PalEntry(Expand6(pp[o]), Expand6(pp[o + 1]), Expand6(pp[o + 2]), 255);
                }
            }
            else
            {
                for (int i = 0; i < 256; i++)
                {
                    bitmap[i] = (byte)i;
                    PalEntry src = Host.BaseColors[i];
                    pe[i] = new PalEntry(src.R, src.G, src.B, 255);
                }
            }

            bmp.CopyPaletted(0, 0, bitmap, Width, Height, 1, 16, pe);
            return 0;
        }
    }

    public sealed class VertexBuffer
    {
        public Vertex[] Vertices { get; private set; } = Array.Empty<Vertex>();
        public int[] Indices { get; private set; } = Array.Empty<int>();
        public bool VerticesUnlocked { get; private set; }
        public bool IndicesUnlocked { get; private set; }

        public Vertex[] LockVertexBuffer(int size)
        {
            Vertices = new Vertex[size];
            return Vertices;
        }

        public int[] LockIndexBuffer(int size)
        {
            Indices = new int[size];
            return Indices;
        }

        public void UnlockVertexBuffer()
        {
            VerticesUnlocked = true;
        }

        public void UnlockIndexBuffer()
        {
            IndicesUnlocked = true;
        }
    }

    public sealed class Renderer
    {
        public Renderer(int type)
        {
            Type = type;
        }

        public int Type { get; }
        public int CreateCount { get; private set; }
        public bool CreatedWithVertices { get; private set; }
        public bool CreatedWithIndices { get; private set; }
        public VoxelTexture? MaterialSkin { get; private set; }
        public bool MaterialClamp { get; private set; }
        public int MaterialTranslation { get; private set; }
        public Model? FrameModel { get; private set; }
        public int FrameA { get; private set; }
        public int FrameB { get; private set; }
        public int FrameC { get; private set; }
        public int FrameD { get; private set; }
        public int DrawCount { get; private set; }
        public int DrawStart { get; private set; }

        public VertexBuffer CreateVertexBuffer(bool vertices, bool indices)
        {
            CreateCount++;
            CreatedWithVertices = vertices;
            CreatedWithIndices = indices;
            return new VertexBuffer();
        }

        public void SetMaterial(VoxelTexture skin, bool clamp, int translation)
        {
            MaterialSkin = skin;
            MaterialClamp = clamp;
            MaterialTranslation = translation;
        }

        public void SetupFrame(Model model, int a, int b, int c, int d)
        {
            FrameModel = model;
            FrameA = a;
            FrameB = b;
            FrameC = c;
            FrameD = d;
        }

        public void DrawElements(int count, int start)
        {
            DrawCount = count;
            DrawStart = start;
        }
    }

    public sealed class Model
    {
        readonly Dictionary<int, VertexBuffer> _buffers = new();

        public Model(Voxel voxel, bool owned, VmVoxelModel1324 host)
        {
            Voxel = voxel;
            Owning = owned;
            Host = host;
            PaletteImage = new VoxelTexture(voxel, host);
            PaletteId = host.AddGameTexture(PaletteImage, null, TextureOverride);
        }

        public Voxel Voxel { get; }
        public bool Owning { get; }
        public VmVoxelModel1324 Host { get; }
        public VoxelTexture PaletteImage { get; }
        public int PaletteId { get; }
        public int NumIndices { get; private set; }
        public List<Vertex> Vertices { get; } = new();
        public List<int> Indices { get; } = new();

        public void Destroy()
        {
            if (Owning)
            {
                Voxel.Deleted = true;
            }
        }

        public VertexBuffer? GetVertexBuffer(int type)
        {
            return _buffers.TryGetValue(type, out VertexBuffer? buf) ? buf : null;
        }

        public void Initialize()
        {
            var check = new Dictionary<VertexKey, int>();
            Mip mip = Voxel.Mips[0];
            byte[] slab = mip.SlabData;
            for (int x = 0; x < mip.SizeX; x++)
            {
                int slabBase = mip.OffsetX[x];
                int xyBase = x * (mip.SizeY + 1);
                for (int y = 0; y < mip.SizeY; y++)
                {
                    int start = slabBase + mip.OffsetXY[xyBase + y];
                    int end = slabBase + mip.OffsetXY[xyBase + y + 1];
                    int p = start;
                    while (p < end)
                    {
                        byte ztop = slab[p];
                        byte zleng = slab[p + 1];
                        byte cull = slab[p + 2];
                        MakeSlabPolys(x, y, ztop, zleng, cull, slab, p + 3, check);
                        p += zleng + 3;
                    }
                }
            }
        }

        public void BuildVertexBuffer(Renderer renderer)
        {
            if (GetVertexBuffer(renderer.Type) != null)
            {
                return;
            }

            Initialize();
            VertexBuffer vbuf = renderer.CreateVertexBuffer(true, true);
            _buffers[renderer.Type] = vbuf;
            Vertex[] verts = vbuf.LockVertexBuffer(Vertices.Count);
            int[] inds = vbuf.LockIndexBuffer(Indices.Count);
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = Vertices[i];
            }

            for (int i = 0; i < inds.Length; i++)
            {
                inds[i] = Indices[i];
            }

            vbuf.UnlockVertexBuffer();
            vbuf.UnlockIndexBuffer();
            NumIndices = Indices.Count;
            Vertices.Clear();
            Indices.Clear();
            Vertices.TrimExcess();
            Indices.TrimExcess();
        }

        public void AddSkins(byte[] hitlist)
        {
            hitlist[PaletteId] |= HitFlat;
        }

        public bool Load(string? fn, int lump, string? buffer, int length)
        {
            _ = fn;
            _ = lump;
            _ = buffer;
            _ = length;
            return false;
        }

        public int FindFrame(string? name, bool noDefault)
        {
            _ = name;
            if (noDefault)
            {
                return ErrVoxel;
            }

            return 0;
        }

        public float GetAspectFactor(float stretch)
        {
            return stretch;
        }

        public void RenderFrame(Renderer renderer, VoxelTexture skin, int frame, int frame2, double inter, int translation, int boneStart)
        {
            _ = frame;
            _ = frame2;
            _ = inter;
            _ = boneStart;
            renderer.SetMaterial(skin, true, translation);
            renderer.SetupFrame(this, 0, 0, 0, -1);
            renderer.DrawElements(NumIndices, 0);
        }

        int AddVertex(Vertex vert, Dictionary<VertexKey, int> check)
        {
            var key = new VertexKey(vert.X, vert.Y, vert.Z, vert.U, vert.V);
            if (!check.TryGetValue(key, out int index))
            {
                index = Vertices.Count;
                Vertices.Add(vert);
                check[key] = index;
            }

            return index;
        }

        void AddFace(int x1, int y1, int z1, int x2, int y2, int z2, int x3, int y3, int z3, int x4, int y4, int z4, byte col, Dictionary<VertexKey, int> check)
        {
            Mip mip = Voxel.Mips[0];
            float pivotX = (float)mip.PivotX;
            float pivotY = (float)mip.PivotY;
            float pivotZ = (float)mip.PivotZ;
            var vert = new Vertex
            {
                PackedNormal = 0,
                U = ((col & 15) + 0.5f) / 16f,
                V = ((col / 16) + 0.5f) / 16f,
            };
            var indx = new int[4];
            vert.X = x1 - pivotX;
            vert.Z = -y1 + pivotY;
            vert.Y = -z1 + pivotZ;
            indx[0] = AddVertex(vert, check);
            vert.X = x2 - pivotX;
            vert.Z = -y2 + pivotY;
            vert.Y = -z2 + pivotZ;
            indx[1] = AddVertex(vert, check);
            vert.X = x4 - pivotX;
            vert.Z = -y4 + pivotY;
            vert.Y = -z4 + pivotZ;
            indx[2] = AddVertex(vert, check);
            vert.X = x3 - pivotX;
            vert.Z = -y3 + pivotY;
            vert.Y = -z3 + pivotZ;
            indx[3] = AddVertex(vert, check);
            Indices.Add(indx[0]);
            Indices.Add(indx[1]);
            Indices.Add(indx[3]);
            Indices.Add(indx[1]);
            Indices.Add(indx[2]);
            Indices.Add(indx[3]);
        }

        void MakeSlabPolys(int x, int y, int ztop, int zleng, byte cull, byte[] slab, int colAt, Dictionary<VertexKey, int> check)
        {
            if ((cull & 16) != 0)
            {
                AddFace(x, y, ztop, x + 1, y, ztop, x, y + 1, ztop, x + 1, y + 1, ztop, slab[colAt], check);
            }

            int z = ztop;
            int colorAt = colAt;
            while (z < ztop + zleng)
            {
                int c = 0;
                while (z + c < ztop + zleng && slab[colorAt + c] == slab[colorAt])
                {
                    c++;
                }

                byte color = slab[colorAt];
                if ((cull & 1) != 0)
                {
                    AddFace(x, y, z, x, y + 1, z, x, y, z + c, x, y + 1, z + c, color, check);
                }

                if ((cull & 2) != 0)
                {
                    AddFace(x + 1, y + 1, z, x + 1, y, z, x + 1, y + 1, z + c, x + 1, y, z + c, color, check);
                }

                if ((cull & 4) != 0)
                {
                    AddFace(x + 1, y, z, x, y, z, x + 1, y, z + c, x, y, z + c, color, check);
                }

                if ((cull & 8) != 0)
                {
                    AddFace(x, y + 1, z, x + 1, y + 1, z, x, y + 1, z + c, x + 1, y + 1, z + c, color, check);
                }

                z += c;
                colorAt += c;
            }

            if ((cull & 32) != 0)
            {
                int zz = ztop + zleng - 1;
                AddFace(x + 1, y, zz + 1, x, y, zz + 1, x + 1, y + 1, zz + 1, x, y + 1, zz + 1, slab[colAt + zleng - 1], check);
            }
        }
    }
}
