using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedNoAutofreezeTests
{
    private static AuthoritySimulation Room(int value) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        dehacked: DehackedPatch.Apply($"Misc 0\nNo Autofreeze = {value}\n"));

    [Theory]
    [InlineData(0, false, 4)]
    [InlineData(1, false, 2)]
    [InlineData(-1, false, 2)]
    [InlineData(1, true, 4)]
    public void SettingDisablesOnlyGenericIceFallback(int value, bool typed, int expected)
    {
        var player = Room(value).Players.Single();
        player.States.Configure(player, [new(-1, 0), new(-1, 1), new(-1, 2), new(-1, 3), new(-1, 4)], 0);
        player.GenericFreezeDeath = 4;
        if (typed) player.SetTypedDeath("Ice", 4);
        ActorDamage.Apply(player, 100, damageType: "Ice");
        Assert.Equal(expected, player.States.Current);
    }

    [Fact]
    public void BaselineAndInvalidInputPreserveSetting()
    {
        var baseline = DehackedPatch.Apply("Misc 0\nNo Autofreeze = 1\n");
        Assert.True(DehackedPatch.Apply("", baseline).NoAutofreeze);
        var invalid = DehackedPatch.Apply("Misc 0\nNo Autofreeze = invalid\n", baseline);
        Assert.NotEmpty(invalid.Errors); Assert.True(invalid.NoAutofreeze);
    }

    [Fact]
    public void SettingChangesChecksum()
    {
        var baseline = Room(0); var patched = Room(1);
        baseline.Tick(); patched.Tick(); Assert.NotEqual(baseline.Checksum, patched.Checksum);
    }
}
