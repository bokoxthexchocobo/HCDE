namespace HCDE.Client;

/// <summary><c>AMColor</c> from <c>am_map.cpp</c> (326–349).</summary>
public readonly struct VmAutomapColorArgb
{
    public uint Rgb { get; }

    public VmAutomapColorArgb(uint rgb) => Rgb = rgb;

    public static VmAutomapColorArgb FromRgb(int r, int g, int b) =>
        new(MakeArgb(255, r, g, b));

    public static VmAutomapColorArgb Invalid => new(0);

    public bool IsValid => Rgb != 0;

    private static uint MakeArgb(int a, int r, int g, int b) =>
        ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | (uint)b;
}
