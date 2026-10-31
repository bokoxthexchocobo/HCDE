namespace HCDE.Playsim;

/// <summary>
/// Eternity resource detection from <c>hcde_eternity_compat.cpp</c> (<b>1–176</b>).
/// Chunk <b>1213</b>. Names are backslash-folded and ASCII-lowered before the base,
/// suffix, and path-segment checks. Samples keep the filesystem path and stop at eight.
/// </summary>
public sealed class VmEternityLump1213
{
    public int Container;
    public string? ShortName;
    public string? FullName;
    public string? FullPath;
}

public static class VmEternityCompat1213Slice
{
    public const uint FlagEmapInfo = 1u << 0;
    public const uint FlagEdf = 1u << 1;
    public const uint FlagExtraData = 1u << 2;
    public const int SampleCap = 8;

    static uint flags;
    static int emapInfoCount;
    static int edfCount;
    static int extraDataCount;
    static readonly List<string> samples = new();

    public static uint GetFlags()
    {
        return flags;
    }

    public static int EmapInfoCount()
    {
        return emapInfoCount;
    }

    public static int EdfCount()
    {
        return edfCount;
    }

    public static int ExtraDataCount()
    {
        return extraDataCount;
    }

    public static string[] Samples()
    {
        string[] copy = new string[samples.Count];
        for (int i = 0; i < samples.Count; i++)
            copy[i] = samples[i];
        return copy;
    }

    public static bool IsActive(uint mask)
    {
        return (flags & mask) == mask;
    }

    public static void Detect(IReadOnlyList<VmEternityLump1213> lumps, int maxIwadNum, Action<string> print)
    {
        flags = 0;
        emapInfoCount = 0;
        edfCount = 0;
        extraDataCount = 0;
        samples.Clear();
        for (int i = 0; i < lumps.Count; i++)
        {
            VmEternityLump1213 lump = lumps[i];
            if (lump.Container <= maxIwadNum)
                continue;
            string shortName = LowerCopy(lump.ShortName);
            string fullName = LowerCopy(lump.FullName);
            string path = fullName.Length == 0 ? shortName : fullName;
            string baseName = BaseName(path);
            if (baseName == "emapinfo")
                RecordEmapInfo(lump);
            if (baseName == "edfroot" || baseName == "edf" || EndsWith(baseName, ".edf") || HasPathSegment(path, "edf"))
                RecordEdf(lump);
            if (baseName == "extradata" || baseName == "extradata.txt" || EndsWith(path, "/extradata") || EndsWith(path, "/extradata.txt"))
                RecordExtraData(lump);
        }
        Report(print);
    }

    static void RecordEmapInfo(VmEternityLump1213 lump)
    {
        flags |= FlagEmapInfo;
        emapInfoCount++;
        RememberSample(lump);
    }

    static void RecordEdf(VmEternityLump1213 lump)
    {
        flags |= FlagEdf;
        edfCount++;
        RememberSample(lump);
    }

    static void RecordExtraData(VmEternityLump1213 lump)
    {
        flags |= FlagExtraData;
        extraDataCount++;
        RememberSample(lump);
    }

    static void RememberSample(VmEternityLump1213 lump)
    {
        if (samples.Count >= SampleCap)
            return;
        string sample = lump.FullPath ?? "";
        for (int i = 0; i < samples.Count; i++)
        {
            if (samples[i] == sample)
                return;
        }
        samples.Add(sample);
    }

    static void Report(Action<string> print)
    {
        if (flags == 0)
            return;
        print(
            "HCDE: Eternity compatibility resources detected: EMAPINFO="
            + emapInfoCount
            + " EDF="
            + edfCount
            + " ExtraData="
            + extraDataCount
            + ".");
        print("HCDE: Eternity compatibility will parse supported EMAPINFO, ExtraData, XLAT, and EDF manifest properties; unsupported behavior still needs compat patches.");
        for (int i = 0; i < samples.Count; i++)
            print("HCDE: Eternity resource: " + samples[i]);
    }

    static string LowerCopy(string? value)
    {
        if (value == null)
            return "";
        char[] chars = new char[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
            char ch = value[i] == '\\' ? '/' : value[i];
            if (ch >= 'A' && ch <= 'Z')
                ch = (char)(ch + 32);
            chars[i] = ch;
        }
        return new string(chars);
    }

    static string BaseName(string value)
    {
        int slash = value.LastIndexOf('/');
        if (slash < 0)
            return value;
        return value.Substring(slash + 1);
    }

    static bool EndsWith(string value, string suffix)
    {
        if (value.Length < suffix.Length)
            return false;
        return value.Substring(value.Length - suffix.Length) == suffix;
    }

    static bool HasPathSegment(string value, string segment)
    {
        string needle = "/" + segment + "/";
        if (value.IndexOf(needle, StringComparison.Ordinal) >= 0)
            return true;
        string prefix = segment + "/";
        return value.StartsWith(prefix, StringComparison.Ordinal);
    }
}
