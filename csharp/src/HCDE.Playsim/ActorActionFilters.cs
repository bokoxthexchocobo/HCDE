using HCDE.Gamedata;

namespace HCDE.Playsim;

internal static class ActorActionFilters
{
    internal static bool Matches(Actor actor, string? classFilter, string? speciesFilter,
        bool excludeClass, bool excludeSpecies, bool either)
    {
        var type = string.IsNullOrEmpty(classFilter) || classFilter.Equals("None", StringComparison.OrdinalIgnoreCase)
            || DoomActorCatalog.MatchesClassName(actor.ClassDoomEdNum, classFilter) != excludeClass;
        var species = string.IsNullOrEmpty(speciesFilter) || speciesFilter.Equals("None", StringComparison.OrdinalIgnoreCase)
            || actor.SpeciesName.Equals(speciesFilter, StringComparison.OrdinalIgnoreCase) != excludeSpecies;
        return either ? type || species : type && species;
    }
}
