using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// String map, JSON round trip, and iterator from <c>dictionary.cpp</c> (<b>1–241</b>).
/// Chunk <b>1205</b>. Pair order is insertion order. TMap slot order is unspecified.
/// The JSON codec is the serializer.cpp helper these exports call.
/// </summary>
public sealed class VmDictionary1205Slice
{
    readonly List<(string Key, string Value)> Order = new();
    readonly Dictionary<string, int> Index = new();

    public sealed class ScriptError : Exception
    {
        public ScriptError(string message) : base(message) { Detail = message; }
        public string Detail;
    }

    public static VmDictionary1205Slice Create()
    {
        return new VmDictionary1205Slice();
    }

    public void Insert(string key, string value)
    {
        if (Index.TryGetValue(key, out int at))
            Order[at] = (key, value);
        else
        {
            Index.Add(key, Order.Count);
            Order.Add((key, value));
        }
    }

    public string At(string key)
    {
        return Index.TryGetValue(key, out int at) ? Order[at].Value : "";
    }

    public void Remove(string key)
    {
        if (!Index.Remove(key, out int at))
            return;
        Order.RemoveAt(at);
        for (int i = at; i < Order.Count; i++)
            Index[Order[i].Key] = i;
    }

    public string ToJson()
    {
        var built = new StringBuilder();
        built.Append('{');
        for (int i = 0; i < Order.Count; i++)
        {
            if (i != 0)
                built.Append(',');
            AppendJson(built, Order[i].Key);
            built.Append(':');
            AppendJson(built, Order[i].Value);
        }

        built.Append('}');
        return built.ToString();
    }

    public static VmDictionary1205Slice? FromJson(string text)
    {
        if (text == "null")
            return null;
        if (text.Length == 0)
            return Create();
        int i = 0;
        Skip(text, ref i);
        if (i >= text.Length || text[i] != '{')
            throw ObjectError();
        i++;
        var dict = Create();
        Skip(text, ref i);
        if (i < text.Length && text[i] == '}')
        {
            i++;
            Skip(text, ref i);
            if (i != text.Length)
                throw ObjectError();
            return dict;
        }

        while (true)
        {
            Skip(text, ref i);
            if (i >= text.Length || text[i] != '"')
                throw ObjectError();
            string key = ReadString(text, ref i);
            Skip(text, ref i);
            if (i >= text.Length || text[i] != ':')
                throw ObjectError();
            i++;
            Skip(text, ref i);
            if (i >= text.Length)
                throw ObjectError();
            if (text[i] != '"')
            {
                if (text[i] is '{' or '[' or '-' or 't' or 'f' or 'n' || text[i] is >= '0' and <= '9')
                    throw new ScriptError("Dictionary value is expected to be a JSON string.");
                throw ObjectError();
            }

            dict.Insert(key, ReadString(text, ref i));
            Skip(text, ref i);
            if (i >= text.Length)
                throw ObjectError();
            if (text[i] == ',')
            {
                i++;
                continue;
            }

            if (text[i] == '}')
            {
                i++;
                Skip(text, ref i);
                if (i != text.Length)
                    throw ObjectError();
                return dict;
            }

            throw ObjectError();
        }
    }

    public void Serialize(bool writing, ref string contents)
    {
        if (writing)
        {
            contents = ToJson();
            return;
        }

        var loaded = FromJson(contents);
        if (loaded == null)
            throw new NullReferenceException();
        Order.Clear();
        Index.Clear();
        foreach (var pair in loaded.Order)
            Insert(pair.Key, pair.Value);
    }

    public sealed class Iterator
    {
        VmDictionary1205Slice? Dict;
        int Cursor;
        string CurrentKey = "";
        string CurrentValue = "";
        bool Have;

        public static Iterator Create(VmDictionary1205Slice dict)
        {
            var iterator = new Iterator();
            iterator.Init(dict);
            return iterator;
        }

        public void Init(VmDictionary1205Slice dict)
        {
            Dict = dict;
            Cursor = 0;
            Have = false;
        }

        public bool Next()
        {
            if (Dict == null)
                throw new InvalidOperationException("Iterator is null.");
            if (Cursor >= Dict.Order.Count)
                return false;
            (CurrentKey, CurrentValue) = Dict.Order[Cursor];
            Have = true;
            Cursor++;
            return true;
        }

        public string Key()
        {
            return Have ? CurrentKey : "";
        }

        public string Value()
        {
            return Have ? CurrentValue : "";
        }

        public void Serialize(bool writing)
        {
            if (writing)
                throw new ScriptError("Attempt to save pointer to unhandled type DictionaryIterator");
        }
    }

    static void AppendJson(StringBuilder built, string text)
    {
        built.Append('"');
        foreach (char c in text)
        {
            switch (c)
            {
                case '"': built.Append("\\\""); break;
                case '\\': built.Append("\\\\"); break;
                case '\b': built.Append("\\b"); break;
                case '\f': built.Append("\\f"); break;
                case '\n': built.Append("\\n"); break;
                case '\r': built.Append("\\r"); break;
                case '\t': built.Append("\\t"); break;
                default:
                    if (c < 0x20)
                    {
                        built.Append("\\u");
                        built.Append(((int)c).ToString("X4"));
                    }
                    else
                        built.Append(c);
                    break;
            }
        }

        built.Append('"');
    }

    static string ReadString(string text, ref int i)
    {
        if (i >= text.Length || text[i] != '"')
            throw ObjectError();
        i++;
        var built = new StringBuilder();
        while (i < text.Length)
        {
            char c = text[i++];
            if (c == '"')
                return built.ToString();
            if (c != '\\')
            {
                built.Append(c);
                continue;
            }

            if (i >= text.Length)
                throw ObjectError();
            char escape = text[i++];
            switch (escape)
            {
                case '"': built.Append('"'); break;
                case '\\': built.Append('\\'); break;
                case '/': built.Append('/'); break;
                case 'b': built.Append('\b'); break;
                case 'f': built.Append('\f'); break;
                case 'n': built.Append('\n'); break;
                case 'r': built.Append('\r'); break;
                case 't': built.Append('\t'); break;
                case 'u': built.Append(ReadHex(text, ref i)); break;
                default: throw ObjectError();
            }
        }

        throw ObjectError();
    }

    static char ReadHex(string text, ref int i)
    {
        int value = 0;
        for (int k = 0; k < 4; k++)
        {
            if (i >= text.Length)
                throw ObjectError();
            int digit = Hex(text[i++]);
            if (digit < 0)
                throw ObjectError();
            value = (value << 4) | digit;
        }

        return (char)value;
    }

    static int Hex(char c)
    {
        if (c is >= '0' and <= '9')
            return c - '0';
        if (c is >= 'a' and <= 'f')
            return c - 'a' + 10;
        if (c is >= 'A' and <= 'F')
            return c - 'A' + 10;
        return -1;
    }

    static void Skip(string text, ref int i)
    {
        while (i < text.Length && text[i] is ' ' or '\t' or '\n' or '\r')
            i++;
    }

    static ScriptError ObjectError()
    {
        return new ScriptError("Dictionary is expected to be a JSON object.");
    }
}
