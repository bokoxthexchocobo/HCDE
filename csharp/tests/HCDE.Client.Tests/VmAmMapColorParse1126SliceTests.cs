namespace HCDE.Client.Tests;

public class VmAmMapColorParse1126SliceTests
{
    [Fact]
    public void RavenArm_ShowsLocksAndBaseDoomClearsThem()
    {
        Assert.Equal(("raven", true), VmAmMapColorParse1126Slice.RavenArm());
        var edit = VmAmMapColorParse1126Slice.ParseAmColors(
            ["{", "base", "=", "Doom", "}"],
            _ => 0);
        Assert.Null(edit.Error);
        Assert.Equal(VmAmMapColorParse1126Slice.BasePalette.Doom, edit.Base);
        Assert.False(edit.DisplayLocks);
        Assert.True(edit.Defined);
        Assert.False(edit.Whitened);
    }

    [Fact]
    public void ParseAmColors_RejectsBaseAfterAColorAndUnknownKeys()
    {
        var lateBase = VmAmMapColorParse1126Slice.ParseAmColors(
            ["{", "WallColor", "=", "ff0000", "base", "=", "doom", "}"],
            text => Convert.ToUInt32(text, 16));
        Assert.Equal("'base' must be specified before the first color", lateBase.Error);

        var unknown = VmAmMapColorParse1126Slice.ParseAmColors(
            ["{", "nope", "=", "ff0000", "}"],
            _ => 0);
        Assert.Equal("Unknown key 'nope'", unknown.Error);
    }

    [Fact]
    public void ParseAmColors_WritesOpaqueWallFromResolver()
    {
        var edit = VmAmMapColorParse1126Slice.ParseAmColors(
            ["{", "showlocks", "=", "true", "wallcolor", "=", "red", "}"],
            text => text == "red" ? 0x2c1808u : 0u);
        Assert.Null(edit.Error);
        Assert.True(edit.DisplayLocks);
        Assert.Equal(0xFF2c1808u, edit.Colors[2]);
        Assert.True(edit.AnyColorWritten);
    }

    [Fact]
    public void ArrowLump_ScalesByPlayerRadiusAndStaticInitClears()
    {
        Assert.Equal(18, VmAmMapColorParse1126Slice.ArrowRadiusUnits());
        Assert.Equal(18, VmAmMapColorParse1126Slice.ScaleArrowComponent(1));
        var lines = VmAmMapColorParse1126Slice.ParseArrowLump("(0.5,-0.25),(1,0)");
        Assert.Equal(9d, lines[0].Ax);
        Assert.Equal(-4.5, lines[0].Ay);
        Assert.Equal(18d, lines[0].Bx);
        Assert.Empty(VmAmMapColorParse1126Slice.ParseArrowLump(null));
        var triangle = VmAmMapColorParse1126Slice.ThinTriangle();
        Assert.Equal(-0.5, triangle[0].Ax);
        Assert.Equal(-0.7, triangle[0].Ay);
        var map = new List<VmAmMapColorParse1126Slice.Segment> { triangle[0] };
        var cheat = new List<VmAmMapColorParse1126Slice.Segment> { triangle[1] };
        var key = new List<VmAmMapColorParse1126Slice.Segment>();
        var easy = new List<VmAmMapColorParse1126Slice.Segment> { triangle[2] };
        VmAmMapColorParse1126Slice.ClearStaticArrows(map, cheat, key, easy);
        Assert.Empty(map);
        Assert.Empty(cheat);
        Assert.Empty(easy);
    }
}
