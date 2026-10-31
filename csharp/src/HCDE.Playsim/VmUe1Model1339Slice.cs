using System.Buffers.Binary;

namespace HCDE.Playsim;

/// <summary>
/// Unreal 1 model load, geometry, and frame draw from <c>models_ue1.cpp</c>
/// (1-324). Chunk 1339. Lump bytes, texture lookup, and the renderer stay injected.
/// </summary>
public sealed class VmUe1Model1339
{
    public const int ErrNotFound = -1;
    public const int HitFlat = 2;
    public const int PtWeaponTriangle = 0x08;
    public const int PtCurvy = 0x20;
    public const int DataHeaderBytes = 48;
    public const int PolyBytes = 16;
    public const int AnimHeaderBytes = 4;

    public bool HasSurfaces { get; private set; }
    public bool HasWeaponTriangle { get; private set; }
    public int DataLump { get; private set; } = -1;
    public int AnivLump { get; private set; } = -1;
    public int NumVerts { get; private set; }
    public int NumFrames { get; private set; }
    public int NumPolys { get; private set; }
    public int NumGroups { get; private set; }
    public int GeometryReads { get; private set; }
    public Dictionary<int, string> LumpNames { get; } = new();
    public Dictionary<string, int> NameToLump { get; } = new();
    public Dictionary<int, byte[]> Lumps { get; } = new();
    public List<string> CheckedNames { get; } = new();
    public List<Vertex> Verts { get; } = new();
    public List<Poly> Polys { get; } = new();
    public List<Group> Groups { get; } = new();

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

    public struct Vec3
    {
        public float X;
        public float Y;
        public float Z;
    }

    public sealed class Vertex
    {
        public float X;
        public float Y;
        public float Z;
        public float Nx;
        public float Ny;
        public float Nz;
        public List<int> P { get; } = new();
        public int RefCount;
    }

    public sealed class Poly
    {
        public int[] V { get; } = new int[3];
        public float[] S { get; } = new float[3];
        public float[] T { get; } = new float[3];
        public List<Vec3> Normals { get; } = new();
    }

    public sealed class Group
    {
        public List<int> P { get; } = new();
        public int NumPolys;
        public int TexNum;
        public int Type;
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
        public bool Unlocked { get; private set; }

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

        public void UnlockVertexBuffer()
        {
            Unlocked = true;
        }
    }

    public readonly record struct MaterialCall(GameTexture Skin, bool Clamp, int Translation);
    public readonly record struct SetupCall(int Start, int Start2, int Count, int Bone);
    public readonly record struct DrawCall(int First, int Count);

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
        public int Kind { get; }
        public List<(bool Indexed, bool OneFrame)> Created { get; } = new();
        public List<double> Interpolations { get; } = new();
        public List<MaterialCall> Materials { get; } = new();
        public List<SetupCall> FramesSet { get; } = new();
        public List<DrawCall> Draws { get; } = new();
        public List<(TextureId Id, bool Animate)> Lookups { get; } = new();
        public Dictionary<int, GameTexture?> Textures { get; } = new();

        public Renderer(int kind)
        {
            Kind = kind;
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

        public void SetupFrame(VmUe1Model1339 model, int start, int start2, int count, int bone)
        {
            _ = model;
            FramesSet.Add(new SetupCall(start, start2, count, bone));
        }

        public void DrawArrays(int first, int count)
        {
            Draws.Add(new DrawCall(first, count));
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

    public static float UnpackUvert(uint n, int c)
    {
        switch (c)
        {
            case 0:
                return unchecked((short)((n & 0x7ffu) << 5)) / 128f;
            case 1:
                return unchecked((short)(((n >> 11) & 0x7ffu) << 5)) / 128f;
            case 2:
                return unchecked((short)(((n >> 22) & 0x3ffu) << 6)) / 128f;
            default:
                return 0f;
        }
    }

    public static long ParseFrameIndex(string? text)
    {
        if (text == null)
        {
            return 0;
        }

        int i = 0;
        int n = text.Length;
        while (i < n && IsCSpace(text[i]))
        {
            i++;
        }

        int sign = 1;
        if (i < n && (text[i] == '+' || text[i] == '-'))
        {
            if (text[i] == '-')
            {
                sign = -1;
            }

            i++;
        }

        int radix = 10;
        if (i < n && text[i] == '0')
        {
            int hex = i + 1;
            if (hex + 1 < n && (text[hex] == 'x' || text[hex] == 'X') && HexDigit(text[hex + 1]) >= 0)
            {
                radix = 16;
                i = hex + 1;
            }
            else
            {
                radix = 8;
            }
        }

        long value = 0;
        bool any = false;
        while (i < n)
        {
            int dig = radix == 16 ? HexDigit(text[i]) : text[i] - '0';
            if (dig < 0 || dig >= radix)
            {
                break;
            }

            any = true;
            value = value * radix + dig;
            i++;
        }

        if (!any)
        {
            return 0;
        }

        if (sign < 0)
        {
            value = -value;
        }

        return value;
    }

    public VertexBuffer? GetVertexBuffer(int kind)
    {
        if (_buffers.TryGetValue(kind, out VertexBuffer? buffer))
        {
            return buffer;
        }

        return null;
    }

    public void SetVertexBuffer(int kind, VertexBuffer buffer)
    {
        _buffers[kind] = buffer;
    }

    public bool Load(string? filename, int lumpnum, byte[]? buffer, int length)
    {
        _ = filename;
        _ = buffer;
        _ = length;
        HasSurfaces = true;
        string real = "";
        if (LumpNames.TryGetValue(lumpnum, out string? found) && found != null)
        {
            real = found;
        }

        if (SuffixSelectsData(real))
        {
            string anivName = real.Replace("_d.3d", "_a.3d", StringComparison.Ordinal);
            DataLump = lumpnum;
            AnivLump = Check(anivName);
        }
        else
        {
            string dataName = real.Replace("_a.3d", "_d.3d", StringComparison.Ordinal);
            AnivLump = lumpnum;
            DataLump = Check(dataName);
        }

        return true;
    }

    public void LoadGeometry()
    {
        GeometryReads++;
        if (!Lumps.TryGetValue(DataLump, out byte[]? data) || data == null)
        {
            throw new InvalidOperationException("missing data lump");
        }

        if (!Lumps.TryGetValue(AnivLump, out byte[]? anim) || anim == null)
        {
            throw new InvalidOperationException("missing anim lump");
        }

        if (data.Length < DataHeaderBytes || anim.Length < AnimHeaderBytes)
        {
            throw new InvalidOperationException("truncated UE1 model");
        }

        int numPolys = ReadU16(data, 0);
        int numVerts = ReadU16(data, 2);
        int numFrames = ReadU16(anim, 0);
        int frameSize = ReadU16(anim, 2);
        if (numVerts == 0)
        {
            throw new InvalidOperationException("numverts is zero");
        }

        bool deusEx = frameSize / numVerts == 8;
        long stride = deusEx ? 8 : 4;
        long dataNeed = DataHeaderBytes + (long)numPolys * PolyBytes;
        long animNeed = AnimHeaderBytes + (long)numFrames * numVerts * stride;
        if (data.Length < dataNeed || anim.Length < animNeed)
        {
            throw new InvalidOperationException("truncated UE1 model");
        }

        NumVerts = numVerts;
        NumFrames = numFrames;
        NumPolys = numPolys;
        NumGroups = 0;
        for (int i = 0; i < numFrames; i++)
        {
            for (int j = 0; j < numVerts; j++)
            {
                var vert = new Vertex();
                int slot = j + i * numVerts;
                if (deusEx)
                {
                    int at = AnimHeaderBytes + slot * 8;
                    short x = ReadI16(anim, at);
                    short y = ReadI16(anim, at + 2);
                    short z = ReadI16(anim, at + 4);
                    vert.X = x;
                    vert.Y = z;
                    vert.Z = -y;
                }
                else
                {
                    uint packed = ReadU32(anim, AnimHeaderBytes + slot * 4);
                    vert.X = UnpackUvert(packed, 0);
                    vert.Y = UnpackUvert(packed, 2);
                    vert.Z = -UnpackUvert(packed, 1);
                }

                Verts.Add(vert);
            }
        }

        for (int i = 0; i < numPolys; i++)
        {
            int basis = DataHeaderBytes + i * PolyBytes;
            var poly = new Poly();
            for (int j = 0; j < 3; j++)
            {
                poly.V[j] = ReadU16(data, basis + j * 2);
                poly.S[j] = data[basis + 8 + j * 2] / 255f;
                poly.T[j] = data[basis + 9 + j * 2] / 255f;
            }

            for (int j = 0; j < numFrames; j++)
            {
                Vertex v0 = Verts[poly.V[0] + numVerts * j];
                Vertex v1 = Verts[poly.V[1] + numVerts * j];
                Vertex v2 = Verts[poly.V[2] + numVerts * j];
                float d0x = v1.X - v0.X;
                float d0y = v1.Y - v0.Y;
                float d0z = v1.Z - v0.Z;
                float d1x = v2.X - v0.X;
                float d1y = v2.Y - v0.Y;
                float d1z = v2.Z - v0.Z;
                float nx = d0y * d1z - d0z * d1y;
                float ny = d0z * d1x - d0x * d1z;
                float nz = d0x * d1y - d0y * d1x;
                Unit(ref nx, ref ny, ref nz);
                poly.Normals.Add(new Vec3 { X = nx, Y = ny, Z = nz });
                for (int k = 0; k < 3; k++)
                {
                    Vertex corner = Verts[poly.V[k] + numVerts * j];
                    corner.P.Add(i);
                    corner.RefCount++;
                }
            }

            Polys.Add(poly);
        }

        for (int i = 0; i < numFrames; i++)
        {
            for (int j = 0; j < numVerts; j++)
            {
                Vertex vert = Verts[j + numVerts * i];
                float sx = 0f;
                float sy = 0f;
                float sz = 0f;
                for (int k = 0; k < vert.RefCount; k++)
                {
                    Vec3 normal = Polys[vert.P[k]].Normals[i];
                    sx += normal.X;
                    sy += normal.Y;
                    sz += normal.Z;
                }

                Unit(ref sx, ref sy, ref sz);
                vert.Nx = sx;
                vert.Ny = sy;
                vert.Nz = sz;
            }
        }

        Groups.Clear();
        int current = -1;
        HasWeaponTriangle = false;
        int weaponTri = -1;
        for (int i = 0; i < numPolys; i++)
        {
            int basis = DataHeaderBytes + i * PolyBytes;
            int type = data[basis + 6];
            int tex = data[basis + 14];
            if ((type & PtWeaponTriangle) != 0)
            {
                weaponTri = i;
            }

            if (current == -1)
            {
                Groups.Add(new Group
                {
                    NumPolys = 0,
                    TexNum = tex,
                    Type = type,
                });
                current = NumGroups++;
            }
            else if (tex != Groups[current].TexNum || type != Groups[current].Type)
            {
                current = -1;
                for (int j = 0; j < NumGroups; j++)
                {
                    if (Groups[j].TexNum != tex || Groups[j].Type != type)
                    {
                        continue;
                    }

                    current = j;
                    break;
                }

                i--;
                continue;
            }

            Groups[current].P.Add(i);
            Groups[current].NumPolys++;
        }

        if (weaponTri != -1)
        {
            HasWeaponTriangle = true;
        }
    }

    public void UnloadGeometry()
    {
        for (int i = 0; i < Verts.Count; i++)
        {
            Verts[i].P.Clear();
        }

        Verts.Clear();
        for (int i = 0; i < Polys.Count; i++)
        {
            Polys[i].Normals.Clear();
        }

        Polys.Clear();
        for (int i = 0; i < Groups.Count; i++)
        {
            Groups[i].P.Clear();
        }
    }

    public int FindFrame(string? name, bool nodefault)
    {
        _ = nodefault;
        long index = ParseFrameIndex(name);
        if (index < 0 || index >= NumFrames)
        {
            return ErrNotFound;
        }

        return (int)index;
    }

    public void RenderFrame(
        Renderer renderer,
        GameTexture? skin,
        int frame,
        int frame2,
        double inter,
        int translation,
        TextureId[]? surfaceSkinIds,
        int boneStartPosition)
    {
        _ = boneStartPosition;
        if (frame < 0 || frame2 < 0 || frame >= NumFrames || frame2 >= NumFrames)
        {
            return;
        }

        renderer.SetInterpolation(inter);
        int fsize = 0;
        for (int i = 0; i < NumGroups; i++)
        {
            fsize += Groups[i].NumPolys * 3;
        }

        int vofs = 0;
        for (int i = 0; i < NumGroups; i++)
        {
            int vsize = Groups[i].NumPolys * 3;
            if ((Groups[i].Type & PtWeaponTriangle) != 0)
            {
                vofs += vsize;
                continue;
            }

            GameTexture? sskin = skin;
            if (sskin == null)
            {
                int ssIndex = Groups[i].TexNum;
                if (surfaceSkinIds != null && surfaceSkinIds[ssIndex].IsValid())
                {
                    sskin = renderer.GetGameTexture(surfaceSkinIds[ssIndex], true);
                }

                if (sskin == null)
                {
                    vofs += vsize;
                    continue;
                }
            }

            renderer.SetMaterial(sskin, false, translation);
            renderer.SetupFrame(this, vofs + frame * fsize, vofs + frame2 * fsize, vsize, -1);
            renderer.DrawArrays(0, vsize);
            vofs += vsize;
        }

        renderer.SetInterpolation(0f);
    }

    public void BuildVertexBuffer(Renderer renderer)
    {
        if (GetVertexBuffer(renderer.Kind) != null)
        {
            return;
        }

        LoadGeometry();
        int vsize = 0;
        for (int i = 0; i < NumGroups; i++)
        {
            vsize += Groups[i].NumPolys * 3;
        }

        vsize *= NumFrames;
        VertexBuffer vbuf = renderer.CreateVertexBuffer(false, NumFrames == 1);
        SetVertexBuffer(renderer.Kind, vbuf);
        ModelVertex[] locked = vbuf.LockVertexBuffer(vsize);
        int vidx = 0;
        for (int i = 0; i < NumFrames; i++)
        {
            for (int j = 0; j < NumGroups; j++)
            {
                Group group = Groups[j];
                for (int k = 0; k < group.NumPolys; k++)
                {
                    Poly poly = Polys[group.P[k]];
                    for (int l = 0; l < 3; l++)
                    {
                        Vertex vert = Verts[poly.V[l] + i * NumVerts];
                        ModelVertex slot = locked[vidx];
                        slot.Set(vert.X, vert.Y, vert.Z, poly.S[l], poly.T[l]);
                        if ((group.Type & PtCurvy) != 0)
                        {
                            Vec3 facet = poly.Normals[i];
                            slot.SetNormal(facet.X, facet.Y, facet.Z);
                        }
                        else
                        {
                            slot.SetNormal(vert.Nx, vert.Ny, vert.Nz);
                        }

                        locked[vidx] = slot;
                        vidx++;
                    }
                }
            }
        }

        vbuf.UnlockVertexBuffer();
        UnloadGeometry();
    }

    public void AddSkins(byte[] hitlist, TextureId[]? surfaceSkinIds)
    {
        for (int i = 0; i < NumGroups; i++)
        {
            int ssIndex = Groups[i].TexNum;
            if (surfaceSkinIds != null && surfaceSkinIds[ssIndex].IsValid())
            {
                hitlist[surfaceSkinIds[ssIndex].GetIndex()] |= HitFlat;
            }
        }
    }

    public void Destroy()
    {
        Groups.Clear();
    }

    int Check(string name)
    {
        CheckedNames.Add(name);
        if (NameToLump.TryGetValue(name, out int lump))
        {
            return lump;
        }

        return -1;
    }

    static bool SuffixSelectsData(string name)
    {
        int index = name.IndexOf("_d.3d", StringComparison.Ordinal);
        ulong left = unchecked((ulong)index);
        ulong right = unchecked((ulong)name.Length - 5UL);
        return left == right;
    }

    static void Unit(ref float x, ref float y, ref float z)
    {
        float squared = x * x + y * y + z * z;
        double len = Math.Sqrt(squared);
        if (len != 0)
        {
            len = 1 / len;
        }

        float scale = (float)len;
        x *= scale;
        y *= scale;
        z *= scale;
    }

    static bool IsCSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
    }

    static int HexDigit(char c)
    {
        if (c >= '0' && c <= '9')
        {
            return c - '0';
        }

        if (c >= 'a' && c <= 'f')
        {
            return c - 'a' + 10;
        }

        if (c >= 'A' && c <= 'F')
        {
            return c - 'A' + 10;
        }

        return -1;
    }

    static int ReadU16(byte[] bytes, int at)
    {
        return BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(at, 2));
    }

    static short ReadI16(byte[] bytes, int at)
    {
        return BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(at, 2));
    }

    static uint ReadU32(byte[] bytes, int at)
    {
        return BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(at, 4));
    }
}
