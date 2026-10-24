namespace HCDE.Scripting;

/// <summary>Native <c>ParseHex</c> from <c>sc_man.cpp</c>.</summary>
public static class VmScriptParseHex
{
    public static int Parse(string hex, bool warnOnBad = false)
    {
        var num = 0;
        foreach (var ch in hex)
        {
            var digit = ch switch
            {
                >= '0' and <= '9' => ch - '0',
                >= 'a' and <= 'f' => 10 + ch - 'a',
                >= 'A' and <= 'F' => 10 + ch - 'A',
                _ => -1
            };
            if (digit < 0) return warnOnBad ? 0 : throw new FormatException($"Bad hex number: {hex}");
            num = (num << 4) + digit;
        }

        return num;
    }
}
