namespace HCDE.Client;

public sealed class VmMusicEnableState
{
    public bool RuntimeEnabled { get; set; } = true;
    public bool NoMusic { get; set; }
    public bool Enabled => RuntimeEnabled && !NoMusic;
    public int ScriptValue => Enabled ? 1 : 0;

    public void ApplyRuntime(bool enabled, Action restartMusic, Action<bool> stopMusic)
    {
        ArgumentNullException.ThrowIfNull(restartMusic);
        ArgumentNullException.ThrowIfNull(stopMusic);
        RuntimeEnabled = enabled;
        if (enabled) restartMusic(); else stopMusic(true);
    }
}
