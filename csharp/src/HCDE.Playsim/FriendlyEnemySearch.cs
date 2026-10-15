namespace HCDE.Playsim;

/// <summary>Represented flat blockmap subset of P_LookForEnemies.</summary>
internal static class FriendlyEnemySearch
{
    internal static Actor? Find(AuthoritySimulation sim, Actor observer, ActorLookParameters? parameters = null)
    {
        if (!(observer.Friendly || observer.SeeFriendlyMonsters) || sim.Level.Blockmap is not { } map) return null;
        var header = map.Header;
        if (header.Width <= 0 || header.Height <= 0) return null;
        var startX = (int)Math.Floor((observer.X.ToDouble() - header.OriginX) / 128);
        var startY = (int)Math.Floor((observer.Y.ToDouble() - header.OriginY) / 128);
        Actor? InCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= header.Width || y >= header.Height) return null;
            var left = header.OriginX + x * 128.0; var bottom = header.OriginY + y * 128.0;
            foreach (var candidate in sim.Actors.Reverse())
            {
                if (!candidate.IsBlockmapActor || candidate.Destroyed) continue;
                var radius = candidate.Radius.ToDouble();
                if (candidate.X.ToDouble() + radius < left || candidate.X.ToDouble() - radius >= left + 128
                    || candidate.Y.ToDouble() + radius < bottom || candidate.Y.ToDouble() - radius >= bottom + 128) continue;
                if (IsValid(sim, observer, candidate, parameters)) return candidate;
            }
            return null;
        }
        var found = InCell(startX, startY);
        if (found is not null) return found;
        // Once all map cells are covered, avoid unbounded repeated edge scans for arbitrary ACS values.
        var maximum = Math.Max(Math.Max(Math.Abs((long)startX), Math.Abs((long)startY)),
            Math.Max(Math.Abs((long)startX - header.Width + 1), Math.Abs((long)startY - header.Height + 1)));
        var range = Math.Min((long)observer.FriendlySeeBlocks, maximum);
        for (long ring = 1; ring <= range; ring++)
        {
            var left = (int)Math.Clamp(startX - ring, 0, header.Width - 1);
            var bottom = (int)Math.Clamp(startY - ring, 0, header.Height - 1);
            var right = (int)Math.Min(startX + ring, header.Width - 1);
            var top = (int)Math.Min(startY + ring, header.Height - 1);
            if (right < 0 || top < 0) continue;
            for (var x = left; x <= right; x++) if ((found = InCell(x, bottom)) is not null) return found;
            for (var y = bottom; y <= top; y++) if ((found = InCell(right, y)) is not null) return found;
            for (var x = right; x >= left; x--) if ((found = InCell(x, top)) is not null) return found;
            for (var y = top; y > bottom; y--) if ((found = InCell(left, y)) is not null) return found;
        }
        return null;
    }

    internal static bool IsValid(AuthoritySimulation sim, Actor observer, Actor candidate, ActorLookParameters? parameters = null)
    {
        if (candidate == observer || candidate.Destroyed || !candidate.Shootable || candidate.IsDead
            || candidate.Dormant || !candidate.IsMonster || candidate.NeverTarget) return false;
        if (observer.Friendly ? candidate.Friendly && observer.IsFriend(candidate)
            : !observer.SeeFriendlyMonsters || !candidate.Friendly) return false;
        var target = sim.Actors.FirstOrDefault(a => !a.Destroyed && a.Id == candidate.TargetId);
        if (target is not null && target.TargetId == candidate.Id && sim.NextSkipTargetByte() > 100
            && observer.IsFriend(target) && (long)target.Health * 2 >= target.SpawnHealth()) return false;
        return ActorVisibility.IsVisible(sim, observer, candidate, true, parameters);
    }
}
