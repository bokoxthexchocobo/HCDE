namespace HCDE.Playsim;

public enum ManagedPowerupKind
{
    Damage,
    Protection,
    Buddha,
}

/// <summary>Native Powerup.HandlePickup duration rules for finite managed powers.</summary>
internal static class PowerupDuration
{
    internal static int FromDefinition(int duration) => duration >= 0
        ? duration : checked((int)(-(long)duration * GameTicClock.TicRate));

    internal static int Merge(int current, int incoming, bool additive, bool alwaysPickup)
    {
        if (incoming < 0) throw new ArgumentOutOfRangeException(nameof(incoming));
        if (incoming == 0) return current;
        if (additive) return checked(current + incoming);
        if (current > PlayerPawn.PowerBuddhaBlinkThreshold && !alwaysPickup) return current;
        return Math.Max(current, incoming);
    }
}
