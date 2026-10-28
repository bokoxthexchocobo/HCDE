namespace HCDE.Playsim.Tests;

public class VmPInteractionTouchObituaryTests
{
    [Fact]
    public void TouchSpecialThing_SkipsWhenOutOfReach()
    {
        var touched = false;
        VmPInteractionTouchObituary.TouchSpecialThing(new()
        {
            SpecialZ = 100,
            ToucherZ = 0,
            ToucherHeight = 41,
            SpecialHeight = 16,
            ToucherHealth = 100,
            CallTouch = () => touched = true,
        });
        Assert.False(touched);
    }

    [Fact]
    public void TouchSpecialThing_CallsTouchWhenInReach()
    {
        var touched = false;
        VmPInteractionTouchObituary.TouchSpecialThing(new()
        {
            SpecialZ = 32,
            ToucherZ = 0,
            ToucherHeight = 64,
            SpecialHeight = 16,
            ToucherHealth = 50,
            CallTouch = () => touched = true,
        });
        Assert.True(touched);
    }

    [Fact]
    public void PronounMessage_ExpandsSubjectPronoun()
    {
        Span<char> buf = stackalloc char[64];
        VmPInteractionTouchObituary.PronounMessage("%g died", buf, 2, "v", "k");
        Assert.Equal("they died", new string(buf.Slice(0, buf.IndexOf('\0'))));
    }

    [Fact]
    public void ClientObituary_PrintsResolvedSuicide()
    {
        string? printed = null;
        var s = new VmPInteractionTouchObituary.ClientObituaryState
        {
            HasPlayer = true,
            MoIsSelf = true,
            ShowObituaries = true,
            AttackerNull = false,
            AttackerIsSelf = true,
            ModIndex = 1,
            NameSuicide = 1,
            Gender = 0,
            VictimName = "Alice",
            AttackerName = "Alice",
            ResolveString = (key, _) => key == "OB_KILLEDSELF" ? "%g suicides" : null,
            Printf = msg => printed = msg,
        };
        VmPInteractionTouchObituary.ClientObituary(s);
        Assert.Equal("he suicides", printed);
    }

    [Fact]
    public void ClientObituary_NoSkyWhenShowObituariesOff()
    {
        var s = new VmPInteractionTouchObituary.ClientObituaryState
        {
            HasPlayer = true,
            MoIsSelf = true,
            ShowObituaries = false,
            Printf = _ => { },
        };
        VmPInteractionTouchObituary.ClientObituary(s);
        Assert.Null(s.PrintedMessage);
    }
}
