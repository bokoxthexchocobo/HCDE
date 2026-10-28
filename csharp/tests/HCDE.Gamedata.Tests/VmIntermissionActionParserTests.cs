namespace HCDE.Gamedata.Tests;

public class VmIntermissionActionParserTests
{
    [Fact]
    public void BaseKeys_ParseMusicTimeAndDraw()
    {
        var action = new VmIntermissionAction();
        var errors = new List<string>();
        const string body = """
            music = "INTMUS", 2
            Time = 3.5
            Background = "PIC", 1
            Draw = "OVL", 10, 20
            DrawConditional = "secret" , "ENDPIC" , 0 , 0
            """;

        Assert.True(VmIntermissionActionParser.TryParseBlock(body, action, errors), string.Join("; ", errors));
        Assert.Equal("INTMUS", action.Music);
        Assert.Equal(2, action.MusicOrder);
        Assert.Equal((int)(3.5 * 35), action.DurationTics);
        Assert.Equal("PIC", action.Background);
        Assert.True(action.FlatFill);
        Assert.Equal(2, action.Overlays.Count);
        Assert.Equal("OVL", action.Overlays[0].Name);
        Assert.Equal("secret", action.Overlays[1].Condition);
    }

    [Fact]
    public void FaderAndWiper_ParseSpecializedKeys()
    {
        var fader = new VmIntermissionActionFader();
        var wiper = new VmIntermissionActionWiper();
        var errors = new List<string>();
        Assert.True(VmIntermissionActionParser.TryParseBlock("FadeType = FadeOut", fader, errors), string.Join("; ", errors));
        Assert.Equal(VmIntermissionFadeType.Out, fader.FadeType);
        Assert.True(VmIntermissionActionParser.TryParseBlock("WipeType = Melt", wiper, errors));
        Assert.Equal(VmIntermissionWipeType.Melt, wiper.WipeType);
    }

    [Fact]
    public void Scroller_ConvertsSecondsToTics()
    {
        var scroller = new VmIntermissionActionScroller();
        var errors = new List<string>();
        const string body = """
            ScrollDirection = Up
            InitialDelay = 1.0
            ScrollTime = -100
            Background2 = "PIC2"
            """;
        Assert.True(VmIntermissionActionParser.TryParseBlock(body, scroller, errors), string.Join("; ", errors));
        Assert.Equal(VmIntermissionScrollDir.Up, scroller.ScrollDir);
        Assert.Equal(35, scroller.ScrollDelayTics);
        Assert.Equal(100, scroller.ScrollTimeTics);
        Assert.Equal("PIC2", scroller.SecondBackground);
    }

    [Fact]
    public void Textscreen_MultilineAndLookup()
    {
        var text = new VmIntermissionActionTextscreen();
        var errors = new List<string>();
        Assert.True(VmIntermissionActionParser.TryParseBlock("Text = \"line1\" , \"line2\"", text, errors), string.Join("; ", errors));
        Assert.Equal("line1\nline2", text.Text);
        Assert.True(VmIntermissionActionParser.TryParseBlock("Text = \"$LOOKUP\"", text, errors));
        Assert.Equal("$LOOKUP", text.Text);
    }

    [Fact]
    public void Descriptor_FactoryRejectsUnknownType()
    {
        var desc = new VmIntermissionDescriptor();
        var errors = new List<string>();
        Assert.False(VmIntermissionDescriptorParser.TryAddAction("bogus", "{ }", desc, errors));
        Assert.Empty(desc.Actions);
        Assert.True(VmIntermissionDescriptorParser.TryAddAction("Fader", "FadeType = FadeIn", desc, errors));
        Assert.True(VmIntermissionDescriptorParser.TrySetLink("Next", desc));
        Assert.Equal("Next", desc.Link);
    }
}
