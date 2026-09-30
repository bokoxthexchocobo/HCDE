namespace HCDE.Playsim;

/// <summary>Native <c>DScroller::sc_carry</c> rate for one sector. Rates are summed each tic after the scroll buffer is cleared.</summary>
internal readonly record struct SectorCarryScroll(int SectorIndex, double Dx, double Dy);
