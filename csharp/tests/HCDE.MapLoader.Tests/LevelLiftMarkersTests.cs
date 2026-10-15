namespace HCDE.MapLoader.Tests;

public class LevelLiftMarkersTests
{
    [Theory]
    [InlineData(62, true, 0, true)]
    [InlineData(64, true, 0, true)]
    [InlineData(172, true, 0, true)]
    [InlineData(206, true, 0, true)]
    [InlineData(203, true, 1, true)]
    [InlineData(203, true, 2, false)]
    [InlineData(62, false, 0, false)]
    [InlineData(63, true, 0, false)]
    public void NativeSpecialsRequireRepeatAndSupportedPlatformType(int special, bool repeat, int type, bool expected)
    {
        var level = Level(new LevelLine { Special = special, Repeat = repeat, Arg0 = 7, Arg3 = type });
        LevelLiftMarkers.Apply(level); Assert.Equal(expected, level.Sectors[0].IsLift); Assert.False(level.Sectors[1].IsLift);
    }

    [Theory]
    [InlineData(62, true)]
    [InlineData(88, true)]
    [InlineData(120, true)]
    [InlineData(123, true)]
    [InlineData(21, false)]
    [InlineData(121, false)]
    [InlineData(0x3401, true)]
    [InlineData(0x3400, false)]
    [InlineData(0x3501, false)]
    public void DoomTranslationUsesRepeatAndGenericTargetBits(int special, bool expected)
    {
        var level = Level(new LevelLine { Special = special, Tag = 7 }, doom: true);
        LevelLiftMarkers.Apply(level); Assert.Equal(expected, level.Sectors[0].IsLift);
    }

    [Fact]
    public void ZeroTagTargetsBackSectorAndBinaryRepeatFlagIsAccepted()
    {
        var level = Level(new LevelLine { Special = 62, Flags = LevelLine.RepeatSpecialFlag, SideBack = 1 });
        LevelLiftMarkers.Apply(level); Assert.False(level.Sectors[0].IsLift); Assert.True(level.Sectors[1].IsLift);
    }

    [Fact]
    public void SecondaryTagsAndCopiesRetainInitializationMarker()
    {
        var line = new LevelLine { Special = 62, Repeat = true, Arg0 = 9 };
        var level = Level(line); LevelLiftMarkers.Apply(level); Assert.True(level.Sectors[1].IsLift);
        line.Special = 0; var copy = level.CopyForSimulation(); LevelLiftMarkers.Apply(copy);
        Assert.True(copy.Sectors[1].IsLift); Assert.NotSame(level.Sectors[1], copy.Sectors[1]);
    }

    private static PlayLevel Level(LevelLine line, bool doom = false) => new() {
        Format = doom ? MapDataFormat.DoomBinary : MapDataFormat.UdmfText, Namespace = doom ? "Doom" : "ZDoom",
        Sectors = [new LevelSector { Tag = 7 }, new LevelSector { Tag = 8, AdditionalTags = [9] }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }], Lines = [line]
    };
}
