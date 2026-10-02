namespace HCDE.Playsim;

/// <summary>Native accelerative <c>DScroller::sc_carry</c> (base rate added to velocity each tic).</summary>
internal sealed class AcceleratingSectorCarryScroll
{
    public AcceleratingSectorCarryScroll(int sectorIndex, double baseDx, double baseDy, ScrollCarryAffect affect = ScrollCarryAffect.All)
    {
        SectorIndex = sectorIndex;
        BaseDx = baseDx;
        BaseDy = baseDy;
        Affect = affect;
    }

    public int SectorIndex { get; }
    public double BaseDx { get; set; }
    public double BaseDy { get; set; }
    public ScrollCarryAffect Affect { get; }
    public double Vdx { get; set; }
    public double Vdy { get; set; }
}
