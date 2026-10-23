namespace HCDE.MapLoader.Tests;

public class VmMapLoaderSlopesTests
{
    [Fact]
    public void SetSlope_SetsFloorNormalFromAngles()
    {
        var level = new VmMapLoaderSlopes.SlopeLevel();
        var plane = new VmMapLoaderSlopes.SecPlane();
        VmMapLoaderSlopes.SetSlope(level, plane, setCeil: false, xyangi: 0, zangi: 45, posX: 0, posY: 0, posZ: 10);
        Assert.True(plane.C > 0);
        Assert.Equal(10, Math.Abs(plane.ZAtPoint(0, 0)), 3);
    }

    [Fact]
    public void SlopeLineToPoint_LogsWhenPointOnLine()
    {
        var level = new VmMapLoaderSlopes.SlopeLevel();
        var sec = new VmMapLoaderSlopes.SlopeSector { FloorTexZ = 0 };
        var line = new VmMapLoaderSlopes.SlopeLine();
        line.V1.Set(0, 0);
        line.V2.Set(100, 0);
        line.FrontSector = sec;
        level.Lines.Add(line);
        level.TagManager.AddLineId(0, 1);
        level.TagManager.HashTags();

        VmMapLoaderSlopes.SlopeLineToPoint(level, 1, 50, 0, 0, slopeCeil: false);
        Assert.Single(level.Log);
    }

    [Fact]
    public void SlopeLineToPoint_TiltsFloorTowardPoint()
    {
        var level = new VmMapLoaderSlopes.SlopeLevel();
        var sec = new VmMapLoaderSlopes.SlopeSector { FloorTexZ = 0 };
        level.Sectors.Add(sec);
        var line = new VmMapLoaderSlopes.SlopeLine();
        line.V1.Set(0, 0);
        line.V2.Set(100, 0);
        line.FrontSector = sec;
        line.BackSector = null;
        level.Lines.Add(line);
        level.TagManager.AddLineId(0, 7);
        level.TagManager.HashTags();

        VmMapLoaderSlopes.SlopeLineToPoint(level, 7, 50, -10, 32, slopeCeil: false);
        Assert.True(Math.Abs(sec.FloorPlane.C) > VmMapLoaderSlopes.EqualEpsilon);
    }

    [Fact]
    public void CopyPlane_CopiesFloorBySectorTag()
    {
        var level = new VmMapLoaderSlopes.SlopeLevel();
        var src = new VmMapLoaderSlopes.SlopeSector();
        src.FloorPlane.Set(0, 0, 1, -64);
        var dest = new VmMapLoaderSlopes.SlopeSector();
        level.Sectors.Add(src);
        level.Sectors.Add(dest);
        level.TagManager.AddSectorTag(0, 5);
        level.TagManager.HashTags();

        VmMapLoaderSlopes.CopyPlane(level, 5, dest, copyCeil: false);
        Assert.Equal(64, Math.Abs(dest.FloorPlane.ZAtPoint(0, 0)), 3);
    }

    [Fact]
    public void VavoomSlope_UsesLineIdInSector()
    {
        var level = new VmMapLoaderSlopes.SlopeLevel();
        var sec = new VmMapLoaderSlopes.SlopeSector { FloorTexZ = 0 };
        var line = new VmMapLoaderSlopes.SlopeLine();
        line.V1.Set(0, 0);
        line.V2.Set(64, 0);
        line.Args[0] = 3;
        sec.Lines.Add(line);
        line.FrontSector = sec;
        level.Sectors.Add(sec);

        VmMapLoaderSlopes.VavoomSlope(level, sec, 3, 32, 32, 16, ceiling: false);
        Assert.True(Math.Abs(sec.FloorPlane.C) > VmMapLoaderSlopes.EqualEpsilon);
    }

    [Fact]
    public void SetSlopesFromVertexHeights_ReadsVertexThingHeights()
    {
        var level = new VmMapLoaderSlopes.SlopeLevel();
        level.Vertices.Add(new VmMapLoaderSlopes.SlopeVertex { X = 10, Y = 10 });
        level.Vertices.Add(new VmMapLoaderSlopes.SlopeVertex { X = 20, Y = 10 });
        level.Vertices.Add(new VmMapLoaderSlopes.SlopeVertex { X = 10, Y = 20 });
        var sec = BuildTriangleSector(level);
        level.Sectors.Add(sec);

        var things = new List<VmMapLoaderSlopes.SlopeMapThing>
        {
            new()
            {
                Info = new VmMapLoaderSlopes.MapThingInfo { Type = null, Special = VmMapLoaderSlopes.SmtVertexFloorZ },
                PosX = 10,
                PosY = 10,
                PosZ = 24
            }
        };
        VmMapLoaderSlopes.SetSlopesFromVertexHeights(level, things, 0, things.Count, null);
        Assert.Equal(0, things[0].EdNum);
        Assert.True(Math.Abs(sec.FloorPlane.C) > VmMapLoaderSlopes.EqualEpsilon);
    }

    [Fact]
    public void SetSlopesFromVertexHeights_SlopesTriangleSectorFloor()
    {
        var level = new VmMapLoaderSlopes.SlopeLevel();
        var v0 = new VmMapLoaderSlopes.SlopeVertex { X = 0, Y = 0 };
        var v1 = new VmMapLoaderSlopes.SlopeVertex { X = 64, Y = 0 };
        var v2 = new VmMapLoaderSlopes.SlopeVertex { X = 0, Y = 64 };
        level.Vertices.AddRange(new[] { v0, v1, v2 });
        var sec = BuildTriangleSector(level, v0, v1, v2);
        level.Sectors.Add(sec);

        level.VertexDatas.Add(new VmMapLoaderSlopes.VertexData
        {
            Flags = VmMapLoaderSlopes.VertexFlagZFloorEnabled,
            ZFloor = 32
        });

        var things = new List<VmMapLoaderSlopes.SlopeMapThing>();
        level.VertexDatas.Add(new VmMapLoaderSlopes.VertexData
        {
            Flags = VmMapLoaderSlopes.VertexFlagZFloorEnabled,
            ZFloor = 8
        });
        level.VertexDatas.Add(new VmMapLoaderSlopes.VertexData
        {
            Flags = VmMapLoaderSlopes.VertexFlagZFloorEnabled,
            ZFloor = 16
        });
        VmMapLoaderSlopes.SetSlopesFromVertexHeights(level, things, 0, 0, oldVertexTable: new[] { 0, 1, 2 });
        Assert.True(Math.Abs(sec.FloorPlane.C) > VmMapLoaderSlopes.EqualEpsilon);
    }

    [Fact]
    public void SpawnSlopeMakers_AppliesSetFloorSlopeThing()
    {
        var level = new VmMapLoaderSlopes.SlopeLevel();
        var sec = new VmMapLoaderSlopes.SlopeSector { FloorTexZ = 0 };
        var line = new VmMapLoaderSlopes.SlopeLine();
        line.V1.Set(0, 0);
        line.V2.Set(128, 0);
        line.FrontSector = sec;
        sec.Lines.Add(line);
        level.Sectors.Add(sec);

        var things = new List<VmMapLoaderSlopes.SlopeMapThing>
        {
            new()
            {
                Info = new VmMapLoaderSlopes.MapThingInfo { Type = null, Special = VmMapLoaderSlopes.SmtSetFloorSlope },
                PosX = 32,
                PosY = 32,
                PosZ = 0,
                Angle = 0,
                Args = { [0] = 45 }
            }
        };

        VmMapLoaderSlopes.SpawnSlopeMakers(level, things, 0, things.Count, null);
        Assert.Equal(0, things[0].EdNum);
        Assert.True(sec.FloorPlane.C > 0);
    }

    static VmMapLoaderSlopes.SlopeSector BuildTriangleSector(
        VmMapLoaderSlopes.SlopeLevel level,
        VmMapLoaderSlopes.SlopeVertex? v0 = null,
        VmMapLoaderSlopes.SlopeVertex? v1 = null,
        VmMapLoaderSlopes.SlopeVertex? v2 = null)
    {
        v0 ??= level.Vertices[0];
        v1 ??= level.Vertices[1];
        v2 ??= level.Vertices[2];
        var sec = new VmMapLoaderSlopes.SlopeSector { FloorTexZ = 0 };
        var l0 = new VmMapLoaderSlopes.SlopeLine();
        l0.V1.Set(v0.X, v0.Y);
        l0.V2.Set(v1.X, v1.Y);
        l0.FrontSector = sec;
        var l1 = new VmMapLoaderSlopes.SlopeLine();
        l1.V1.Set(v1.X, v1.Y);
        l1.V2.Set(v2.X, v2.Y);
        l1.FrontSector = sec;
        var l2 = new VmMapLoaderSlopes.SlopeLine();
        l2.V1.Set(v2.X, v2.Y);
        l2.V2.Set(v0.X, v0.Y);
        l2.FrontSector = sec;
        sec.Lines.AddRange(new[] { l0, l1, l2 });
        return sec;
    }
}
