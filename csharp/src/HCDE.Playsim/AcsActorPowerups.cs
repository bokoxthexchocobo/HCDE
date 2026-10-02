namespace HCDE.Playsim;

/// <summary>Native <c>GetActorPowerupTics</c> subset for managed player powerups.</summary>
internal static class AcsActorPowerups
{
    public static int RemainingTics(Actor? actor, string? powerName)
    {
        if (actor is not PlayerPawn { Destroyed: false } player || string.IsNullOrEmpty(powerName))
            return 0;
        if (powerName.Equals("PowerBuddha", StringComparison.OrdinalIgnoreCase))
            return player.PowerBuddhaTics;
        return 0;
    }
}
