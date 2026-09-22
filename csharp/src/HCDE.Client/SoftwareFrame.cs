namespace HCDE.Client;

public sealed class SoftwareFrame
{
    public SoftwareFrame(int width, int height)
    {
        if (width < 1 || height < 1)
            throw new ArgumentOutOfRangeException(nameof(width));
        Width = width;
        Height = height;
        Pixels = new byte[width * height * 3];
    }

    public int Width { get; }
    public int Height { get; }
    public byte[] Pixels { get; }

    public void Clear(byte r, byte g, byte b)
    {
        for (var i = 0; i < Pixels.Length; i += 3)
        {
            Pixels[i] = r;
            Pixels[i + 1] = g;
            Pixels[i + 2] = b;
        }
    }

    public void Plot(int x, int y, byte r, byte g, byte b)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            return;
        var i = (y * Width + x) * 3;
        Pixels[i] = r;
        Pixels[i + 1] = g;
        Pixels[i + 2] = b;
    }

    public void Get(int x, int y, out byte r, out byte g, out byte b)
    {
        var i = (y * Width + x) * 3;
        r = Pixels[i];
        g = Pixels[i + 1];
        b = Pixels[i + 2];
    }
}
