namespace HCDE.Playsim;

internal sealed class ControlWallScroll(int side, int control, double dx, double dy, double height, bool accelerating,
    WallScrollParts parts = WallScrollParts.All)
{
    public int Side { get; } = side;
    public int Control { get; } = control;
    public double Dx { get; set; } = dx;
    public double Dy { get; set; } = dy;
    public double LastHeight { get; set; } = height;
    public bool Accelerating { get; } = accelerating;
    public WallScrollParts Parts { get; } = parts;
    public int ArchiveKind => (Parts == WallScrollParts.All ? 17 : 19 + ((int)Parts - 1) * 2) + (Accelerating ? 1 : 0);
    public static WallScrollParts PartsFromKind(int kind) => kind <= 18 ? WallScrollParts.All : (WallScrollParts)((kind - 19) / 2 + 1);
    public static bool AccelerationFromKind(int kind) => kind <= 18 ? kind == 18 : (kind - 19) % 2 != 0;
    public double Vdx { get; set; }
    public double Vdy { get; set; }
}
