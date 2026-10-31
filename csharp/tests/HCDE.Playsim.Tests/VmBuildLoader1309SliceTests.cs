using System.Buffers.Binary;

namespace HCDE.Playsim.Tests;

public class VmBuildLoader1309SliceTests
{
    [Fact]
    public void Palette_ExpandsVgaAndShiftsBlood()
    {
        Assert.Equal(0, VmBuildLoader1309.CountTiles(new byte[] { 1, 0, 0, 0 }));
        Assert.Equal(0, VmBuildLoader1309.CountTiles(Header(2, 0, 1)));
        Assert.Equal(3, VmBuildLoader1309.CountTiles(Header(1, 3, 5)));
        Assert.Equal(0, VmBuildLoader1309.CountTiles(Header(1, 5, 3)));
        Assert.Equal("foo/", VmBuildLoader1309.ExtractFilePath("foo/palette.dat"));
        Assert.Equal("", VmBuildLoader1309.ExtractFilePath("palette.dat"));
        Assert.Equal("palette.dat", VmBuildLoader1309.ExtractFileBase("FOO/palette.dat", true));
        Assert.Equal("palette", VmBuildLoader1309.ExtractFileBase("palette.dat", false));

        var host = new VmBuildLoader1309.Host();
        host.Lumps.Add(new VmBuildLoader1309.Lump { Data = new byte[100] });
        host.Lumps.Add(new VmBuildLoader1309.Lump { Data = Vga(1) });
        host.Lumps.Add(new VmBuildLoader1309.Lump { Data = Blood(64) });
        var loader = new VmBuildLoader1309(host);

        Assert.Equal(0, loader.BuildPalette(0));
        Assert.Empty(host.Stored);
        Assert.Equal(0, loader.BuildPalette(1));
        var vga = Assert.Single(host.Stored);
        Assert.Equal(VmBuildLoader1309.TranslationStandard, vga.slot);
        Assert.Equal(0xFF040404u, vga.palette[0]);
        Assert.Equal((byte)((4 + 4 + 4) & 255), vga.remap[0]);
        Assert.Equal(0u, vga.palette[255]);
        Assert.Equal(0, vga.remap[255]);

        Assert.Equal(1, loader.BuildPalette(2));
        var blood = host.Stored[1];
        Assert.Equal(0xFF000000u, blood.palette[0]);
        Assert.Equal((byte)((256 + 256 + 256) & 255), blood.remap[0]);
    }

    [Fact]
    public void Init_StopsAtTheMissingArtAndCountsZeroTiles()
    {
        var host = new VmBuildLoader1309.Host();
        host.Lumps.Add(new VmBuildLoader1309.Lump
        {
            Name = "hidden/palette.dat",
            Container = 0,
            Data = Vga(1),
        });
        host.Lumps.Add(new VmBuildLoader1309.Lump
        {
            Name = "hidden/palette.dat",
            Container = 1,
            Data = new byte[10],
        });
        host.Lumps.Add(new VmBuildLoader1309.Lump
        {
            Name = "foo/Palette.DAT",
            Container = 2,
            Data = Vga(2),
        });
        uint anim = 2u | 64u | (0x80u << 8) | (1u << 16) | (1u << 28);
        host.Lumps.Add(new VmBuildLoader1309.Lump
        {
            Name = "foo/tiles000.art",
            Container = 2,
            Data = Art(2, 3, new (int w, int h, uint anm)[] { (2, 2, anim), (0, 1, 0) }, new byte[] { 9, 8, 7, 6 }),
        });
        host.Lumps.Add(new VmBuildLoader1309.Lump
        {
            Name = "foo/tiles001.art",
            Container = 9,
            Data = Art(1, 1, new (int w, int h, uint anm)[] { (1, 1, 2u << 28) }, new byte[] { 3 }),
        });
        host.Lumps.Add(new VmBuildLoader1309.Lump
        {
            Name = "foo/tiles002.art",
            Container = 2,
            Data = Art(4, 4, new (int w, int h, uint anm)[] { (1, 1, 0) }, new byte[] { 5 }),
        });

        int total = new VmBuildLoader1309(host).Init();

        Assert.Equal(2, total);
        Assert.Equal(
            "1:foo/:2:foo/BTIL0002:2x2:-127,2:3:0",
            Assert.Single(host.Names));
        Assert.Equal(new byte[] { 9, 8, 7, 6 }, Assert.Single(host.Pixels));
        Assert.Equal("1:2:2:8", Assert.Single(host.Anims));
        var rot = Assert.Single(host.Rotations);
        Assert.Equal(1, rot.id);
        Assert.Equal(0x00FC, rot.flip);
        Assert.Equal(new[] { 1, 1, 2, 2, 3, 3, 4, 4, 1, 1, 4, 4, 3, 3, 2, 2 }, rot.tex);
        Assert.Equal(VmBuildLoader1309.TranslationStandard, Assert.Single(host.Stored).slot);
    }

    static byte[] Vga(byte value)
    {
        var data = new byte[768];
        Array.Fill(data, value);
        return data;
    }

    static byte[] Blood(byte value)
    {
        var data = new byte[768];
        Array.Fill(data, value);
        return data;
    }

    static byte[] Header(int version, int start, int end)
    {
        var data = new byte[16];
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(0), version);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8), start);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12), end);
        return data;
    }

    static byte[] Art(int start, int end, (int w, int h, uint anm)[] tiles, byte[] pixels)
    {
        int count = end - start + 1;
        var data = new byte[16 + count * 8 + pixels.Length];
        Header(1, start, end).CopyTo(data, 0);
        for (int i = 0; i < count; i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(16 + i * 2), (ushort)tiles[i].w);
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(16 + count * 2 + i * 2), (ushort)tiles[i].h);
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(16 + count * 4 + i * 4), tiles[i].anm);
        }

        pixels.CopyTo(data, 16 + count * 8);
        return data;
    }
}
