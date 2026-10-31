using System.Buffers.Binary;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmUe1Model1339SliceTests
{
    [Fact]
    public void NamesUnpackAndPackedFrame()
    {
        Assert.Equal(0.25f, VmUe1Model1339.UnpackUvert(1, 0));
        Assert.Equal(0f, VmUe1Model1339.UnpackUvert(1, 1));
        Assert.Equal(0f, VmUe1Model1339.UnpackUvert(1, 2));
        Assert.Equal(0f, VmUe1Model1339.UnpackUvert(1, 3));
        Assert.Equal(-256f, VmUe1Model1339.UnpackUvert(0x400, 0));
        Assert.Equal(0.25f, VmUe1Model1339.UnpackUvert(0x800, 1));
        Assert.Equal(-256f, VmUe1Model1339.UnpackUvert(1u << 31, 2));

        AssertPair("ab_d.3d", 7, true, "ab_a.3d", 3);
        AssertPair("xx_d.3dx", 7, false, "xx_d.3dx", 4);
        AssertPair("a_a.3d_a.3d", 7, false, "a_d.3d_d.3d", 5);
        AssertPair("abcd", 7, true, "abcd", 6);
        AssertPair("abc", 7, false, "abc", 8);
        AssertPair("a_d.3d_d.3d", 7, false, "a_d.3d_d.3d", 9);

        byte[] data = DataHeader(1, 3);
        PutPoly(data, 0, 0, 1, 2, 0, 4, new byte[] { 255, 0, 0, 255, 128, 64 });
        byte[] anim = new byte[4 + 12];
        BinaryPrimitives.WriteUInt16LittleEndian(anim.AsSpan(0), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(anim.AsSpan(2), 12);
        BinaryPrimitives.WriteUInt32LittleEndian(anim.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(anim.AsSpan(8), 0x800);
        BinaryPrimitives.WriteUInt32LittleEndian(anim.AsSpan(12), 1u << 31);
        var model = new VmUe1Model1339();
        model.LumpNames[2] = "gun_d.3d";
        model.NameToLump["gun_a.3d"] = 3;
        model.Lumps[2] = data;
        model.Lumps[3] = anim;
        Assert.True(model.Load("ignored_a.3d", 2, new byte[] { 9 }, 99));
        Assert.True(model.HasSurfaces);
        Assert.Equal(2, model.DataLump);
        Assert.Equal(3, model.AnivLump);
        Assert.Equal("gun_a.3d", model.CheckedNames[0]);
        model.LoadGeometry();
        Assert.Equal(1, model.GeometryReads);
        Assert.Equal(3, model.NumVerts);
        Assert.Equal(1, model.NumFrames);
        Assert.Equal(1, model.NumPolys);
        Assert.Equal(0.25f, model.Verts[0].X);
        Assert.Equal(0f, model.Verts[0].Y);
        Assert.Equal(0f, model.Verts[0].Z);
        Assert.Equal(0f, model.Verts[1].X);
        Assert.Equal(0f, model.Verts[1].Y);
        Assert.Equal(-0.25f, model.Verts[1].Z);
        Assert.Equal(0f, model.Verts[2].X);
        Assert.Equal(-256f, model.Verts[2].Y);
        Assert.Equal(0f, model.Verts[2].Z);
        Assert.Equal(1f, model.Polys[0].S[0]);
        Assert.Equal(0f, model.Polys[0].T[0]);
        Assert.Equal(0f, model.Polys[0].S[1]);
        Assert.Equal(1f, model.Polys[0].T[1]);
        Assert.Equal(128 / 255f, model.Polys[0].S[2]);
        Assert.Equal(64 / 255f, model.Polys[0].T[2]);
        Assert.Equal(new[] { 0 }, model.Groups[0].P);
        Assert.Equal(4, model.Groups[0].TexNum);
        Assert.Equal(0, model.Groups[0].Type);
        Assert.False(model.HasWeaponTriangle);
        Assert.Equal(0, model.FindFrame("0", true));
        Assert.Equal(0, model.FindFrame("00", false));
        Assert.Equal(0, model.FindFrame("0x", true));
        Assert.Equal(0, model.FindFrame("0x0", false));
        Assert.Equal(0, model.FindFrame("09", true));
        Assert.Equal(0, model.FindFrame("  +0", false));
        Assert.Equal(0, model.FindFrame("-0", true));
        Assert.Equal(0, model.FindFrame("", false));
        Assert.Equal(0, model.FindFrame(null, true));
        Assert.Equal(VmUe1Model1339.ErrNotFound, model.FindFrame("1", true));
        Assert.Equal(VmUe1Model1339.ErrNotFound, model.FindFrame("1", false));
        Assert.Equal(VmUe1Model1339.ErrNotFound, model.FindFrame("010", true));
        Assert.Equal(VmUe1Model1339.ErrNotFound, model.FindFrame("0x1", false));
        Assert.Equal(VmUe1Model1339.ErrNotFound, model.FindFrame("-1", true));
        model.UnloadGeometry();
        Assert.Empty(model.Verts);
        Assert.Empty(model.Polys);
        Assert.Empty(model.Groups[0].P);
        Assert.Equal(1, model.Groups[0].NumPolys);
        Assert.Equal(4, model.Groups[0].TexNum);
        Assert.Equal(1, model.NumGroups);
        var hit = new byte[] { 0, 1, 0 };
        var ids = new VmUe1Model1339.TextureId[5];
        ids[4] = new VmUe1Model1339.TextureId(1, true);
        model.AddSkins(hit, null);
        Assert.Equal(1, hit[1]);
        model.AddSkins(hit, ids);
        Assert.Equal(3, hit[1]);
        model.Destroy();
        Assert.Empty(model.Groups);
        Assert.Equal(1, model.NumGroups);

        var broken = new VmUe1Model1339();
        broken.LumpNames[1] = "zero_d.3d";
        broken.NameToLump["zero_a.3d"] = 2;
        broken.Lumps[1] = DataHeader(1, 0);
        var anim0 = new byte[4];
        BinaryPrimitives.WriteUInt16LittleEndian(anim0.AsSpan(0), 1);
        broken.Lumps[2] = anim0;
        Assert.True(broken.Load("nope", 1, null, 0));
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => broken.LoadGeometry());
        Assert.Contains("numverts", ex.Message);
    }

    [Fact]
    public void DeusExGroupsNormalsAndDraw()
    {
        byte[] data = DataHeader(5, 4);
        PutPoly(data, 0, 0, 1, 2, 0, 1, new byte[] { 255, 0, 0, 255, 128, 64 });
        PutPoly(data, 1, 0, 2, 3, 0, 1, new byte[] { 0, 0, 0, 0, 0, 0 });
        PutPoly(data, 2, 0, 1, 3, 0x28, 2, new byte[] { 255, 255, 255, 255, 255, 255 });
        PutPoly(data, 3, 1, 2, 3, 0, 1, new byte[] { 1, 2, 3, 4, 5, 6 });
        PutPoly(data, 4, 0, 1, 2, 0, 3, new byte[] { 10, 20, 30, 40, 50, 60 });
        (short X, short Y, short Z)[] frame =
        {
            (0, 0, 0),
            (2, 0, 0),
            (0, 0, 2),
            (0, -2, 0),
        };
        (short X, short Y, short Z)[] frame1 =
        {
            (10, 0, 0),
            (2, 0, 0),
            (0, 0, 2),
            (0, -2, 0),
        };
        byte[] anim = DeusAnim(frame.Concat(frame1).ToArray());
        var model = new VmUe1Model1339();
        model.LumpNames[4] = "troop_d.3d";
        model.NameToLump["troop_a.3d"] = 9;
        model.Lumps[4] = data;
        model.Lumps[9] = anim;
        Assert.True(model.Load("other_a.3d", 4, Array.Empty<byte>(), 0));
        Assert.Equal(4, model.DataLump);
        Assert.Equal(9, model.AnivLump);
        model.LoadGeometry();
        Assert.Equal(new[] { 0, 1, 3 }, model.Groups[0].P);
        Assert.Equal(1, model.Groups[0].TexNum);
        Assert.Equal(0, model.Groups[0].Type);
        Assert.Equal(3, model.Groups[0].NumPolys);
        Assert.Equal(new[] { 2 }, model.Groups[1].P);
        Assert.Equal(2, model.Groups[1].TexNum);
        Assert.Equal(0x28, model.Groups[1].Type);
        Assert.Equal(new[] { 4 }, model.Groups[2].P);
        Assert.Equal(3, model.Groups[2].TexNum);
        Assert.True(model.HasWeaponTriangle);
        Assert.Equal(0f, model.Verts[0].X);
        Assert.Equal(0f, model.Verts[2].X);
        Assert.Equal(2f, model.Verts[2].Y);
        Assert.Equal(0f, model.Verts[2].Z);
        Assert.Equal(0f, model.Verts[3].Y);
        Assert.Equal(2f, model.Verts[3].Z);
        Assert.Equal(10f, model.Verts[4].X);
        Assert.Equal(4, model.Verts[0].RefCount);
        Assert.Equal(0f, model.Polys[0].Normals[0].X);
        Assert.Equal(0f, model.Polys[0].Normals[0].Y);
        Assert.Equal(1f, model.Polys[0].Normals[0].Z);
        Assert.Equal(-1f, model.Polys[0].Normals[1].Z);
        Assert.Equal(0f, model.Polys[2].Normals[0].X);
        Assert.Equal(-1f, model.Polys[2].Normals[0].Y);
        Assert.Equal(0f, model.Polys[2].Normals[0].Z);
        Assert.Equal(1f, model.Polys[2].Normals[1].Y);
        float scale = (float)(1.0 / Math.Sqrt(6.0));
        Assert.Equal(1f * scale, model.Verts[0].Nx);
        Assert.Equal(-1f * scale, model.Verts[0].Ny);
        Assert.Equal(2f * scale, model.Verts[0].Nz);
        model.UnloadGeometry();
        Assert.Empty(model.Verts);
        Assert.Empty(model.Groups[0].P);
        Assert.Equal(3, model.Groups[0].NumPolys);
        Assert.Equal(0x28, model.Groups[1].Type);

        var renderer = new VmUe1Model1339.Renderer(5);
        model.BuildVertexBuffer(renderer);
        model.BuildVertexBuffer(renderer);
        Assert.Equal(2, model.GeometryReads);
        Assert.Single(renderer.Created);
        Assert.False(renderer.Created[0].Indexed);
        Assert.False(renderer.Created[0].OneFrame);
        VmUe1Model1339.VertexBuffer buffer = model.GetVertexBuffer(5)!;
        Assert.True(buffer.Unlocked);
        Assert.Equal(30, buffer.Vertices.Length);
        Assert.Equal(0f, buffer.Vertices[0].X);
        Assert.Equal(1f, buffer.Vertices[0].S);
        Assert.Equal(0f, buffer.Vertices[0].T);
        Assert.Equal(1f * scale, buffer.Vertices[0].Nx);
        Assert.Equal(-1f * scale, buffer.Vertices[0].Ny);
        Assert.Equal(2f * scale, buffer.Vertices[0].Nz);
        Assert.Equal(0f, buffer.Vertices[9].Nx);
        Assert.Equal(-1f, buffer.Vertices[9].Ny);
        Assert.Equal(0f, buffer.Vertices[9].Nz);
        Assert.Equal(10f, buffer.Vertices[15].X);
        Assert.Equal(0f, buffer.Vertices[24].Nx);
        Assert.Equal(1f, buffer.Vertices[24].Ny);
        Assert.Equal(0f, buffer.Vertices[24].Nz);
        Assert.Empty(model.Verts);
        Assert.Empty(model.Groups[1].P);
        Assert.Equal(1, model.Groups[1].NumPolys);
        Assert.Equal(3, model.NumGroups);

        var quiet = new VmUe1Model1339.Renderer(5);
        model.RenderFrame(quiet, new VmUe1Model1339.GameTexture(1), -1, 0, 0.5, 1, null, 0);
        model.RenderFrame(quiet, new VmUe1Model1339.GameTexture(1), 0, 2, 0.5, 1, null, 0);
        Assert.Empty(quiet.Interpolations);

        var skin = new VmUe1Model1339.GameTexture(40);
        var draw = new VmUe1Model1339.Renderer(5);
        model.RenderFrame(draw, skin, 0, 1, 0.25, 6, null, 3);
        Assert.Equal(new[] { 0.25, 0d }, draw.Interpolations);
        Assert.Equal(2, draw.Materials.Count);
        Assert.Same(skin, draw.Materials[0].Skin);
        Assert.False(draw.Materials[0].Clamp);
        Assert.Equal(6, draw.Materials[0].Translation);
        Assert.Equal(new VmUe1Model1339.SetupCall(0, 15, 9, -1), draw.FramesSet[0]);
        Assert.Equal(new VmUe1Model1339.SetupCall(12, 27, 3, -1), draw.FramesSet[1]);
        Assert.Equal(new VmUe1Model1339.DrawCall(0, 9), draw.Draws[0]);
        Assert.Equal(new VmUe1Model1339.DrawCall(0, 3), draw.Draws[1]);
        Assert.Empty(draw.Lookups);

        var ids = new VmUe1Model1339.TextureId[4];
        ids[1] = new VmUe1Model1339.TextureId(11, true);
        ids[2] = new VmUe1Model1339.TextureId(12, true);
        ids[3] = new VmUe1Model1339.TextureId(0, false);
        var lookup = new VmUe1Model1339.Renderer(5);
        var resolved = new VmUe1Model1339.GameTexture(11);
        lookup.Textures[11] = resolved;
        model.RenderFrame(lookup, null, 0, 1, 0.5, 2, ids, 0);
        Assert.Single(lookup.Lookups);
        Assert.Equal(11, lookup.Lookups[0].Id.GetIndex());
        Assert.True(lookup.Lookups[0].Animate);
        Assert.Single(lookup.Materials);
        Assert.Same(resolved, lookup.Materials[0].Skin);
        Assert.Equal(new VmUe1Model1339.SetupCall(0, 15, 9, -1), lookup.FramesSet[0]);
        Assert.Single(lookup.Draws);

        Assert.Equal(1, model.FindFrame("0x1", true));
        Assert.Equal(1, model.FindFrame("1", false));
        Assert.Equal(VmUe1Model1339.ErrNotFound, model.FindFrame("2", true));
        Assert.Equal(VmUe1Model1339.ErrNotFound, model.FindFrame("010", false));

        var hit = new byte[8];
        hit[4] = 1;
        var skinIds = new VmUe1Model1339.TextureId[4];
        skinIds[1] = new VmUe1Model1339.TextureId(4, true);
        skinIds[2] = new VmUe1Model1339.TextureId(6, true);
        skinIds[3] = new VmUe1Model1339.TextureId(0, false);
        model.AddSkins(hit, skinIds);
        Assert.Equal(3, hit[4]);
        Assert.Equal(2, hit[6]);
        Assert.Equal(0, hit[0]);
        model.Destroy();
        Assert.Empty(model.Groups);
        Assert.Equal(3, model.NumGroups);
        Assert.True(model.HasWeaponTriangle);
    }

    static void AssertPair(string name, int lump, bool dataIsLump, string checkedName, int other)
    {
        var model = new VmUe1Model1339();
        model.LumpNames[lump] = name;
        model.NameToLump[checkedName] = other;
        Assert.True(model.Load("unused", lump, null, 0));
        Assert.True(model.HasSurfaces);
        Assert.Equal(checkedName, Assert.Single(model.CheckedNames));
        if (dataIsLump)
        {
            Assert.Equal(lump, model.DataLump);
            Assert.Equal(other, model.AnivLump);
        }
        else
        {
            Assert.Equal(other, model.DataLump);
            Assert.Equal(lump, model.AnivLump);
        }
    }

    static byte[] DataHeader(int polys, int verts)
    {
        var bytes = new byte[48 + polys * 16];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0), (ushort)polys);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2), (ushort)verts);
        return bytes;
    }

    static void PutPoly(byte[] data, int index, int v0, int v1, int v2, byte type, byte tex, byte[] uv)
    {
        int at = 48 + index * 16;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(at), (ushort)v0);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(at + 2), (ushort)v1);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(at + 4), (ushort)v2);
        data[at + 6] = type;
        data[at + 7] = 0xEE;
        for (int i = 0; i < 6; i++)
        {
            data[at + 8 + i] = uv[i];
        }

        data[at + 14] = tex;
        data[at + 15] = 0xFF;
    }

    static byte[] DeusAnim((short X, short Y, short Z)[] points)
    {
        var bytes = new byte[4 + points.Length * 8];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(0), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2), 32);
        for (int i = 0; i < points.Length; i++)
        {
            int at = 4 + i * 8;
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(at), points[i].X);
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(at + 2), points[i].Y);
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(at + 4), points[i].Z);
        }

        return bytes;
    }
}
