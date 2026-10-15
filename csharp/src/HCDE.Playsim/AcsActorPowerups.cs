namespace HCDE.Playsim;

/// <summary>Native <c>GetActorPowerupTics</c> subset for managed player powerups.</summary>
internal static class AcsActorPowerups
{
    public static int RemainingTics(Actor? actor, string? powerName)
    {
        if (actor is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(powerName))
            return 0;
        if (powerName.Equals("PowerIronFeet", StringComparison.OrdinalIgnoreCase))
            return player.PowerIronFeetTics;
        if (powerName.Equals("PowerMask", StringComparison.OrdinalIgnoreCase))
            return player.PowerMaskTics;
        if (powerName.Equals("PowerStrength", StringComparison.OrdinalIgnoreCase))
            return player.PowerStrengthTics;
        if (powerName.Equals("PowerBuddha", StringComparison.OrdinalIgnoreCase))
            return player.PowerBuddhaTics;
        if (powerName.Equals("PowerDamage", StringComparison.OrdinalIgnoreCase))
            return player.PowerDamageTics;
        if (powerName.Equals("PowerProtection", StringComparison.OrdinalIgnoreCase))
            return player.PowerProtectionTics;
        if (powerName.Equals("PowerInfiniteAmmo", StringComparison.OrdinalIgnoreCase))
            return player.PowerInfiniteAmmoTics;
        if (powerName.Equals("PowerDoubleFiringSpeed", StringComparison.OrdinalIgnoreCase))
            return player.PowerDoubleFiringSpeedTics;
        if (powerName.Equals("PowerHighJump", StringComparison.OrdinalIgnoreCase))
            return player.PowerHighJumpTics;
        if (powerName.Equals("PowerProsperity", StringComparison.OrdinalIgnoreCase))
            return player.PowerProsperityTics;
        if (powerName.Equals("PowerSpread", StringComparison.OrdinalIgnoreCase))
            return player.PowerSpreadTics;
        if (powerName.Equals("PowerRegeneration", StringComparison.OrdinalIgnoreCase))
            return player.PowerRegenerationTics;
        if (powerName.Equals("PowerDrain", StringComparison.OrdinalIgnoreCase))
            return player.PowerDrainTics;
        return 0;
    }
}
