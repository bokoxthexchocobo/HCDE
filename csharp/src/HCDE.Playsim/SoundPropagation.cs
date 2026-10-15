using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Flat-sector weapon noise using Doom's one-sound-block boundary rule.</summary>
public static class SoundPropagation
{
    public static void Alert(AuthoritySimulation sim, Actor source) => Alert(sim, source, source);

    public static void Alert(AuthoritySimulation sim, Actor emitter, Actor? target, double maxDistance = 0, bool splash = false)
    {
        if (target is PlayerPawn { NoTargetCheat: true }) return;
        var reached = ReachableSectors(sim, emitter.SectorIndex);
        foreach (var sector in reached) sim.SetSectorSoundTarget(sector, target);
        foreach (var actor in sim.Actors)
            if (actor != target && actor.IsSectorLinked && (!splash || !actor.NoSplashAlert) && reached.Contains(actor.SectorIndex)
                && (maxDistance == 0 || actor.Distance2D(emitter) <= maxDistance))
            {
                // Native NoiseMarkSector marks linked listeners even when they cannot wake now.
                actor.LastHeardTargetId = target?.Id;
                if (target is not null) actor.Brain?.Hear(sim, actor, target);
            }
    }

    public static IReadOnlySet<int> ReachableSectors(AuthoritySimulation sim, int origin)
    {
        var level = sim.Level;
        if ((uint)origin >= (uint)level.Sectors.Count) return new HashSet<int>();
        var edges = new List<(int Other, int Cost)>[level.Sectors.Count];
        for (var i = 0; i < edges.Length; i++) edges[i] = [];
        foreach (var line in level.Lines)
        {
            if ((line.Flags & LevelLine.TwoSidedFlag) == 0) continue;
            if ((uint)line.SideFront >= (uint)level.Sides.Count || (uint)line.SideBack >= (uint)level.Sides.Count) continue;
            var front = level.Sides[line.SideFront].Sector;
            var back = level.Sides[line.SideBack].Sector;
            if (front == back || (uint)front >= (uint)edges.Length || (uint)back >= (uint)edges.Length) continue;
            var cost = (line.Flags & LevelLine.BlockSoundFlag) != 0 ? 1 : 0;
            // Native checks the destination's own closure, so traversal can be directional.
            if (HasSoundOpening(sim, front, back)) edges[front].Add((back, cost));
            if (HasSoundOpening(sim, back, front)) edges[back].Add((front, cost));
        }
        var blocks = Enumerable.Repeat(int.MaxValue, edges.Length).ToArray();
        blocks[origin] = 0;
        var queue = new Queue<int>();
        queue.Enqueue(origin);
        while (queue.TryDequeue(out var sector))
        {
            foreach (var edge in edges[sector])
            {
                var next = blocks[sector] + edge.Cost;
                if (next > 1 || next >= blocks[edge.Other]) continue;
                blocks[edge.Other] = next;
                queue.Enqueue(edge.Other); // A cheaper alternate path must revisit this sector.
            }
        }
        return Enumerable.Range(0, blocks.Length).Where(sector => blocks[sector] <= 1).ToHashSet();
    }

    private static bool HasSoundOpening(AuthoritySimulation sim, int from, int to)
        => sim.FloorOf(from) < sim.CeilingOf(to)
            && sim.FloorOf(to) < sim.CeilingOf(from)
            && sim.FloorOf(to) < sim.CeilingOf(to);
}
