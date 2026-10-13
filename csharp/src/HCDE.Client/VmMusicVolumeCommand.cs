namespace HCDE.Client;

public static class VmMusicVolumeCommand
{
    public static void Execute(VmMusicVolume volume, IReadOnlyList<string> arguments,
        Action<float> relativeSetting, Action musicVolumeCallback, Action<string> report)
    {
        ArgumentNullException.ThrowIfNull(volume);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(report);
        if (arguments.Count > 1)
            volume.ChangeRelativeVolume(VmReplayGainIni.ParseNumber(arguments[1]), relativeSetting, musicVolumeCallback);
        else report($"Current relative volume is {VmMusicNumberFormat.FixedTwo(volume.RelativeLevel)}\n");
    }
}
