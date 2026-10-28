namespace HCDE.Gamedata.Tests;

public class VmDsdaOptionsApplierTests
{
    [Fact]
    public void ApplyToLevel_SetsNoInfightingFlag()
    {
        VmDsdaOptionsParser.TryParse("monster_infighting 0\n", out var opt);
        var level = new VmWadLevelInfoPatch();
        VmDsdaOptionsApplier.ApplyToLevel(level, opt);
        Assert.Equal(0u, level.Flags2 & 0x00004000);
    }

    [Fact]
    public void ApplyToLevel_SetsCompatMask()
    {
        VmDsdaOptionsParser.TryParse("comp_dropoff 1\n", out var opt);
        var level = new VmWadLevelInfoPatch();
        VmDsdaOptionsApplier.ApplyToLevel(level, opt);
        Assert.NotEqual(0u, level.CompatMask & (1u << 20));
    }
}
