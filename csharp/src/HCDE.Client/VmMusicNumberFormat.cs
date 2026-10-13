using System.Globalization;

namespace HCDE.Client;

public static class VmMusicNumberFormat
{
    public static string Persistence(float value, bool? windows = null)
    {
        var bits = BitConverter.SingleToInt32Bits(value);
        if (float.IsNaN(value))
        {
            if (bits >= 0) return "nan";
            return (windows ?? OperatingSystem.IsWindows()) && bits == unchecked((int)0xffc00000)
                ? "-nan(ind)" : "-nan";
        }
        if (float.IsInfinity(value)) return bits < 0 ? "-inf" : "inf";
        return ((double)value).ToString("F6", CultureInfo.InvariantCulture);
    }

    public static string FixedSix(float value) => Fixed(value, 6);
    public static string FixedTwo(float value) => Fixed(value, 2);

    private static string Fixed(float value, int precision)
    {
        var negative = BitConverter.SingleToInt32Bits(value) < 0;
        if (float.IsNaN(value)) return negative ? "-NaN" : "NaN";
        if (float.IsInfinity(value)) return negative ? "-Inf" : "Inf";
        // Native varargs promotes the exact float value to double before formatting.
        return ((double)value).ToString("F" + precision, CultureInfo.InvariantCulture);
    }
}
