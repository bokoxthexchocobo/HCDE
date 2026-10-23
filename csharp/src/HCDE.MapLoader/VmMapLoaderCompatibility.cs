using System.Globalization;
using System.Text;

namespace HCDE.MapLoader;

/// <summary>
/// <c>SkipZandronumCompatCommand</c>, <c>ParseCompatibility</c>, and <c>MapLoader::CheckCompatibility</c>
/// from <c>compatibility.cpp</c> (204–391), plus the <c>ApplyCompatibility</c> / <c>ApplyCompatibility2</c>
/// merge steps invoked at the end of <c>CheckCompatibility</c> (<c>g_level.cpp</c> 2581–2606).
/// </summary>
public static class VmMapLoaderCompatibility
{
    public const int GiCompatShortTex = 0x00000010;
    public const int GiCompatStairs = 0x00000020;
    public const int GiNoSectionMerge = 0x00000400;

    public const int MapTypeDoom = 0;

    public const int Level2HexenHack = 0x00800000;

    public const uint CompatfShortTex = 1u << 0;
    public const uint CompatfStairIndex = 1u << 1;
    public const uint CompatfLight = 1u << 29;
    public const uint CompatfPolyobj = 1u << 30;

    public const uint BcompatfFloatBob = 1u << 8;

    public const int DmsgNotify = 2;

    private enum CompatSlot
    {
        SlotCompat = 0,
        SlotCompat2 = 1,
        SlotBcompat = 2,
    }

    private readonly record struct CompatOption(string Name, uint CompatFlags, CompatSlot Slot);

    private static readonly CompatOption[] Options =
    [
        new("setslopeoverflow", 1u << 0, CompatSlot.SlotBcompat),
        new("resetplayerspeed", 1u << 1, CompatSlot.SlotBcompat),
        new("vileghosts", 1u << 25, CompatSlot.SlotCompat),
        new("ignoreteleporttags", 1u << 3, CompatSlot.SlotBcompat),
        new("rebuildnodes", 1u << 5, CompatSlot.SlotBcompat),
        new("linkfrozenprops", 1u << 6, CompatSlot.SlotBcompat),
        new("floatbob", 1u << 8, CompatSlot.SlotBcompat),
        new("noslopeid", 1u << 9, CompatSlot.SlotBcompat),
        new("clipmidtex", 1u << 10, CompatSlot.SlotBcompat),
        new("nosectionmerge", 1u << 11, CompatSlot.SlotBcompat),
        new("nomirrors", 1u << 12, CompatSlot.SlotBcompat),
        new("shorttex", 1u << 0, CompatSlot.SlotCompat),
        new("stairs", 1u << 1, CompatSlot.SlotCompat),
        new("limitpain", 1u << 2, CompatSlot.SlotCompat),
        new("nopassover", 1u << 4, CompatSlot.SlotCompat),
        new("notossdrops", 1u << 7, CompatSlot.SlotCompat),
        new("useblocking", 1u << 8, CompatSlot.SlotCompat),
        new("nodoorlight", 1u << 9, CompatSlot.SlotCompat),
        new("ravenscroll", 1u << 10, CompatSlot.SlotCompat),
        new("soundtarget", 1u << 11, CompatSlot.SlotCompat),
        new("dehhealth", 1u << 12, CompatSlot.SlotCompat),
        new("trace", 1u << 13, CompatSlot.SlotCompat),
        new("dropoff", 1u << 14, CompatSlot.SlotCompat),
        new("boomscroll", 1u << 15, CompatSlot.SlotCompat),
        new("invisibility", 1u << 16, CompatSlot.SlotCompat),
        new("silentinstantfloors", 1u << 17, CompatSlot.SlotCompat),
        new("sectorsounds", 1u << 18, CompatSlot.SlotCompat),
        new("missileclip", 1u << 19, CompatSlot.SlotCompat),
        new("crossdropoff", 1u << 20, CompatSlot.SlotCompat),
        new("wallrun", 1u << 6, CompatSlot.SlotCompat),
        new("anybossdeath", 1u << 21, CompatSlot.SlotCompat),
        new("mushroom", 1u << 23, CompatSlot.SlotCompat),
        new("mbfmonstermove", 1u << 24, CompatSlot.SlotCompat),
        new("noblockfriends", 1u << 26, CompatSlot.SlotCompat),
        new("spritesort", 1u << 27, CompatSlot.SlotCompat),
        new("hitscan", 1u << 28, CompatSlot.SlotCompat),
        new("lightlevel", 1u << 29, CompatSlot.SlotCompat),
        new("polyobj", 1u << 30, CompatSlot.SlotCompat),
        new("maskedmidtex", 1u << 31, CompatSlot.SlotCompat),
        new("badangles", 1u << 0, CompatSlot.SlotCompat2),
        new("floormove", 1u << 1, CompatSlot.SlotCompat2),
        new("soundcutoff", 1u << 2, CompatSlot.SlotCompat2),
        new("pointonline", 1u << 3, CompatSlot.SlotCompat2),
        new("multiexit", 1u << 4, CompatSlot.SlotCompat2),
        new("teleport", 1u << 5, CompatSlot.SlotCompat2),
        new("disablepushwindowcheck", 1u << 6, CompatSlot.SlotCompat2),
        new("checkswitchrange", 1u << 7, CompatSlot.SlotCompat2),
        new("explode1", 1u << 8, CompatSlot.SlotCompat2),
        new("explode2", 1u << 9, CompatSlot.SlotCompat2),
        new("railing", 1u << 10, CompatSlot.SlotCompat2),
        new("scriptwait", 1u << 11, CompatSlot.SlotCompat2),
        new("nombf21", 1u << 14, CompatSlot.SlotCompat2),
        new("noid24", 1u << 24, CompatSlot.SlotCompat2),
        new("voodoozombies", 1u << 15, CompatSlot.SlotCompat2),
        new("fdteleport", 1u << 16, CompatSlot.SlotCompat2),
        new("noacsargcheck", 1u << 17, CompatSlot.SlotCompat2),
        new("novdolllockmsg", 1u << 18, CompatSlot.SlotCompat2),
        new("emulatemikoportals", 1u << 19, CompatSlot.SlotCompat2),
    ];

    private static readonly string[] ZandronumCompatCommands =
    [
        "sectorflooroffset",
        "setwallyscale",
        "setlineflags",
        "clearlineflags",
        "setactivation",
        "clearlinespecial",
        "setlinespecial",
        "setthingz",
        "corpsegibs",
    ];

    public struct CompatValues
    {
        public uint Flags1;
        public uint Flags2;
        public uint BugCompatFlags;
        public uint ExtCommandIndex;
    }

    public sealed class CompatibilityDatabase
    {
        internal Dictionary<CompatMd5Key, CompatValues> BCompatMap { get; } = new();

        public void Clear() => BCompatMap.Clear();

        public bool TryGet(ReadOnlySpan<byte> md5, out CompatValues values) =>
            BCompatMap.TryGetValue(CompatMd5Key.FromBytes(md5), out values);
    }

    public sealed class CompatibilityLevelState
    {
        public int MapType { get; set; }
        public int LevelFlags2 { get; set; }
        public int GameInfoFlags { get; set; }
        public bool MapLumpFromIwad { get; set; }

        public uint MapInfoCompatFlags { get; set; }
        public uint MapInfoCompatMask { get; set; }
        public uint MapInfoCompatFlags2 { get; set; }
        public uint MapInfoCompatMask2 { get; set; }

        public uint DmCompatFlags { get; set; }
        public uint DmCompatFlags2 { get; set; }

        public uint IiCompatFlags { get; set; }
        public uint IiCompatFlags2 { get; set; }
        public uint IbCompatFlags { get; set; }

        public uint ICompatFlags { get; set; }
        public uint ICompatFlags2 { get; set; }

        public bool HasMapInfo { get; set; }

        public bool SubsectorLinksCleared { get; set; }
    }

    public readonly struct CompatMd5Key : IEquatable<CompatMd5Key>
    {
        private readonly ulong _a;
        private readonly ulong _b;

        private CompatMd5Key(ulong a, ulong b)
        {
            _a = a;
            _b = b;
        }

        public static CompatMd5Key FromBytes(ReadOnlySpan<byte> md5)
        {
            if (md5.Length != 16) throw new ArgumentException("MD5 must be 16 bytes.", nameof(md5));
            return new CompatMd5Key(
                BitConverter.ToUInt64(md5),
                BitConverter.ToUInt64(md5[8..]));
        }

        public static CompatMd5Key ParseHex(string hex)
        {
            if (hex.Length != 32) throw new CompatibilityParseException("MD5 signature must be exactly 32 characters long");
            Span<byte> bytes = stackalloc byte[16];
            for (var i = 0; i < 32; ++i)
            {
                var c = hex[i];
                int x;
                if (c is >= '0' and <= '9') x = c - '0';
                else
                {
                    c = (char)(c | ('a' ^ 'A'));
                    if (c is >= 'a' and <= 'f') x = c - 'a' + 10;
                    else throw new CompatibilityParseException("MD5 signature must be a hexadecimal value");
                }

                if ((i & 1) == 0) bytes[i / 2] = (byte)(x << 4);
                else bytes[i / 2] |= (byte)x;
            }

            return FromBytes(bytes);
        }

        public bool Equals(CompatMd5Key other) => _a == other._a && _b == other._b;
        public override bool Equals(object? obj) => obj is CompatMd5Key other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_a, _b);
    }

    public sealed class CompatibilityParseException(string message) : Exception(message);

    private static void SkipZandronumCompatCommand(ref CompatTxtScanner sc)
    {
        while (sc.GetString(out var token))
        {
            if (sc.Crossed || token == "}")
            {
                sc.UnGet(token);
                return;
            }
        }
    }

    public static void ParseCompatibility(CompatibilityDatabase database, string text)
    {
        database.Clear();
        var sc = new CompatTxtScanner(text);
        var md5Batch = new List<CompatMd5Key>();

        while (sc.GetString(out var token))
        {
            do
            {
                var md5 = CompatMd5Key.ParseHex(token);
                md5Batch.Add(md5);
                if (!sc.GetString(out token))
                    throw new CompatibilityParseException("Unexpected end of compatibility.txt");
            } while (token != "{");

            var flags = new CompatValues { ExtCommandIndex = uint.MaxValue };
            while (sc.GetString(out token))
            {
                if (TryMatchOption(token, out var opt))
                {
                    switch (opt.Slot)
                    {
                        case CompatSlot.SlotCompat:
                            flags.Flags1 |= opt.CompatFlags;
                            break;
                        case CompatSlot.SlotCompat2:
                            flags.Flags2 |= opt.CompatFlags;
                            break;
                        case CompatSlot.SlotBcompat:
                            flags.BugCompatFlags |= opt.CompatFlags;
                            break;
                    }
                }
                else if (IsZandronumCompatCommand(token))
                {
                    SkipZandronumCompatCommand(ref sc);
                }
                else
                {
                    sc.UnGet(token);
                    break;
                }
            }

            sc.MustGetStringName("}");
            var stored = flags;
            foreach (var md5 in md5Batch)
                database.BCompatMap[md5] = stored;
            md5Batch.Clear();
        }
    }

    public static string CheckCompatibility(
        CompatibilityDatabase database,
        CompatibilityLevelState level,
        ReadOnlySpan<byte> mapChecksum,
        int developer,
        IList<string>? notifyLines = null)
    {
        if (mapChecksum.Length != 16)
            throw new ArgumentException("Map checksum must be 16 bytes.", nameof(mapChecksum));

        if (database.BCompatMap.Count == 0)
            ParseCompatibility(database, ReadDefaultCompatibilityTxt());

        level.IiCompatFlags = 0;
        level.IiCompatFlags2 = 0;
        level.IbCompatFlags = 0;

        if (level.MapLumpFromIwad)
        {
            if ((level.GameInfoFlags & GiCompatShortTex) != 0 && level.MapType == MapTypeDoom)
            {
                level.IiCompatFlags = CompatfShortTex | CompatfLight;
                if ((level.GameInfoFlags & GiCompatStairs) != 0)
                    level.IiCompatFlags |= CompatfStairIndex;
            }
        }

        var found = database.TryGet(mapChecksum, out var flags);

        var hash = FormatMd5Hex(mapChecksum);

        if (developer >= DmsgNotify)
        {
            var line = found
                ? $"MD5 = {hash}, cflags = {flags.Flags1:X8}, cflags2 = {flags.Flags2:X8}, bflags = {flags.BugCompatFlags:X8}\n"
                : $"MD5 = {hash}\n";
            notifyLines?.Add(line);
        }

        if (found)
        {
            level.IiCompatFlags |= flags.Flags1;
            level.IiCompatFlags2 |= flags.Flags2;
            level.IbCompatFlags |= flags.BugCompatFlags;
        }

        ApplyCompatibility(level);
        ApplyCompatibility2(level);

        if ((level.LevelFlags2 & Level2HexenHack) != 0)
            level.IbCompatFlags |= BcompatfFloatBob;

        return hash;
    }

    public static void ApplyCompatibility(CompatibilityLevelState level)
    {
        var old = level.ICompatFlags;
        level.ICompatFlags = GetCompatibility(level, level.DmCompatFlags) | level.IiCompatFlags;
        if (((old ^ level.ICompatFlags) & CompatfPolyobj) != 0)
            level.SubsectorLinksCleared = true;
    }

    public static void ApplyCompatibility2(CompatibilityLevelState level) =>
        level.ICompatFlags2 = GetCompatibility2(level, level.DmCompatFlags2) | level.IiCompatFlags2;

    private static uint GetCompatibility(CompatibilityLevelState level, uint mask) =>
        !level.HasMapInfo
            ? mask
            : (mask & ~level.MapInfoCompatMask) | (level.MapInfoCompatFlags & level.MapInfoCompatMask);

    private static uint GetCompatibility2(CompatibilityLevelState level, uint mask) =>
        !level.HasMapInfo
            ? mask
            : (mask & ~level.MapInfoCompatMask2) | (level.MapInfoCompatFlags2 & level.MapInfoCompatMask2);

    private static string FormatMd5Hex(ReadOnlySpan<byte> md5)
    {
        var sb = new StringBuilder(32);
        foreach (var b in md5)
            sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
        return sb.ToString();
    }

    private static bool TryMatchOption(string token, out CompatOption opt)
    {
        foreach (var candidate in Options)
        {
            if (string.Equals(candidate.Name, token, StringComparison.OrdinalIgnoreCase))
            {
                opt = candidate;
                return true;
            }
        }

        opt = default;
        return false;
    }

    private static bool IsZandronumCompatCommand(string token)
    {
        foreach (var name in ZandronumCompatCommands)
        {
            if (string.Equals(name, token, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private ref struct CompatTxtScanner
    {
        private readonly string _text;
        private int _index;
        private bool _crossed;
        private string? _pushedBack;

        public CompatTxtScanner(string text) => _text = text;

        public bool Crossed => _crossed;

        public bool GetString(out string token)
        {
            while (true)
            {
                if (_pushedBack != null)
                {
                    token = _pushedBack;
                    _pushedBack = null;
                    _crossed = false;
                    return true;
                }

                _crossed = false;
                SkipWhitespaceAndComments();
                if (_index >= _text.Length)
                {
                    token = "";
                    return false;
                }

                var start = _index;
                if (_text[_index] == '{')
                {
                    _index++;
                    token = "{";
                    return true;
                }

                if (_text[_index] == '}')
                {
                    _index++;
                    token = "}";
                    return true;
                }

                while (_index < _text.Length && !char.IsWhiteSpace(_text[_index]) && _text[_index] != '{' && _text[_index] != '}')
                    _index++;

                token = _text[start.._index].Trim();
                if (token.Length != 0)
                    return true;
            }
        }

        public void UnGet(string token)
        {
            _crossed = true;
            _pushedBack = token;
        }

        public void MustGetStringName(string expected)
        {
            if (!GetString(out var token) || token != expected)
                throw new CompatibilityParseException($"Expected '{expected}'.");
        }

        private void SkipWhitespaceAndComments()
        {
            while (_index < _text.Length)
            {
                if (char.IsWhiteSpace(_text[_index]))
                {
                    _index++;
                    continue;
                }

                if (_index + 1 < _text.Length && _text[_index] == '/' && _text[_index + 1] == '/')
                {
                    _index += 2;
                    while (_index < _text.Length && _text[_index] != '\n')
                        _index++;
                    continue;
                }

                break;
            }
        }
    }

    private static string ReadDefaultCompatibilityTxt()
    {
        foreach (var root in EnumerateRepoRootCandidates())
        {
            var path = Path.Combine(root, "wadsrc", "static", "compatibility.txt");
            if (File.Exists(path))
                return File.ReadAllText(path);
        }

        throw new FileNotFoundException("compatibility.txt not found for ParseCompatibility.");
    }

    internal static IEnumerable<string> EnumerateRepoRootCandidates()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            if (string.IsNullOrWhiteSpace(start))
                continue;
            for (var dir = new DirectoryInfo(Path.GetFullPath(start)); dir != null; dir = dir.Parent)
            {
                if (!seen.Add(dir.FullName))
                    continue;
                yield return dir.FullName;
            }
        }
    }
}
