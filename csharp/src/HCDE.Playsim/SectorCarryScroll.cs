namespace HCDE.Playsim;

/// <summary>Native <c>DScroller::sc_carry</c> rate for one sector. Rates are summed each tic after the scroll buffer is cleared.</summary>
internal readonly record struct SectorCarryScroll(int SectorIndex, double Dx, double Dy, ScrollCarryAffect Affect = ScrollCarryAffect.All)
{
    internal static bool Affects(Actor actor, ScrollCarryAffect affect)
    {
        if (actor is PlayerPawn)
            return (affect & ScrollCarryAffect.Players) != 0;
        if (actor.IsMonster)
            return (affect & ScrollCarryAffect.Monsters) != 0;
        return (affect & ScrollCarryAffect.StaticObjects) != 0;
    }
}
