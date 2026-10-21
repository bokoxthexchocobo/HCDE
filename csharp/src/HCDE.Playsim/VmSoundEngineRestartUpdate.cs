namespace HCDE.Playsim;

/// <summary><c>SoundEngine::RestartChannel</c> policy from <c>s_sound.cpp</c> (649–800).</summary>
public static class VmSoundEngineRestartUpdate
{
    public const int ChanfEvicted = 1;
    public const int ChanfLoop = 2;

    public static bool RequiresEvictedFlag(int chanFlags) => (chanFlags & ChanfEvicted) != 0;

    public static bool ShouldRestartLoop(bool wantsLoop, bool stillEvicted) => wantsLoop && stillEvicted;
}
