namespace HCDE.Gamedata;

/// <summary><c>LoadDehSupp</c> <c>SoundMap</c> and <c>InfoNames</c> from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedDehSuppSoundInfo
{
    public static List<string> ParseSoundMapSection(IEnumerable<string> tokens, Func<string, int> resolveSound)
    {
        var list = new List<int>();
        var names = new List<string>();
        foreach (var raw in tokens)
        {
            var token = raw.Trim();
            if (token is "{" or "}" or ",")
                continue;
            if (string.IsNullOrEmpty(token))
                break;
            names.Add(token);
            list.Add(resolveSound(token));
        }

        return names;
    }

    public static bool TryParseInfoNamesSection(
        IEnumerable<string> tokens,
        Func<string, bool> findActor,
        out List<string> infoNames,
        out string? error)
    {
        infoNames = [];
        error = null;
        foreach (var raw in tokens)
        {
            var token = raw.Trim();
            if (token is "{" or "}" or ",")
                continue;
            if (string.IsNullOrWhiteSpace(token))
                break;
            if (!findActor(token))
            {
                error = $"Unknown actor type '{token}'";
                return false;
            }

            infoNames.Add(token);
        }

        return true;
    }
}
