namespace HCDE.Playsim;

internal static class ActorMovementHazards
{
    internal static bool IsOnLift(AuthoritySimulation sim, Actor actor)
        => (uint)actor.SectorIndex < (uint)sim.Level.Sectors.Count && sim.Level.Sectors[actor.SectorIndex].IsLift
            || sim.Motions.Any(m => m.SectorIndex == actor.SectorIndex && m.Kind == MotionKind.Lift);

    internal static bool SharesPrimaryTag(AuthoritySimulation sim, Actor first, Actor second)
    {
        if ((uint)first.SectorIndex >= (uint)sim.Level.Sectors.Count || (uint)second.SectorIndex >= (uint)sim.Level.Sectors.Count)
            return false;
        var a = sim.Level.Sectors[first.SectorIndex]; var b = sim.Level.Sectors[second.SectorIndex];
        var firstTag = a.Tag != 0 ? a.Tag : a.AdditionalTags.FirstOrDefault(t => t != 0);
        var secondTag = b.Tag != 0 ? b.Tag : b.AdditionalTags.FirstOrDefault(t => t != 0);
        return firstTag == 0 || secondTag == 0 ? first.SectorIndex == second.SectorIndex : firstTag == secondTag;
    }

    internal static int UnderDamage(AuthoritySimulation sim, Actor actor)
    {
        var direction = 0;
        for (var i = 0; i < sim.Level.Sectors.Count; i++)
        {
            if (!ActorPhysics.TouchesSector(sim, actor, i)) continue;
            var ceiling = sim.Motions.FirstOrDefault(m => m.SectorIndex == i && m.Kind == MotionKind.Ceiling);
            if (ceiling is { CrushDamage: > 0, Paused: false, Completed: false, Wait: 0 })
                direction |= ceiling.Closing ? 1 : ceiling.CeilingDirection;
            var sector = sim.Level.Sectors[i];
            if (!actor.NoSectorDamage && sector.DamageAmount > 0
                && (sector.HarmInAir || actor.Z.ToDouble() == sim.FloorOf(i)))
                return actor is PlayerPawn || sector.HurtMonsters ? -1 : 0;
        }
        return direction;
    }
}
