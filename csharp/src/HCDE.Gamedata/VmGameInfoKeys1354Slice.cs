using System.Globalization;

namespace HCDE.Gamedata;

/// <summary>
/// Game names, screen borders, VM field names, and the GAMEINFO key macros
/// from <c>gi.cpp</c> (1-260). Chunk 1354. Color lookup, sound lookup, and
/// the file container stay injected. <c>ParseGameInfo</c> stays in chunk 206.
/// </summary>
public static class VmGameInfoKeys1354Slice
{
    public const string ScriptClass = "AGameInfoStruct";
    public const string GlobalName = "gameinfo";
    public const string TurboCvar = "turbo";
    public const string NameNone = "None";
    public const string NameNull = "Null";

    public static readonly string?[] GameNames =
    [
        null, "Doom", "Heretic", null, "Hexen", null, null, null,
        "Strife", null, null, null, null, null, null, null, "Chex",
    ];

    public static readonly string?[] BorderNames = ["DoomBorder", "HereticBorder", "StrifeBorder", null];

    public static readonly Border1354 DoomBorder = Border(8, 8, "brdr_tl", "brdr_t", "brdr_tr", "brdr_l", "brdr_r", "brdr_bl", "brdr_b", "brdr_br");
    public static readonly Border1354 HereticBorder = Border(4, 16, "bordtl", "bordt", "bordtr", "bordl", "bordr", "bordbl", "bordb", "bordbr");
    public static readonly Border1354 StrifeBorder = Border(8, 8, "brdr_tl", "brdr_t", "brdr_tr", "brdr_l", "brdr_r", "brdr_bl", "brdr_b", "brdr_br");

    public static readonly string[] FieldNames =
    [
        "backpacktype", "Armor2Percent", "ArmorIcon1", "ArmorIcon2", "BasicArmorClass", "HexenArmorClass",
        "gametype", "norandomplayerclass", "infoPages", "mBackButton", "mStatscreenMapNameFont",
        "mStatscreenEnteringFont", "mStatscreenFinishedFont", "mStatscreenContentFont", "mStatscreenAuthorFont",
        "gibfactor", "intermissioncounter", "statusscreen_single", "statusscreen_coop", "statusscreen_dm",
        "mSliderColor", "mSliderBackColor", "defaultbloodcolor", "telefogheight", "defKickback", "healthpic",
        "berserkpic", "defaultdropstyle", "normforwardmove", "normsidemove", "mHideParTimes", "BloodSplatDecalDistance",
    ];

    public static bool ApplyCString(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable, int length)
    {
        if (!Same(nextKey, variable))
            return false;
        sc.MustString();
        if (sc.Text.Length > length)
            sc.ScriptError($"Value for '{field}' can not be longer than {length} characters.");
        game.Text[field] = sc.Text;
        return true;
    }

    public static bool ApplyString(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable)
    {
        if (!Same(nextKey, variable))
            return false;
        sc.MustString();
        game.Text[field] = sc.Text;
        return true;
    }

    public static bool ApplyStringArray(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable, int length, bool clear)
    {
        if (!Same(nextKey, variable))
            return false;
        if (clear || !game.Texts.ContainsKey(field))
            game.Texts[field] = [];
        do
        {
            sc.MustString();
            if (length > 0 && sc.Text.Length > length)
                sc.ScriptError($"Value for '{field}' can not be longer than {length} characters.");
            game.Texts[field].Add(sc.Text);
        }
        while (sc.Check(','));
        return true;
    }

    public static bool ApplySoundArray(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable, int length, bool clear)
    {
        if (!Same(nextKey, variable))
            return false;
        if (sc.FindSound == null)
            throw new InvalidOperationException("S_FindSound");
        if (clear || !game.Sounds.ContainsKey(field))
            game.Sounds[field] = [];
        do
        {
            sc.MustString();
            if (length > 0 && sc.Text.Length > length)
                sc.ScriptError($"Value for '{field}' can not be longer than {length} characters.");
            game.Sounds[field].Add(sc.FindSound(sc.Text));
        }
        while (sc.Check(','));
        return true;
    }

    public static bool ApplyStamped(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable, string stamp)
    {
        if (!Same(nextKey, variable))
            return false;
        sc.MustString();
        if (sc.Container == null)
            throw new InvalidOperationException("GetFileContainer");
        game.Text[field] = sc.Text;
        game.Ints[stamp] = sc.Container(sc.LumpNum);
        return true;
    }

    public static bool ApplyInt(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable)
    {
        if (!Same(nextKey, variable))
            return false;
        sc.MustNumber();
        game.Ints[field] = sc.Number;
        return true;
    }

    public static bool ApplyFloat(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable)
    {
        if (!Same(nextKey, variable))
            return false;
        sc.MustFloat();
        game.Floats[field] = (float)sc.Float;
        return true;
    }

    public static bool ApplyDouble(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable)
    {
        if (!Same(nextKey, variable))
            return false;
        sc.MustFloat();
        game.Doubles[field] = sc.Float;
        return true;
    }

    public static bool ApplyTwoDoubles(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable)
    {
        if (!Same(nextKey, variable))
            return false;
        sc.MustValue();
        double first = sc.Float;
        sc.Must(',');
        sc.MustValue();
        game.Pairs[field] = (first, sc.Float);
        return true;
    }

    public static bool ApplyColor(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable)
    {
        if (!Same(nextKey, variable))
            return false;
        sc.MustString();
        if (sc.ColorByName == null)
            throw new InvalidOperationException("V_GetColorStringByName");
        if (sc.ColorFromString == null)
            throw new InvalidOperationException("V_GetColorFromString");
        string color = sc.Text;
        string named = sc.ColorByName(color);
        if (named.Length != 0)
            color = named;
        game.Ints[field] = sc.ColorFromString(color);
        return true;
    }

    public static bool ApplyBool(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable)
    {
        if (!Same(nextKey, variable))
            return false;
        if (sc.Check('b'))
            game.Bools[field] = false;
        else
        {
            sc.Must('t');
            game.Bools[field] = true;
        }
        return true;
    }

    public static bool ApplyFont(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable)
    {
        if (!Same(nextKey, variable))
            return false;
        sc.MustString();
        string name = sc.Text;
        string color = NameNone;
        if (sc.Check(','))
        {
            sc.MustString();
            color = sc.Text;
        }
        game.Fonts[field] = (name, color);
        return true;
    }

    public static bool ApplyPatch(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string variable)
    {
        if (!Same(nextKey, variable))
            return false;
        sc.MustString();
        game.Fonts[field] = (sc.Text, NameNull);
        return true;
    }

    public static bool ApplyMusic(GameInfo1354 game, Scan1354 sc, string nextKey, string field, string orderField, string variable)
    {
        if (!Same(nextKey, variable))
            return false;
        sc.MustString();
        int order = 0;
        string song = sc.Text;
        int colon = song.IndexOf(':');
        if (colon >= 0)
        {
            order = Atoi(song[(colon + 1)..]);
            song = song[..colon];
        }
        game.Text[field] = song;
        game.Ints[orderField] = order;
        return true;
    }

    public static int Atoi(string text)
    {
        int i = 0;
        while (i < text.Length && char.IsWhiteSpace(text[i]))
            i++;
        int sign = 1;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }
        int value = 0;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            value = value * 10 + (text[i] - '0');
            i++;
        }
        return sign * value;
    }

    private static bool Same(string nextKey, string variable) =>
        nextKey.Equals(variable, StringComparison.OrdinalIgnoreCase);

    private static Border1354 Border(byte offset, byte size, string tl, string t, string tr, string l, string r, string bl, string b, string br) =>
        new() { Offset = offset, Size = size, Tl = tl, T = t, Tr = tr, L = l, R = r, Bl = bl, B = b, Br = br };
}

public sealed class Border1354
{
    public byte Offset;
    public byte Size;
    public string Tl = "";
    public string T = "";
    public string Tr = "";
    public string L = "";
    public string R = "";
    public string Bl = "";
    public string B = "";
    public string Br = "";
}

public sealed class GameInfo1354
{
    public Dictionary<string, string> Text = new();
    public Dictionary<string, List<string>> Texts = new();
    public Dictionary<string, List<int>> Sounds = new();
    public Dictionary<string, int> Ints = new();
    public Dictionary<string, float> Floats = new();
    public Dictionary<string, double> Doubles = new();
    public Dictionary<string, (double First, double Second)> Pairs = new();
    public Dictionary<string, bool> Bools = new();
    public Dictionary<string, (string Name, string Color)> Fonts = new();
}

public sealed class Tok1354
{
    public Tok1354(char kind, string text = "")
    {
        Kind = kind;
        Text = text;
    }

    public char Kind { get; }
    public string Text { get; }
}

public sealed class Scan1354
{
    public List<Tok1354> Items = [];
    public int At;
    public string Text = "";
    public int Number;
    public double Float;
    public int LumpNum;
    public Func<string, string>? ColorByName;
    public Func<string, int>? ColorFromString;
    public Func<string, int>? FindSound;
    public Func<int, int>? Container;

    public void ScriptError(string message) => throw new InvalidOperationException(message);

    public bool Check(char kind)
    {
        if (At >= Items.Count || Items[At].Kind != kind)
            return false;
        Take();
        return true;
    }

    public void Must(char kind)
    {
        if (At >= Items.Count)
            ScriptError("Missing token (unexpected end of file).");
        if (Items[At].Kind != kind)
            ScriptError("Expected " + Expected(kind) + " but got " + Shown(Items[At]) + " instead.");
        Take();
    }

    public void MustString()
    {
        Must('s');
        Text = Items[At - 1].Text;
    }

    public void MustNumber()
    {
        if (At >= Items.Count || Items[At].Kind != 'n')
            ScriptError("Missing integer (unexpected end of file).");
        Number = int.Parse(Items[At].Text, CultureInfo.InvariantCulture);
        At++;
    }

    public void MustFloat()
    {
        if (At >= Items.Count)
            ScriptError("Missing floating-point number (unexpected end of file).");
        if (Items[At].Kind != 'n' && Items[At].Kind != 'd')
            ScriptError("SC_GetFloat: Bad numeric constant \"" + Items[At].Text + "\".");
        Float = double.Parse(Items[At].Text, CultureInfo.InvariantCulture);
        Number = (int)Float;
        At++;
    }

    public void MustValue()
    {
        if (At >= Items.Count || (Items[At].Kind != 'n' && Items[At].Kind != 'd'))
            ScriptError("Numeric constant expected");
        Float = double.Parse(Items[At].Text, CultureInfo.InvariantCulture);
        Number = (int)Float;
        At++;
    }

    private Tok1354 Take()
    {
        if (At >= Items.Count)
            ScriptError("Missing token (unexpected end of file).");
        return Items[At++];
    }

    private static string Expected(char kind) => kind switch
    {
        's' => "string constant",
        ',' => "','",
        't' => "'true'",
        _ => "token",
    };

    private static string Shown(Tok1354 tok) => tok.Kind switch
    {
        's' => "string constant",
        'n' => "integer constant",
        'd' => "float constant",
        ',' => "','",
        't' => "'true'",
        'b' => "'false'",
        _ => tok.Text,
    };
}
