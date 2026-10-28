namespace HCDE.Playsim.Tests;

public class VmPSectorSerializeAlignTests
{
    [Fact]
    public void SerializeSectorSpecial_RoundTripsFields()
    {
        var spec = new VmPSectorSerializeAlign.SectorSpecial
        {
            Special = 7,
            DamageAmount = 5,
            DamageType = 2,
            DamageInterval = 32,
            LeakyDamage = 1,
            Flags = 0x10000,
        };
        var arc = new VmPSectorSerializeAlign.SerializerSink();
        VmPSectorSerializeAlign.SerializeSectorSpecial(arc, "spec", spec);
        var read = VmPSectorSerializeAlign.ReadSectorSpecial(arc);
        Assert.Equal(7, read.Special);
        Assert.Equal(5, read.DamageAmount);
        Assert.Equal(0x10000u, read.Flags);
    }

    [Fact]
    public void CopyPlaneIfValid_CopiesWhenNormalsDiffer()
    {
        var src = new VmPSectorSerializeAlign.SecPlane { D = 10, Normal = () => (0, 0, 1) };
        var dest = new VmPSectorSerializeAlign.SecPlane { D = 0, Normal = () => (0, 1, 0) };
        var opp = new VmPSectorSerializeAlign.SecPlane { D = -5, Normal = () => (0, -1, 0) };
        Assert.True(VmPSectorSerializeAlign.CopyPlaneIfValid(src, dest, opp));
        Assert.Equal(10, dest.D);
    }

    [Fact]
    public void AlignFlat_SetsSectorBaseFromFrontSide()
    {
        var level = new VmPSectorSerializeAlign.LevelLocals();
        var sec = new VmPSectorSerializeAlign.MapSector();
        var line = new VmPSectorSerializeAlign.MapLine { FrontSector = sec };
        line.V1.X = 0;
        line.V1.Y = 0;
        line.V2.X = 64;
        line.V2.Y = 0;
        level.Lines.Add(line);
        line.V2.X = 64;
        line.V2.Y = 64;
        Assert.True(VmPSectorSerializeAlign.AlignFlat(level, 0, 0, 0));
        Assert.NotEqual(0, sec.BaseAngleRad);
    }

    [Fact]
    public void ReplaceTextures_SwapsMatchingWallTier()
    {
        var level = new VmPSectorSerializeAlign.LevelLocals();
        var side = new VmPSectorSerializeAlign.MapSide();
        side.SetTexture(0, 10);
        level.Sides.Add(side);
        VmPSectorSerializeAlign.ReplaceTextures(
            level,
            "OLD",
            "NEW",
            0,
            new VmPSectorSerializeAlign.TextureLookup
            {
                ResolveWall = n => n == "OLD" ? 10 : 20,
            });
        Assert.Equal(20, side.GetTexture(0));
    }

    [Fact]
    public void AdjustLine_FillsBoundingBox()
    {
        var line = new VmPSectorSerializeAlign.MapLine();
        line.V1.X = 10;
        line.V1.Y = 5;
        line.V2.X = 0;
        line.V2.Y = 15;
        VmPSectorSerializeAlign.AdjustLine(line);
        Assert.Equal(0, line.Bbox[VmPSectorSerializeAlign.MapLine.BoxLeft]);
        Assert.Equal(10, line.Bbox[VmPSectorSerializeAlign.MapLine.BoxRight]);
    }

    [Fact]
    public void GetLightLevel_AppliesHorizFakeContrast()
    {
        var level = new VmPSectorSerializeAlign.LevelLocals { WallHorizLight = 8, FakeContrast = 1 };
        var side = new VmPSectorSerializeAlign.SideHost
        {
            LineDef = new VmPSectorSerializeAlign.MapLine(),
            Sector = new VmPSectorSerializeAlign.MapSector(),
        };
        side.LineDef.V1.X = 0;
        side.LineDef.V2.X = 64;
        var lit = VmPSectorSerializeAlign.GetLightLevel(side, foggy: false, 100, 0, false, level, false, out _);
        Assert.Equal(108, lit);
    }

    [Fact]
    public void RecalcVertexHeights_SortsDistinctHeights()
    {
        var sec = new VmPSectorSerializeAlign.MapSector();
        sec.CeilingPlaneZ = _ => 128;
        sec.FloorPlaneZ = _ => 0;
        var sec2 = new VmPSectorSerializeAlign.MapSector();
        sec2.CeilingPlaneZ = _ => 96;
        sec2.FloorPlaneZ = _ => 32;
        var vtx = new VmPSectorSerializeAlign.VertexHeights
        {
            NumSectors = 2,
            Sectors = [sec, sec2],
        };
        VmPSectorSerializeAlign.RecalcVertexHeights(vtx, new VmPSectorSerializeAlign.Vertex());
        Assert.Equal(4, vtx.NumHeights);
        Assert.Equal(0f, vtx.HeightList[0]);
        Assert.Equal(32f, vtx.HeightList[1]);
        Assert.Equal(96f, vtx.HeightList[2]);
        Assert.Equal(128f, vtx.HeightList[3]);
    }

    [Fact]
    public void BuildPolyBsp_InvokesNodeBuilderHooks()
    {
        var calls = new List<string>();
        var sub = new VmPSectorSerializeAlign.Subsector
        {
            Sector = new VmPSectorSerializeAlign.MapSector(),
            Bsp = new VmPSectorSerializeAlign.MiniBsp { Dirty = true },
            Polys = new VmPSectorSerializeAlign.PolyNode(),
        };
        VmPSectorSerializeAlign.BuildPolyBsp(
            sub,
            new VmPSectorSerializeAlign.LevelLocals(),
            new VmPSectorSerializeAlign.PolyBspContext
            {
                Clear = () => calls.Add("clear"),
                AddSegs = (a, b) => calls.Add($"segs:{a},{b}"),
                AddPolySegs = _ => calls.Add("poly"),
                BuildMini = _ => calls.Add("build"),
                ExtractMini = _ => calls.Add("extract"),
            });
        Assert.Contains("clear", calls);
        Assert.Contains("poly", calls);
    }
}
