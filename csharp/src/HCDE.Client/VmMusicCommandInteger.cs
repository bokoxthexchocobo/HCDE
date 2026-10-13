namespace HCDE.Client;

internal static class VmMusicCommandInteger
{
    public static int Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var index = 0;
        while (index < value.Length && value[index] is ' ' or '\t' or '\r' or '\n' or '\v' or '\f') index++;
        var start = index;
        if (index < value.Length && value[index] is '+' or '-') index++;
        var digits = index;
        while (index < value.Length && value[index] >= '0' && value[index] <= '9') index++;
        return index == digits ? 0 : int.Parse(value.AsSpan(start, index - start), System.Globalization.CultureInfo.InvariantCulture);
    }
}
