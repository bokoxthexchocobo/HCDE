namespace HCDE.Playsim;

/// <summary>Represented flat blockmap query for native AActor.LookForEnemiesEx.</summary>
public static class ActorEnemySearchActions
{
    /// <summary>Native rough search ring order over the represented flat actor blockmap.</summary>
    public static Actor? RoughMonsterSearch(Actor observer, int distance, bool onlySeekable = false,
        bool frontOnly = false, double fieldOfView = 0)
    {
        ArgumentNullException.ThrowIfNull(observer);
        if (!double.IsFinite(fieldOfView)) throw new ArgumentOutOfRangeException(nameof(fieldOfView));
        var sim = observer.Simulation ?? throw new InvalidOperationException("Rough search requires an attached simulation.");
        var map = sim.Level.Blockmap ?? throw new NotSupportedException("Rough search requires blockmap data.");
        var header = map.Header;
        if (header.Width <= 0 || header.Height <= 0) return null;
        var startX = (long)Math.Floor((observer.X.ToDouble() - header.OriginX) / 128);
        var startY = (long)Math.Floor((observer.Y.ToDouble() - header.OriginY) / 128);
        var radians = observer.Angle.ToDegrees() * Math.PI / 180;
        var frontDx = -Math.Sin(radians); var frontDy = -Math.Cos(radians);
        Actor? InCell(long x, long y)
        {
            if (x < 0 || y < 0 || x >= header.Width || y >= header.Height) return null;
            var left = header.OriginX + x * 128.0; var bottom = header.OriginY + y * 128.0;
            foreach (var candidate in sim.Actors.Reverse())
            {
                if (candidate == observer || candidate.Destroyed || !candidate.IsBlockmapActor) continue;
                var radius = candidate.Radius.ToDouble();
                if (candidate.X.ToDouble() + radius < left || candidate.X.ToDouble() - radius >= left + 128
                    || candidate.Y.ToDouble() + radius < bottom || candidate.Y.ToDouble() - radius >= bottom + 128) continue;
                if (onlySeekable && !observer.CanSeek(candidate)) continue;
                // Match native frontline signs and P_PointOnDivlineSide's strict epsilon test.
                if (frontOnly && (candidate.Y.ToDouble() - observer.Y.ToDouble()) * frontDx
                    + (observer.X.ToDouble() - candidate.X.ToDouble()) * frontDy > 1.0 / 65536) continue;
                if (fieldOfView > 0 && !observer.CheckFov(candidate, fieldOfView)) continue;
                if (observer.IsOkayToAttack(candidate)) return candidate;
            }
            return null;
        }
        var found = InCell(startX, startY);
        if (found is not null) return found;
        for (long ring = 1; ring <= distance; ring++)
        {
            var left = Math.Clamp(startX - ring, 0, header.Width - 1);
            var bottom = Math.Clamp(startY - ring, 0, header.Height - 1);
            var right = Math.Min(startX + ring, header.Width - 1);
            var top = Math.Min(startY + ring, header.Height - 1);
            if (right < 0 || top < 0) continue;
            // Native revisits corners and clamped edges; do not deduplicate sight checks.
            for (var x = left; x <= right; x++) if ((found = InCell(x, bottom)) is not null) return found;
            for (var y = bottom; y <= top; y++) if ((found = InCell(right, y)) is not null) return found;
            for (var x = right; x >= left; x--) if ((found = InCell(x, top)) is not null) return found;
            for (var y = top; y > bottom; y--) if ((found = InCell(left, y)) is not null) return found;
        }
        return null;
    }

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
