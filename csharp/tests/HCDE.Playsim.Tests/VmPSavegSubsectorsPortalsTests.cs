namespace HCDE.Playsim.Tests;

public class VmPSavegSubsectorsPortalsTests
{
    [Fact]
    public void RecalculateDrawnSubsectors_SetsFlagWhenLinedefMapped()
    {
        var level = new VmPSavegSubsectorsPortals.LevelLocals();
        var sub = new VmPSavegSubsectorsPortals.Subsector { NumLines = 1 };
        sub.FirstLine.Add(new VmPSavegSubsectorsPortals.SubsectorSeg
        {
            Linedef = new VmPSavegSubsectorsPortals.LineDef { Flags = VmPSavegSubsectorsPortals.MlMapped },
        });
        level.Subsectors.Add(sub);

        VmPSavegSubsectorsPortals.RecalculateDrawnSubsectors(level);

        Assert.Equal(VmPSavegSubsectorsPortals.SsecmfDrawn, sub.Flags);
    }

    [Fact]
    public void SerializeSubsectors_WriteThenRead_RestoresDrawnFlags()
    {
        var level = new VmPSavegSubsectorsPortals.LevelLocals();
        for (var i = 0; i < 7; i++)
        {
            level.Subsectors.Add(new VmPSavegSubsectorsPortals.Subsector
            {
                Flags = i % 2 == 0 ? VmPSavegSubsectorsPortals.SsecmfDrawn : 0,
            });
        }

        for (var i = 0; i < 10; i++)
            level.Vertexes.Add(new object());

        var writeArc = new VmPSavegSubsectorsPortals.SavegArchive { Writing = true };
        VmPSavegSubsectorsPortals.SerializeSubsectors(level, writeArc, "subsectors");

        foreach (var sub in level.Subsectors)
            sub.Flags = 0;

        var readArc = new VmPSavegSubsectorsPortals.SavegArchive
        {
            Writing = false,
            ArrayVertexCount = writeArc.ArrayVertexCount,
            ArraySubsectorCount = writeArc.ArraySubsectorCount,
            ArrayDrawnEncoding = writeArc.ArrayDrawnEncoding,
        };
        VmPSavegSubsectorsPortals.SerializeSubsectors(level, readArc, "subsectors");

        Assert.Equal(VmPSavegSubsectorsPortals.SsecmfDrawn, level.Subsectors[0].Flags);
        Assert.Equal(0u, level.Subsectors[1].Flags);
    }

    [Fact]
    public void SerializeSubsectors_InvalidEncoding_RecalculatesFromMappedLines()
    {
        var level = new VmPSavegSubsectorsPortals.LevelLocals();
        var sub = new VmPSavegSubsectorsPortals.Subsector { NumLines = 1 };
        sub.FirstLine.Add(new VmPSavegSubsectorsPortals.SubsectorSeg
        {
            Linedef = new VmPSavegSubsectorsPortals.LineDef { Flags = VmPSavegSubsectorsPortals.MlMapped },
        });
        level.Subsectors.Add(sub);
        level.Vertexes.Add(new object());

        var readArc = new VmPSavegSubsectorsPortals.SavegArchive
        {
            Writing = false,
            ArrayVertexCount = 1,
            ArraySubsectorCount = 1,
            ArrayDrawnEncoding = "!",
        };
        VmPSavegSubsectorsPortals.SerializeSubsectors(level, readArc, "subsectors");

        Assert.Equal(VmPSavegSubsectorsPortals.SsecmfDrawn, sub.Flags);
    }

    [Fact]
    public void SerializeLinePortal_Read_LoadsSeededFields()
    {
        var readArc = new VmPSavegSubsectorsPortals.SavegArchive { Writing = false };
        readArc.SeedField("origin", 1);
        readArc.SeedField("destination", 2);
        readArc.SeedField("displacement", (3.0, 4.0));
        readArc.SeedField("type", 5);
        readArc.SeedField("flags", 6u);
        readArc.SeedField("defflags", 7u);
        readArc.SeedField("align", 8);

        var readPort = new VmPSavegSubsectorsPortals.LinePortal();
        VmPSavegSubsectorsPortals.SerializeLinePortal(readArc, "lp", readPort);

        Assert.Equal(1, readPort.Origin);
        Assert.Equal(8, readPort.Align);
    }

    [Fact]
    public void SerializePolyObj_OnRead_AppliesRotateAndMoveWhenOriginalPtsExist()
    {
        var poly = new VmPSavegSubsectorsPortals.PolyObj
        {
            StartX = 10,
            StartY = 20,
            OriginalPtsCount = 1,
        };
        var readArc = new VmPSavegSubsectorsPortals.SavegArchive { Writing = false };
        readArc.SeedField("angle", 90.0);
        readArc.SeedField("pos", (15.0, 25.0));
        readArc.SeedField("interpolation", 0);
        readArc.SeedField("blocked", false);
        readArc.SeedField("hasportals", false);
        readArc.SeedField("specialdata", 0);
        readArc.SeedField("level", 0);

        VmPSavegSubsectorsPortals.SerializePolyObj(readArc, "poly", poly);

        Assert.Equal(1, poly.RotateCalls);
        Assert.Equal(1, poly.MoveCalls);
        Assert.Equal(15.0, poly.StartX);
        Assert.Equal(25.0, poly.StartY);
    }

    [Fact]
    public void SerializeReverbContainer_Read_ResolvesEnvironmentById()
    {
        VmPSavegSubsectorsPortals.EnvironmentCatalog.Clear();
        VmPSavegSubsectorsPortals.EnvironmentCatalog[42] = new VmPSavegSubsectorsPortals.ReverbContainer { Id = 42 };
        VmPSavegSubsectorsPortals.ReverbContainer? container = null;
        var readArc = new VmPSavegSubsectorsPortals.SavegArchive { Writing = false };
        readArc.SeedField("reverb", 42);
        VmPSavegSubsectorsPortals.SerializeReverbContainer(readArc, "reverb", ref container);

        Assert.NotNull(container);
        Assert.Equal(42, container!.Id);
    }
}
