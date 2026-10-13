using System.Security.Cryptography;

namespace HCDE.Client;

public delegate bool VmMusicMidiProbe(ReadOnlySpan<byte> header);

public static class VmReplayGainHash
{
    public static string Create(VmMusicByteReader reader, int fileLength, int playerType, string playerParameter,
        VmReplayGainConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return Create(reader, fileLength, playerType, playerParameter, configuration.DefaultDevice,
            configuration.ParameterFor);
    }

    public static string Create(VmMusicByteReader reader, int fileLength, int playerType, string playerParameter,
        int defaultDevice, Func<int, string> defaultParameter) => Create(reader, fileLength, playerType,
            playerParameter, defaultDevice, header => VmMusicMidiDetector.Identify(header) != VmMusicMidiType.None,
            defaultParameter);

    public static string Create(VmMusicByteReader reader, int fileLength, int playerType, string playerParameter,
        int defaultDevice, VmMusicMidiProbe isMidi, Func<int, string> defaultParameter)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(playerParameter);
        ArgumentNullException.ThrowIfNull(isMidi);
        ArgumentNullException.ThrowIfNull(defaultParameter);
        var buffer = new byte[50000];
        var length = reader.Read(buffer);
        reader.Seek(0, SeekOrigin.Begin);
        var digest = Convert.ToHexString(MD5.HashData(buffer.AsSpan(0, length)));
        if (!isMidi(buffer.AsSpan(0, 32))) return $"{fileLength}:{digest}";
        if (playerType == -1)
        {
            playerType = defaultDevice switch
            {
                -1 or -5 => 4,
                -2 => 3,
                -3 => 1,
                -4 => 5,
                -6 => 6,
                -7 => 7,
                -8 => 8,
                _ => -1
            };
            if (playerType == -1) return "";
        }
        else if (playerType == 2) return "";
        if (playerParameter.Length == 0) playerParameter = defaultParameter(playerType);
        return $"{fileLength}:{digest}:{playerType}:{playerParameter}".ToUpperInvariant();
    }
}
