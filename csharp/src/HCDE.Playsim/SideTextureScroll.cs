namespace HCDE.Playsim;

/// <summary>Native <c>DScroller::sc_side</c> rate for one sidedef and part mask.</summary>
internal readonly record struct SideTextureScroll(
    int SideIndex,
    double Dx,
    double Dy,
    WallScrollParts Parts);
