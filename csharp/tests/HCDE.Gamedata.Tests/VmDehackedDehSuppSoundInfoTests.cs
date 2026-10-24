namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppSoundInfoTests
{
    [Fact]
    public void ParseSoundMapSection_ResolvesEachSound()
    {
        var names = VmDehackedDehSuppSoundInfo.ParseSoundMapSection(
            ["{", "weap/pistol", ",", "weap/shotg", "}"],
            s => s switch { "weap/pistol" => 1, "weap/shotg" => 2, _ => -1 });
        Assert.Equal(2, names.Count);
        Assert.Equal("weap/pistol", names[0]);
    }

    [Fact]
    public void TryParseInfoNamesSection_AcceptsKnownActors()
    {
        Assert.True(VmDehackedDehSuppSoundInfo.TryParseInfoNamesSection(
            ["ZombieMan", "ShotgunGuy"],
            findActor: n => n is "ZombieMan" or "ShotgunGuy",
            out var names,
            out _));
        Assert.Equal(2, names.Count);
    }

    [Fact]
    public void TryParseInfoNamesSection_RejectsUnknownActor()
    {
        Assert.False(VmDehackedDehSuppSoundInfo.TryParseInfoNamesSection(
            ["NotReal"],
            findActor: _ => false,
            out _,
            out var error));
        Assert.Contains("Unknown actor", error);
    }
}
