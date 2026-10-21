namespace HCDE.Playsim;

/// <summary><c>CheckSingular</c> and <c>ChangeSoundVolume</c> from <c>s_sound.cpp</c> (802–812, 1023–1044).</summary>
public static class VmSoundEngineCheckSingular
{
    public static bool CheckSingular(
        int soundId,
        IReadOnlyList<int> channelOrgIds)
    {
        for (var i = 0; i < channelOrgIds.Count; i++)
        {
            if (channelOrgIds[i] == soundId)
                return true;
        }

        return false;
    }

    public static void ChangeSoundVolume(
        int sourcetype,
        object? source,
        int channel,
        double dvolume,
        IReadOnlyList<(int SourceType, object? Source, int EntChannel, float Volume)> channels,
        Action<int, float> setChannelVolume)
    {
        var volume = (float)dvolume;
        if (volume < 0)
            volume = 0;
        else if (volume > 1)
            volume = 1;

        for (var i = 0; i < channels.Count; i++)
        {
            var chan = channels[i];
            if (chan.SourceType != sourcetype || chan.Source != source)
                continue;
            if (chan.EntChannel != channel && channel != -1)
                continue;

            setChannelVolume(i, volume);
        }
    }
}
