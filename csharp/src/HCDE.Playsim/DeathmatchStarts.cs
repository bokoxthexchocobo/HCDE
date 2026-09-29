using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>
/// <c>G_DeathMatchSpawnPlayer</c> for a respawn. Thing 11 is a deathmatch start and is not an actor.
/// The random stream is this port's own counter, not the native <c>DMSpawn</c> table.
/// Twenty blocked tries still return the last spot. The revive then telefrags what is standing there.
/// </summary>
public static class DeathmatchStarts
{
    public const int ThingType = 11;

    public static LevelThing PickRandom(IReadOnlyList<LevelThing> starts, Func<int, int> roll, Func<LevelThing, bool> open)
    {
        var last = starts[0];
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var index = roll(starts.Count);
            if ((uint)index >= (uint)starts.Count)
                index %= starts.Count;
            last = starts[index];
            if (open(last))
                return last;
        }
        return last;
    }

    /// <summary>The start farthest from the nearest living player. A tie keeps the earlier start. No living player returns null.</summary>
    public static LevelThing? PickFarthest(IReadOnlyList<LevelThing> starts, IReadOnlyList<(double X, double Y)> living)
    {
        if (living.Count == 0)
            return null;
        LevelThing? best = null;
        var bestDistance = 0.0;
        foreach (var start in starts)
        {
            var nearest = double.MaxValue;
            foreach (var player in living)
            {
                var dx = start.X - player.X;
                var dy = start.Y - player.Y;
                var distance = Math.Sqrt(dx * dx + dy * dy);
                if (distance < nearest)
                    nearest = distance;
            }
            if (nearest > bestDistance)
            {
                bestDistance = nearest;
                best = start;
            }
        }
        return best;
    }
}
