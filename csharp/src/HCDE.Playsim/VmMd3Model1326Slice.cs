using System.Buffers.Binary;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// MD3 load, surface geometry, and the interpolated frame from
/// <c>models_md3.cpp</c> (1-377). Chunk 1326.
/// </summary>
public sealed class VmMd3Model1326
{
    public const int ErrNotFound = -1;
    public const int HitFlat = 2;
    public const int HeaderBytes = 108;
    public const int FrameBytes = 56;
    public const int SurfaceBytes = 108;
    public const int ShaderBytes = 68;

    public bool HasSurfaces { get; private set; }
    public int NumTags { get; private set; }
    public int LumpNum { get; private set; }
    public int GeometryReads { get; private set; }
    public List<Frame> Frames { get; } = new();
    public List<Surface> Surfaces { get; } = new();
    public Dictionary<int, byte[]> Lumps { get; } = new();
    public List<(string Path, string Name)> SkinCalls { get; } = new();
    public Func<string, string, TextureId>? ResolveSkin { get; set; }

    readonly Dictionary<int, VertexBuffer> _buffers = new();

    public readonly record struct TextureId(int Index, bool Valid)
    {
        public bool IsValid()
        {
            return Valid;
        }

        public int GetIndex()
        {
            return Index;
        }
    }

    public sealed class Frame
    {
        public string Name { get; set; } = "";
        public float[] Origin { get; } = new float[3];
    }

    public sealed class Triangle
    {
        public int[] VertIndex { get; } = new int[3];
    }

    public struct TexCoord
    {
        public float S;
        public float T;
    }

    public struct Md3Vertex
    {
        public float X;
        public float Y;
        public float Z;
        public float Nx;
        public float Ny;
        public float Nz;
    }

    public sealed class Surface
    {
        public int NumVertices;
        public int NumTriangles;
        public int NumSkins;
        public List<TextureId> Skins { get; } = new();
        public List<Triangle> Tris { get; } = new();
        public List<TexCoord> Texcoords { get; } = new();
        public List<Md3Vertex> Vertices { get; } = new();
        public uint VIndex = uint.MaxValue;
        public uint IIndex = uint.MaxValue;

        public void UnloadGeometry()
        {
            Tris.Clear();
            Vertices.Clear();
            Texcoords.Clear();
        }
    }

    public struct ModelVertex
    {
        public float X;
        public float Y;
        public float Z;
        public float S;
        public float T;
        public float Nx;
        public float Ny;
        public float Nz;

        public void Set(float x, float y, float z, float s, float t)
        {
            X = x;
            Y = y;
            Z = z;
            S = s;
            T = t;
        }

        public void SetNormal(float nx, float ny, float nz)
        {
            Nx = nx;
            Ny = ny;
            Nz = nz;
        }
    }

    public sealed class VertexBuffer
    {
        public bool Indexed { get; }
        public bool OneFrame { get; }
        public ModelVertex[] Vertices { get; private set; } = Array.Empty<ModelVertex>();
        public uint[] Indices { get; private set; } = Array.Empty<uint>();
        public bool VerticesUnlocked { get; private set; }
        public bool IndicesUnlocked { get; private set; }

        public VertexBuffer(bool indexed, bool oneFrame)
        {
            Indexed = indexed;
            OneFrame = oneFrame;
        }

        public ModelVertex[] LockVertexBuffer(int count)
        {
            Vertices = new ModelVertex[count];
            return Vertices;
        }

        public uint[] LockIndexBuffer(int count)
        {
            Indices = new uint[count];
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

    public readonly record struct MaterialCall(GameTexture Skin, bool Clamp, int Translation);
    public readonly record struct SetupCall(uint Start, uint Start2, int Count, int Bone);
    public readonly record struct DrawCall(int Count, uint Offset);

    public sealed class GameTexture
    {
        public int Id { get; }

        public GameTexture(int id)
        {
            Id = id;
        }
    }

    public sealed class Renderer
    {
        public int Type { get; }
        public List<(bool Indexed, bool OneFrame)> Created { get; } = new();
        public List<double> Interpolations { get; } = new();
        public List<MaterialCall> Materials { get; } = new();
        public List<SetupCall> FramesSet { get; } = new();
        public List<DrawCall> Draws { get; } = new();
        public List<(TextureId Id, bool Animate)> Lookups { get; } = new();
        public Dictionary<int, GameTexture?> Textures { get; } = new();

        public Renderer(int type)
        {
            Type = type;
        }

        public VertexBuffer CreateVertexBuffer(bool indexed, bool oneFrame)
        {
            Created.Add((indexed, oneFrame));
            return new VertexBuffer(indexed, oneFrame);
        }

        public void SetInterpolation(double inter)
        {
            Interpolations.Add(inter);
        }

        public void SetMaterial(GameTexture skin, bool clamp, int translation)
        {
            Materials.Add(new MaterialCall(skin, clamp, translation));
        }

        public void SetupFrame(VmMd3Model1326 model, uint start, uint start2, int count, int bone)
        {
            _ = model;
            FramesSet.Add(new SetupCall(start, start2, count, bone));
        }

        public void DrawElements(int count, uint offset)
        {
            Draws.Add(new DrawCall(count, offset));
        }

        public GameTexture? GetGameTexture(TextureId id, bool animate)
        {
            Lookups.Add((id, animate));
            if (Textures.TryGetValue(id.Index, out GameTexture? tex))
            {
                return tex;
            }

            return null;
        }
    }

    public static void UnpackVector(ushort packed, out float nx, out float ny, out float nz)
    {
        double lat = (packed >> 8) & 0xff;
        double lng = packed & 0xff;
        lat *= Math.PI / 128.0;
        lng *= Math.PI / 128.0;
        nx = (float)(Math.Cos(lat) * Math.Sin(lng));
        ny = (float)(Math.Sin(lat) * Math.Sin(lng));
        nz = (float)Math.Cos(lng);
    }

    public VertexBuffer? GetVertexBuffer(int type)
    {
        if (_buffers.TryGetValue(type, out VertexBuffer? buffer))
        {
            return buffer;
        }

        return null;
    }

    public void SetVertexBuffer(int type, VertexBuffer buffer)
    {
        _buffers[type] = buffer;
    }

    public TextureId LoadSkin(string path, string name)
    {
        SkinCalls.Add((path, name));
        if (ResolveSkin == null)
        {
            return new TextureId(0, false);
        }

        return ResolveSkin(path, name);
    }

    public bool Load(string path, int lumpNum, byte[] buffer, int length)
    {
        _ = length;
        int numFrames = (int)ReadU32(buffer, 76);
        int numSurfaces = (int)ReadU32(buffer, 84);
        HasSurfaces = numSurfaces > 1;
        NumTags = (int)ReadU32(buffer, 80);
        int frameAt = (int)ReadU32(buffer, 92);
        Frames.Clear();
        for (int i = 0; i < numFrames; i++)
        {
            int at = frameAt + i * FrameBytes;
            var frame = new Frame
            {
                Name = CopyFrameName(buffer.AsSpan(at + 40, 16)),
            };
            frame.Origin[0] = ReadF32(buffer, at + 24);
            frame.Origin[1] = ReadF32(buffer, at + 28);
            frame.Origin[2] = ReadF32(buffer, at + 32);
            Frames.Add(frame);
        }

        int surfAt = (int)ReadU32(buffer, 100);
        Surfaces.Clear();
        for (int i = 0; i < numSurfaces; i++)
        {
            int ss = surfAt;
            surfAt += (int)ReadU32(buffer, ss + 104);
            var surface = new Surface
            {
                NumSkins = (int)ReadU32(buffer, ss + 76),
                NumVertices = (int)ReadU32(buffer, ss + 80),
                NumTriangles = (int)ReadU32(buffer, ss + 84),
            };
            int shaderAt = ss + (int)ReadU32(buffer, ss + 92);
            for (int ii = 0; ii < surface.NumSkins; ii++)
            {
                string name = FixPathSeperator(FieldString(buffer.AsSpan(shaderAt + ii * ShaderBytes, 64)));
                TextureId skin = LoadSkin("", name);
                if (!skin.IsValid())
                {
                    skin = LoadSkin(path, name);
                }

                surface.Skins.Add(skin);
            }

            Surfaces.Add(surface);
        }

        LumpNum = lumpNum;
        return true;
    }

    public void LoadGeometry()
    {
        GeometryReads++;
        byte[] buffer = Lumps[LumpNum];
        int surfAt = (int)ReadU32(buffer, 100);
        for (int i = 0; i < Surfaces.Count; i++)
        {
            Surface surface = Surfaces[i];
            int ss = surfAt;
            surfAt += (int)ReadU32(buffer, ss + 104);
            int triAt = ss + (int)ReadU32(buffer, ss + 88);
            surface.Tris.Clear();
            for (int ii = 0; ii < surface.NumTriangles; ii++)
            {
                var tri = new Triangle();
                for (int j = 0; j < 3; j++)
                {
                    tri.VertIndex[j] = (int)ReadU32(buffer, triAt + (ii * 3 + j) * 4);
                }

                surface.Tris.Add(tri);
            }

            int tcAt = ss + (int)ReadU32(buffer, ss + 96);
            surface.Texcoords.Clear();
            for (int ii = 0; ii < surface.NumVertices; ii++)
            {
                surface.Texcoords.Add(new TexCoord
                {
                    S = ReadF32(buffer, tcAt + ii * 8),
                    T = ReadF32(buffer, tcAt + ii * 8 + 4),
                });
            }

            int vertAt = ss + (int)ReadU32(buffer, ss + 100);
            surface.Vertices.Clear();
            int count = surface.NumVertices * Frames.Count;
            for (int ii = 0; ii < count; ii++)
            {
                int at = vertAt + ii * 8;
                short x = ReadI16(buffer, at);
                short y = ReadI16(buffer, at + 2);
                short z = ReadI16(buffer, at + 4);
                short n = ReadI16(buffer, at + 6);
                UnpackVector((ushort)n, out float nx, out float ny, out float nz);
                surface.Vertices.Add(new Md3Vertex
                {
                    X = x / 64f,
                    Y = y / 64f,
                    Z = z / 64f,
                    Nx = nx,
                    Ny = ny,
                    Nz = nz,
                });
            }
        }
    }

    public void BuildVertexBuffer(Renderer renderer)
    {
        if (GetVertexBuffer(renderer.Type) != null)
        {
            return;
        }

        LoadGeometry();
        int vbufsize = 0;
        int ibufsize = 0;
        for (int i = 0; i < Surfaces.Count; i++)
        {
            Surface surf = Surfaces[i];
            vbufsize += Frames.Count * surf.NumVertices;
            ibufsize += 3 * surf.NumTriangles;
        }

        VertexBuffer vbuf = renderer.CreateVertexBuffer(true, Frames.Count == 1);
        SetVertexBuffer(renderer.Type, vbuf);
        ModelVertex[] vertptr = vbuf.LockVertexBuffer(vbufsize);
        uint[] indxptr = vbuf.LockIndexBuffer(ibufsize);
        int vindex = 0;
        int iindex = 0;
        for (int i = 0; i < Surfaces.Count; i++)
        {
            Surface surf = Surfaces[i];
            surf.VIndex = (uint)vindex;
            surf.IIndex = (uint)iindex;
            for (int j = 0; j < Frames.Count * surf.NumVertices; j++)
            {
                Md3Vertex vert = surf.Vertices[j];
                var bvert = new ModelVertex();
                int tc = j % surf.NumVertices;
                bvert.Set(vert.X, vert.Z, vert.Y, surf.Texcoords[tc].S, surf.Texcoords[tc].T);
                bvert.SetNormal(vert.Nx, vert.Nz, vert.Ny);
                vertptr[vindex] = bvert;
                vindex++;
            }

            for (int k = 0; k < surf.NumTriangles; k++)
            {
                for (int l = 0; l < 3; l++)
                {
                    indxptr[iindex] = (uint)surf.Tris[k].VertIndex[l];
                    iindex++;
                }
            }

            surf.UnloadGeometry();
        }

        vbuf.UnlockVertexBuffer();
        vbuf.UnlockIndexBuffer();
    }

    public void AddSkins(byte[] hitlist, TextureId[]? surfaceSkinIds)
    {
        for (int i = 0; i < Surfaces.Count; i++)
        {
            if (surfaceSkinIds != null && surfaceSkinIds[i].IsValid())
            {
                hitlist[surfaceSkinIds[i].GetIndex()] |= HitFlat;
            }

            Surface surf = Surfaces[i];
            for (int j = 0; j < surf.NumSkins; j++)
            {
                if (surf.Skins[j].IsValid())
                {
                    hitlist[surf.Skins[j].GetIndex()] |= HitFlat;
                }
            }
        }
    }

    public int FindFrame(string name, bool noDefault)
    {
        _ = noDefault;
        for (int i = 0; i < Frames.Count; i++)
        {
            if (string.Equals(name, Frames[i].Name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return ErrNotFound;
    }

    public void RenderFrame(Renderer renderer, GameTexture? skin, int frameNo, int frameNo2, double inter, int translation, TextureId[]? surfaceSkinIds, int boneStart)
    {
        _ = boneStart;
        if ((uint)frameNo >= (uint)Frames.Count || (uint)frameNo2 >= (uint)Frames.Count)
        {
            return;
        }

        renderer.SetInterpolation(inter);
        for (int i = 0; i < Surfaces.Count; i++)
        {
            Surface surf = Surfaces[i];
            GameTexture? surfaceSkin = skin;
            if (surfaceSkin == null)
            {
                if (surfaceSkinIds != null && surfaceSkinIds[i].IsValid())
                {
                    surfaceSkin = renderer.GetGameTexture(surfaceSkinIds[i], true);
                }
                else if (surf.NumSkins > 0 && surf.Skins[0].IsValid())
                {
                    surfaceSkin = renderer.GetGameTexture(surf.Skins[0], true);
                }

                if (surfaceSkin == null)
                {
                    continue;
                }
            }

            renderer.SetMaterial(surfaceSkin, false, translation);
            uint start = surf.VIndex + (uint)frameNo * (uint)surf.NumVertices;
            uint start2 = surf.VIndex + (uint)frameNo2 * (uint)surf.NumVertices;
            renderer.SetupFrame(this, start, start2, surf.NumVertices, -1);
            renderer.DrawElements(surf.NumTriangles * 3, surf.IIndex * 4u);
        }

        renderer.SetInterpolation(0f);
    }

    static string CopyFrameName(ReadOnlySpan<byte> src)
    {
        Span<byte> dest = stackalloc byte[16];
        dest.Clear();
        int copied = 0;
        int max = src.Length < 15 ? src.Length : 15;
        while (copied < max && src[copied] != 0)
        {
            dest[copied] = src[copied];
            copied++;
        }

        return Encoding.Latin1.GetString(dest[..copied]);
    }

    static string FieldString(ReadOnlySpan<byte> field)
    {
        int n = 0;
        while (n < field.Length && field[n] != 0)
        {
            n++;
        }

        return Encoding.Latin1.GetString(field[..n]);
    }

    static string FixPathSeperator(string path)
    {
        return path.Replace('\\', '/');
    }

    static uint ReadU32(byte[] buffer, int offset)
    {
        return BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset, 4));
    }

    static short ReadI16(byte[] buffer, int offset)
    {
        return BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(offset, 2));
    }

    static float ReadF32(byte[] buffer, int offset)
    {
        return BinaryPrimitives.ReadSingleLittleEndian(buffer.AsSpan(offset, 4));
    }
}
