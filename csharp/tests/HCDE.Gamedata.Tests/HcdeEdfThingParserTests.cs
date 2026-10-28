using HCDE.Gamedata;

namespace HCDE.Gamedata.Tests;

public class HcdeEdfThingParserTests
{
    [Fact]
    public void ThingType_ParsesDoomednumAndInherits()
    {
        Assert.True(HcdeEdfThingParser.TryParseThingBlock(
            "thingtype HCDEImp {",
            ["doomednum = 3001;", "inherits = DoomImp;"],
            out var thing));
        Assert.NotNull(thing);
        Assert.Equal("HCDEImp", thing!.Name);
        Assert.Equal(3001, thing.DoomedNum);
        Assert.Equal("DoomImp", thing.BaseName);
        Assert.False(thing.HasUnsupportedBehavior);
    }

    [Fact]
    public void ThingDelta_MarksUnknownBehaviorProperty()
    {
        Assert.True(HcdeEdfThingParser.TryParseThingBlock(
            "thingdelta CustomZombie : DoomImp 0 {",
            ["+NOBLOCKMAP"],
            out var thing));
        Assert.NotNull(thing);
        Assert.Equal("CustomZombie", thing!.Name);
        Assert.Equal("DoomImp", thing.BaseName);
        Assert.Equal(0, thing.DoomedNum);
        Assert.True(thing.HasUnsupportedBehavior);
    }

    [Fact]
    public void MissingName_ReturnsFalse()
    {
        Assert.False(HcdeEdfThingParser.TryParseThingBlock("thingtype {", [], out _));
    }
}
