using System.Globalization;
using System.Numerics;

namespace HCDE.Scripting;

/// <summary>Native <c>FScanner::GetNumber</c> / <c>mystrtoll</c> numeric parsing.</summary>
public static class VmScriptNumber
{
    public static bool TryParseInteger(string text, bool noOctals, out long bigNumber, out int number)
    {
        bigNumber = 0;
        number = 0;
        if (text == "MAXINT") { number = int.MaxValue; bigNumber = int.MaxValue; return true; }
        if (text.Length == 0) return true;
        var index = 0;
        while (index < text.Length && text[index] is ' ' or '\t' or '\r' or '\n') index++;
        var negative = index < text.Length && text[index] == '-';
        if (index < text.Length && text[index] is '+' or '-') index++;
        var radix = 10;
        if (index < text.Length && text[index] == '0')
        {
            radix = noOctals ? 10 : 8;
            if (index + 2 < text.Length && text[index + 1] is 'x' or 'X')
            {
                radix = 16;
                index += 2;
            }
        }

        var start = index;
        BigInteger magnitude = 0;
        for (; index < text.Length; index++)
        {
            var ch = text[index];
            var digit = ch is >= '0' and <= '9' ? ch - '0'
                : ch is >= 'a' and <= 'f' ? ch - 'a' + 10
                : ch is >= 'A' and <= 'F' ? ch - 'A' + 10 : -1;
            if (digit < 0 || digit >= radix) return false;
            magnitude = magnitude * radix + digit;
        }

        if (index == start) return false;
        var value = negative ? -magnitude : magnitude;
        var saturated = value < long.MinValue ? long.MinValue : value > long.MaxValue ? long.MaxValue : (long)value;
        bigNumber = saturated;
        number = unchecked((int)saturated);
        return true;
    }

    public static bool TryParseFloat(string text, out double value, out int stopIndex)
    {
        value = 0;
        stopIndex = 0;
        if (text.Length == 0) return false;
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return false;
        stopIndex = text.Length;
        return true;
    }
}
