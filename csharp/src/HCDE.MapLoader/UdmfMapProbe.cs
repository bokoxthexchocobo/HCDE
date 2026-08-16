using System.Text;

namespace HCDE.MapLoader;

public static class UdmfMapProbe
{
    public const string NamespacePrefix = "namespace";

    public static bool LooksLikeUdmf(ReadOnlySpan<byte> lumpData)
    {
        if (lumpData.Length < NamespacePrefix.Length)
            return false;

        var prefix = Encoding.ASCII.GetString(lumpData[..Math.Min(lumpData.Length, 64)]);
        return prefix.TrimStart().StartsWith(NamespacePrefix, StringComparison.OrdinalIgnoreCase);
    }
}
