namespace HCDE.Playsim;

/// <summary><c>SoundEngine::UpdateSounds</c> policy from <c>s_sound.cpp</c> (1262–1334).</summary>
public static class VmSoundEngineUpdateSounds
{
    public static bool UpdateChannelWhenOpen(float openLimit, int activeChannels) =>
        openLimit <= 0 || activeChannels < openLimit;

    public static bool ChannelStillPlaying(bool isPlaying) => isPlaying;

    public static bool ShouldVirtualize(bool isVirtual, float priority) =>
        isVirtual && priority < 0;

    public static float ClampListenerZ(float z, float minZ, float maxZ) =>
        Math.Clamp(z, minZ, maxZ);
}
