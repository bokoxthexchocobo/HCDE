namespace HCDE.Playsim.Tests;

public class VmVoxelDef1338SliceTests
{
    [Fact]
    public void SpriteNamesAndOptions()
    {
        var host = new VmVoxelDef1338();
        var seen = new List<string>();
        host.SpriteIndex = name =>
        {
            seen.Add(name);
            return name switch
            {
                "POSS" => 3,
                "trooA" => 4,
                "MISS" => -1,
                _ => -1,
            };
        };
        var names = new VmVoxelDef1338.Scan()
            .Add(0, "POSS")
            .Add(0, "trooa")
            .Add(0, "ab")
            .Add(0, "xxxx^")
            .Add(0, "MISS")
            .Add(0, "=");
        var sprites = new List<uint>();
        Assert.True(host.ReadSpriteNames(names, sprites));
        Assert.Equal(new[] { "POSS", "trooA", "MISS" }, seen);
        Assert.Equal(new uint[] { (255u << 24) | 3u, 4u }, sprites);
        Assert.Equal(
            new[]
            {
                "Sprite name \"ab\" is wrong size.\n",
                "Sprite frame ^ is invalid.\n",
            },
            names.Messages);

        var empty = new VmVoxelDef1338.Scan().Add(0, "=");
        var none = new List<uint> { 1u };
        Assert.True(host.ReadSpriteNames(empty, none));
        Assert.Empty(none);
        Assert.Equal("No sprites specified for voxel.\n", empty.Messages[0]);

        var eof = new VmVoxelDef1338.Scan().Add(0, "POSS");
        var leftover = new List<uint>();
        Assert.False(host.ReadSpriteNames(eof, leftover));
        Assert.Equal(new uint[] { (255u << 24) | 3u }, leftover);
        Assert.Equal("Unexpected end of file\n", eof.Messages[0]);

        var bare = new VmVoxelDef1338.Scan();
        var bareList = new List<uint>();
        Assert.False(host.ReadSpriteNames(bare, bareList));
        Assert.Empty(bareList);
        Assert.Empty(bare.Messages);

        var opts = new VmVoxelDef1338.Options();
        var block = new VmVoxelDef1338.Scan()
            .Add(VmVoxelDef1338.TkIdentifier, "Scale")
            .Add((int)'=', "=")
            .Add(VmVoxelDef1338.TkFloat, "2.5", floatValue: 2.5)
            .Add(VmVoxelDef1338.TkIdentifier, "spin")
            .Add((int)'=', "=")
            .Add((int)'-', "-")
            .Add(VmVoxelDef1338.TkInt, "3", 3)
            .Add(VmVoxelDef1338.TkIdentifier, "placedspin")
            .Add((int)'=', "=")
            .Add(VmVoxelDef1338.TkInt, "4", 4)
            .Add(VmVoxelDef1338.TkIdentifier, "droppedspin")
            .Add((int)'=', "=")
            .Add((int)'-', "-")
            .Add(VmVoxelDef1338.TkInt, "2", 2)
            .Add(VmVoxelDef1338.TkIdentifier, "angleoffset")
            .Add((int)'=', "=")
            .Add((int)'-', "-")
            .Add(VmVoxelDef1338.TkInt, "10", 10)
            .Add(VmVoxelDef1338.TkIdentifier, "xoffset")
            .Add((int)'=', "=")
            .Add(VmVoxelDef1338.TkFloat, "1.5", floatValue: 1.5)
            .Add(VmVoxelDef1338.TkIdentifier, "yoffset")
            .Add((int)'=', "=")
            .Add((int)'-', "-")
            .Add(VmVoxelDef1338.TkInt, "2", 2)
            .Add(VmVoxelDef1338.TkIdentifier, "zoffset")
            .Add((int)'=', "=")
            .Add(VmVoxelDef1338.TkInt, "3", 3)
            .Add(VmVoxelDef1338.TkIdentifier, "overridepalette")
            .Add(VmVoxelDef1338.TkIdentifier, "PitchFromMomentum")
            .Add(VmVoxelDef1338.TkIdentifier, "useactorpitch")
            .Add(VmVoxelDef1338.TkIdentifier, "useactorroll")
            .Add(VmVoxelDef1338.TkIdentifier, "bogus")
            .Add((int)'=', "=")
            .Add(VmVoxelDef1338.TkInt, "1", 1)
            .Add(VmVoxelDef1338.TkIdentifier, "nope")
            .Add(VmVoxelDef1338.TkIdentifier, "scale")
            .Add((int)'=', "=")
            .Add(VmVoxelDef1338.TkFloat, "9", floatValue: 9)
            .Add((int)'}', "}");
        host.ReadOptions(block, opts);
        Assert.Equal(9, opts.Scale);
        Assert.Equal(-2, opts.DroppedSpin);
        Assert.Equal(4, opts.PlacedSpin);
        Assert.Equal(80, opts.AngleOffset);
        Assert.Equal(1.5, opts.XOffset);
        Assert.Equal(-2, opts.YOffset);
        Assert.Equal(3, opts.ZOffset);
        Assert.True(opts.OverridePalette);
        Assert.True(opts.PitchFromMomentum);
        Assert.True(opts.UseActorPitch);
        Assert.True(opts.UseActorRoll);
        Assert.Equal(
            new[]
            {
                "Unknown voxel option 'bogus'\n",
                "Unknown voxel option 'nope'\n",
            },
            block.Messages);

        var open = new VmVoxelDef1338.Scan();
        var dangling = new VmVoxelDef1338.Options();
        host.ReadOptions(open, dangling);
        Assert.Equal("Unterminated voxel option block\n", open.Messages[0]);
        Assert.Equal(1, dangling.Scale);
    }

    [Fact]
    public void InitAppliesEveryFrameAndSkipsMissingFiles()
    {
        var host = new VmVoxelDef1338();
        var voxel = new VmVoxelDef1338.Voxel();
        var scan = new VmVoxelDef1338.Scan()
            .Add(0, "POSS")
            .Add(0, "=")
            .Add(VmVoxelDef1338.TkString, "foo.kvx")
            .Add((int)'{', "{")
            .Add(VmVoxelDef1338.TkIdentifier, "overridepalette")
            .Add((int)'}', "}")
            .Add(0, "trooa")
            .Add(0, "=")
            .Add(VmVoxelDef1338.TkString, "bar.kvx");
        var finds = new List<(string Name, int Last)>();
        host.FindLump = (string name, ref int last) =>
        {
            finds.Add((name, last));
            if (last == 0)
            {
                last = 6;
                return 5;
            }

            last = 100;
            return -1;
        };
        host.Open = lump =>
        {
            Assert.Equal(5, lump);
            return scan;
        };
        host.SpriteIndex = name => name == "POSS" ? 2 : 7;
        var full = new List<(string Name, bool TryNormal, int Space)>();
        host.FullName = (name, tryNormal, space) =>
        {
            full.Add((name, tryNormal, space));
            return name == "foo.kvx" ? 9 : 4;
        };
        host.GetVoxel = file => file == 9 ? voxel : null;
        host.Init();

        Assert.Equal(new[] { ("VOXELDEF", 0), ("VOXELDEF", 6) }, finds);
        Assert.Equal(new[] { ("foo.kvx", true, 11), ("bar.kvx", true, 11) }, full);
        Assert.Equal(new[] { true, false, true, false }, scan.CModeHistory);
        Assert.True(voxel.PaletteRemoved);
        Assert.Equal("\"bar.kvx\" is not a valid voxel file.\n", scan.Messages[0]);
        Assert.Single(host.VoxelDefs);
        var def = host.VoxelDefs[0];
        Assert.Same(voxel, def.Voxel);
        Assert.Equal(1, def.Scale);
        Assert.Equal(90, def.AngleOffset);
        Assert.Equal(0, def.DroppedSpin);
        Assert.Equal(0, def.PlacedSpin);
        Assert.Equal(29, host.Added.Count);
        for (var i = 0; i < 29; i++)
        {
            Assert.Equal(2, host.Added[i].Sprite);
            Assert.Equal(28 - i, host.Added[i].Frame);
            Assert.Same(def, host.Added[i].Def);
        }

        var missing = new VmVoxelDef1338();
        var gone = new VmVoxelDef1338.Scan()
            .Add(0, "=")
            .Add(VmVoxelDef1338.TkString, "gone.kvx");
        missing.FindLump = (string name, ref int last) =>
        {
            if (last == 0)
            {
                last = 2;
                return 1;
            }

            return -1;
        };
        missing.Open = lump => gone;
        missing.SpriteIndex = name => 1;
        missing.FullName = (name, tryNormal, space) => -1;
        missing.GetVoxel = file => new VmVoxelDef1338.Voxel();
        missing.Init();
        Assert.Empty(missing.VoxelDefs);
        Assert.Empty(missing.Added);
        Assert.Equal(
            new[]
            {
                "No sprites specified for voxel.\n",
                "Voxel \"gone.kvx\" not found.\n",
            },
            gone.Messages);

        var broken = new VmVoxelDef1338();
        broken.FindLump = (string name, ref int last) =>
        {
            if (last == 0)
            {
                last = 3;
                return 2;
            }

            return -1;
        };
        broken.Open = lump => new VmVoxelDef1338.Scan().Add(0, "POSS").Add(0, "=");
        broken.SpriteIndex = name => 1;
        broken.FullName = (name, tryNormal, space) => 1;
        broken.GetVoxel = file => new VmVoxelDef1338.Voxel();
        Assert.Throws<InvalidOperationException>(() => broken.Init());
        Assert.Empty(broken.VoxelDefs);
    }
}
