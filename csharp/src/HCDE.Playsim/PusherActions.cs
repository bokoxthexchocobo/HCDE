using HCDE.MapLoader;

namespace HCDE.Playsim;

internal enum PusherKind { Wind, Current, PointPush, PointPull }

internal sealed class SectorPusher
{
    internal PusherKind Kind { get; init; }
    internal int SectorIndex { get; init; }
    internal double PushX { get; set; }
    internal double PushY { get; set; }
    internal uint? SourceActorId { get; init; }
    internal double Radius { get; init; }
    internal double Magnitude { get; init; }
}

/// <summary>Native <c>DPusher</c> from <c>a_pusher.cpp</c> (flat-sector subset).</summary>
internal static class PusherActions
{
    private const double PushFactor = 128.0;

    internal static bool AdjustWind(AuthoritySimulation sim, int tag, int magnitude, int angleByte)
        => Adjust(sim, tag, magnitude, angleByte, PusherKind.Wind);

    internal static bool AdjustCurrent(AuthoritySimulation sim, int tag, int magnitude, int angleByte)
        => Adjust(sim, tag, magnitude, angleByte, PusherKind.Current);

    private static bool Adjust(AuthoritySimulation sim, int tag, int magnitude, int angleByte, PusherKind kind)
    {
        var radians = angleByte * (Math.PI * 2 / 256);
        var pushX = Math.Cos(radians) * magnitude;
        var pushY = Math.Sin(radians) * magnitude;
        var touched = new HashSet<int>();
        var started = false;
        foreach (var pusher in sim.SectorPushers.Where(p => p.Kind == kind))
        {
            if (!sim.Level.Sectors[pusher.SectorIndex].MatchesTag(tag)) continue;
            pusher.PushX = pushX;
            pusher.PushY = pushY;
            touched.Add(pusher.SectorIndex);
            started = true;
        }
        foreach (var sector in sim.Level.Sectors.Where(s => s.MatchesTag(tag)))
        {
            if (touched.Contains(sector.Index)) continue;
            sim.SectorPushers.Add(new SectorPusher { Kind = kind, SectorIndex = sector.Index, PushX = pushX, PushY = pushY });
            started = true;
        }
        return started;
    }

    internal static void Tick(AuthoritySimulation sim)
    {
        foreach (var pusher in sim.SectorPushers)
        {
            if (pusher.Kind is PusherKind.PointPush or PusherKind.PointPull)
            {
                TickPoint(sim, pusher);
                continue;
            }
            foreach (var actor in sim.Actors)
            {
                if (actor.Destroyed || actor is not PlayerPawn || actor.NoClip) continue;
                if (!ActorPhysics.TouchesSector(sim, actor, pusher.SectorIndex)) continue;
                var onGround = actor.Z.ToDouble() <= ActorPhysics.SectorSupportFloor(sim, actor) + 0.25;
                var scale = pusher.Kind == PusherKind.Wind ? (onGround ? 0.5 : 1.0) : onGround ? 1.0 : 0.0;
                if (scale == 0) continue;
                actor.VelocityX = Fixed.FromDouble(actor.VelocityX.ToDouble() + pusher.PushX * scale / PushFactor);
                actor.VelocityY = Fixed.FromDouble(actor.VelocityY.ToDouble() + pusher.PushY * scale / PushFactor);
            }
        }
    }

    private static void TickPoint(AuthoritySimulation sim, SectorPusher pusher)
    {
        if (pusher.SourceActorId is not uint sourceId) return;
        var source = sim.Actors.FirstOrDefault(a => a.Id == sourceId && !a.Destroyed);
        if (source is null) return;
        foreach (var actor in sim.Actors)
        {
            if (actor.Destroyed || actor is not PlayerPawn || actor.NoClip) continue;
            var dx = actor.X.ToDouble() - source.X.ToDouble();
            var dy = actor.Y.ToDouble() - source.Y.ToDouble();
            var dist = Math.Sqrt(dx * dx + dy * dy);
            var speed = (pusher.Magnitude - dist / 2) / (PushFactor * 2);
            if (speed <= 0 || dist < 1e-6) continue;
            var angle = Math.Atan2(dy, dx) + (pusher.Kind == PusherKind.PointPull ? Math.PI : 0);
            actor.VelocityX = Fixed.FromDouble(actor.VelocityX.ToDouble() + Math.Cos(angle) * speed);
            actor.VelocityY = Fixed.FromDouble(actor.VelocityY.ToDouble() + Math.Sin(angle) * speed);
        }
    }
}
