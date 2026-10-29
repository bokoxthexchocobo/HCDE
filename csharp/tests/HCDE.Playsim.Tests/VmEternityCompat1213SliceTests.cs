namespace HCDE.Playsim.Tests;

public class VmEternityCompat1213SliceTests
{
    static VmEternityLump1213 Lump(int container, string? shortName, string? fullName, string? fullPath) => new()
    {
        Container = container,
        ShortName = shortName,
        FullName = fullName,
        FullPath = fullPath,
    };

    [Fact]
    public void Detect_ClassifiesEmapinfoEdfAndExtradata()
    {
        var lines = new List<string>();
        VmEternityCompat1213Slice.Detect(
            new[]
            {
                Lump(0, "EMAPINFO", "iwad/emapinfo", "iwad.wad:EMAPINFO"),
                Lump(1, "EDFROOT", "iwad/edfroot", "iwad.wad:EDFROOT"),
                Lump(2, "EMAPINFO", "", "mod.wad:EMAPINFO"),
                Lump(2, "Foo.EDF", "mods/Foo.EDF", "mod.wad:mods/Foo.EDF"),
                Lump(2, "edfroot", "defs/edfroot", "mod.wad:defs/edfroot"),
                Lump(2, "edf", "defs/edf", "mod.wad:defs/edf"),
                Lump(2, "thing.txt", "maps/edf/thing.txt", "mod.wad:maps/edf/thing.txt"),
                Lump(2, "root.txt", "edf/root.txt", "mod.wad:edf/root.txt"),
                Lump(2, "ExtraData", "", "mod.wad:ExtraData"),
                Lump(2, "z", "foo/ExtraData.TXT", "mod.wad:foo/ExtraData.TXT"),
                Lump(2, "a", "foo/extradata", "mod.wad:foo/extradata"),
                Lump(2, "note", "maps/notextra", "mod.wad:notextra"),
                Lump(2, "bak", "models/ship.edf.bak", "mod.wad:ship.edf.bak"),
                Lump(2, "nested", @"Mods\EDF\A.EDF", @"C:\Mods\A.EDF"),
                Lump(2, "both", "mods/edf/emapinfo", "mod.wad:mods/edf/emapinfo"),
                Lump(2, "pair", "foo/edf/extradata.txt", "mod.wad:foo/edf/extradata.txt"),
                Lump(2, "dir", "mods/edf/", "mod.wad:mods/edf/"),
                Lump(2, "myedf", "myedf/file", "mod.wad:myedf/file"),
            },
            maxIwadNum: 1,
            lines.Add);

        Assert.Equal(VmEternityCompat1213Slice.FlagEmapInfo | VmEternityCompat1213Slice.FlagEdf | VmEternityCompat1213Slice.FlagExtraData, VmEternityCompat1213Slice.GetFlags());
        Assert.Equal(2, VmEternityCompat1213Slice.EmapInfoCount());
        Assert.Equal(9, VmEternityCompat1213Slice.EdfCount());
        Assert.Equal(4, VmEternityCompat1213Slice.ExtraDataCount());
        Assert.True(VmEternityCompat1213Slice.IsActive(VmEternityCompat1213Slice.FlagEmapInfo));
        Assert.True(VmEternityCompat1213Slice.IsActive(VmEternityCompat1213Slice.FlagEdf | VmEternityCompat1213Slice.FlagExtraData));
        Assert.True(VmEternityCompat1213Slice.IsActive(7));
        Assert.False(VmEternityCompat1213Slice.IsActive(8));
        Assert.True(VmEternityCompat1213Slice.IsActive(0));
        Assert.Equal(
            new[]
            {
                "mod.wad:EMAPINFO",
                "mod.wad:mods/Foo.EDF",
                "mod.wad:defs/edfroot",
                "mod.wad:defs/edf",
                "mod.wad:maps/edf/thing.txt",
                "mod.wad:edf/root.txt",
                "mod.wad:ExtraData",
                "mod.wad:foo/ExtraData.TXT",
            },
            VmEternityCompat1213Slice.Samples());
        Assert.Equal(
            new[]
            {
                "HCDE: Eternity compatibility resources detected: EMAPINFO=2 EDF=9 ExtraData=4.",
                "HCDE: Eternity compatibility will parse supported EMAPINFO, ExtraData, XLAT, and EDF manifest properties; unsupported behavior still needs compat patches.",
                "HCDE: Eternity resource: mod.wad:EMAPINFO",
                "HCDE: Eternity resource: mod.wad:mods/Foo.EDF",
                "HCDE: Eternity resource: mod.wad:defs/edfroot",
                "HCDE: Eternity resource: mod.wad:defs/edf",
                "HCDE: Eternity resource: mod.wad:maps/edf/thing.txt",
                "HCDE: Eternity resource: mod.wad:edf/root.txt",
                "HCDE: Eternity resource: mod.wad:ExtraData",
                "HCDE: Eternity resource: mod.wad:foo/ExtraData.TXT",
            },
            lines);
    }

    [Fact]
    public void Detect_CapsSamplesAndSkipsIwadLumps()
    {
        var quiet = new List<string>();
        VmEternityCompat1213Slice.Detect(Array.Empty<VmEternityLump1213>(), 0, quiet.Add);
        Assert.Empty(quiet);
        Assert.Equal(0u, VmEternityCompat1213Slice.GetFlags());
        Assert.True(VmEternityCompat1213Slice.IsActive(0));
        Assert.False(VmEternityCompat1213Slice.IsActive(VmEternityCompat1213Slice.FlagEmapInfo));

        var lumps = new List<VmEternityLump1213>
        {
            Lump(2, "EMAPINFO", "emapinfo", "iwad-boundary"),
            Lump(1, "edf", "edf", "older-iwad"),
            Lump(3, null, null, "ignored"),
            Lump(3, "EMAPINFO", null, "same-path"),
            Lump(3, "EMAPINFO", "", "same-path"),
            Lump(3, "plain", "plain.txt", "not-a-resource"),
        };
        for (int i = 0; i < 7; i++)
            lumps.Add(Lump(5, "EMAPINFO", "pack/emapinfo", "sample-" + i));
        lumps.Add(Lump(5, "EMAPINFO", "pack/emapinfo", "sample-overflow"));

        var lines = new List<string>();
        VmEternityCompat1213Slice.Detect(lumps, 2, lines.Add);
        Assert.Equal(VmEternityCompat1213Slice.FlagEmapInfo, VmEternityCompat1213Slice.GetFlags());
        Assert.Equal(10, VmEternityCompat1213Slice.EmapInfoCount());
        Assert.Equal(0, VmEternityCompat1213Slice.EdfCount());
        Assert.Equal(0, VmEternityCompat1213Slice.ExtraDataCount());
        Assert.Equal(
            new[]
            {
                "same-path",
                "sample-0",
                "sample-1",
                "sample-2",
                "sample-3",
                "sample-4",
                "sample-5",
                "sample-6",
            },
            VmEternityCompat1213Slice.Samples());
        Assert.Equal("HCDE: Eternity compatibility resources detected: EMAPINFO=10 EDF=0 ExtraData=0.", lines[0]);
        Assert.Equal(10, lines.Count);
        Assert.Equal("HCDE: Eternity resource: sample-6", lines[^1]);

        Assert.Throws<NullReferenceException>(() =>
            VmEternityCompat1213Slice.Detect(
                new[] { Lump(9, "edf", "edf", "mod.wad:edf") },
                0,
                null!));
        Assert.Equal(VmEternityCompat1213Slice.FlagEdf, VmEternityCompat1213Slice.GetFlags());
        Assert.Equal(1, VmEternityCompat1213Slice.EdfCount());
        Assert.Equal(new[] { "mod.wad:edf" }, VmEternityCompat1213Slice.Samples());
    }
}
