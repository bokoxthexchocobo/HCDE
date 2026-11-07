namespace HCDE.Playsim;

/// <summary>
/// Version assignment and scanner open from <c>sc_man.cpp</c> 1-170. Chunk 1395.
/// <c>OpenString</c> starts at line 171 and stays outside this band.
/// </summary>
public static class VmScanner1395Slice
{
    public const ushort VersionMax = 65535;
    public const int OpenStringLine = 171;
    public const int OpenLumpLine = 188;
    public const int PrepareLine = 211;
    public const int CloseLine = 261;
    public const string MissingLump = "Could not find script lump '";

    public struct Version
    {
        public ushort Major;
        public ushort Minor;
        public uint Revision;

        public Version(ushort major, ushort minor, uint revision)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
        }
    }

    public sealed class Scanner
    {
        public bool ScriptOpen;
        public int LumpNum;
        public string ScriptName = "";
        public string ScriptBuffer = "";
        public int Closes;
        public int Prepares;
        readonly Dictionary<string, int> own = new();
        readonly Dictionary<string, int>? external;

        public Scanner(Dictionary<string, int>? symbols)
        {
            external = symbols;
            ScriptOpen = false;
        }

        public Dictionary<string, int> Symbols => external ?? own;
    }

    public readonly record struct FileBytes(long Length, string Bytes);

    public static void Assign(ref Version version, string text)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));
        version.Major = AsU16(Parse(text, 0, out int end));
        if (At(text, end) == '.')
        {
            version.Minor = AsU16(Parse(text, end + 1, out end));
            if (At(text, end) == '.')
            {
                version.Revision = AsU32(Parse(text, end + 1, out end));
                if (At(text, end) != '\0')
                    version.Major = VersionMax;
            }
            else if (At(text, end) == '\0')
                version.Revision = 0;
            else
                version.Major = VersionMax;
        }
        else if (At(text, end) == '\0')
        {
            version.Minor = 0;
            version.Revision = 0;
        }
        else
            version.Major = VersionMax;
    }

    public static Scanner Create(Dictionary<string, int>? symbols)
    {
        return new Scanner(symbols);
    }

    public static Scanner CreateForLump(int lump, Dictionary<string, int>? symbols, Action<Scanner, int> openLump)
    {
        if (openLump == null)
            throw new ArgumentNullException(nameof(openLump));
        Scanner scanner = new Scanner(symbols);
        scanner.ScriptOpen = false;
        openLump(scanner, lump);
        return scanner;
    }

    public static void Open(Scanner scanner, string name, Func<string, bool, int> findLump, Action<Scanner, int> openLump)
    {
        if (scanner == null || name == null || findLump == null || openLump == null)
            throw new ArgumentNullException(nameof(scanner));
        int lump = findLump(name, true);
        if (lump == -1)
            throw new InvalidOperationException(MissingLump + name + "'\n");
        openLump(scanner, lump);
    }

    public static bool OpenFile(Scanner scanner, string name, Func<string, FileBytes?> read)
    {
        if (scanner == null || name == null || read == null)
            throw new ArgumentNullException(nameof(scanner));
        scanner.Closes++;
        FileBytes? got = read(name);
        if (got == null)
            return false;
        FileBytes file = got.Value;
        if (file.Bytes.Length == 0 && file.Length > 0)
            return false;
        if (file.Length < 0 || file.Length > file.Bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(read));
        scanner.ScriptBuffer = file.Bytes.Substring(0, (int)file.Length);
        scanner.ScriptName = name;
        scanner.LumpNum = -1;
        scanner.Prepares++;
        return true;
    }

    public static void OpenMem(Scanner scanner, string name, string? buffer, int size, Action<string, string> openString)
    {
        if (scanner == null || name == null || openString == null)
            throw new ArgumentNullException(nameof(scanner));
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        if (size > 0 && buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        string text = size == 0 ? "" : buffer!.Substring(0, size);
        openString(name, text);
    }

    private static char At(string text, int index)
    {
        return index >= text.Length ? '\0' : text[index];
    }

    private static bool IsSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v';
    }

    private static ulong Parse(string text, int start, out int end)
    {
        int i = start;
        while (i < text.Length && IsSpace(text[i]))
            i++;
        int j = i;
        bool negative = false;
        if (j < text.Length && (text[j] == '+' || text[j] == '-'))
        {
            negative = text[j] == '-';
            j++;
        }
        int k = j;
        while (k < text.Length && text[k] >= '0' && text[k] <= '9')
            k++;
        if (k == j)
        {
            end = start;
            return 0;
        }
        ulong value = 0;
        bool overflow = false;
        for (int p = j; p < k; p++)
        {
            ulong digit = (ulong)(text[p] - '0');
            if (value > (ulong.MaxValue - digit) / 10)
                overflow = true;
            else
                value = value * 10 + digit;
        }
        if (overflow)
            value = ulong.MaxValue;
        if (negative)
            value = unchecked(0UL - value);
        end = k;
        return value;
    }

    private static ushort AsU16(ulong value)
    {
        if (value > VersionMax)
            value = VersionMax;
        return unchecked((ushort)(short)value);
    }

    private static uint AsU32(ulong value)
    {
        if (value > VersionMax)
            value = VersionMax;
        return unchecked((uint)(short)value);
    }
}
