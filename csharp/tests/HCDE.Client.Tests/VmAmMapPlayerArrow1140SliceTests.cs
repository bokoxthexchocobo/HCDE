namespace HCDE.Client.Tests;

public class VmAmMapPlayerArrow1140SliceTests
{
    [Fact]
    public void FinishGlyph_ScalesRotatesAndTranslatesBothEnds()
    {
        var plain = VmAmMapPlayerArrow1140Slice.FinishGlyph(1, 2, 3, 4, 2, false, new VmAmMapWallColor1139Slice.Spin(0, 1), 0, 0);
        Assert.Equal((2d, 4d, 6d, 8d), plain);
        var turned = VmAmMapPlayerArrow1140Slice.FinishGlyph(1, 0, 0, 1, 0, true, new VmAmMapWallColor1139Slice.Spin(1, 0), 0, 0);
        Assert.Equal((0d, 1d, -1d, 0d), turned);
    }

    [Fact]
    public void SinglePlayerMark_UsesNorthWhenRotatedAndTheCheatArrowWhenPresent()
    {
        Assert.True(VmAmMapPlayerArrow1140Slice.PreferThingSprites(2, 1));
        Assert.True(VmAmMapPlayerArrow1140Slice.PreferThingSprites(5, 1));
        Assert.False(VmAmMapPlayerArrow1140Slice.PreferThingSprites(4, 1));
        Assert.False(VmAmMapPlayerArrow1140Slice.PreferThingSprites(2, 0));
        var yaw = VmAmMapPlayerArrow1140Slice.SinglePlayerMark(0, 3, 0, true, 4, 5, 30, (x, y) => (x + 1, y));
        Assert.Equal(30d, yaw.AngleDegrees);
        Assert.False(yaw.CheatArrow);
        Assert.Equal(0d, yaw.Scale);
        var north = VmAmMapPlayerArrow1140Slice.SinglePlayerMark(1, 2, 2, true, 4, 5, 30, (x, y) => (x + 1, y));
        Assert.Equal(90d, north.AngleDegrees);
        Assert.Equal(5d, north.X);
        Assert.True(north.CheatArrow);
        var noCheatLines = VmAmMapPlayerArrow1140Slice.SinglePlayerMark(3, 0, 1, false, 4, 5, 30, (x, y) => (x, y));
        Assert.False(noCheatLines.CheatArrow);
    }

    [Fact]
    public void RemoteMark_SkipsAlliesAndDeathmatchStrangers()
    {
        var self = Player(alpha: 1, hue: 0, saturation: 1, value: 1);
        var hidden = VmAmMapPlayerArrow1140Slice.RemoteMark(1, 0, 0, false, false, false, 0, self with { InGame = false }, (x, y) => (x, y));
        Assert.False(hidden.Draw);
        var ally = VmAmMapPlayerArrow1140Slice.RemoteMark(
            1, 0, VmAmMapPlayerArrow1140Slice.NoAutomapAllies, false, false, false, 0, self, (x, y) => (x, y));
        Assert.False(ally.Draw);
        var console = VmAmMapPlayerArrow1140Slice.RemoteMark(
            0, 0, VmAmMapPlayerArrow1140Slice.NoAutomapAllies, true, false, false, 0, self with { Teammate = false, CameraPlayer = true }, (x, y) => (x, y));
        Assert.True(console.Draw);
        var stranger = VmAmMapPlayerArrow1140Slice.RemoteMark(
            2, 0, 0, true, false, false, 0, self with { Teammate = false, CameraPlayer = false }, (x, y) => (x, y));
        Assert.False(stranger.Draw);
        var demo = VmAmMapPlayerArrow1140Slice.RemoteMark(
            2, 0, 0, true, true, false, 0, self with { Teammate = false, CameraPlayer = false }, (x, y) => (x, y));
        Assert.True(demo.Draw);
    }

    [Fact]
    public void RemoteMark_ColorsFromHsvAndRotatesTheOffsetPosition()
    {
        Assert.Equal((255, 0, 0), Bytes(0, 1, 1));
        Assert.Equal((0, 255, 0), Bytes(120, 1, 1));
        Assert.Equal((255, 255, 255), Bytes(40, 0, 1));
        Assert.Equal((byte)127, VmAmMapPlayerArrow1140Slice.ScaleByte(0.5f));
        var faded = VmAmMapPlayerArrow1140Slice.RemoteMark(1, 0, 0, false, false, true, 0, Player(alpha: 0.5), (x, y) => (x + 1, y));
        Assert.True(faded.AlmostBackground);
        Assert.Equal(7d, faded.X);
        Assert.Equal(8d, faded.Y);
        var colored = VmAmMapPlayerArrow1140Slice.RemoteMark(
            1, 0, 0, false, false, true, 0, Player(alpha: 1, hue: 0, saturation: 1, value: 1, yaw: 10), (x, y) => (x + 1, y));
        Assert.False(colored.AlmostBackground);
        Assert.Equal((byte)255, colored.R);
        Assert.Equal(100d, colored.AngleDegrees);
    }

    private static (byte R, byte G, byte B) Bytes(float h, float s, float v)
    {
        var rgb = VmAmMapPlayerArrow1140Slice.HsvToRgb(h, s, v);
        return (
            VmAmMapPlayerArrow1140Slice.ScaleByte(rgb.R),
            VmAmMapPlayerArrow1140Slice.ScaleByte(rgb.G),
            VmAmMapPlayerArrow1140Slice.ScaleByte(rgb.B));
    }

    private static VmAmMapPlayerArrow1140Slice.RemotePlayer Player(
        bool inGame = true,
        bool hasActor = true,
        bool teammate = true,
        bool camera = false,
        double alpha = 1,
        float hue = 0,
        float saturation = 0,
        float value = 1,
        double x = 5,
        double y = 6,
        double ox = 1,
        double oy = 2,
        double yaw = 15)
    {
        return new VmAmMapPlayerArrow1140Slice.RemotePlayer(
            inGame, hasActor, teammate, camera, alpha, hue, saturation, value, x, y, ox, oy, yaw);
    }
}
