using System.Globalization;

namespace HCDE.Client;

public readonly record struct VmMusicMidiDevice(int Id, string Name, ushort Technology);
public readonly record struct VmMusicMidiOption(string Text, float Value);
public readonly record struct VmMusicMidiMenuCommand(string Text, string Command, bool Centered);

public static class VmMusicMidiDevices
{
    public static void BuildMenu(IReadOnlyList<VmMusicMidiDevice> devices,
        Action<VmMusicMidiOption>? addOption, Action<VmMusicMidiMenuCommand>? addCommand)
    {
        ArgumentNullException.ThrowIfNull(devices);
        foreach (var device in devices)
        {
            addOption?.Invoke(new(device.Name, device.Id));
            addCommand?.Invoke(new(device.Name,
                "snd_mididevice " + device.Id.ToString(CultureInfo.InvariantCulture), true));
        }
    }

    public static void List(IReadOnlyList<VmMusicMidiDevice> devices, int selected, Action<string> report)
    {
        ArgumentNullException.ThrowIfNull(devices);
        ArgumentNullException.ThrowIfNull(report);
        foreach (var device in devices)
        {
            var id = device.Id.ToString(CultureInfo.InvariantCulture);
            if (device.Id >= 0) id = " " + id;
            var technology = device.Technology switch
            {
                1 => "MIDIPORT", 2 => "SYNTH", 3 => "SQSYNTH", 4 => "FMSYNTH",
                5 => "MAPPER", 6 => "WAVETABLE", 7 => "SWSYNTH", _ => "",
            };
            report((device.Id == selected ? "\u001c+" : "")
                + id.PadLeft(2) + ". " + device.Name + " : " + technology + "\u001c-\n");
        }
    }
}
