using S = HCDE.Playsim.VmSlopes1399Slice;

namespace HCDE.Playsim.Tests;

public class VmSlopes1399SliceTests
{
    [Fact]
    public void AlignPlaneUsesTheFirstFurthestVertexAndFlipsCeilingNormals()
    {
        var origin = new S.Vertex { X = 0, Y = 0 };
        var far = new S.Vertex { X = 0, Y = 2 };
        var sector = new S.Sector { FloorTexZ = 0, CeilingTexZ = 10 };
        sector.Lines.Add(new S.SectorEdge { V1 = origin, V2 = far });
        var line = new S.MapLine
        {
            V1 = origin,
            DeltaX = 2,
            DeltaY = 0,
            Front = sector,
            Back = new S.Sector { FloorTexZ = 0, CeilingTexZ = 10 },
        };
        S.AlignPlane(sector, line, 0);
        Assert.Equal(0, sector.Floor.X);
        Assert.Equal(0, sector.Floor.Y);
        Assert.Equal(1, sector.Floor.Z);
        Assert.Equal(0, sector.Floor.D);
        Assert.Equal(-1, sector.Floor.NegiC);
        Assert.Equal(0, sector.Ceiling.Z);

        S.AlignPlane(sector, line, 1);
        Assert.Equal(0, sector.Ceiling.X);
        Assert.Equal(0, sector.Ceiling.Y);
        Assert.Equal(-1, sector.Ceiling.Z);
        Assert.Equal(10, sector.Ceiling.D);
        Assert.Equal(1, sector.Ceiling.NegiC);

        sector.Floor.Z = 7;
        S.AlignPlane(sector, line, 2);
        Assert.Equal(7, sector.Floor.Z);
        Assert.Equal(1, sector.Ceiling.Z);

        var tied = new S.Sector { FloorTexZ = 0 };
        var first = new S.Vertex { X = 2, Y = 0 };
        var second = new S.Vertex { X = 0, Y = 2 };
        tied.Lines.Add(new S.SectorEdge { V1 = first, V2 = second });
        var tieLine = new S.MapLine
        {
            V1 = origin,
            DeltaX = 1,
            DeltaY = 1,
            Front = tied,
            Back = new S.Sector { FloorTexZ = 2 },
        };
        S.AlignPlane(tied, tieLine, 0);
        Assert.True(tied.Floor.X > 0);
        Assert.True(tied.Floor.Z > 0);

        var flat = sector.Floor.Z;
        line.Back = null;
        S.AlignPlane(sector, line, 0);
        Assert.Equal(flat, sector.Floor.Z);
        sector.Lines.Clear();
        Assert.Throws<InvalidOperationException>(() => S.AlignPlane(sector, line, 0));
        Assert.Throws<ArgumentNullException>(() => S.AlignPlane(null!, line, 0));
    }

    [Fact]
    public void SetSlopesAndCopySlopesFollowArgsAndThenTheSideCopy()
    {
        var front = Square(0);
        var back = Square(4);
        var align = new S.MapLine
        {
            Special = S.PlaneAlign,
            V1 = new S.Vertex(),
            DeltaX = 2,
            Front = front,
            Back = back,
        };
        align.Args[0] = 1;
        var idle = new S.MapLine { Special = 4 };
        var oneSided = new S.MapLine { Special = S.PlaneAlign, Front = front };
        oneSided.Args[0] = 1;
        var ceilingBack = new S.MapLine
        {
            Special = S.PlaneAlign,
            V1 = new S.Vertex(),
            DeltaX = 2,
            Front = front,
            Back = back,
        };
        ceilingBack.Args[0] = 8;
        S.SetSlopes(new[] { align, idle, oneSided, ceilingBack });
        Assert.Equal(0, align.Special);
        Assert.True(front.Floor.Z > 0);
        Assert.Equal(0, front.Ceiling.Z);
        Assert.Equal(4, idle.Special);
        Assert.Equal(0, oneSided.Special);
        Assert.True(back.Ceiling.Z < 0);
        Assert.Equal(0, back.Floor.Z);

        front.Floor.X = 3;
        back.Floor.X = 9;
        front.Ceiling.Y = 6;
        back.Ceiling.Y = 8;
        var copy = new S.MapLine { Special = S.PlaneCopy, Front = front, Back = back };
        copy.Args[0] = 5;
        copy.Args[2] = 9;
        copy.Args[4] = 1;
        var calls = new List<(int Tag, S.Sector Dest, bool Ceiling)>();
        S.CopySlopes(new[] { copy }, (tag, dest, ceiling) =>
        {
            calls.Add((tag, dest, ceiling));
            if (tag == 5)
                dest.Floor.X = 11;
            if (tag == 9)
                dest.Floor.X = 13;
        });
        Assert.Equal(0, copy.Special);
        Assert.Equal(2, calls.Count);
        Assert.Equal((5, front, false), calls[0]);
        Assert.Equal((9, back, false), calls[1]);
        Assert.Equal(11, back.Floor.X);
        Assert.Equal(8, back.Ceiling.Y);

        var open = new S.MapLine { Special = S.PlaneCopy, Front = front };
        open.Args[0] = 3;
        open.Args[1] = 4;
        open.Args[4] = 1;
        calls.Clear();
        S.CopySlopes(new[] { open }, (tag, dest, ceiling) => calls.Add((tag, dest, ceiling)));
        Assert.Equal(new[] { (3, front, false), (4, front, true) }, calls);

        front.Ceiling.Y = 1;
        back.Ceiling.Y = 2;
        var lift = new S.MapLine { Special = S.PlaneCopy, Front = front, Back = back };
        lift.Args[4] = 8;
        S.CopySlopes(new[] { lift }, (_, _, _) => { });
        Assert.Equal(2, front.Ceiling.Y);
        Assert.Equal(2, back.Ceiling.Y);
        Assert.Throws<ArgumentNullException>(() => S.CopySlopes(new[] { lift }, null!));
        Assert.Throws<ArgumentNullException>(() => S.SetSlopes(null!));
    }

    private static S.Sector Square(double floorZ)
    {
        var sector = new S.Sector { FloorTexZ = floorZ, CeilingTexZ = floorZ + 16 };
        sector.Lines.Add(new S.SectorEdge
        {
            V1 = new S.Vertex(),
            V2 = new S.Vertex { Y = 2 },
        });
        return sector;
    }
}
