namespace HCDE.MapLoader.Tests;

public class VmMapLoaderCompatibilityTests
{
    [Fact]
    public void ParseCompatibility_AssignsFlagsToMd5Block()
    {
        var db = new VmMapLoaderCompatibility.CompatibilityDatabase();
        const string text = "AABBCCDDEEFF00112233445566778899 { trace stairs }";

        VmMapLoaderCompatibility.ParseCompatibility(db, text);
        Assert.True(db.TryGet(HexToBytes("AABBCCDDEEFF00112233445566778899"), out var flags));
        Assert.Equal((1u << 13) | VmMapLoaderCompatibility.CompatfStairIndex, flags.Flags1);
    }

    [Fact]
    public void ParseCompatibility_SharesBlockAcrossMultipleMd5Lines()
    {
        var db = new VmMapLoaderCompatibility.CompatibilityDatabase();
        const string text = """
            11111111111111111111111111111111
            22222222222222222222222222222222
            {
            vileghosts
            }
            """;

        VmMapLoaderCompatibility.ParseCompatibility(db, text);
        Assert.True(db.TryGet(HexToBytes("11111111111111111111111111111111"), out var a));
        Assert.True(db.TryGet(HexToBytes("22222222222222222222222222222222"), out var b));
        Assert.Equal(a.Flags1, b.Flags1);
        Assert.Equal(1u << 25, a.Flags1);
    }

    [Fact]
    public void ParseCompatibility_RejectsInvalidMd5Length()
    {
        var db = new VmMapLoaderCompatibility.CompatibilityDatabase();
        Assert.Throws<VmMapLoaderCompatibility.CompatibilityParseException>(() =>
            VmMapLoaderCompatibility.ParseCompatibility(db, "SHORT { trace }"));
    }

    [Fact]
    public void ParseCompatibility_SkipsZandronumCommandBlock()
    {
        var db = new VmMapLoaderCompatibility.CompatibilityDatabase();
        const string text = """
            AABBCCDDEEFF00112233445566778899
            {
            setlineflags
            foo bar
            }
            """;

        VmMapLoaderCompatibility.ParseCompatibility(db, text);
        Assert.True(db.TryGet(HexToBytes("AABBCCDDEEFF00112233445566778899"), out _));
    }

    [Fact]
    public void CheckCompatibility_AppliesDatabaseFlagsForKnownMap31Hash()
    {
        var db = new VmMapLoaderCompatibility.CompatibilityDatabase();
        VmMapLoaderCompatibility.ParseCompatibility(db, """
            1CE294781A2455DE72C197E0B3DF6212
            {
            setslopeoverflow
            resetplayerspeed
            }
            """);

        var level = new VmMapLoaderCompatibility.CompatibilityLevelState();
        var checksum = HexToBytes("1CE294781A2455DE72C197E0B3DF6212");

        var hash = VmMapLoaderCompatibility.CheckCompatibility(db, level, checksum, developer: 0);

        Assert.Equal("1CE294781A2455DE72C197E0B3DF6212", hash);
        Assert.Equal((1u << 0) | (1u << 1), level.IbCompatFlags);
    }

    [Fact]
    public void CheckCompatibility_IwadDoomAppliesShortTexAndLight()
    {
        var db = new VmMapLoaderCompatibility.CompatibilityDatabase();
        VmMapLoaderCompatibility.ParseCompatibility(db, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA { }");

        var level = new VmMapLoaderCompatibility.CompatibilityLevelState
        {
            MapLumpFromIwad = true,
            MapType = VmMapLoaderCompatibility.MapTypeDoom,
            GameInfoFlags = VmMapLoaderCompatibility.GiCompatShortTex | VmMapLoaderCompatibility.GiCompatStairs,
        };

        VmMapLoaderCompatibility.CheckCompatibility(
            db,
            level,
            HexToBytes("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"),
            developer: 0);

        Assert.Equal(
            VmMapLoaderCompatibility.CompatfShortTex
                | VmMapLoaderCompatibility.CompatfLight
                | VmMapLoaderCompatibility.CompatfStairIndex,
            level.IiCompatFlags);
    }

    [Fact]
    public void CheckCompatibility_HexenHackSetsFloatBob()
    {
        var db = new VmMapLoaderCompatibility.CompatibilityDatabase();
        VmMapLoaderCompatibility.ParseCompatibility(db, "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB { }");

        var level = new VmMapLoaderCompatibility.CompatibilityLevelState
        {
            LevelFlags2 = VmMapLoaderCompatibility.Level2HexenHack,
        };

        VmMapLoaderCompatibility.CheckCompatibility(
            db,
            level,
            HexToBytes("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"),
            developer: 0);

        Assert.Equal(VmMapLoaderCompatibility.BcompatfFloatBob, level.IbCompatFlags);
    }

    [Fact]
    public void ApplyCompatibility_ClearsSubsectorLinksWhenPolyobjFlagToggles()
    {
        var level = new VmMapLoaderCompatibility.CompatibilityLevelState
        {
            DmCompatFlags = VmMapLoaderCompatibility.CompatfPolyobj,
            IiCompatFlags = 0,
            ICompatFlags = 0,
        };

        VmMapLoaderCompatibility.ApplyCompatibility(level);
        Assert.True(level.SubsectorLinksCleared);
    }

    [Fact]
    public void CheckCompatibility_EmitsNotifyLineWhenDeveloperEnabled()
    {
        var db = new VmMapLoaderCompatibility.CompatibilityDatabase();
        VmMapLoaderCompatibility.ParseCompatibility(db, """
            CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC
            {
            trace
            }
            """);

        var lines = new List<string>();
        VmMapLoaderCompatibility.CheckCompatibility(
            db,
            new VmMapLoaderCompatibility.CompatibilityLevelState(),
            HexToBytes("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"),
            developer: VmMapLoaderCompatibility.DmsgNotify,
            notifyLines: lines);

        Assert.Single(lines);
        Assert.Contains("cflags = ", lines[0], StringComparison.Ordinal);
    }

    private static byte[] HexToBytes(string hex)
    {
        var bytes = new byte[16];
        for (var i = 0; i < 16; i++)
            bytes[i] = byte.Parse(hex.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
        return bytes;
    }
}
