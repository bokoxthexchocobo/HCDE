namespace HCDE.Playsim;

/// <summary>Represented flat blockmap query for native AActor.LookForEnemiesEx.</summary>
public static class ActorEnemySearchActions
{
    public static int LookForEnemiesEx(Actor observer, ICollection<Actor> targets, double range = -1,
        bool noPlayers = false, bool allAround = false, ActorLookParameters? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(observer);
        ArgumentNullException.ThrowIfNull(targets);
        var sim = observer.Simulation ?? throw new InvalidOperationException("Enemy queries require a simulation.");
        var map = sim.Level.Blockmap ?? throw new NotSupportedException("Enemy queries require blockmap data.");
        if (range == -1) range = observer.FriendlySeeBlocks * 128.0;
        if (!double.IsFinite(range) || range < 0) throw new ArgumentOutOfRangeException(nameof(range));
        var header = map.Header;
        if (header.Width <= 0 || header.Height <= 0) return targets.Count;
        var minX = Math.Max(0, Math.Floor((observer.X.ToDouble() - range - header.OriginX) / 128));
        var maxX = Math.Min(header.Width - 1, Math.Floor((observer.X.ToDouble() + range - header.OriginX) / 128));
        var minY = Math.Max(0, Math.Floor((observer.Y.ToDouble() - range - header.OriginY) / 128));
        var maxY = Math.Min(header.Height - 1, Math.Floor((observer.Y.ToDouble() + range - header.OriginY) / 128));
        var visited = new HashSet<uint>();
        for (var y = minY; y <= maxY; y++)
        for (var x = minX; x <= maxX; x++)
        {
            var left = header.OriginX + x * 128; var bottom = header.OriginY + y * 128;
            foreach (var candidate in sim.Actors.Reverse())
            {
                if (candidate.Destroyed || !candidate.IsBlockmapActor || visited.Contains(candidate.Id)) continue;
                var radius = candidate.Radius.ToDouble();
                if (candidate.X.ToDouble() + radius < left || candidate.X.ToDouble() - radius >= left + 128
                    || candidate.Y.ToDouble() + radius < bottom || candidate.Y.ToDouble() - radius >= bottom + 128) continue;
                visited.Add(candidate.Id);
                var valid = candidate is PlayerPawn player
                    ? !noPlayers && MonsterBrain.IsTargetablePlayer(sim, observer, player, allAround, parameters)
                    : FriendlyEnemySearch.IsValid(sim, observer, candidate, parameters);
                if (valid) targets.Add(candidate);
            }
        }
        return targets.Count;
    }
}
