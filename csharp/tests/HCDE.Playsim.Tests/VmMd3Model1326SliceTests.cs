using System.Buffers.Binary;
using System.Text;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmMd3Model1326SliceTests
{
    [Fact]
    public void Load_CopiesFramesAndFallsBackTheSkin()
    {
        VmMd3Model1326.UnpackVector(0x0040, out float nx, out float ny, out float nz);
        Assert.Equal(1d, nx, 5);
        Assert.Equal(0d, ny, 5);
        Assert.Equal(0d, nz, 5);
        VmMd3Model1326.UnpackVector(0x4000, out nx, out ny, out nz);
        Assert.Equal(0d, nx, 5);
        Assert.Equal(0d, ny, 5);
        Assert.Equal(1d, nz, 5);

        var name = new byte[16];
        Array.Fill(name, (byte)'A');
        byte[] file = Build(
            new[] { Frame("ABCDEFGHIJKLMNOP", 1.5f, -2f, 3.25f, name) },
            new[]
            {
                Surface(1, new[] { "models\\A.TGA" }, new[] { 2, 0, 1 }, new[] { (0.25f, 0.75f) }, new[] { Vert(64, -128, 32, 0x0040) }),
            },
            4);
        var model = new VmMd3Model1326
        {
            ResolveSkin = (path, skin) =>
                path == "base/" && skin == "models/A.TGA"
                    ? new VmMd3Model1326.TextureId(4, true)
                    : new VmMd3Model1326.TextureId(0, false),
        };
        Assert.True(model.Load("base/", 9, file, file.Length));
        Assert.False(model.HasSurfaces);
        Assert.Equal(4, model.NumTags);
        Assert.Equal(9, model.LumpNum);
        Assert.Equal(new string('A', 15), model.Frames[0].Name);
        Assert.Equal(1.5f, model.Frames[0].Origin[0]);
        Assert.Equal(-2f, model.Frames[0].Origin[1]);
        Assert.Equal(3.25f, model.Frames[0].Origin[2]);
        Assert.Equal(4, model.Surfaces[0].Skins[0].GetIndex());
        Assert.Equal(("", "models/A.TGA"), model.SkinCalls[0]);
        Assert.Equal(("base/", "models/A.TGA"), model.SkinCalls[1]);
        Assert.Equal(0, model.FindFrame("aaaaaaaaaaaaaaa", true));
        Assert.Equal(VmMd3Model1326.ErrNotFound, model.FindFrame("missing", true));
        Assert.Equal(VmMd3Model1326.ErrNotFound, model.FindFrame("missing", false));

        model.Lumps[9] = file;
        var renderer = new VmMd3Model1326.Renderer(3);
        model.BuildVertexBuffer(renderer);
        model.BuildVertexBuffer(renderer);
        Assert.Equal(1, model.GeometryReads);
        Assert.Single(renderer.Created);
        Assert.True(renderer.Created[0].Indexed);
        Assert.True(renderer.Created[0].OneFrame);
        VmMd3Model1326.VertexBuffer buffer = model.GetVertexBuffer(3)!;
        Assert.True(buffer.VerticesUnlocked);
        Assert.True(buffer.IndicesUnlocked);
        Assert.Equal(1f, buffer.Vertices[0].X);
        Assert.Equal(0.5f, buffer.Vertices[0].Y);
        Assert.Equal(-2f, buffer.Vertices[0].Z);
        Assert.Equal(0.25f, buffer.Vertices[0].S);
        Assert.Equal(0.75f, buffer.Vertices[0].T);
        Assert.Equal(1d, buffer.Vertices[0].Nx, 5);
        Assert.Equal(0d, buffer.Vertices[0].Ny, 5);
        Assert.Equal(0d, buffer.Vertices[0].Nz, 5);
        Assert.Equal(new uint[] { 2, 0, 1 }, buffer.Indices);
        Assert.Empty(model.Surfaces[0].Tris);
        Assert.Empty(model.Surfaces[0].Vertices);
        Assert.Empty(model.Surfaces[0].Texcoords);
    }

    [Fact]
    public void Geometry_SwapsAxesAndInterpolatesTheFrame()
    {
        byte[] file = Build(
            new[]
            {
                Frame("Walk", 0, 0, 0, Encoding.Latin1.GetBytes("Walk\0")),
                Frame("Shot", 0, 0, 0, Encoding.Latin1.GetBytes("Shot\0")),
            },
            new[]
            {
                Surface(
                    2,
                    new[] { "models\\a.tga" },
                    new[] { 0, 1, 0 },
                    new[] { (0.5f, 0.25f), (0.125f, 0.875f) },
                    new[]
                    {
                        Vert(64, 0, 128, 0x0040),
                        Vert(0, 64, 0, 0x0040),
                        Vert(128, 0, 0, 0x4000),
                        Vert(0, 0, 64, 0x0040),
                    }),
                Surface(
                    1,
                    new[] { "ok.pcx" },
                    new[] { 5, 6, 7 },
                    new[] { (1f, 0f) },
                    new[] { Vert(-64, 0, 0, 0x0040), Vert(0, -64, 64, 0x0040) }),
            },
            2);
        var model = new VmMd3Model1326
        {
            ResolveSkin = (path, skin) =>
            {
                if (skin == "models/a.tga" && path == "base/")
                {
                    return new VmMd3Model1326.TextureId(4, true);
                }

                if (skin == "ok.pcx" && path == "")
                {
                    return new VmMd3Model1326.TextureId(8, true);
                }

                return new VmMd3Model1326.TextureId(0, false);
            },
        };
        Assert.True(model.Load("base/", 3, file, file.Length));
        Assert.True(model.HasSurfaces);
        Assert.Equal(0, model.FindFrame("walk", false));
        Assert.Equal(1, model.FindFrame("SHOT", true));
        Assert.Equal(3, model.SkinCalls.Count);
        Assert.Equal(("base/", "models/a.tga"), model.SkinCalls[1]);
        Assert.Equal(("", "ok.pcx"), model.SkinCalls[2]);

        model.Lumps[3] = file;
        var renderer = new VmMd3Model1326.Renderer(1);
        model.BuildVertexBuffer(renderer);
        Assert.False(renderer.Created[0].OneFrame);
        VmMd3Model1326.VertexBuffer buffer = model.GetVertexBuffer(1)!;
        Assert.Equal(6, buffer.Vertices.Length);
        Assert.Equal(2f, buffer.Vertices[2].X);
        Assert.Equal(0f, buffer.Vertices[2].Y);
        Assert.Equal(0f, buffer.Vertices[2].Z);
        Assert.Equal(0.5f, buffer.Vertices[2].S);
        Assert.Equal(0d, buffer.Vertices[2].Nx, 5);
        Assert.Equal(1d, buffer.Vertices[2].Ny, 5);
        Assert.Equal(0d, buffer.Vertices[2].Nz, 5);
        Assert.Equal(-1f, buffer.Vertices[4].X);
        Assert.Equal(1f, buffer.Vertices[5].Y);
        Assert.Equal(-1f, buffer.Vertices[5].Z);
        Assert.Equal(1f, buffer.Vertices[4].S);
        Assert.Equal(new uint[] { 0, 1, 0, 5, 6, 7 }, buffer.Indices);
        Assert.Equal(0u, model.Surfaces[0].VIndex);
        Assert.Equal(0u, model.Surfaces[0].IIndex);
        Assert.Equal(4u, model.Surfaces[1].VIndex);
        Assert.Equal(3u, model.Surfaces[1].IIndex);
        Assert.Empty(model.Surfaces[1].Texcoords);

        var hit = new byte[16];
        var ids = new[]
        {
            new VmMd3Model1326.TextureId(3, true),
            new VmMd3Model1326.TextureId(0, false),
        };
        model.AddSkins(hit, ids);
        Assert.Equal(VmMd3Model1326.HitFlat, hit[3]);
        Assert.Equal(VmMd3Model1326.HitFlat, hit[4]);
        Assert.Equal(VmMd3Model1326.HitFlat, hit[8]);
        Array.Clear(hit);
        model.AddSkins(hit, null);
        Assert.Equal(0, hit[3]);
        Assert.Equal(VmMd3Model1326.HitFlat, hit[4]);
        Assert.Equal(VmMd3Model1326.HitFlat, hit[8]);

        var draw = new VmMd3Model1326.Renderer(1);
        model.RenderFrame(draw, null, -1, 0, 0.25, 9, null, 0);
        Assert.Empty(draw.Interpolations);
        model.RenderFrame(draw, null, 0, 2, 0.25, 9, null, 0);
        Assert.Empty(draw.Interpolations);

        var explicitSkin = new VmMd3Model1326.GameTexture(70);
        model.RenderFrame(draw, explicitSkin, 0, 1, 0.25, 9, ids, 0);
        Assert.Equal(new[] { 0.25, 0d }, draw.Interpolations);
        Assert.Equal(2, draw.Materials.Count);
        Assert.Same(explicitSkin, draw.Materials[0].Skin);
        Assert.False(draw.Materials[0].Clamp);
        Assert.Equal(9, draw.Materials[0].Translation);
        Assert.Empty(draw.Lookups);
        Assert.Equal((0u, 2u, 2, -1), (draw.FramesSet[0].Start, draw.FramesSet[0].Start2, draw.FramesSet[0].Count, draw.FramesSet[0].Bone));
        Assert.Equal((3, 0u), (draw.Draws[0].Count, draw.Draws[0].Offset));
        Assert.Equal((4u, 5u, 1, -1), (draw.FramesSet[1].Start, draw.FramesSet[1].Start2, draw.FramesSet[1].Count, draw.FramesSet[1].Bone));
        Assert.Equal((3, 12u), (draw.Draws[1].Count, draw.Draws[1].Offset));

        var surfaced = new VmMd3Model1326.Renderer(1);
        var tex3 = new VmMd3Model1326.GameTexture(3);
        var tex8 = new VmMd3Model1326.GameTexture(8);
        surfaced.Textures[3] = tex3;
        surfaced.Textures[8] = tex8;
        model.RenderFrame(surfaced, null, 0, 1, 0.5, 1, ids, 4);
        Assert.Same(tex3, surfaced.Materials[0].Skin);
        Assert.True(surfaced.Lookups[0].Animate);
        Assert.Equal(3, surfaced.Lookups[0].Id.Index);
        Assert.Same(tex8, surfaced.Materials[1].Skin);
        Assert.Equal(8, surfaced.Lookups[1].Id.Index);

        var missing = new VmMd3Model1326.Renderer(1);
        missing.Textures[8] = tex8;
        model.RenderFrame(missing, null, 0, 0, 0.5, 1, ids, 0);
        Assert.Single(missing.Materials);
        Assert.Same(tex8, missing.Materials[0].Skin);
        Assert.Equal(0d, missing.Interpolations[^1]);
    }

    static (string Name, float X, float Y, float Z, byte[] Raw) Frame(string name, float x, float y, float z, byte[] raw)
    {
        _ = name;
        return ("", x, y, z, raw);
    }

    static (int NumVertices, string[] Shaders, int[] Indices, (float S, float T)[] Tc, (short X, short Y, short Z, ushort N)[] Verts) Surface(
        int numVertices,
        string[] shaders,
        int[] indices,
        (float S, float T)[] tc,
        (short X, short Y, short Z, ushort N)[] verts)
    {
        return (numVertices, shaders, indices, tc, verts);
    }

    static (short X, short Y, short Z, ushort N) Vert(short x, short y, short z, ushort n)
    {
        return (x, y, z, n);
    }

    static byte[] Build(
        (string Name, float X, float Y, float Z, byte[] Raw)[] frames,
        (int NumVertices, string[] Shaders, int[] Indices, (float S, float T)[] Tc, (short X, short Y, short Z, ushort N)[] Verts)[] surfaces,
        int numTags)
    {
        int surfStart = VmMd3Model1326.HeaderBytes + frames.Length * VmMd3Model1326.FrameBytes;
        var sizes = new int[surfaces.Length];
        for (int i = 0; i < surfaces.Length; i++)
        {
            int tris = surfaces[i].Indices.Length / 3;
            sizes[i] = VmMd3Model1326.SurfaceBytes
                + tris * 12
                + surfaces[i].Shaders.Length * VmMd3Model1326.ShaderBytes
                + surfaces[i].NumVertices * 8
                + surfaces[i].Verts.Length * 8;
        }

        int total = surfStart;
        foreach (int size in sizes)
        {
            total += size;
        }

        var buf = new byte[total];
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(76), (uint)frames.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(80), (uint)numTags);
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(84), (uint)surfaces.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(92), (uint)VmMd3Model1326.HeaderBytes);
        BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(100), (uint)surfStart);
        for (int i = 0; i < frames.Length; i++)
        {
            int at = VmMd3Model1326.HeaderBytes + i * VmMd3Model1326.FrameBytes;
            WriteF(buf, at + 24, frames[i].X);
            WriteF(buf, at + 28, frames[i].Y);
            WriteF(buf, at + 32, frames[i].Z);
            frames[i].Raw.CopyTo(buf.AsSpan(at + 40));
        }

        int cursor = surfStart;
        for (int i = 0; i < surfaces.Length; i++)
        {
            var surface = surfaces[i];
            int ss = cursor;
            int tris = surface.Indices.Length / 3;
            int ofsTri = VmMd3Model1326.SurfaceBytes;
            int ofsShader = ofsTri + tris * 12;
            int ofsTc = ofsShader + surface.Shaders.Length * VmMd3Model1326.ShaderBytes;
            int ofsVert = ofsTc + surface.NumVertices * 8;
            int ofsEnd = ofsVert + surface.Verts.Length * 8;
            BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(ss + 76), (uint)surface.Shaders.Length);
            BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(ss + 80), (uint)surface.NumVertices);
            BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(ss + 84), (uint)tris);
            BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(ss + 88), (uint)ofsTri);
            BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(ss + 92), (uint)ofsShader);
            BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(ss + 96), (uint)ofsTc);
            BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(ss + 100), (uint)ofsVert);
            BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(ss + 104), (uint)ofsEnd);
            for (int k = 0; k < surface.Shaders.Length; k++)
            {
                byte[] raw = Encoding.Latin1.GetBytes(surface.Shaders[k]);
                raw.CopyTo(buf.AsSpan(ss + ofsShader + k * VmMd3Model1326.ShaderBytes));
            }

            for (int k = 0; k < surface.Indices.Length; k++)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(ss + ofsTri + k * 4), (uint)surface.Indices[k]);
            }

            for (int k = 0; k < surface.NumVertices; k++)
            {
                WriteF(buf, ss + ofsTc + k * 8, surface.Tc[k].S);
                WriteF(buf, ss + ofsTc + k * 8 + 4, surface.Tc[k].T);
            }

            for (int k = 0; k < surface.Verts.Length; k++)
            {
                int at = ss + ofsVert + k * 8;
                BinaryPrimitives.WriteInt16LittleEndian(buf.AsSpan(at), surface.Verts[k].X);
                BinaryPrimitives.WriteInt16LittleEndian(buf.AsSpan(at + 2), surface.Verts[k].Y);
                BinaryPrimitives.WriteInt16LittleEndian(buf.AsSpan(at + 4), surface.Verts[k].Z);
                BinaryPrimitives.WriteInt16LittleEndian(buf.AsSpan(at + 6), (short)surface.Verts[k].N);
            }

            cursor += ofsEnd;
        }

        return buf;
    }

    static void WriteF(byte[] buf, int offset, float value)
    {
        BinaryPrimitives.WriteSingleLittleEndian(buf.AsSpan(offset), value);
    }
}
