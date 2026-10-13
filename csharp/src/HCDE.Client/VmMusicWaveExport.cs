namespace HCDE.Client;

public readonly record struct VmMusicWaveRequest(int Device, string? Soundfont, string Output, int Subsong, int SampleRate);

public static class VmMusicWaveExport
{
    public static void WriteWave<TSong, TSource>(VmMusicPlaybackState<TSong> state,
        IReadOnlyList<string> arguments, int preferredDevice, Func<string, TSource?> openSource,
        Action<bool> stopMusic, Func<TSource, VmMusicWaveRequest, bool> dumpWave,
        Func<string, int, bool, bool, bool> changeMusic, Func<string> lastError, Action<string> report,
        bool lite = false) where TSong : class where TSource : class, IDisposable
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(openSource);
        ArgumentNullException.ThrowIfNull(stopMusic);
        ArgumentNullException.ThrowIfNull(dumpWave);
        ArgumentNullException.ThrowIfNull(changeMusic);
        ArgumentNullException.ThrowIfNull(lastError);
        ArgumentNullException.ThrowIfNull(report);
        if (arguments.Count is < 3 or > 7)
        {
            report("Usage: writewave <midi> <filename> [subsong] [sample rate] [synth] [soundfont]\n"
                + " - use '*' as song name to dump the currently playing song\n"
                + " - use 0 for subsong and sample rate to play the default\n");
            return;
        }
        using var source = openSource(arguments[1]);
        if (source is null) return;
        var device = -1;
        if (!lite && arguments.Count >= 6)
        {
            device = arguments[5].ToLowerInvariant() switch
            {
                "wildmidi" => 6, "gus" => 5, "timidity" or "timidity++" => 3,
                "fluidsynth" => 4, "opl" => 1, "opn" => 8, "adl" => 7, _ => -2,
            };
            if (device == -2)
            {
                report($"{arguments[5]}: Unknown MIDI device\n");
                return;
            }
        }
        var savedName = state.Name;
        var savedOrder = state.BaseOrder;
        var savedLoop = state.Loop;
        stopMusic(true);
        try
        {
            if (device == -1 && preferredDevice >= 0) device = 4;
            var request = new VmMusicWaveRequest(device, arguments.Count >= 7 ? arguments[6] : null,
                arguments[2], arguments.Count >= 4 ? VmMusicCommandInteger.Parse(arguments[3]) : 0,
                arguments.Count >= 5 ? VmMusicCommandInteger.Parse(arguments[4]) : 0);
            // The dumper borrows the source and must release its synth before returning.
            if (!dumpWave(source, request)) report($"MIDI dump of {arguments[1]} failed: {lastError()}\n");
        }
        finally
        {
            changeMusic(savedName, savedOrder, savedLoop, true);
        }
    }
}
