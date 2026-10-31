namespace HCDE.Playsim.Tests;

public class VmVoxelModel1324SliceTests
{
    [Fact]
    public void Palette_ExpandsLuminanceAndCopiesTheBitmap()
    {
        var host = new VmVoxelModel1324();
        Assert.Equal(1, host.Pick(10, 20, 30));

        var match = new VmVoxelModel1324.PalEntry[256];
        for (int i = 0; i < 256; i++)
        {
            match[i] = new VmVoxelModel1324.PalEntry(255, 255, 255, 255);
        }

        match[0] = new VmVoxelModel1324.PalEntry(0, 0, 0, 255);
        match[254] = new VmVoxelModel1324.PalEntry(1, 2, 3, 255);
        match[255] = new VmVoxelModel1324.PalEntry(0, 0, 0, 255);
        host.MatchPalette = match;
        Assert.Equal(254, host.Pick(0, 0, 0));
        Assert.Equal(254, host.Pick(1, 2, 3));

        host.IndexMap = new byte[256];
        host.IndexMap[0] = 255;
        Assert.Equal(255, host.Pick(0, 0, 0));

        var vox = new VmVoxelModel1324.Voxel();
        var pal = new byte[768];
        pal[3] = 0x10;
        pal[48] = 0x3F;
        vox.Palette = pal;
        var tex = new VmVoxelModel1324.VoxelTexture(vox, host);
        byte[] pix = tex.CreatePalettedPixels(VmVoxelModel1324.Luminance, 0);
        Assert.Equal(255, pix[1]);
        Assert.Equal(65, pix[16]);
        Assert.Equal(0, pix[0]);

        vox.Palette = null;
        pix = tex.CreatePalettedPixels(VmVoxelModel1324.Normal, 0);
        Assert.Equal(16, pix[1]);
        Assert.Equal(1, pix[16]);

        host.MatchPalette = null;
        host.IndexMap = null;
        vox.Palette = pal;
        pix = tex.CreatePalettedPixels(VmVoxelModel1324.Normal, 4);
        Assert.Equal(1, pix[0]);
        Assert.Equal(1, pix[100]);

        host.BaseColors[1] = new VmVoxelModel1324.PalEntry(9, 8, 7, 0);
        vox.Palette = null;
        var bmp = new VmVoxelModel1324.Bitmap(16, 16);
        Array.Fill(bmp.Data, (byte)0xAB);
        Assert.Equal(0, tex.CopyPixels(bmp, 0, 0));
        Assert.Equal(7, bmp.Data[4]);
        Assert.Equal(8, bmp.Data[5]);
        Assert.Equal(9, bmp.Data[6]);
        Assert.Equal(255, bmp.Data[7]);

        vox.Palette = pal;
        bmp = new VmVoxelModel1324.Bitmap(16, 16);
        Assert.Equal(0, tex.CopyPixels(bmp, 1, 2));
        Assert.Equal(0, bmp.Data[4]);
        Assert.Equal(0, bmp.Data[5]);
        Assert.Equal(65, bmp.Data[6]);
        Assert.Equal(255, bmp.Data[7]);

        var owned = new VmVoxelModel1324.Voxel();
        var model = new VmVoxelModel1324.Model(owned, true, host);
        Assert.Equal(VmVoxelModel1324.TextureOverride, host.LastTextureType);
        Assert.True(host.LastTextureNameWasNull);
        Assert.Equal(16, model.PaletteImage.Width);
        Assert.Equal(16, model.PaletteImage.Height);
        Assert.Equal(1, model.PaletteId);
        Assert.False(model.Load("kvx", 3, "buf", 4));
        Assert.Equal(VmVoxelModel1324.ErrVoxel, model.FindFrame("frame", true));
        Assert.Equal(0, model.FindFrame("frame", false));
        Assert.Equal(1.25f, model.GetAspectFactor(1.25f));
        var hit = new byte[8];
        hit[1] = 1;
        model.AddSkins(hit);
        Assert.Equal(3, hit[1]);
        model.Destroy();
        Assert.True(owned.Deleted);

        var kept = new VmVoxelModel1324.Voxel();
        var other = new VmVoxelModel1324.Model(kept, false, host);
        other.Destroy();
        Assert.False(kept.Deleted);
    }

    [Fact]
    public void Slab_WeldsTheSharedEdgeAndSplitsTheColorRun()
    {
        var host = new VmVoxelModel1324();
        var vox = new VmVoxelModel1324.Voxel();
        VmVoxelModel1324.Mip mip = vox.Mips[0];
        mip.SizeX = 1;
        mip.SizeY = 1;
        mip.PivotX = 1;
        mip.PivotY = 2;
        mip.PivotZ = 3;
        mip.OffsetX = new[] { 0 };
        mip.OffsetXY = new short[] { 0, 4 };
        mip.SlabData = new byte[] { 2, 1, 17, 7 };

        var model = new VmVoxelModel1324.Model(vox, false, host);
        var renderer = new VmVoxelModel1324.Renderer(4);
        model.BuildVertexBuffer(renderer);
        VmVoxelModel1324.VertexBuffer buf = model.GetVertexBuffer(4)!;
        Assert.Equal(1, renderer.CreateCount);
        Assert.True(renderer.CreatedWithVertices);
        Assert.True(renderer.CreatedWithIndices);
        Assert.True(buf.VerticesUnlocked);
        Assert.True(buf.IndicesUnlocked);
        Assert.Equal(6, buf.Vertices.Length);
        Assert.Equal(12, model.NumIndices);
        Assert.Empty(model.Vertices);
        Assert.Empty(model.Indices);
        Assert.Equal(new[] { 0, 1, 3, 1, 2, 3, 0, 3, 5, 3, 4, 5 }, buf.Indices);

        float u = 7.5f / 16f;
        float v = 0.5f / 16f;
        AssertVertex(buf.Vertices[0], -1f, 1f, 2f, u, v);
        AssertVertex(buf.Vertices[2], 0f, 1f, 1f, u, v);
        AssertVertex(buf.Vertices[4], -1f, 0f, 1f, u, v);
        AssertVertex(buf.Vertices[5], -1f, 0f, 2f, u, v);
        Assert.Equal(0u, buf.Vertices[0].PackedNormal);

        model.BuildVertexBuffer(renderer);
        Assert.Equal(1, renderer.CreateCount);

        model.RenderFrame(renderer, model.PaletteImage, 9, 8, 0.5, 44, 3);
        Assert.Same(model.PaletteImage, renderer.MaterialSkin);
        Assert.True(renderer.MaterialClamp);
        Assert.Equal(44, renderer.MaterialTranslation);
        Assert.Same(model, renderer.FrameModel);
        Assert.Equal(0, renderer.FrameA);
        Assert.Equal(0, renderer.FrameB);
        Assert.Equal(0, renderer.FrameC);
        Assert.Equal(-1, renderer.FrameD);
        Assert.Equal(12, renderer.DrawCount);
        Assert.Equal(0, renderer.DrawStart);

        var split = new VmVoxelModel1324.Voxel();
        VmVoxelModel1324.Mip splitMip = split.Mips[0];
        splitMip.SizeX = 1;
        splitMip.SizeY = 1;
        splitMip.OffsetX = new[] { 0 };
        splitMip.OffsetXY = new short[] { 0, 5 };
        splitMip.SlabData = new byte[] { 0, 2, 63, 7, 9 };
        var splitModel = new VmVoxelModel1324.Model(split, false, host);
        var splitRenderer = new VmVoxelModel1324.Renderer(1);
        splitModel.BuildVertexBuffer(splitRenderer);
        VmVoxelModel1324.Vertex[] verts = splitModel.GetVertexBuffer(1)!.Vertices;
        Assert.Contains(verts, vert => vert.U == 9.5f / 16f && vert.Y == -1f);
        Assert.Contains(verts, vert => vert.U == 7.5f / 16f && vert.Y == 0f);

        var stacked = new VmVoxelModel1324.Voxel();
        VmVoxelModel1324.Mip stackedMip = stacked.Mips[0];
        stackedMip.SizeX = 1;
        stackedMip.SizeY = 1;
        stackedMip.OffsetX = new[] { 0 };
        stackedMip.OffsetXY = new short[] { 0, 8 };
        stackedMip.SlabData = new byte[] { 0, 1, 16, 1, 5, 1, 16, 2 };
        var stackedModel = new VmVoxelModel1324.Model(stacked, false, host);
        var stackedRenderer = new VmVoxelModel1324.Renderer(2);
        stackedModel.BuildVertexBuffer(stackedRenderer);
        VmVoxelModel1324.VertexBuffer stackedBuf = stackedModel.GetVertexBuffer(2)!;
        Assert.Equal(8, stackedBuf.Vertices.Length);
        Assert.Equal(12, stackedBuf.Indices.Length);
        Assert.Contains(stackedBuf.Vertices, vert => vert.U == 2.5f / 16f && vert.Y == -5f);
    }

    static void AssertVertex(VmVoxelModel1324.Vertex vert, float x, float y, float z, float u, float v)
    {
        Assert.Equal(x, vert.X);
        Assert.Equal(y, vert.Y);
        Assert.Equal(z, vert.Z);
        Assert.Equal(u, vert.U);
        Assert.Equal(v, vert.V);
    }
}
