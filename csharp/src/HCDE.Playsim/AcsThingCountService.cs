using HCDE.Gamedata;

namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::ThingCount</c> actor census for ACS opcodes.</summary>
public static class AcsThingCountService
{
    public delegate bool ActorPredicate(Actor actor);

    public static int Count(
        AuthoritySimulation sim,
        int editorNumber,
        int tid,
        int sectorTag,
        Func<int, int?>? resolveReplacementEditorNumber = null)
    {
        var count = CountOnce(sim, editorNumber, tid, sectorTag);
        if (editorNumber <= 0 || resolveReplacementEditorNumber == null)
            return count;

        var replacement = resolveReplacementEditorNumber(editorNumber);
        if (replacement is null || replacement.Value == editorNumber)
            return count;

        return count + CountOnce(sim, replacement.Value, tid, sectorTag);
    }

    public static int CountByClassName(
        AuthoritySimulation sim,
        string className,
        int tid,
        int sectorTag,
        Func<int, int?>? resolveReplacementEditorNumber = null)
    {
        if (!DoomActorCatalog.TryEditorNumberForClassName(className, out var editorNumber))
            return 0;

        return Count(sim, editorNumber, tid, sectorTag, resolveReplacementEditorNumber);
    }

    private static int CountOnce(AuthoritySimulation sim, int editorNumber, int tid, int sectorTag)
    {
        var count = 0;
        foreach (var actor in EnumerateCandidates(sim, tid))
        {
            if (actor.Destroyed || actor.Health <= 0 || !actor.IsMapActor())
                continue;

            if (editorNumber > 0 && actor.DoomEdNum != editorNumber)
                continue;

            if (sectorTag >= 0)
            {
                if (actor.SectorIndex < 0 || actor.SectorIndex >= sim.Level.Sectors.Count)
                    continue;
                if (!sim.Level.Sectors[actor.SectorIndex].MatchesTag(sectorTag))
                    continue;
            }

            count++;
        }

        return count;
    }

    private static IEnumerable<Actor> EnumerateCandidates(AuthoritySimulation sim, int tid)
    {
        if (tid != 0)
        {
            foreach (var actor in sim.Actors)
            {
                if (actor.ThingId == tid)
                    yield return actor;
            }

            yield break;
        }

        foreach (var actor in sim.Actors)
            yield return actor;
    }
}
