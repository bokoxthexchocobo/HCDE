namespace HCDE.Playsim;

internal sealed class ControlPlaneScroll(int target, int control, double dx, double dy,
    double lastHeight, bool accelerating, SectorTextureScrollPlane plane)
{
    public int Target { get; } = target;
    public int Control { get; } = control;
    public double Dx { get; set; } = dx;
    public double Dy { get; set; } = dy;
    public double LastHeight { get; set; } = lastHeight;
    public bool Accelerating { get; } = accelerating;
    public SectorTextureScrollPlane Plane { get; } = plane;
    public int ArchiveKind => (Plane == SectorTextureScrollPlane.Ceiling ? 13 : 15) + (Accelerating ? 1 : 0);
    public double Vdx { get; set; }
    public double Vdy { get; set; }
}
