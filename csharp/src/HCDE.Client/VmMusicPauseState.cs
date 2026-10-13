namespace HCDE.Client;

public sealed class VmMusicPauseState
{
    public bool Paused { get; private set; }

    public void Pause(bool hasSong, Action pauseSong, Action<bool> pauseStream)
    {
        ArgumentNullException.ThrowIfNull(pauseSong);
        ArgumentNullException.ThrowIfNull(pauseStream);
        if (!hasSong || Paused) return;
        pauseSong();
        pauseStream(true);
        Paused = true;
    }

    public void Resume(bool hasSong, Action resumeSong, Action<bool> pauseStream)
    {
        ArgumentNullException.ThrowIfNull(resumeSong);
        ArgumentNullException.ThrowIfNull(pauseStream);
        if (!hasSong || !Paused) return;
        resumeSong();
        pauseStream(false);
        Paused = false;
    }

    public void Reset(Action<bool> stopMusic)
    {
        ArgumentNullException.ThrowIfNull(stopMusic);
        if (Paused) stopMusic(true);
        Paused = false;
    }
}
