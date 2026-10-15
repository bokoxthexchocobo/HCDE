using System.Text;

namespace HCDE.Playsim;

/// <summary>UTF-8 C-string comparison with byte limits and C-locale ASCII case folding.</summary>
internal static class AcsByteStringComparison
{
    internal static int Compare(string left, string right, bool ignoreCase, int maximumLength)
    {
        var a = Encoding.UTF8.GetBytes(left);
        var b = Encoding.UTF8.GetBytes(right);
        var limit = maximumLength < 0 ? int.MaxValue : maximumLength;
        for (var i = 0; i < limit; i++)
        {
            var first = i < a.Length ? a[i] : 0;
            var second = i < b.Length ? b[i] : 0;
            if (ignoreCase)
            {
                if (first is >= 'A' and <= 'Z') first += 'a' - 'A';
                if (second is >= 'A' and <= 'Z') second += 'a' - 'A';
            }
            if (first != second) return first - second;
            if (first == 0) return 0;
        }
        return 0;
    }
}
