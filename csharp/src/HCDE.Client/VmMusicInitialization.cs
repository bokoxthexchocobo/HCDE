namespace HCDE.Client;

public static class VmMusicInitialization
{
    public static void Initialize(VmMusicEnableState enablement, bool noMusic,
        Action initializeSoundfonts, Action musicVolumeChanged, Action runtimeEnablementChanged,
        Action midiDeviceChanged, Action registerCallbacks, Action setupResources, bool lite = false)
    {
        ArgumentNullException.ThrowIfNull(enablement);
        ArgumentNullException.ThrowIfNull(initializeSoundfonts);
        ArgumentNullException.ThrowIfNull(musicVolumeChanged);
        ArgumentNullException.ThrowIfNull(runtimeEnablementChanged);
        ArgumentNullException.ThrowIfNull(midiDeviceChanged);
        ArgumentNullException.ThrowIfNull(registerCallbacks);
        ArgumentNullException.ThrowIfNull(setupResources);
        initializeSoundfonts();
        musicVolumeChanged();
        runtimeEnablementChanged();
        enablement.NoMusic = noMusic;
        midiDeviceChanged();
        registerCallbacks();
        if (!lite) setupResources();
    }
}
