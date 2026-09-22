namespace HCDE.Client;

public readonly record struct HardwareQuad(int X, int Y, int Width, int Height, uint Argb);

public interface IHardwareDevice
{
    string BackendName { get; }
    bool IsNative { get; }
    void BeginFrame();
    void DrawQuad(int x, int y, int width, int height, uint argb);
    void Blit(SoftwareFrame frame);
}

/// <summary>
/// Records quads and blits them on the CPU. Vulkan is probed separately and does not draw.
/// </summary>
public sealed class RecordedHardwareDevice : IHardwareDevice
{
    private bool _open;

    public string BackendName => "recorded";
    public bool IsNative => false;
    public List<HardwareQuad> Quads { get; } = new();

    public void BeginFrame()
    {
        Quads.Clear();
        _open = true;
    }

    public void DrawQuad(int x, int y, int width, int height, uint argb)
    {
        if (!_open)
            throw new InvalidOperationException("hardware frame is not open");
        if (width <= 0 || height <= 0)
            return;
        Quads.Add(new HardwareQuad(x, y, width, height, argb));
    }

    public void Blit(SoftwareFrame frame)
    {
        foreach (var quad in Quads)
        {
            var r = (byte)((quad.Argb >> 16) & 0xFF);
            var g = (byte)((quad.Argb >> 8) & 0xFF);
            var b = (byte)(quad.Argb & 0xFF);
            var x1 = Math.Min(frame.Width, Math.Max(0, quad.X + quad.Width));
            var y1 = Math.Min(frame.Height, Math.Max(0, quad.Y + quad.Height));
            for (var y = Math.Max(0, quad.Y); y < y1; y++)
            {
                for (var x = Math.Max(0, quad.X); x < x1; x++)
                    frame.Plot(x, y, r, g, b);
            }
        }

        _open = false;
    }
}
