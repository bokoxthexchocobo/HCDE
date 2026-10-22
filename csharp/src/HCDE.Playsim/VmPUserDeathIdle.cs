namespace HCDE.Playsim;

/// <summary><c>PlayIdle</c> and <c>A_PlayerScream</c> from <c>p_user.cpp</c> (1435–1520).</summary>
public static class VmPUserDeathIdle
{
    public const int ChanVoice = 2;
    public const int ChanBody = 4;

    public const int DfForceFallingZd = 8;
    public const int DfForceFallingHx = 16;

    public static bool ShouldCallPlayIdleVirtual(bool isPlayerPawn, bool hasVirtual) =>
        isPlayerPawn && hasVirtual;

    public static bool UseConfiguredDeathSound(bool hasPlayer, bool deathSoundValid) =>
        !hasPlayer || deathSoundValid;

    public static bool FallingDeathSplatEnabled(int levelFlags, int dmflags) =>
        (((levelFlags >> 15) | dmflags) & (DfForceFallingZd | DfForceFallingHx)) != 0;

    public static bool FastFallSplat(double velZ) => velZ <= -39;

    public static bool WimpyDeath(int special1) => special1 < 10;

    public static bool ExtremeDeath(int health) => health <= -50;

    public static bool CrazyDeathBand(int health) => health > -100;

    public static bool UseBodyChannel(int channel) => channel != ChanVoice;

    public static int StopChannelMask(int channel) => channel;

    public static bool ShouldStopAuxChannels(int channel) => UseBodyChannel(channel);

    public readonly record struct PlayerScreamContext(
        bool HasPlayer,
        bool DeathSoundValid,
        int LevelFlags,
        int Dmflags,
        double VelZ,
        int Special1,
        int Health,
        Func<string, string, string?> FindSkinnedEx,
        Func<string, string?> FindSkinned);

    public readonly record struct PlayerScreamPlan(int Channel, string SoundId, bool StopAuxChannels);

    public static PlayerScreamPlan PlanPlayerScream(PlayerScreamContext ctx)
    {
        if (UseConfiguredDeathSound(ctx.HasPlayer, ctx.DeathSoundValid))
            return new PlayerScreamPlan(ChanVoice, ctx.DeathSoundValid ? "configured" : "*death", false);

        var channel = ChanVoice;
        string? sound = null;

        if (FallingDeathSplatEnabled(ctx.LevelFlags, ctx.Dmflags) && FastFallSplat(ctx.VelZ))
        {
            sound = ctx.FindSkinned("*splat");
            channel = ChanBody;
        }

        if (sound is null && WimpyDeath(ctx.Special1))
            sound = ctx.FindSkinnedEx("*wimpydeath", "damage");

        if (sound is null && ExtremeDeath(ctx.Health))
        {
            if (CrazyDeathBand(ctx.Health))
                sound = ctx.FindSkinnedEx("*crazydeath", "damage");
            if (sound is null)
            {
                sound = ctx.FindSkinnedEx("*xdeath", "damage");
                if (sound is null)
                {
                    sound = ctx.FindSkinnedEx("*gibbed", "damage");
                    if (sound is not null) channel = ChanBody;
                }
            }
        }

        sound ??= ctx.FindSkinnedEx("*death", "damage") ?? "*death";

        return new PlayerScreamPlan(channel, sound, ShouldStopAuxChannels(channel));
    }

    public static bool ChannelStopsSlot(int slot, int activeChannel) =>
        slot != 1 && slot != ChanVoice && activeChannel != ChanVoice;

    public static IEnumerable<int> AuxChannelsToStop(int channel)
    {
        if (!ShouldStopAuxChannels(channel)) yield break;
        for (var i = 0; i < 8; i++)
        {
            if (ChannelStopsSlot(i, channel)) yield return i;
        }
    }
}
