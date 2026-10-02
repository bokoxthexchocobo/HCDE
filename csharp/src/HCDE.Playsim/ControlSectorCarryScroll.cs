namespace HCDE.Playsim;

/// <summary>Native displacement <c>DScroller::sc_carry</c> driven by a control sector center height.</summary>
internal sealed class ControlSectorCarryScroll
{
    public ControlSectorCarryScroll(
        int targetSector,
        int controlSector,
        double scaleX,
        double scaleY,
        double lastCenter,
        bool accelerative = false)
    {
        TargetSector = targetSector;
        ControlSector = controlSector;
        ScaleX = scaleX;
        ScaleY = scaleY;
        LastCenter = lastCenter;
        Accelerative = accelerative;
    }

    public int TargetSector { get; }
    public int ControlSector { get; }
    public double ScaleX { get; set; }
    public double ScaleY { get; set; }
    public double LastCenter { get; set; }
    public bool Accelerative { get; }
    public double Vdx { get; set; }
    public double Vdy { get; set; }
}
