namespace HCDE.Client;

public static class VmReplayGainCommand
{
    public static void Execute(bool hasSong, string hash, float musicVolume, IReadOnlyList<string> arguments,
        Action<string, float> storeGain, Action saveGains, Action<float> setMusicVolume, Action<string> report)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(report);
        if (!hasSong || hash.Length == 0)
        {
            report("setreplaygain needs some music playing\n");
            return;
        }
        if (arguments.Count < 2)
        {
            report("Usage: setreplaygain {dB}\n");
            report($"Current replay gain is {VmMusicNumberFormat.FixedSix(VmReplayGain.AmplitudeToDecibels(musicVolume))} dB\n");
            return;
        }
        VmReplayGain.SetManualGain(hash, VmReplayGainIni.ParseNumber(arguments[1]), storeGain, saveGains, setMusicVolume);
    }
}
