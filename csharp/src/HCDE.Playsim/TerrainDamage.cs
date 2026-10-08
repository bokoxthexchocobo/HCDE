namespace HCDE.Playsim;

/// <summary>Flat-floor periodic P_ActorOnSpecialFlat subset. Protection items,
/// water-level contact, 3D floors, and splash audio remain absent.</summary>
internal static class TerrainDamage
{
    internal static void Land(AuthoritySimulation sim, Actor actor)
    {
        if (actor is not PlayerPawn || actor.IsDead || actor.Destroyed) return;
        var terrain = sim.Level.FloorTerrainDefinition(actor.SectorIndex);
        if (terrain is not { DamageOnLand: true } || terrain.DamageAmount == 0 || terrain.Splash.Length == 0
            || !sim.Level.TerrainSplashes.Contains(terrain.Splash, StringComparer.OrdinalIgnoreCase)) return;
        if ((sim.Thinkers.Clock.Tic & terrain.DamageTimeMask) == 0) return;
        ActorDamage.Apply(actor, terrain.DamageAmount, damageType: terrain.DamageType);
    }

    internal static void Tick(AuthoritySimulation sim, Actor actor)
    {
        if (actor.IsDead || actor.Destroyed || (uint)actor.SectorIndex >= (uint)sim.Level.Sectors.Count) return;
        var sector = sim.Level.Sectors[actor.SectorIndex];
        if (!actor.ForceSectorDamage && (actor.NoSectorDamage || actor is not PlayerPawn && !sector.HurtMonsters)) return;
        if (actor.Z.ToDouble() > sim.FloorOf(actor.SectorIndex)) return;
        var terrain = sim.Level.FloorTerrainDefinition(actor.SectorIndex);
        if (terrain is null || terrain.DamageAmount == 0) return;
        if (terrain.DamageTimeMask < 0) throw new InvalidOperationException("Terrain damage time mask must be nonnegative.");
        if (sim.Thinkers.Clock.Tic % ((long)terrain.DamageTimeMask + 1) != 0) return;
        ActorDamage.Apply(actor, terrain.DamageAmount, damageType: terrain.DamageType);
    }
}
