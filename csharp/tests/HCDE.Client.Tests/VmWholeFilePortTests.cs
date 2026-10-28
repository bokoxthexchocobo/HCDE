namespace HCDE.Client.Tests;

public class VmWholeFilePortTests
{

    [Fact]
    public void ColorBlendTables_BuildSwizzledRows()
    {
        var palette = Enumerable.Range(0, 256)
            .Select(index => new VmPaletteEntry((byte)index, (byte)(index / 2), (byte)(index / 3)))
            .ToArray();
        byte Pick(int r, int g, int b, ReadOnlySpan<VmPaletteEntry> _) => (byte)((r + g + b) % 256);
        var tables = VmColorBlendTables.Build(palette, Pick);
        Assert.Equal(32 * 32 * 32, tables.Rgb32k.Length);
        Assert.NotEqual(0U, tables.Col2Rgb8Inverse[1, 2]);
        Assert.Equal(tables.Col2Rgb8[0, 10], tables.Col2Rgb8LessPrecision[0][10]);
        Assert.Equal(tables.Col2Rgb8[4, 10] & 0x3feffbffU, tables.Col2Rgb8LessPrecision[4][10]);
    }

    [Fact]
    public void VanillaTransparency_UsesDehackedInAutoMode()
    {
        var state = new VmVanillaTransparencyProbeState();
        var lumps = new[] { "DEHACKED" };
        int Find(string name, ref int last)
        {
            for (var index = last; index < lumps.Length; index++)
            {
                if (!lumps[index].Equals(name, StringComparison.OrdinalIgnoreCase))
                    continue;
                last = index;
                return index;
            }

            return -1;
        }

        Assert.True(VmVanillaTransparency.UseVanillaTransparency(2, ref state, Find));
    }


    [Fact]
    public void MvlResourceProbe_ReadsDirectoryOffsets()
    {
        var payload = new byte[] { 1, 2, 3 };
        var name = "test.lmp"u8.ToArray();
        var header = new byte[8 + 17];
        "DMVL"u8.CopyTo(header);
        BitConverter.TryWriteBytes(header.AsSpan(4), (uint)1);
        name.AsSpan(0, Math.Min(12, name.Length)).CopyTo(header.AsSpan(8));
        BitConverter.TryWriteBytes(header.AsSpan(8 + 13), (uint)payload.Length);
        var archive = header.Concat(payload).ToArray();
        var entry = Assert.Single(VmMvlResourceProbe.ReadDirectory(archive));
        Assert.Equal(25UL, entry.Position);
        Assert.Equal("test.lmp", entry.FileName);
    }
}
