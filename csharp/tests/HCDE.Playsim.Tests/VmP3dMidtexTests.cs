namespace HCDE.Playsim.Tests;

public class VmP3dMidtexTests
{
    [Fact]
    public void Scroll3dMidtex_OffsetsSidesAndChangesSectors()
    {
        var line = new VmP3dMidtex.LineStub { Index = 0, FrontSector = new(), BackSector = new(), Flags = VmP3dMidtex.Ml3dMidtex };
        var sector = new VmP3dMidtex.SectorStub { Index = 0 };
        sector.FloorMidtex.AttachedLines.Add(line);
        sector.FloorMidtex.AttachedSectors.Add(sector);
        var changed = 0;
        var ok = VmP3dMidtex.Scroll3dMidtex(sector, 0, 4, false, false, new VmP3dMidtex.ScrollContext
        {
            ChangeSector = (_, _, _, _, _, _) => { changed++; return false; },
        });
        Assert.True(ok);
        Assert.Equal(4, line.Sides[0].MidTextureYOffset);
        Assert.Equal(1, changed);
    }

    [Fact]
    public void Attach3dMidtexLinesToSector_ByLineId()
    {
        var sec = new VmP3dMidtex.SectorStub { Index = 0 };
        var line = new VmP3dMidtex.LineStub
        {
            Index = 1,
            FrontSector = sec,
            BackSector = new VmP3dMidtex.SectorStub { Index = 2 },
            Flags = VmP3dMidtex.Ml3dMidtex,
        };
        VmP3dMidtex.Attach3dMidtexLinesToSector(sec, 5, 0, false, new VmP3dMidtex.AttachContext
        {
            AllLines = new[] { new VmP3dMidtex.LineStub { Index = 0 }, line },
            AllSectors = new[] { sec, line.BackSector! },
            LineIdsByIterator = id => id == 5 ? new[] { 1 } : Array.Empty<int>(),
            SectorIndicesByTag = _ => Array.Empty<int>(),
            LineHasId = (_, _) => true,
        });
        Assert.Single(sec.FloorMidtex.AttachedLines);
        Assert.Equal(2, sec.FloorMidtex.AttachedSectors.Count);
    }

    [Fact]
    public void Attach3dMidtexLinesToSector_BySectorTag()
    {
        var sec = new VmP3dMidtex.SectorStub { Index = 0 };
        var tagged = new VmP3dMidtex.SectorStub
        {
            Index = 1,
            Lines = new[]
            {
                new VmP3dMidtex.LineStub
                {
                    Index = 4,
                    FrontSector = sec,
                    BackSector = new VmP3dMidtex.SectorStub { Index = 3 },
                    Flags = VmP3dMidtex.Ml3dMidtex,
                },
            },
        };
        VmP3dMidtex.Attach3dMidtexLinesToSector(sec, 0, 7, true, new VmP3dMidtex.AttachContext
        {
            AllLines = tagged.Lines,
            AllSectors = new[] { sec, tagged, tagged.Lines.First().BackSector! },
            LineIdsByIterator = _ => Array.Empty<int>(),
            SectorIndicesByTag = tag => tag == 7 ? new[] { 1 } : Array.Empty<int>(),
            LineHasId = (_, _) => true,
        });
        Assert.Single(sec.CeilingMidtex.AttachedLines);
    }

    [Fact]
    public void GetMidTexturePosition_DontPegBottom()
    {
        var front = new VmP3dMidtex.SectorStub { Index = 0 };
        var back = new VmP3dMidtex.SectorStub { Index = 1 };
        var line = new VmP3dMidtex.LineStub
        {
            FrontSector = front,
            BackSector = back,
            Flags = 1,
        };
        var tex = new VmP3dMidtex.TextureContext
        {
            TextureValid = _ => true,
            TexCoordInfo = (_, _, _) => (0, 64),
            PlaneTexZ = (_, plane) => plane == 0 ? 10 : 100,
            MlDontpegbottom = 1,
        };
        Assert.True(VmP3dMidtex.GetMidTexturePosition(line, 0, tex, out var top, out var bottom));
        Assert.Equal(74, top);
        Assert.Equal(10, bottom);
    }

    [Fact]
    public void LineOpening3dMidtex_AdjustsCeilingWhenBelowMidpoint()
    {
        var line = new VmP3dMidtex.LineStub
        {
            FrontSector = new(),
            BackSector = new(),
            Flags = VmP3dMidtex.Ml3dMidtex,
        };
        var open = new VmP3dMidtex.LineOpening { Top = 200, Bottom = 0 };
        var tex = new VmP3dMidtex.TextureContext
        {
            TextureValid = _ => true,
            TexCoordInfo = (_, _, _) => (0, 40),
            PlaneTexZ = (_, plane) => plane == 1 ? 120 : 50,
            MlDontpegbottom = 0,
        };
        VmP3dMidtex.LineOpening3dMidtex(
            new VmP3dMidtex.ThingStub { CenterZ = 40, Z = 40, MaxStepHeight = 24 },
            line,
            open,
            false,
            tex);
        Assert.True(open.Top < 200);
    }
}
