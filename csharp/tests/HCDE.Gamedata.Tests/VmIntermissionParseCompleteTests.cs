namespace HCDE.Gamedata.Tests;

public class VmIntermissionParseCompleteTests
{
    [Fact]
    public void CastAction_ParsesAttackSound()
    {
        var cast = new VmIntermissionActionCast();
        var errors = new List<string>();
        Assert.True(VmIntermissionActionParser.TryParseBlock(
            "CastName = \"DOOM\" AttackSound = \"Missile\" , 1 , \"weapons/pistol\"", cast, errors), string.Join("; ", errors));
        Assert.Equal("DOOM", cast.CastName);
        var snd = Assert.Single(cast.CastSounds);
        Assert.Equal("Missile", snd.Sequence);
        Assert.Equal(1, snd.Index);
    }

    [Fact]
    public void DocumentParser_RegistersCastAndLink()
    {
        var reg = new VmIntermissionRegistry();
        var errors = new List<string>();
        const string doc = """
            {
              cast { CastClass = "DoomGuy" }
              Link = NextMap
              GotoTitle
            }
            """;
        Assert.True(VmIntermissionDocumentParser.TryParse("INT1", doc, reg, errors), string.Join("; ", errors));
        var desc = reg.Descriptors["INT1"];
        Assert.Equal("NextMap", desc.Link);
        Assert.Contains(desc.Actions, a => a is VmIntermissionActionCast);
        Assert.Contains(desc.Actions, a => a.GotoTitle);
    }

    [Fact]
    public void TextLump_UsesResolver()
    {
        var text = new VmIntermissionActionTextscreen
        {
            LumpResolver = new StubLumpResolver("LUMP", "Hello", null),
        };
        var errors = new List<string>();
        Assert.True(VmIntermissionActionParser.TryParseBlock("TextLump = \"LUMP\"", text, errors));
        Assert.Equal("Hello", text.Text);
    }

    [Fact]
    public void EndGame_BunnyScrollBuildsDescriptor()
    {
        VmEndGameParser.ResetGeneratedNames();
        var errors = new List<string>();
        const string body = "{ hscroll = \"BUNNY1\" , \"BUNNY2\" 1 music = \"TUNE\" 0 }";
        Assert.True(VmEndGameParser.TryParse(body, out var result, errors), string.Join("; ", errors));
        Assert.Equal("TheEnd", result.Descriptor.Link);
        var scroll = Assert.IsType<VmIntermissionActionScroller>(Assert.Single(result.Descriptor.Actions));
        Assert.Equal(VmIntermissionScrollDir.Left, scroll.ScrollDir);
        Assert.Equal(1130, scroll.DurationTics);
    }

    [Fact]
    public void EndSequenceResolver_MapsLegacyKeywords()
    {
        Assert.True(VmEndSequenceResolver.TryResolveKeyword("endbunny", out var name, out _));
        Assert.Equal("Inter_Bunny", name);
        var reg = new VmIntermissionRegistry();
        Assert.True(VmEndSequenceResolver.TryMakeEndPic("VICTORY", reg, out var seq));
        Assert.Equal("@EndPic_VICTORY", seq);
        Assert.Equal("VICTORY", reg.Descriptors[seq].Actions[0].Background);
    }

    [Fact]
    public void FinaleBuilder_AddsWiperWhenEnding()
    {
        var reg = new VmIntermissionRegistry();
        var req = new VmFinaleBuildRequest
        {
            Text = "FINALE",
            LookupText = true,
            Flat = "FLOOR",
            FinalePic = false,
            Music = "D_FINALE",
            MusicOrder = 2,
            Ending = true,
            EndSequence = "Inter_Cast",
        };
        Assert.True(VmFinaleIntermissionBuilder.TryBuild(req, null, reg, out var desc, out _));
        Assert.Equal("$FINALE", Assert.IsType<VmIntermissionActionTextscreen>(desc!.Actions[0]).Text);
        Assert.IsType<VmIntermissionActionWiper>(desc.Actions[1]);
        Assert.Equal("Inter_Cast", desc.Link);
    }

    [Fact]
    public void ScreenInit_ResolvesFinalePageIndex()
    {
        var pages = new[] { "PAGE0", "PAGE1" };
        Assert.Equal("PAGE1", VmIntermissionScreenInit.ResolveBackground("@2", pages, "TITLE", _ => null));
        Assert.Equal("LOC", VmIntermissionScreenInit.ResolveBackground("$KEY", pages, "TITLE", k => "LOC"));
    }

    private sealed class StubLumpResolver(string lump, string text, string? hexen) : IIntermissionTextLumpResolver
    {
        public bool TryResolve(string lumpName, out string resolved, out string? hexenLookupKey)
        {
            if (!lump.Equals(lumpName, StringComparison.OrdinalIgnoreCase)) { resolved = ""; hexenLookupKey = null; return false; }
            resolved = text;
            hexenLookupKey = hexen;
            return true;
        }
    }
}
