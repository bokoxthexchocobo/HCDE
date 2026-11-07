using Xunit;

namespace HCDE.Playsim.Tests;

public class VmSpecials1464SliceTests
{
    static VmSpecials1464Slice.Line Ln(VmSpecials1464Slice.Sector front, int special, params int[] args)
    {
        var line = new VmSpecials1464Slice.Line { Front = front, Special = special, Args = new int[5] };
        for (int i = 0; i < args.Length && i < 5; i++)
            line.Args[i] = args[i];
        return line;
    }

    [Fact]
    public void FrictionClampsAndContentsSwapsSolid()
    {
        var room = new VmSpecials1464Slice.Sector { Tag = 1 };
        var other = new VmSpecials1464Slice.Sector { Tag = 4 };
        var world = new VmSpecials1464Slice.World { Sectors = { room, other } };
        var high = Ln(room, VmSpecials1464Slice.SetFriction, 4, 250);
        var delta = Ln(room, VmSpecials1464Slice.SetFriction, 4);
        delta.Delta = 12.9;
        var keep = Ln(room, 62, 1);
        world.Lines.AddRange(new[] { high, delta, keep });
        VmSpecials1464Slice.SpawnFriction(world);
        Assert.Equal(0, high.Special);
        Assert.Equal(0, delta.Special);
        Assert.Equal(62, keep.Special);
        Assert.Equal(200, world.Drags[0].Length);
        Assert.False(world.Drags[0].Affect);
        Assert.Equal(12, world.Drags[1].Length);

        var model = new VmSpecials1464Slice.Sector { Tag = 9 };
        var dest = new VmSpecials1464Slice.Sector { Tag = 2 };
        var contents = Ln(model, VmSpecials1464Slice.SetContents, 1, 50, 1 | 8);
        model.Lines.Add(contents);
        var floor = Ln(model, VmSpecials1464Slice.Set3D, 2);
        var box = new VmSpecials1464Slice.World { Sectors = { model, dest } };
        Assert.Equal(1, VmSpecials1464Slice.Set3DFloor(box, floor, 0, 0, 10));
        var made = Assert.Single(box.Floors);
        Assert.Same(dest, made.Dest);
        Assert.Equal(127, made.Alpha);
        Assert.Equal(0x825032, model.Fade);
        Assert.Equal(0, model.Fog);
        Assert.Equal(0, made.Flags & VmSpecials1464Slice.Solid);
        Assert.NotEqual(0, made.Flags & VmSpecials1464Slice.Flood);
        Assert.NotEqual(0, made.Flags & VmSpecials1464Slice.Additive);
    }

    [Fact]
    public void HexenLineIdLightOnlyAndPlaneFlags()
    {
        var model = new VmSpecials1464Slice.Sector();
        var dest = new VmSpecials1464Slice.Sector { Tag = 3 };
        var untagged = new VmSpecials1464Slice.Sector();
        var world = new VmSpecials1464Slice.World
        {
            MapType = VmSpecials1464Slice.MapHexen,
            Sectors = { model, dest, untagged },
        };
        var linked = Ln(model, VmSpecials1464Slice.Set3D, 3, 8, 0, 40, 9);
        linked.Index = 6;
        var light = Ln(model, VmSpecials1464Slice.LightOnly, 3, 9);
        var plain = Ln(model, 62, 1, 2, 3, 4, 5);
        var folded = Ln(model, VmSpecials1464Slice.Set3D, 1, 0, 0, 255, 4);
        folded.TopExists = false;
        folded.TopValid = false;
        folded.TopIndex = 3;
        world.Lines.AddRange(new[] { linked, light, plain, folded });
        world.MapType = VmSpecials1464Slice.MapDoom;
        VmSpecials1464Slice.Spawn3DFloors(world);

        Assert.Empty(world.LineIds);
        Assert.Equal(62, plain.Special);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, plain.Args);
        Assert.Equal(0, linked.Special);
        Assert.Equal(0, light.Special);
        Assert.Equal(0, folded.Args[4]);
        Assert.True(folded.TopCleared);
        Assert.Equal(3, world.Recalculated.Count);
        Assert.Equal(2, world.Floors.Count);
        Assert.Equal(VmSpecials1464Slice.Exists | VmSpecials1464Slice.RenderAll | VmSpecials1464Slice.Solid | VmSpecials1464Slice.InvertSector, world.Floors[0].Flags);
        Assert.Equal(255, world.Floors[0].Alpha);
        int lightFlags = VmSpecials1464Slice.ShootThrough | VmSpecials1464Slice.SeeThrough | VmSpecials1464Slice.Exists
            | VmSpecials1464Slice.FadeWalls | VmSpecials1464Slice.NoDamage;
        Assert.Equal(lightFlags, world.Floors[1].Flags);
        Assert.Equal(0, world.Floors[1].Alpha);

        world.MapType = VmSpecials1464Slice.MapHexen;
        var hex = Ln(model, VmSpecials1464Slice.Set3D, 3, 8 | 1, 4, 10, 12);
        hex.Index = 8;
        world.Lines.Clear();
        world.Floors.Clear();
        world.Recalculated.Clear();
        world.Lines.Add(hex);
        VmSpecials1464Slice.Spawn3DFloors(world);
        Assert.Equal((8, 12), Assert.Single(world.LineIds));
        Assert.Equal(0, hex.Special);

        var plane = Ln(model, 0, 3);
        var flags = new VmSpecials1464Slice.World { Sectors = { model, dest, untagged } };
        VmSpecials1464Slice.Set3DFloor(flags, plane, 4, 1, 10);
        Assert.Equal(255, Assert.Single(flags.Floors).Alpha);
        Assert.NotEqual(0, flags.Floors[0].Flags & VmSpecials1464Slice.Fix);
        Assert.NotEqual(0, flags.Floors[0].Flags & VmSpecials1464Slice.SeeThrough);

        var swim = Ln(model, 0, 3);
        var xor = new VmSpecials1464Slice.World { Sectors = { model, dest, untagged } };
        VmSpecials1464Slice.Set3DFloor(xor, swim, 2 | 16, 0, 255);
        var row = Assert.Single(xor.Floors);
        Assert.Equal(0, row.Flags & VmSpecials1464Slice.SeeThrough);
        Assert.NotEqual(0, row.Flags & VmSpecials1464Slice.ShootThrough);
        Assert.Equal(255, row.Alpha);
    }
}
