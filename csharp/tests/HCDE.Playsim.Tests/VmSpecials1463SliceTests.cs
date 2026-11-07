using Xunit;

namespace HCDE.Playsim.Tests;

public class VmSpecials1463SliceTests
{
    static VmSpecials1463Slice.Sector Sec(int tag = 0) => new() { Tag = tag };

    static VmSpecials1463Slice.Line Ln(VmSpecials1463Slice.Sector front, int special, int[] args, double dx = 0, double dy = 0, int id = 0)
    {
        var full = new int[5];
        for (int i = 0; i < args.Length && i < 5; i++)
            full[i] = args[i];
        return new VmSpecials1463Slice.Line
        {
            Front = front,
            Special = special,
            Id = id,
            Dx = dx,
            Dy = dy,
            Args = full,
            Side = new VmSpecials1463Slice.Side { Sector = front },
        };
    }

    [Fact]
    public void CeilingCopiesDisplacementAndFloorCarryOnly()
    {
        var control = Sec(5);
        var target = Sec(7);
        var plain = Sec();
        var world = new VmSpecials1463Slice.World { Sectors = { control, target, plain } };
        world.Lines.Add(Ln(control, VmSpecials1463Slice.CopyScroller, new[] { 5, 1 }));
        world.Lines.Add(Ln(plain, VmSpecials1463Slice.CopyScroller, new[] { 7, 1 }));
        world.Lines.Add(Ln(control, VmSpecials1463Slice.ScrollCeiling, new[] { 7, 6 }, 64, 32));
        world.Lines.Add(Ln(control, 62, new[] { 1, 2, 3 }));
        world.Lines.Add(Ln(control, VmSpecials1463Slice.ScrollFloor, new[] { 7, 0, 1, 160, 128 }));
        VmSpecials1463Slice.Spawn(world);

        Assert.Equal(0, world.Lines[0].Special);
        Assert.Equal(0, world.Lines[1].Special);
        Assert.Equal(0, world.Lines[2].Special);
        Assert.Equal(62, world.Lines[3].Special);
        Assert.Empty(world.Warnings);
        Assert.Equal(3, world.Scrollers.Count);
        var ceiling = world.Scrollers[0];
        Assert.Equal(VmSpecials1463Slice.Ceiling, ceiling.Type);
        Assert.Equal(-2, ceiling.X);
        Assert.Equal(1, ceiling.Y);
        Assert.Same(control, ceiling.Control);
        Assert.Same(target, ceiling.Sector);
        Assert.Equal(1, ceiling.Accel);
        Assert.Equal(7, ceiling.Parts);
        Assert.Same(plain, world.Scrollers[1].Sector);
        Assert.Equal(VmSpecials1463Slice.Carry, world.Scrollers[2].Type);
        Assert.Equal(1, world.Scrollers[2].X);
        Assert.Equal(0, world.Scrollers[2].Y);
        Assert.Null(world.Scrollers[2].Control);
        Assert.Same(target, world.Scrollers[2].Sector);
        Assert.DoesNotContain(world.Scrollers, s => s.Sector == control);
    }

    [Fact]
    public void ModelOffsetsCardinalAndBoth()
    {
        var room = Sec(1);
        var world = new VmSpecials1463Slice.World { Sectors = { room } };
        var model = Ln(room, VmSpecials1463Slice.ScrollModel, new[] { 4, 4, 9 }, 32, 64, 4);
        var other = Ln(room, 0, Array.Empty<int>(), id: 4);
        other.Side.MidX = 8;
        other.Side.MidY = -12;
        var offsets = Ln(room, VmSpecials1463Slice.ScrollOffsets, new[] { 2, 0, 2, 4 });
        offsets.Side.MidX = 8;
        offsets.Side.MidY = -12;
        var left = Ln(room, VmSpecials1463Slice.ScrollLeft, new[] { 128, 8 });
        var both = Ln(room, VmSpecials1463Slice.ScrollBoth, new[] { 0, 128, 64, 32, 96 });
        var skipped = Ln(room, VmSpecials1463Slice.ScrollBoth, new[] { 3 });
        world.Lines.AddRange(new[] { model, other, offsets, left, both, skipped });
        VmSpecials1463Slice.Spawn(world);

        Assert.Equal(
            "Line 0 (type 222:Scroll_Texture_Model), arg 3 is 9 (should be 0)\n",
            Assert.Single(world.Warnings));
        Assert.Equal(0, model.Special);
        Assert.Equal(VmSpecials1463Slice.ScrollLeft, left.Special);
        Assert.Equal(0, both.Special);
        Assert.Equal(0, skipped.Special);
        Assert.Equal(4, world.Scrollers.Count);
        var wall = world.Scrollers[0];
        Assert.Equal("wall", wall.Form);
        Assert.Equal(1, wall.X);
        Assert.Equal(2, wall.Y);
        Assert.Equal(1, wall.WallLine);
        Assert.Null(wall.Control);
        Assert.Equal(0, wall.Accel);
        var off = world.Scrollers[1];
        Assert.Equal(VmSpecials1463Slice.ScSide, off.Type);
        Assert.Equal(-2, off.X);
        Assert.Equal(-3, off.Y);
        Assert.Same(room, off.Control);
        Assert.Equal(1, off.Accel);
        Assert.Equal(2, off.Parts);
        Assert.Equal(2, off.SideLine);
        Assert.Equal(2, world.Scrollers[2].X);
        Assert.Equal(0, world.Scrollers[2].Y);
        Assert.Equal(7, world.Scrollers[2].Parts);
        Assert.Equal(3, world.Scrollers[2].SideLine);
        Assert.Equal(1, world.Scrollers[3].X);
        Assert.Equal(1, world.Scrollers[3].Y);
        Assert.Equal(7, world.Scrollers[3].Parts);
        Assert.Equal(4, world.Scrollers[3].SideLine);
    }
}
