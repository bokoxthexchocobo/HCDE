namespace HCDE.Client;

/// <summary><c>cp437_to_ascii</c> and terminal capability probe from <c>vga2ansi.cpp</c>.</summary>
public static class VmVgaCp437
{
    private static readonly char[] Lo =
    [
        ' ', '+', '#', '+', '+', '+', '+', '+', '#', '+', '#', '+', '+', '+', '+', '+',
        '<', '>', '|', '!', '$', '$', '-', '|', '^', 'v', '<', '>', '-', '-', '^', 'v'
    ];

    private static readonly char[] Hi =
    [
        'C', 'u', 'e', 'a', 'a', 'a', 'a', 'c', 'e', 'e', 'e', 'i', 'i', 'i', 'A', 'A',
        'E', 'a', 'A', 'o', 'o', 'o', 'u', 'u', 'y', 'O', 'U', '$', '$', '$', '$', 'f',
        'a', 'i', 'o', 'u', 'n', 'N', '*', '*', '?', '-', '-', '/', '/', '!', '"', '"',
        ':', '+', '#', '|', '+', '+', '+', '+', '+', '+', '|', '+', '+', '+', '+', '+',
        '+', '+', '+', '+', '-', '+', '+', '+', '+', '+', '+', '+', '+', '-', '+', '+',
        '+', '+', '+', '+', '+', '+', '+', '+', '+', '+', '+', '#', '-', '|', '|', '-',
        'a', 'B', 'G', 'p', 'S', 's', 'u', 't', 'P', 'T', 'O', 'd', '8', 'h', 'E', '-',
        '=', '+', '>', '<', '|', '|', '%', '=', '*', '*', '*', 'Q', 'n', '2', '#', ' '
    ];

    public enum TerminalColorSupport
    {
        Dumb,
        None,
        Basic,
        Full
    }

    public static char Cp437ToAscii(byte ch)
    {
        if (ch < 32) return Lo[ch];
        if (ch >= 128) return Hi[ch - 128];
        return (char)ch;
    }

    public static TerminalColorSupport ProbeTerminal(string? term, string? colorTerm)
    {
        if (term is null || term == "dumb") return TerminalColorSupport.Dumb;
        if (colorTerm is null) return TerminalColorSupport.None;
        if (colorTerm == "truecolor" || colorTerm == "24bit") return TerminalColorSupport.Full;
        return TerminalColorSupport.Basic;
    }
}
