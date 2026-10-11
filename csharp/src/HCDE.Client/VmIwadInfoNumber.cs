using System.Numerics;

namespace HCDE.Client;

internal static class VmIwadInfoNumber
{
    public static bool TryParse(string text, out int value)
    {
        value = 0;
        if (text == "MAXINT") { value = int.MaxValue; return true; }
        // GetNumber accepts an empty quoted token because strtoll leaves its end at NUL.
        if (text.Length == 0) return true;
        var index = 0;
        while (index < text.Length && text[index] is ' ' or '\t' or '\r' or '\n' or '\v' or '\f') index++;
        var negative = index < text.Length && text[index] == '-';
        if (index < text.Length && text[index] is '+' or '-') index++;
        var radix = 10;
        if (index < text.Length && text[index] == '0')
        {
            radix = 8;
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
        var number = negative ? -magnitude : magnitude;
        var saturated = number < long.MinValue ? long.MinValue : number > long.MaxValue ? long.MaxValue : (long)number;
        value = unchecked((int)saturated);
        return true;
    }
}
