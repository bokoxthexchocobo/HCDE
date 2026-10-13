namespace HCDE.Client;

public sealed class VmMusicVolume
{
    public float MusicLevel { get; private set; } = 1;
    public float RelativeLevel { get; private set; } = 1;
    public float MasterLevel { get; private set; } = 1;
    public float SavedSongLevel { get; private set; } = 1;

    public bool StartSongLevel(float songLevel, bool useReplayGain = false)
    {
        if (!(songLevel > 0) || useReplayGain) return false;
        var factor = RelativeLevel / SavedSongLevel;
        SavedSongLevel = songLevel;
        SetRelativeLevel(SavedSongLevel * factor);
        return true;
    }

    public float DigitalLevel(double gameFactor = 1) => Math.Clamp(
        MusicLevel * RelativeLevel * (float)Math.Clamp(gameFactor, 0, 2) * MasterLevel, 0, 1);

    public void SetMusicLevel(float value) => MusicLevel = Math.Clamp(value, 0, 1);
    public void SetMasterLevel(float value) => MasterLevel = Math.Clamp(value, 0, 1);
    public void SetRelativeLevel(float value) => RelativeLevel = value;
    public void SetGameLevel(double factor) => RelativeLevel = (float)Math.Clamp(factor, 0, 2);

    public void NotifyMusicVolume(Action<float> musicSetting, Action<float>? digitalVolume,
        bool hasSong, Action volumeChanged, Action restartMusic)
    {
        ArgumentNullException.ThrowIfNull(musicSetting);
        ArgumentNullException.ThrowIfNull(volumeChanged);
        ArgumentNullException.ThrowIfNull(restartMusic);
        musicSetting(MusicLevel);
        digitalVolume?.Invoke(DigitalLevel());
        if (hasSong) volumeChanged(); else restartMusic();
    }

    public void ChangeRelativeVolume(float value, Action<float> relativeSetting, Action musicVolumeCallback)
    {
        ArgumentNullException.ThrowIfNull(relativeSetting);
        ArgumentNullException.ThrowIfNull(musicVolumeCallback);
        SetRelativeLevel(value);
        relativeSetting(RelativeLevel);
        musicVolumeCallback();
    }

    public void ChangeGameVolume(double factor, Action<float> relativeSetting, Action musicVolumeCallback) =>
        ChangeRelativeVolume((float)Math.Clamp(factor, 0, 2), relativeSetting, musicVolumeCallback);

    public void Apply(Action<float> musicSetting, Action<float> relativeSetting, Action<float> digitalVolume,
        bool songPlaying, Action volumeChanged, Action restartMusic)
    {
        ArgumentNullException.ThrowIfNull(musicSetting);
        ArgumentNullException.ThrowIfNull(relativeSetting);
        ArgumentNullException.ThrowIfNull(digitalVolume);
        ArgumentNullException.ThrowIfNull(volumeChanged);
        ArgumentNullException.ThrowIfNull(restartMusic);
        relativeSetting(RelativeLevel);
        musicSetting(MusicLevel);
        digitalVolume(DigitalLevel());
        if (songPlaying) volumeChanged(); else restartMusic();
    }
}
