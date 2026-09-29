namespace HCDE.Playsim;

/// <summary>Flat-sector damage and healing, including sector opt-in for non-player actors and airborne contact.</summary>
internal static class SectorDamage
{
    public static void Initialize(AuthoritySimulation simulation)
    {
        foreach (var sector in simulation.Level.Sectors)
        {
            var doom = simulation.Level.UsesDoomSectorNumbers;
            var raw = (ushort)sector.Special;
            // Doom's translator shifts its Boom bitfield left three places.
            var flags = doom ? (raw & 0x3fe0) << 3 : raw & ~255;
            if ((flags & 0x8000) != 0 && simulation.Compat.HasFlag(CompatSurface.Mbf21)) continue;
            var flagDamage = (flags & 0x300) switch { 0x100 => 5, 0x200 => 10, 0x300 => 20, _ => 0 };
            if (sector.DamageAmount == 0 && flagDamage != 0)
            {
                sector.DamageAmount = flagDamage;
                sector.DamageInterval = 32;
                sector.Leakiness = flagDamage == 20 ? 5 : 0;
                sector.DamageType = flagDamage == 5 ? "Fire" : "Slime";
            }
            var special = doom
                ? (raw & 31) switch { 4 => 68, 5 => 69, 7 => 71, 11 => 75, 16 => 80, _ => 0 }
                : raw & 255;
            var amount = special switch { 68 or 75 or 80 => 20, 69 => 10, 71 or 104 => 5, 85 => 4, 196 => -1, _ => 0 };
            if (amount == 0) continue;
            if (sector.DamageAmount == 0)
            {
                sector.DamageAmount = amount;
                sector.DamageInterval = 32;
                sector.Leakiness = special == 75 ? 256 : amount == 20 ? 5 : 0;
                sector.DamageType = amount < 0 || special == 75 ? "None" : "Slime";
                sector.DamageEndsGodMode = special == 75;
                sector.DamageEndsLevel = special == 75;
            }
            // Lighting has already consumed combined light/damage specials.
            // Preserve unconverted secret/friction/death flags in the managed model.
            sector.Special = unchecked((short)(raw & (doom ? ~31 : ~255)));
        }
    }

    public static bool? ExecuteSpecial(AuthoritySimulation simulation, int special, int tag, int amount,
        int damageType, int interval, int leakiness)
    {
        if (special != 214) return null;
        if (interval <= 0)
        {
            leakiness = amount < 20 ? 0 : amount < 50 ? 5 : 256;
            interval = amount < 50 ? 32 : 1;
        }
        foreach (var sector in simulation.Level.Sectors.Where(sector => sector.Tag == tag))
        {
            sector.DamageAmount = unchecked((short)amount);
            sector.DamageType = damageType switch
            {
                9 => "BFGSplash", 12 => "Drowning", 13 => "Slime", 14 => "Fire",
                15 => "Crush", 16 => "Telefrag", 17 => "Falling", 18 => "Suicide",
                20 => "Exit", 22 => "Melee", 23 => "Railgun", 24 => "Ice",
                25 => "Disintegrate", 26 => "Poison", 27 => "Electric", 1000 => "Massacre",
                _ => "None",
            };
            sector.DamageInterval = unchecked((short)interval);
            sector.Leakiness = unchecked((short)leakiness);
        }
        return true;
    }

    public static void Tick(AuthoritySimulation simulation, Actor actor)
    {
        if (actor.IsDead || actor.Destroyed
            || actor.SectorIndex < 0 || actor.SectorIndex >= simulation.Level.Sectors.Count) return;

        var sector = simulation.Level.Sectors[actor.SectorIndex];
        // Native FORCESECTORDAMAGE wins over NOSECTORDAMAGE and sector opt-in,
        // but does not bypass floor contact, interval or ordinary damage protection.
        if (!actor.ForceSectorDamage && (actor.NoSectorDamage || actor is not PlayerPawn && !sector.HurtMonsters)) return;
        if (!sector.HarmInAir && actor.Z != Fixed.FromDouble(simulation.FloorOf(actor.SectorIndex))) return;
        if (sector.DamageAmount == 0) return;
        if (sector.DamageInterval <= 0) sector.DamageInterval = 32;
        if (sector.DamageAmount > 0 && sector.DamageEndsGodMode && actor is PlayerPawn player)
            player.GodMode = false;
        // Called during the actor thinker, before the shared level clock advances.
        if (simulation.Thinkers.Clock.Tic % sector.DamageInterval != 0) return;
        if (sector.DamageAmount > 0)
        {
            ActorDamage.Apply(actor, sector.DamageAmount, damageType: sector.DamageType);
            if (sector.DamageEndsLevel && actor is PlayerPawn && actor.Health <= 10 && simulation.DamageExitAllowed)
                simulation.MarkExited(secret: false);
        }
        else
        {
            var maximum = actor is PlayerPawn ? 100 : actor.ResurrectionHealth;
            var healing = Math.Min(65536L, -(long)sector.DamageAmount);
            if (actor.Health < maximum)
                actor.Health = (int)Math.Min(maximum, actor.Health + healing);
        }
    }
}
