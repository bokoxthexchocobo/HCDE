namespace HCDE.Client;

/// <summary>Native <c>g_taunt.cpp</c> taunt sound naming and cooldown gate.</summary>
public static class VmHcdeTaunt
{
    public const ulong CooldownMilliseconds = 1500;

    public static string MakeTauntSoundName(string? variant) =>
        string.IsNullOrEmpty(variant) ? "*taunt" : $"*taunt-{variant}";

    public static bool TryBeginTaunt(
        ulong nowMilliseconds,
        ref ulong cooldownExpiryMilliseconds,
        out ulong remainingMilliseconds,
        out string? blockReason)
    {
        if (nowMilliseconds < cooldownExpiryMilliseconds)
        {
            remainingMilliseconds = cooldownExpiryMilliseconds - nowMilliseconds;
            blockReason = $"taunt: cooldown ({remainingMilliseconds}ms remaining).";
            return false;
        }

        cooldownExpiryMilliseconds = nowMilliseconds + CooldownMilliseconds;
        remainingMilliseconds = 0;
        blockReason = null;
        return true;
    }
}
