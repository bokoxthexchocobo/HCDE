using S = HCDE.Playsim.VmSpanDrawer1407Slice;

namespace HCDE.Playsim.Tests;

public class VmSpanDrawer1407SliceTests
{
    [Fact]
    public void StylePicksTheSpanAndTextureShrinksOversizedBits()
    {
        var args = new S.Args();
        Assert.Equal(S.PlainSpan, args.SpanFunc);
        Assert.Null(args.SrcBlend);
        Assert.Equal(1, S.BandStart);
        Assert.Equal(150, S.BandEnd);
        Assert.Equal(65536, S.Opaque);
        Assert.Equal(65536, S.FracUnit);
        Assert.Equal(10, S.BlendShift);

        string? drawnFunc = null;
        string? drawnView = "set";
        args.DrawSpan((func, view) =>
        {
            drawnFunc = func;
            drawnView = view;
        });
        Assert.Equal(S.PlainSpan, drawnFunc);
        Assert.Null(drawnView);

        var open = new S.Camera(-1, 0, null);
        args.SetStyle(false, false, 0, "sector", true, open);
        Assert.Equal(S.TranslucentSpan, args.SpanFunc);
        Assert.Equal(new S.Blend(S.BlendTable.Col2Rgb8, 0), args.SrcBlend);
        Assert.Equal(new S.Blend(S.BlendTable.Col2Rgb8, 64), args.DestBlend);
        Assert.Equal(0, args.SrcAlpha);
        Assert.Equal(S.Opaque, args.DestAlpha);
        Assert.Equal("sector", args.Colormap);
        Assert.Equal(0, args.Shade);

        args.SetStyle(false, true, S.Opaque, "sector", true, new S.Camera(0, 9, "ignored"));
        Assert.Equal(S.AddSpan, args.SpanFunc);
        Assert.Equal(new S.Blend(S.BlendTable.LessPrecision, 64), args.SrcBlend);
        Assert.Equal(new S.Blend(S.BlendTable.LessPrecision, 64), args.DestBlend);
        Assert.Equal(S.FracUnit, args.DestAlpha);
        Assert.Equal(S.FullNormalLight, args.Colormap);
        Assert.Equal(0f, args.Light);
        Assert.Equal(9, args.Shade);

        args.SetStyle(true, false, S.Opaque, "floor", true, open);
        Assert.Equal(S.MaskedSpan, args.SpanFunc);
        Assert.Equal(new S.Blend(S.BlendTable.LessPrecision, 64), args.SrcBlend);
        Assert.Equal(S.FracUnit, args.DestAlpha);
        Assert.Equal("floor", args.Colormap);
        Assert.Equal(9, args.Shade);

        args.SetStyle(true, false, -1024, "floor", true, new S.Camera(-1, 4, "invul"));
        Assert.Equal(S.MaskedTranslucent, args.SpanFunc);
        Assert.Equal(new S.Blend(S.BlendTable.Col2Rgb8, -1), args.SrcBlend);
        Assert.Equal(new S.Blend(S.BlendTable.Col2Rgb8, 65), args.DestBlend);
        Assert.Equal(-1024, args.SrcAlpha);
        Assert.Equal(66560, args.DestAlpha);
        Assert.Equal("invul", args.Colormap);
        Assert.Equal(0, args.Shade);

        args.SetStyle(false, false, S.Opaque, "kept", false, new S.Camera(3, 2, null));
        Assert.Equal(S.PlainSpan, args.SpanFunc);
        Assert.Equal("kept", args.Colormap);
        Assert.Equal(2, args.Shade);

        int styles = 0;
        var tex = new S.Texture(100, 64, 8, 6, true, () => "bgra", style =>
        {
            Assert.Equal("normal", style);
            return "pal";
        });
        args.SetTexture(true, tex, () => throw new InvalidOperationException("style"));
        Assert.Equal(7, args.XBits);
        Assert.Equal(6, args.YBits);
        Assert.Equal("bgra", args.Source);
        Assert.True(args.Mipmapped);

        args.SetTexture(false, tex with { Width = 128, WidthBits = 7, Height = 1, Mipmapped = true }, () =>
        {
            styles++;
            return "normal";
        });
        Assert.Equal(7, args.XBits);
        Assert.Equal(5, args.YBits);
        Assert.Equal("pal", args.Source);
        Assert.Equal(1, styles);
        Assert.False(args.Mipmapped);

        args.SetTexture(false, tex with { Width = 0, Height = 0, WidthBits = 0, HeightBits = 0, Mipmapped = true }, () => "normal");
        Assert.Equal(-1, args.XBits);
        Assert.Equal(-1, args.YBits);
        Assert.False(args.Mipmapped);
    }

    [Fact]
    public void DrawsSetDestBeforeTheShadeAndTheNamedDrawer()
    {
        var args = new S.Args();
        var order = new List<string>();
        S.Tilted? tilted = null;
        args.DrawTiltedSpan("view", 4, 1, 9, "sz", "su", "sv", true, 128, true, 0.5f, 3, 5, "map", (level, foggy, viewport) =>
        {
            order.Add("shade");
            Assert.Equal(128, level);
            Assert.True(foggy);
            Assert.Equal("view", viewport);
            Assert.Equal(4, args.DestY);
            Assert.Equal(1, args.DestX1);
            Assert.Equal(9, args.DestX2);
            Assert.Equal("view", args.Viewport);
            return 42;
        }, call =>
        {
            order.Add("draw");
            tilted = call;
        });
        Assert.Equal(new[] { "shade", "draw" }, order);
        Assert.Equal(42, tilted!.Value.Shade);
        Assert.Equal("sz", tilted.Value.PlaneSz);
        Assert.Equal("su", tilted.Value.PlaneSu);
        Assert.Equal("sv", tilted.Value.PlaneSv);
        Assert.True(tilted.Value.PlaneShade);
        Assert.Equal(0.5f, tilted.Value.PlaneLight);
        Assert.Equal(3, tilted.Value.ViewX);
        Assert.Equal(5, tilted.Value.ViewY);
        Assert.Equal("map", tilted.Value.BaseColormap);
        Assert.Equal("view", tilted.Value.Viewport);

        order.Clear();
        Assert.Throws<InvalidOperationException>(() => args.DrawTiltedSpan("view", 8, 2, 3, "sz", "su", "sv", false, 1, false, 1f, 0, 0, "map", (_, _, _) =>
        {
            order.Add("shade");
            throw new InvalidOperationException("shade");
        }, _ => order.Add("draw")));
        Assert.Equal(new[] { "shade" }, order);
        Assert.Equal(8, args.DestY);
        Assert.Equal(2, args.DestX1);
        Assert.Equal(3, args.DestX2);

        string? fogView = null;
        args.DrawFogBoundaryLine("fog", 6, 0, 2, (view, y, x1, x2) =>
        {
            fogView = view;
            Assert.Equal(6, y);
            Assert.Equal(0, x1);
            Assert.Equal(2, x2);
        });
        Assert.Equal("fog", fogView);

        string? colorView = null;
        args.DrawColoredSpan(null, 1, 4, 5, (view, y, x1, x2) =>
        {
            colorView = view;
            Assert.Equal(1, y);
            Assert.Equal(4, x1);
            Assert.Equal(5, x2);
        });
        Assert.Null(colorView);
        Assert.Null(args.Viewport);

        string? spanView = "pending";
        args.DrawSpan((func, view) =>
        {
            Assert.Equal(S.PlainSpan, func);
            spanView = view;
        });
        Assert.Null(spanView);
        Assert.Throws<ArgumentNullException>(() => args.DrawSpan(null!));
        Assert.Throws<ArgumentNullException>(() => args.DrawFogBoundaryLine("fog", 0, 0, 0, null!));
    }
}
