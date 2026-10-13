namespace HCDE.Client;

public interface IVmMusicIntegerSettingsDevice
{
    bool ChangeInteger(string setting, int value);
}

public interface IVmMusicNumberSettingsDevice
{
    bool ChangeNumber(string setting, double value);
}

public interface IVmMusicStringSettingsDevice
{
    bool ChangeString(string setting, string value);
}

public interface IVmMusicStatisticsDevice
{
    string GetStatistics();
}

public interface IVmMusicAudioSessionDevice : IVmMusicMidiSessionDevice, IDisposable
{
    VmMusicStreamFormat GetStreamInfo();
    bool ServiceStream(Span<float> output);
}
