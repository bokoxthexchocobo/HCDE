namespace HCDE.Playsim;

/// <summary>Mirror-chain walk matching <c>FPolyMirrorIterator</c> (<c>po_man.cpp</c>).</summary>
internal static class VmPoManMirrorChain
{
    public static IEnumerable<TState> Enumerate<TState>(
        int startPolyId,
        Func<int, TState?> resolveById,
        Func<TState, int> mirrorPolyId)
        where TState : class
    {
        var visited = new HashSet<int>();
        var currentId = startPolyId;
        while (visited.Add(currentId))
        {
            var state = resolveById(currentId);
            if (state is null)
                yield break;

            yield return state;

            var nextId = mirrorPolyId(state);
            if (nextId == 0 || visited.Contains(nextId))
                yield break;

            currentId = nextId;
        }
    }
}
