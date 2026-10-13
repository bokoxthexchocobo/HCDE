namespace HCDE.Client;

public sealed class VmMusicMidiDeviceSetting
{
    public const int DefaultDevice = -5;
    public int Device { get; private set; } = DefaultDevice;

    public void Apply(int requested, IReadOnlyList<int> availableDevices,
        Func<int, bool> changeSetting, Action<int> deviceChanged, Action<string> report)
    {
        ArgumentNullException.ThrowIfNull(availableDevices);
        ArgumentNullException.ThrowIfNull(changeSetting);
        ArgumentNullException.ThrowIfNull(deviceChanged);
        ArgumentNullException.ThrowIfNull(report);
        Device = requested;
        if (!availableDevices.Contains(requested))
        {
            if (requested is not 0 and not -1)
                report("ID out of range. Using default device.\n");
            Device = DefaultDevice;
            // Native CVAR assignment invokes the callback again for the default.
            if (requested == DefaultDevice) return;
            if (!availableDevices.Contains(DefaultDevice))
            {
                report("ID out of range. Using default device.\n");
                return;
            }
        }
        if (changeSetting(Device)) deviceChanged(Device);
    }
}
