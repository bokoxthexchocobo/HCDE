namespace HCDE.Playsim;

public static class ActorPickupActions
{
    /// <summary>Native Strife UpgradeAccuracy.TryPickup adds ten without clamping an overshoot.</summary>
    public static bool UpgradeAccuracy(Actor toucher)
    {
        ArgumentNullException.ThrowIfNull(toucher);
        if (toucher is not PlayerPawn { Destroyed: false } player || player.Accuracy >= 100) return false;
        player.Accuracy += 10;
        return true;
    }

    internal static bool IsDoorToken(string typeName) => typeName.Equals("OpenDoor222", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("CloseDoor222", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("OpenDoor224", StringComparison.OrdinalIgnoreCase);

    /// <summary>Native Strife door token TryPickup effects; consuming the token does not require a moving door.</summary>
    public static bool UseDoorToken(Actor toucher, string typeName)
    {
        ArgumentNullException.ThrowIfNull(toucher);
        ArgumentException.ThrowIfNullOrEmpty(typeName);
        if (!IsDoorToken(typeName)) throw new ArgumentException("Unknown Strife door token.", nameof(typeName));
        if (toucher.Destroyed) return false;
        var close = typeName.Equals("CloseDoor222", StringComparison.OrdinalIgnoreCase);
        var tag = typeName.Equals("OpenDoor224", StringComparison.OrdinalIgnoreCase) ? 224 : 222;
        ActorSpecialActions.CallSpecial(toucher, close ? 10 : 11, tag, 16);
        return true;
    }

    /// <summary>Native Strife HealthFillup.TryPickup skill-dependent percentage healing.</summary>
    public static bool HealthFillup(Actor toucher)
    {
        ArgumentNullException.ThrowIfNull(toucher);
        if (toucher.Destroyed) return false;
        var sim = toucher.Simulation ?? throw new InvalidOperationException("HealthFillup requires an attached simulation.");
        var percentage = sim.Skill switch { 1 => -75, 2 or 3 => -50, _ => -100 };
        return toucher.GiveBody(percentage);
    }

    /// <summary>Native Strife UpgradeStamina.TryPickup for a represented player pawn.</summary>
    public static bool UpgradeStamina(Actor toucher, int amount = 10, int maximum = 100)
    {
        ArgumentNullException.ThrowIfNull(toucher);
        if (toucher is not PlayerPawn { Destroyed: false } player) return false;
        player.Stamina = unchecked(player.Stamina + amount);
        if (player.Stamina >= maximum) player.Stamina = maximum;
        player.GiveBody(-100);
        return true;
    }

    /// <summary>Native Inventory.PlayPickupSound for represented pickup classes.</summary>
    public static void PlayPickupSound(Actor pickup, Actor toucher)
    {
        var sound = pickup.ActorSounds.GetValueOrDefault(ActorSoundType.Pickup)
            ?? PickupCatalog.DefaultPickupSound(pickup.DoomEdNum,
                toucher.Simulation?.StrifeHealthPickups ?? false, toucher.Simulation?.RavenHealthPickups ?? false);
        if (sound.Length == 0 || sound.Equals("None", StringComparison.OrdinalIgnoreCase)) return;
        if (toucher.Destroyed) return;
        var sim = toucher.Simulation ?? throw new InvalidOperationException("Pickup sounds require an attached toucher.");
        sim.QueueSoundRequest(new SimulationSoundRequest(toucher.Id, sound, 3, (int)SimulationSoundFlags.MaybeLocal,
            1, false, pickup.NoAttenPickupSound ? 0 : 1, false, PickupSound: true), immediate: true);
    }
}
