using System.Text;

namespace HCDE.Client;

internal static class VmArchivePattern
{
    public static bool Matches(string name, string pattern)
    {
        var input = Encoding.UTF8.GetBytes(name);
        return Match(input, Encoding.UTF8.GetBytes(pattern), false) is not null;
    }

    private static int? Match(byte[] input, byte[] bytes, bool partial)
    {
        var consumed = 0;
        for (var index = 0; index < bytes.Length; index++)
        {
            var value = bytes[index];
            if (value == '*')
            {
                for (var offset = consumed; offset <= input.Length; offset++)
                {
                    var result = Match(input[offset..], bytes[(index + 1)..], partial);
                    if (result is not null) return offset + result.Value;
                }
                return null;
            }
            if (value == '\\')
            {
                if (++index == bytes.Length) break;
                value = bytes[index];
                if (consumed == input.Length || input[consumed++] != value) return null;
                continue;
            }
            if (value == '[')
            {
                var first = index + 1;
                var negated = first < bytes.Length && bytes[first] == '!';
                if (negated) first++;
                var end = first + 1;
                while (end < bytes.Length && bytes[end] != ']') end++;
                if (end < bytes.Length)
                {
                    if (consumed == input.Length || bytes.AsSpan(first, end - first).Contains(input[consumed++]) == negated) return null;
                    index = end;
                    continue;
                }
            }
            if (value == '(' && HasClosingAlternative(bytes, index))
            {
                var closing = FindAlternativeEnd(bytes, index);
                var depth = 0;
                var branchStart = index + 1;
                for (var end = branchStart; end <= closing; end++)
                {
                    if (bytes[end] == '\\') { end++; continue; }
                    if (bytes[end] == '[')
                    {
                        var first = end + 1;
                        if (first < bytes.Length && bytes[first] == '!') first++;
                        var close = first + 1;
                        while (close < bytes.Length && bytes[close] != ']') close++;
                        if (close < bytes.Length) { end = close; continue; }
                    }
                    if (bytes[end] == '(') depth++;
                    if (end != closing && bytes[end] == ')') depth--;
                    if (end == closing || depth == 0 && bytes[end] == '|')
                    {
                        // Native tries the first partial branch match once before advancing branches.
                        var branch = Match(input[consumed..], bytes[branchStart..end], true);
                        if (branch is not null)
                        {
                            var suffix = Match(input[(consumed + branch.Value)..], bytes[(closing + 1)..], partial);
                            if (suffix is not null) return consumed + branch.Value + suffix.Value;
                        }
                        branchStart = end + 1;
                    }
                }
                return null;
            }
            if (consumed == input.Length || value != '?' && input[consumed] != value) return null;
            consumed++;
        }
        return partial || consumed == input.Length ? consumed : null;
    }

    private static bool HasClosingAlternative(byte[] pattern, int start) => FindAlternativeEnd(pattern, start) >= 0;

    private static int FindAlternativeEnd(byte[] pattern, int start)
    {
        var depth = 1;
        for (var index = start + 1; index < pattern.Length; index++)
        {
            if (pattern[index] == '\\') { index++; continue; }
            if (pattern[index] == '[')
            {
                var first = index + 1;
                if (first < pattern.Length && pattern[first] == '!') first++;
                var end = first + 1;
                while (end < pattern.Length && pattern[end] != ']') end++;
                if (end < pattern.Length) { index = end; continue; }
            }
            if (pattern[index] == '(') depth++;
            else if (pattern[index] == ')' && --depth == 0) return index;
        }
        return -1;
    }
}
