using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>p_map.cpp</c> <c>spechit</c> line-cross activation after <c>P_TryMove</c>.</summary>
internal static class MapMoveLineCrossings
{
    internal static void Collect(AuthoritySimulation sim, double ox, double oy, double x, double y, LevelLine line, HashSet<int> pending)
    {
        if (line.Special == 0 || !LineSlide.Crosses(ox, oy, x, y, line)) return;
        var pointOnLine = sim.Compat.HasFlag(CompatSurface.PointOnLine);
        var oldBack = LineSpecials.IsBackSide(line, ox, oy, pointOnLine);
        var newBack = LineSpecials.IsBackSide(line, x, y, pointOnLine);
        if (oldBack == newBack) return;
        pending.Add(line.Index);
    }

    internal static void ActivatePending(AuthoritySimulation sim, Actor actor, double ox, double oy, double x, double y, IEnumerable<int> lineIndices)
    {
        if (actor.NoClip || actor.NoTrigger || actor.NoTeleport) return;
        foreach (var index in lineIndices)
        {
            var line = sim.Level.Lines.FirstOrDefault(candidate => candidate.Index == index);
            if (line is null || line.Special == 0) continue;
            TryActivate(sim, actor, line, ox, oy, x, y);
        }
    }

    internal static bool TryActivate(AuthoritySimulation sim, Actor actor, LevelLine line, double ox, double oy, double x, double y)
    {
        if (actor.NoTrigger || actor.NoClip || line.Special == 0) return false;
        if (!LineSlide.Crosses(ox, oy, x, y, line)) return false;
        var pointOnLine = sim.Compat.HasFlag(CompatSurface.PointOnLine);
        var oldBack = LineSpecials.IsBackSide(line, ox, oy, pointOnLine);
        var newBack = LineSpecials.IsBackSide(line, x, y, pointOnLine);
        if (oldBack == newBack) return false;
        return LineSpecials.ActivateCrossLine(sim, actor, line, oldBack);
    }
}
