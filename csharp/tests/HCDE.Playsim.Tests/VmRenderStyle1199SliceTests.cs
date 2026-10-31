namespace HCDE.Playsim.Tests;

public class VmRenderStyle1199SliceTests
{
    [Fact]
    public void LegacyTable_MatchesNativeStylesAndAlpha()
    {
        VmRenderStyle1199Slice.Reset();
        Assert.Equal(19, VmRenderStyle1199Slice.Legacy.Length);
        Assert.True(VmRenderStyle1199Slice.DrawTrans);
        Assert.Equal(0, VmRenderStyle1199Slice.DrawTransFlags);
        Assert.Equal(1, VmRenderStyle1199Slice.DrawFuzz);
        Assert.Equal(VmRenderStyle1199Slice.Archive, VmRenderStyle1199Slice.DrawFuzzFlags);

        byte[][] expect =
        {
            new byte[] { 0, 0, 0, 0 },
            new byte[] { 1, 2, 3, 2 },
            new byte[] { 4, 2, 3, 0 },
            new byte[] { 1, 2, 3, 1 },
            new byte[] { 5, 2, 3, 0 },
            new byte[] { 1, 2, 3, 10 },
            new byte[] { 1, 2, 3, 0 },
            new byte[] { 1, 2, 1, 0 },
            new byte[] { 1, 2, 3, 12 },
            new byte[] { 1, 2, 3, 8 },
            new byte[] { 8, 0, 0, 0 },
            new byte[] { 3, 2, 1, 0 },
            new byte[] { 1, 2, 1, 8 },
            new byte[] { 1, 2, 1, 12 },
            new byte[] { 1, 6, 0, 0 },
            new byte[] { 1, 7, 0, 0 },
            new byte[] { 1, 4, 5, 0 },
            new byte[] { 1, 1, 0, 0 },
            new byte[] { 1, 4, 1, 0 },
        };
        for (int i = 0; i < expect.Length; i++)
        {
            var style = VmRenderStyle1199Slice.Legacy[i];
            Assert.Equal(expect[i][0], style.BlendOp);
            Assert.Equal(expect[i][1], style.SrcAlpha);
            Assert.Equal(expect[i][2], style.DestAlpha);
            Assert.Equal(expect[i][3], style.Flags);
        }

        Assert.Equal(0, VmRenderStyle1199Slice.GetAlpha(VmRenderStyle1199Slice.AlphaZero, 0.25));
        Assert.Equal(1, VmRenderStyle1199Slice.GetAlpha(VmRenderStyle1199Slice.AlphaOne, 0.25));
        Assert.Equal(0.25, VmRenderStyle1199Slice.GetAlpha(VmRenderStyle1199Slice.AlphaSrc, 0.25));
        Assert.Equal(0.75, VmRenderStyle1199Slice.GetAlpha(VmRenderStyle1199Slice.AlphaInvSrc, 0.25));
        Assert.Equal(0.5, VmRenderStyle1199Slice.GetAlpha(VmRenderStyle1199Slice.AlphaDstCol, 0.25));
        Assert.Equal(0.5, VmRenderStyle1199Slice.GetAlpha(99, 4));
    }

    [Fact]
    public void IsVisible_AndCheckFuzz_FollowBlendAndCvars()
    {
        VmRenderStyle1199Slice.Reset();
        var none = VmRenderStyle1199Slice.Legacy[0];
        var normal = VmRenderStyle1199Slice.Legacy[1];
        var fuzzy = VmRenderStyle1199Slice.Legacy[2];
        var translucent = VmRenderStyle1199Slice.Legacy[6];
        var add = VmRenderStyle1199Slice.Legacy[7];
        var shadow = VmRenderStyle1199Slice.Legacy[10];
        var subtract = VmRenderStyle1199Slice.Legacy[11];
        var source = VmRenderStyle1199Slice.Legacy[17];
        Assert.False(VmRenderStyle1199Slice.IsVisible(none, 1));
        Assert.True(VmRenderStyle1199Slice.IsVisible(normal, 0));
        Assert.True(VmRenderStyle1199Slice.IsVisible(fuzzy, 0));
        Assert.False(VmRenderStyle1199Slice.IsVisible(translucent, 0));
        Assert.True(VmRenderStyle1199Slice.IsVisible(translucent, 0.25));
        Assert.False(VmRenderStyle1199Slice.IsVisible(add, 0));
        Assert.False(VmRenderStyle1199Slice.IsVisible(add, -2));
        Assert.True(VmRenderStyle1199Slice.IsVisible(add, 3));
        Assert.True(VmRenderStyle1199Slice.IsVisible(shadow, 0));
        Assert.False(VmRenderStyle1199Slice.IsVisible(subtract, 0));
        Assert.True(VmRenderStyle1199Slice.IsVisible(source, 0));

        var sub = new VmRenderStyle1199Slice.Style(VmRenderStyle1199Slice.OpSub, 0, 0, 0);
        Assert.True(VmRenderStyle1199Slice.IsVisible(sub, 0));
        var hidden = new VmRenderStyle1199Slice.Style(
            VmRenderStyle1199Slice.OpAdd,
            VmRenderStyle1199Slice.AlphaZero,
            VmRenderStyle1199Slice.AlphaOne,
            0);
        Assert.False(VmRenderStyle1199Slice.IsVisible(hidden, 0.5));

        var opt = VmRenderStyle1199Slice.Legacy[4];
        VmRenderStyle1199Slice.DrawFuzz = 0;
        Assert.Equal(VmRenderStyle1199Slice.OpAdd, VmRenderStyle1199Slice.CheckFuzz(opt).BlendOp);
        var orSub = new VmRenderStyle1199Slice.Style(VmRenderStyle1199Slice.OpFuzzOrSub, 2, 3, 0);
        Assert.Equal(VmRenderStyle1199Slice.OpSub, VmRenderStyle1199Slice.CheckFuzz(orSub).BlendOp);
        var orRev = new VmRenderStyle1199Slice.Style(VmRenderStyle1199Slice.OpFuzzOrRevSub, 2, 1, 0);
        Assert.Equal(VmRenderStyle1199Slice.OpRevSub, VmRenderStyle1199Slice.CheckFuzz(orRev).BlendOp);

        VmRenderStyle1199Slice.DrawTrans = false;
        Assert.Equal(VmRenderStyle1199Slice.OpFuzz, VmRenderStyle1199Slice.CheckFuzz(opt).BlendOp);
        VmRenderStyle1199Slice.DrawTrans = true;
        VmRenderStyle1199Slice.DrawFuzz = 2;
        Assert.Equal(VmRenderStyle1199Slice.OpShadow, VmRenderStyle1199Slice.CheckFuzz(opt).BlendOp);
        VmRenderStyle1199Slice.DrawFuzz = 3;
        Assert.Equal(VmRenderStyle1199Slice.OpFuzz, VmRenderStyle1199Slice.CheckFuzz(opt).BlendOp);
        Assert.Equal(VmRenderStyle1199Slice.OpFuzz, VmRenderStyle1199Slice.CheckFuzz(fuzzy).BlendOp);
        Assert.Equal(VmRenderStyle1199Slice.OpAdd, VmRenderStyle1199Slice.CheckFuzz(normal).BlendOp);
        Assert.Equal(VmRenderStyle1199Slice.OpShadow, VmRenderStyle1199Slice.CheckFuzz(shadow).BlendOp);
    }
}
