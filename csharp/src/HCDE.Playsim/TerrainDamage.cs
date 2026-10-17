namespace HCDE.Playsim;

/// <summary>Flat-floor periodic P_ActorOnSpecialFlat subset. Full inventory inheritance,
/// water-level contact, 3D floors, and splash actor effects remain absent.</summary>
internal static class TerrainDamage
{
    internal static bool Land(AuthoritySimulation sim, Actor actor)
    {
        if (actor.Destroyed) return false;
        if (actor.Touchy && (actor.Armed || actor.IsSentient()) && actor.VelocityZ.ToDouble() < -5)
        {
            actor.Armed = false;
            ActorDamage.Apply(actor, actor.Health, flags: DamageFlags.Forced, damageType: "Crush");
            return false;
        }
        var terrain = sim.Level.FloorTerrainDefinition(actor.SectorIndex);
        if (terrain is null) return false;
        if (terrain.Splash.Length == 0
            || !sim.Level.TerrainSplashes.Contains(terrain.Splash, StringComparer.OrdinalIgnoreCase)) return terrain.IsLiquid;
        if (actor is PlayerPawn && terrain.DamageOnLand && terrain.DamageAmount != 0
            && (sim.Thinkers.Clock.Tic & terrain.DamageTimeMask) != 0)
            ActorDamage.Apply(actor, terrain.DamageAmount, damageType: terrain.DamageType);
        if (actor.DontSplash) return terrain.IsLiquid;
        if ((actor is PlayerPawn || actor.IsMonster) && actor.VelocityZ.ToDouble() >= -6) return terrain.IsLiquid;
        var splash = sim.Level.TerrainSplashDefinitions.LastOrDefault(splash =>
            splash.Name.Equals(terrain.Splash, StringComparison.OrdinalIgnoreCase));
        if (actor is PlayerPawn && splash is not null && !splash.NoAlert)
            SoundPropagation.Alert(sim, actor, actor, splash: true);
        var sound = actor.Mass < 10 ? splash?.SmallSplashSound : splash?.NormalSplashSound;
        if (string.IsNullOrEmpty(sound) || sound.Equals("None", StringComparison.OrdinalIgnoreCase)) return terrain.IsLiquid;
        var origin = new SimulationSoundOrigin((float)actor.X.ToDouble(), (float)actor.Y.ToDouble(),
            (float)sim.FloorOf(actor.SectorIndex), actor.SectorIndex);
        sim.QueueSoundRequest(new(0, sound, 3, 0, 1, false, 1.001f, false, DetachedOrigin: origin));
        return terrain.IsLiquid;
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
        if (terrain.AllowProtection && actor is PlayerPawn { HasIronFeetProtection: true }) return;
        ActorDamage.ApplyWithNativeResult(actor, terrain.DamageAmount, out var damage, damageType: terrain.DamageType);
        var sound = sim.Level.TerrainSplashDefinitions.LastOrDefault(splash =>
            splash.Name.Equals(terrain.Splash, StringComparison.OrdinalIgnoreCase))?.NormalSplashSound;
        if (damage > 0 && !string.IsNullOrEmpty(sound) && !sound.Equals("None", StringComparison.OrdinalIgnoreCase))
            sim.QueueSoundRequest(new(actor.Id, sound, 0, 0, 1, false, 1.001f, false));
    }
}
