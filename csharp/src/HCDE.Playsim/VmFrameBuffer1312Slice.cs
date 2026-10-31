namespace HCDE.Playsim;

/// <summary>
/// Viewport letterbox, frame cap, and the screen palette color from
/// <c>v_framebuffer.cpp</c> (1-311). Chunk 1312.
/// </summary>
public sealed class VmFrameBuffer1312
{
    public const int MinWidth = 320;
    public const int MinHeight = 200;
    public const int MinimizedWidth = 160;
    public const int MinimizedHeight = 120;

    readonly Host _host;

    public VmFrameBuffer1312(Host host, int width, int height)
    {
        _host = host;
        SetSize(width, height);
    }

    public bool ScaleViewport { get; set; } = true;
    public bool CapFps { get; set; }
    public int MaxFps { get; set; } = 500;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int GameScreenWidth { get; private set; }
    public int GameScreenHeight { get; private set; }
    public ulong FpsLimitTime { get; set; }
    public Rect ScreenViewport { get; private set; }
    public Rect SceneViewport { get; private set; }
    public Rect OutputLetterbox { get; private set; }
    public float[] ClearColor { get; } = { 0f, 0f, 0f, 255f };
    public Vertices? VertexData { get; set; }

    public int GetWidth() => Width;

    public int GetHeight() => Height;

    public void SetSize(int width, int height)
    {
        Width = _host.ScaledWidth(width, height);
        Height = _host.ScaledHeight(width, height);
    }

    public void SetVirtualSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void Update()
    {
        int initialWidth = _host.ClientWidth;
        int initialHeight = _host.ClientHeight;
        int clientWidth = _host.ScaledWidth(initialWidth, initialHeight);
        int clientHeight = _host.ScaledHeight(initialWidth, initialHeight);
        if (clientWidth < MinWidth)
            clientWidth = MinWidth;
        if (clientHeight < MinHeight)
            clientHeight = MinHeight;
        if (clientWidth > 0 && clientHeight > 0 && (GetWidth() != clientWidth || GetHeight() != clientHeight))
        {
            SetVirtualSize(clientWidth, clientHeight);
            _host.NoteResized(clientWidth, clientHeight);
            if (VertexData == null)
                throw new NullReferenceException("mVertexData");
            VertexData.OutputResized(clientWidth, clientHeight);
        }
    }

    public void SetClearColor(int color)
    {
        uint packed = _host.Palette(color);
        ClearColor[0] = ((packed >> 16) & 255) / 255f;
        ClearColor[1] = ((packed >> 8) & 255) / 255f;
        ClearColor[2] = (packed & 255) / 255f;
        ClearColor[3] = 1f;
    }

    public void SetVSync(bool vsync)
    {
        _ = vsync;
    }

    public object? WipeStart()
    {
        return null;
    }

    public object? WipeEnd()
    {
        return null;
    }

    public void Destroy()
    {
        _ = Width + Height;
    }

    public void SetViewportRects(Rect? bounds)
    {
        if (bounds.HasValue)
        {
            Rect copy = bounds.Value;
            SceneViewport = copy;
            ScreenViewport = copy;
            OutputLetterbox = copy;
            GameScreenWidth = ScreenViewport.Width;
            GameScreenHeight = ScreenViewport.Height;
            return;
        }

        int clientWidth = _host.ClientWidth;
        int clientHeight = _host.ClientHeight;
        if (clientWidth == 0 || clientHeight == 0)
        {
            clientWidth = MinimizedWidth;
            clientHeight = MinimizedHeight;
        }

        int screenWidth = GetWidth();
        int screenHeight = GetHeight();
        float aspect = _host.PixelAspect();
        float scaleX = Math.Min(clientWidth / (float)screenWidth, clientHeight / ((float)screenHeight * aspect));
        float scaleY = scaleX * aspect;
        var letter = new Rect
        {
            Width = RoundToInt(screenWidth * scaleX),
            Height = RoundToInt(screenHeight * scaleY),
        };
        letter.Left = (clientWidth - letter.Width) / 2;
        letter.Top = (clientHeight - letter.Height) / 2;
        OutputLetterbox = letter;
        ScreenViewport = new Rect { Width = screenWidth, Height = screenHeight };
        SceneViewport = _host.HasSceneRect ? _host.SceneRect() : ScreenViewport;

        bool notScaled = ScreenViewport.Width == _host.ScaledWidth(ScreenViewport.Width, ScreenViewport.Height)
            && ScreenViewport.Width == _host.ScaledHeight(ScreenViewport.Width, ScreenViewport.Height)
            && (double)_host.PixelAspect() == 1.0;
        if (ScaleViewport && !_host.Fullscreen && notScaled)
        {
            var screen = ScreenViewport;
            screen.Width = OutputLetterbox.Width;
            screen.Height = OutputLetterbox.Height;
            ScreenViewport = screen;
            var scene = SceneViewport;
            scene.Left = RoundToInt(scene.Left * scaleX);
            scene.Top = RoundToInt(scene.Top * scaleY);
            scene.Width = RoundToInt(scene.Width * scaleX);
            scene.Height = RoundToInt(scene.Height * scaleY);
            SceneViewport = scene;
        }

        GameScreenWidth = GetWidth();
        GameScreenHeight = GetHeight();
    }

    public int ScreenToWindowX(int x)
    {
        return ScreenViewport.Left + RoundToInt(x * ScreenViewport.Width / (float)GameScreenWidth);
    }

    public int ScreenToWindowY(int y)
    {
        return ScreenViewport.Top + ScreenViewport.Height - RoundToInt(y * ScreenViewport.Height / (float)GameScreenHeight);
    }

    public void ScaleCoordsFromWindow(ref short x, ref short y)
    {
        int localX = (x - OutputLetterbox.Left) * Width / OutputLetterbox.Width;
        int localY = (y - OutputLetterbox.Top) * Height / OutputLetterbox.Height;
        x = (short)localX;
        y = (short)localY;
    }

    public void FpsLimit()
    {
        if (MaxFps <= 0 || CapFps)
            return;
        ulong target = FpsLimitTime + (ulong)(1_000_000 / MaxFps);
        while (true)
        {
            FpsLimitTime = (ulong)_host.NowMicros();
            long wait = unchecked((long)(target - FpsLimitTime));
            if (wait > 1_000_000 || wait <= 0)
                break;
            if (wait <= 2_000)
                _host.Sleep(0);
            else
                _host.Sleep((wait - 2_000) * 1_000L);
        }
    }

    public Material CreateMaterial(object texture, int scaleFlags)
    {
        return new Material(texture, scaleFlags);
    }

    public int ScreenGetWidth(int numret, out int value)
    {
        return ReturnInt(numret, _host.TwoDWidth(), out value);
    }

    public int ScreenGetHeight(int numret, out int value)
    {
        return ReturnInt(numret, _host.TwoDHeight(), out value);
    }

    public int PaletteColor(int index, int numret, out int value)
    {
        if (index < 0 || index > 255)
            index = 0;
        else
            index = unchecked((int)_host.Palette(index));
        return ReturnInt(numret, index, out value);
    }

    static int RoundToInt(float value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

    static int ReturnInt(int numret, int raw, out int value)
    {
        if (numret > 0)
        {
            value = raw;
            return 1;
        }

        value = 0;
        return 0;
    }

    public struct Rect
    {
        public int Left;
        public int Top;
        public int Width;
        public int Height;
    }

    public sealed class Vertices
    {
        public List<(int Width, int Height)> Resized { get; } = new();

        public void OutputResized(int width, int height) => Resized.Add((width, height));
    }

    public sealed class Material
    {
        public Material(object texture, int scaleFlags)
        {
            Texture = texture;
            ScaleFlags = scaleFlags;
        }

        public object Texture { get; }
        public int ScaleFlags { get; }
    }

    public sealed class Host
    {
        public int ClientWidth { get; set; }
        public int ClientHeight { get; set; }
        public bool Fullscreen { get; set; }
        public bool HasSceneRect { get; set; }
        public Func<Rect>? Scene { get; set; }
        public Func<int, int, int> WidthOf { get; set; } = static (width, _) => width;
        public Func<int, int, int> HeightOf { get; set; } = static (_, height) => height;
        public Func<float> Aspect { get; set; } = static () => 1f;
        public Func<int, uint>? Color { get; set; }
        public Func<long>? Now { get; set; }
        public Func<int>? DrawerWidth { get; set; }
        public Func<int>? DrawerHeight { get; set; }
        public List<long> SleepNs { get; } = new();
        public List<(int Width, int Height)> Resized { get; } = new();
        public int PaletteReads { get; private set; }

        public int ScaledWidth(int width, int height) => WidthOf(width, height);

        public int ScaledHeight(int width, int height) => HeightOf(width, height);

        public float PixelAspect() => Aspect();

        public Rect SceneRect()
        {
            if (Scene == null)
                throw new InvalidOperationException("GetSceneRect");
            return Scene();
        }

        public uint Palette(int index)
        {
            PaletteReads++;
            if (Color == null)
                throw new InvalidOperationException("palette");
            return Color(index);
        }

        public long NowMicros()
        {
            if (Now == null)
                throw new InvalidOperationException("clock");
            return Now();
        }

        public void Sleep(long nanoseconds) => SleepNs.Add(nanoseconds);

        public void NoteResized(int width, int height) => Resized.Add((width, height));

        public int TwoDWidth()
        {
            if (DrawerWidth == null)
                throw new InvalidOperationException("twod");
            return DrawerWidth();
        }

        public int TwoDHeight()
        {
            if (DrawerHeight == null)
                throw new InvalidOperationException("twod");
            return DrawerHeight();
        }
    }
}
