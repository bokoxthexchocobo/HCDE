using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Flat-sector weapon noise using Doom's one-sound-block boundary rule.</summary>
public static class SoundPropagation
{
    public static void Alert(AuthoritySimulation sim, Actor source)
    {
        if (!source.CanTakeDamage) return;
        var reached = ReachableSectors(sim, source.SectorIndex);
        foreach (var actor in sim.Actors)
            if (actor.Id != source.Id && reached.Contains(actor.SectorIndex))
            {
                // NoiseMarkSector records LastHeard even when the monster cannot wake now.
                actor.LastHeardTargetId = source.Id;
                actor.Brain?.Hear(sim, actor, source);
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
            if ((uint)line.SideFront >= (uint)level.Sides.Count || (uint)line.SideBack >= (uint)level.Sides.Count) continue;
            var front = level.Sides[line.SideFront].Sector;
            var back = level.Sides[line.SideBack].Sector;
            if (front == back || (uint)front >= (uint)edges.Length || (uint)back >= (uint)edges.Length) continue;
            // Read current planes, so closed doors stop sound and opening them restores it.
            if (Math.Max(sim.FloorOf(front), sim.FloorOf(back)) >= Math.Min(sim.CeilingOf(front), sim.CeilingOf(back))) continue;
            var cost = (line.Flags & LevelLine.BlockSoundFlag) != 0 ? 1 : 0;
            edges[front].Add((back, cost));
            edges[back].Add((front, cost));
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
}
