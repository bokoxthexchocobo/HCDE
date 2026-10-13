using System.Globalization;

namespace HCDE.Client;

public sealed record VmReplayGainConfiguration
{
    public int DefaultDevice { get; init; } = -1;
    public string FluidPatchset { get; init; } = "";
    public string TimidityConfig { get; init; } = "";
    public string GusConfig { get; init; } = "";
    public string WildMidiConfig { get; init; } = "";
    public bool AdlUseCustomBank { get; init; }
    public string AdlCustomBank { get; init; } = "";
    public int AdlBank { get; init; }
    public bool OpnUseCustomBank { get; init; }
    public string OpnCustomBank { get; init; } = "";
    public int OplCore { get; init; }

    public string ParameterFor(int playerType) => playerType switch
    {
        4 => FluidPatchset,
        3 => TimidityConfig,
        5 => GusConfig,
        6 => WildMidiConfig,
        7 => AdlUseCustomBank ? AdlCustomBank : AdlBank.ToString(CultureInfo.InvariantCulture),
        8 => OpnUseCustomBank ? OpnCustomBank : "",
        1 => OplCore.ToString(CultureInfo.InvariantCulture),
        _ => ""
    };
}
