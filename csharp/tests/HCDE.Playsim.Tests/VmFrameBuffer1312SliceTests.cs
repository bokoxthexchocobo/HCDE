namespace HCDE.Playsim.Tests;

public class VmFrameBuffer1312SliceTests
{
    [Fact]
    public void Update_ClampsTheClientAndResizesBeforeTheVertex()
    {
        var host = new VmFrameBuffer1312.Host { ClientWidth = 10, ClientHeight = 10 };
        var buffer = new VmFrameBuffer1312(host, 8, 9);
        Assert.Equal(8, buffer.GetWidth());
        Assert.Equal(9, buffer.GetHeight());
        Assert.Equal(255f, buffer.ClearColor[3]);
        buffer.VertexData = new VmFrameBuffer1312.Vertices();
        buffer.Update();
        Assert.Equal(320, buffer.Width);
        Assert.Equal(200, buffer.Height);
        Assert.Equal((320, 200), Assert.Single(host.Resized));
        Assert.Equal((320, 200), Assert.Single(buffer.VertexData.Resized));
        buffer.Update();
        Assert.Single(host.Resized);

        buffer.VertexData = null;
        host.ClientWidth = 400;
        host.ClientHeight = 300;
        var missing = Assert.Throws<NullReferenceException>(() => buffer.Update());
        Assert.Contains("mVertexData", missing.Message, StringComparison.Ordinal);
        Assert.Equal(400, buffer.Width);
        Assert.Equal(300, buffer.Height);
        Assert.Equal((400, 300), host.Resized[1]);

        uint[] palette = new uint[256];
        palette[0] = 0x10;
        palette[1] = 0xFF010203;
        palette[255] = 0xFF000001;
        host.Color = index => palette[index];
        Assert.Equal(1, buffer.PaletteColor(-1, 1, out int outside));
        Assert.Equal(0, outside);
        Assert.Equal(1, buffer.PaletteColor(256, 1, out outside));
        Assert.Equal(0, outside);
        Assert.Equal(0, host.PaletteReads);
        Assert.Equal(1, buffer.PaletteColor(0, 1, out int zero));
        Assert.Equal(0x10, zero);
        Assert.Equal(0, buffer.PaletteColor(255, 0, out int skipped));
        Assert.Equal(0, skipped);
        Assert.Equal(2, host.PaletteReads);
        buffer.SetClearColor(1);
        Assert.Equal(1f / 255f, buffer.ClearColor[0]);
        Assert.Equal(2f / 255f, buffer.ClearColor[1]);
        Assert.Equal(3f / 255f, buffer.ClearColor[2]);
        Assert.Equal(1f, buffer.ClearColor[3]);
        Assert.Equal(1, buffer.PaletteColor(255, 1, out int packed));
        Assert.Equal(unchecked((int)0xFF000001), packed);

        host.DrawerWidth = () => 64;
        host.DrawerHeight = () => 32;
        Assert.Equal(1, buffer.ScreenGetWidth(1, out int width));
        Assert.Equal(64, width);
        Assert.Equal(0, buffer.ScreenGetHeight(0, out int height));
        Assert.Equal(0, height);
        var texture = new object();
        var material = buffer.CreateMaterial(texture, 6);
        Assert.Same(texture, material.Texture);
        Assert.Equal(6, material.ScaleFlags);
        Assert.Null(buffer.WipeStart());
        Assert.Null(buffer.WipeEnd());
        int kept = buffer.Width;
        buffer.SetVSync(true);
        buffer.Destroy();
        Assert.Equal(kept, buffer.Width);
    }

    [Fact]
    public void Viewport_LetterboxesHalfPixelsAndCapsTheFrame()
    {
        var host = new VmFrameBuffer1312.Host();
        int scaleCalls = 0;
        host.WidthOf = (width, _) =>
        {
            scaleCalls++;
            return width;
        };
        var buffer = new VmFrameBuffer1312(host, 320, 200);
        var bounds = new VmFrameBuffer1312.Rect { Left = 4, Top = 5, Width = 6, Height = 7 };
        int callsBefore = scaleCalls;
        buffer.SetViewportRects(bounds);
        Assert.Equal(callsBefore, scaleCalls);
        Assert.Equal(6, buffer.ScreenViewport.Width);
        Assert.Equal(6, buffer.SceneViewport.Width);
        Assert.Equal(4, buffer.OutputLetterbox.Left);
        Assert.Equal(6, buffer.GameScreenWidth);
        Assert.Equal(7, buffer.GameScreenHeight);

        host.ClientWidth = 0;
        host.ClientHeight = 0;
        buffer.SetViewportRects(null);
        Assert.Equal(160, buffer.OutputLetterbox.Width);
        Assert.Equal(100, buffer.OutputLetterbox.Height);
        Assert.Equal(10, buffer.OutputLetterbox.Top);
        Assert.Equal(320, buffer.GameScreenWidth);

        buffer.SetVirtualSize(3, 10);
        host.HeightOf = (width, _) => width;
        host.ClientWidth = 100;
        host.ClientHeight = 5;
        host.HasSceneRect = true;
        host.Scene = () => new VmFrameBuffer1312.Rect { Left = -3, Top = 1, Width = 3, Height = 10 };
        buffer.ScaleViewport = false;
        buffer.SetViewportRects(null);
        Assert.Equal(2, buffer.OutputLetterbox.Width);
        Assert.Equal(5, buffer.OutputLetterbox.Height);
        Assert.Equal(49, buffer.OutputLetterbox.Left);
        Assert.Equal(3, buffer.ScreenViewport.Width);
        Assert.Equal(-3, buffer.SceneViewport.Left);

        buffer.ScaleViewport = true;
        buffer.SetViewportRects(null);
        Assert.Equal(2, buffer.ScreenViewport.Width);
        Assert.Equal(5, buffer.ScreenViewport.Height);
        Assert.Equal(-2, buffer.SceneViewport.Left);
        Assert.Equal(1, buffer.SceneViewport.Top);
        Assert.Equal(2, buffer.SceneViewport.Width);
        Assert.Equal(5, buffer.SceneViewport.Height);
        Assert.Equal(3, buffer.GameScreenWidth);
        Assert.Equal(10, buffer.GameScreenHeight);
        Assert.Equal(2, buffer.ScreenToWindowX(3));
        Assert.Equal(0, buffer.ScreenToWindowY(10));
        short x = 50;
        short y = 0;
        buffer.ScaleCoordsFromWindow(ref x, ref y);
        Assert.Equal((short)1, x);
        x = 51;
        buffer.ScaleCoordsFromWindow(ref x, ref y);
        Assert.Equal((short)3, x);

        host.Fullscreen = true;
        buffer.SetViewportRects(null);
        Assert.Equal(3, buffer.ScreenViewport.Width);
        host.Fullscreen = false;
        host.HeightOf = (_, height) => height;
        buffer.SetViewportRects(null);
        Assert.Equal(3, buffer.ScreenViewport.Width);

        var clock = new Queue<long>();
        host.Now = clock.Dequeue;
        buffer.MaxFps = 0;
        buffer.FpsLimit();
        Assert.Empty(host.SleepNs);
        buffer.MaxFps = 10;
        buffer.CapFps = true;
        buffer.FpsLimit();
        Assert.Empty(host.SleepNs);
        buffer.CapFps = false;
        clock.Enqueue(0);
        clock.Enqueue(100_000);
        buffer.FpsLimit();
        Assert.Equal(98_000_000L, Assert.Single(host.SleepNs));
        Assert.Equal(100_000ul, buffer.FpsLimitTime);

        host.SleepNs.Clear();
        clock.Enqueue(0);
        buffer.FpsLimitTime = 1_000_000;
        buffer.MaxFps = 1;
        buffer.FpsLimit();
        Assert.Empty(host.SleepNs);
        Assert.Equal(0ul, buffer.FpsLimitTime);

        host.SleepNs.Clear();
        buffer.MaxFps = 1_000;
        clock.Enqueue(0);
        clock.Enqueue(1_000);
        buffer.FpsLimit();
        Assert.Equal(0L, Assert.Single(host.SleepNs));
    }
}
