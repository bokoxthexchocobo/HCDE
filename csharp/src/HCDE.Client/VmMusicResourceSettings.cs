namespace HCDE.Client;

public readonly record struct VmMusicDeviceSetting(int Device, string Arguments);
public readonly record struct VmMusicResolvedSettings(int Device, string Arguments, int ModulePlayer, float? Volume);

public static class VmMusicResourceSettings
{
    public static VmMusicResolvedSettings Resolve(int resource, int preferredModulePlayer,
        IReadOnlyDictionary<int, VmMusicDeviceSetting> devices, IReadOnlyDictionary<int, int> modulePlayers,
        IReadOnlyDictionary<int, float> volumes)
    {
        ArgumentNullException.ThrowIfNull(devices);
        ArgumentNullException.ThrowIfNull(modulePlayers);
        ArgumentNullException.ThrowIfNull(volumes);
        var device = devices.TryGetValue(resource, out var setting) ? setting : new VmMusicDeviceSetting(-1, "");
        ArgumentNullException.ThrowIfNull(device.Arguments);
        var module = modulePlayers.TryGetValue(resource, out var player) ? player : preferredModulePlayer;
        float? volume = volumes.TryGetValue(resource, out var level) ? level : null;
        return new(device.Device, device.Arguments, module, volume);
    }

    public static void ApplyVolume(VmMusicResolvedSettings settings, Action<float> setMusicVolume,
        Action<int, string> checkReplayGain)
    {
        ArgumentNullException.ThrowIfNull(setMusicVolume);
        ArgumentNullException.ThrowIfNull(checkReplayGain);
        if (settings.Volume is { } volume) setMusicVolume(volume);
        else checkReplayGain(settings.Device, settings.Arguments);
    }
}
