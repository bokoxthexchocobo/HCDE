namespace HCDE.Playsim;

/// <summary>Mirror-chain walk matching <c>FPolyMirrorIterator</c> (<c>po_man.cpp</c> 1781–1842).</summary>
internal static class VmPoManMirrorChain
{
    /// <summary><c>countof(UsedPolys)</c> in native.</summary>
    public const int MaxTrackedPolys = 100;

    public static IEnumerable<TState> Enumerate<TState>(
        int startPolyId,
        Func<int, TState?> resolveById,
        Func<TState, int> mirrorPolyId)
        where TState : class
    {
        // Constructor seeds UsedPolys[0] with the starting poly tag; NextMirror returns
        // CurPoly then advances via GetMirror() when that tag is not already listed.
        var visited = new HashSet<int> { startPolyId };
        var currentId = startPolyId;
        while (true)
        {
            var state = resolveById(currentId);
            if (state is null)
                yield break;

            yield return state;

            if (visited.Count >= MaxTrackedPolys)
                yield break;

            var nextId = mirrorPolyId(state);
            if (nextId == 0 || visited.Contains(nextId))
                yield break;

            visited.Add(nextId);
            currentId = nextId;
        }
    }
}
