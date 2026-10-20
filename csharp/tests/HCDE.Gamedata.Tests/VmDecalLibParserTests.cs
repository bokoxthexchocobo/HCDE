namespace HCDE.Gamedata.Tests;

public class VmDecalLibParserTests
{
    [Fact]
    public void ReadScale_ClampsToNativeBounds()
    {
        Assert.Equal(1 / 256.0, VmDecalLibParser.ReadScale(0));
        Assert.Equal(256, VmDecalLibParser.ReadScale(500));
    }

    [Fact]
    public void ParseDecalBody_AppliesShadeKeyword()
    {
        var tokens = VmLockDefTokenize.Tokenize("{ shade FF0000 }");
        var i = 0;
        var template = new VmDecalTemplate();
        var errors = new List<string>();
        Assert.True(VmDecalLibParser.TryParseDecalBody(tokens, ref i, template, errors), string.Join("; ", errors));
        Assert.Equal("Shaded", template.RenderStyle);
        Assert.Equal("FF0000", template.ShadeColor);
    }

    [Fact]
    public void ParseDecalBody_AppliesScaleAndFlipFlags()
    {
        var tokens = VmLockDefTokenize.Tokenize("{ x-scale 2 y-scale 0.5 flipx randomflipy add 0.25 }");
        var i = 0;
        var template = new VmDecalTemplate();
        var errors = new List<string>();
        Assert.True(VmDecalLibParser.TryParseDecalBody(tokens, ref i, template, errors), string.Join("; ", errors));
        Assert.Equal(2, template.ScaleX);
        Assert.Equal(0.5, template.ScaleY);
        Assert.True(template.FlipX);
        Assert.True(template.RandomFlipY);
        Assert.Equal("Add", template.RenderStyle);
        Assert.Equal(0.25, template.Alpha);
    }

    [Fact]
    public void TryParseDecalId_RejectsOutOfRange()
    {
        Assert.False(VmDecalLibParser.TryParseDecalId("70000", out _, out _));
        Assert.True(VmDecalLibParser.TryParseDecalId("12", out var id, out var numeric));
        Assert.True(numeric);
        Assert.Equal((ushort)12, id);
    }
}
