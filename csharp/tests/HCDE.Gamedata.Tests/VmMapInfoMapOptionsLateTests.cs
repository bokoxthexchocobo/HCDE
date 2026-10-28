namespace HCDE.Gamedata.Tests;

public class VmMapInfoMapOptionsLateTests
{
    [Fact]
    public void TryResolveSpecialAction_RejectsUnknown()
    {
        Assert.False(VmMapInfoMapOptionsLate.TryResolveSpecialAction(
            "BOGUS",
            _ => (0, -1, 0),
            out _,
            out var error));
        Assert.Contains("Unknown specialaction", error);
    }

    [Fact]
    public void ParsePrecacheSounds_SkipsNoSound()
    {
        var ids = VmMapInfoMapOptionsLate.ParsePrecacheSounds(
            ["a", "b"],
            s => s == "a" ? 1 : -1,
            noSoundId: -1);
        Assert.Equal([1], ids);
    }

    [Fact]
    public void ParseCommaSeparatedStrings_ReadsEventHandlers()
    {
        Assert.Equal(2, VmMapInfoMapOptionsLate.ParseCommaSeparatedStrings(["HandlerA", ",", "HandlerB"]).Count);
    }

    [Fact]
    public void ApplyRedirect_SetsTypeAndMap()
    {
        var d = new VmMapInfoLevelOptionsDraft();
        VmMapInfoMapOptionsLate.ApplyRedirect(d, "Key", " MAP02 ");
        Assert.Equal("Key", d.AuthorName);
        Assert.Equal("MAP02", d.NextMap);
    }

    [Fact]
    public void ApplyStringOption_SetsTranslator()
    {
        var d = new VmMapInfoLevelOptionsDraft();
        VmMapInfoMapOptionsLate.ApplyStringOption(d, "translator", "TRANS");
        Assert.Equal("TRANS", d.MapLabel);
    }
}
