namespace HCDE.Playsim;

/// <summary>
/// Debugger global scope from <c>GlobalScopeStateNode.cpp</c> 1-163. Chunk 1400.
/// <c>GetVMValue</c> and <c>CreateNodeForVariable</c> stay callees. The file ends at line 163.
/// </summary>
public static class VmGlobalScope1400Slice
{
    public const int BandStart = 1;
    public const int BandEnd = 163;
    public const string ScopeName = "Global";
    public const string PresentationHint = "globals";

    public static readonly string[] GlobalNames =
    [
        "NotifyFontScale", "ConsoleState", "menuactive", "BackbuttonTime", "BackbuttonAlpha",
        "GameTicRate", "menuDelegate", "WP_NOCHANGE", "SmallFont", "SmallFont2", "BigFont",
        "ConFont", "NewConsoleFont", "NewSmallFont", "AlternativeSmallFont", "AlternativeBigFont",
        "OriginalSmallFont", "OriginalBigFont", "IntermissionFont", "CleanXfac", "CleanYfac",
        "CleanWidth", "CleanHeight", "CleanXfac_1", "CleanYfac_1", "CleanWidth_1", "CleanHeight_1",
        "AllServices", "Bindings", "AutomapBindings", "generic_ui", "deh", "gameinfo", "Teams",
        "LocalViewPitch", "StatusBar", "players", "playeringame", "PlayerClasses", "consoleplayer",
        "validcount", "multiplayer", "gameaction", "gamestate", "skyflatnum", "globalfreeze",
        "gametic", "demoplayback", "automapactive", "viewactive", "Net_Arbitrator", "netgame",
        "paused", "Terrains", "OptionMenuSettings", "musplaying", "AllClasses", "Level",
        "AllActorClasses",
    ];

    public sealed class Symbol
    {
        public string Key = "";
        public bool NameIsNone;
        public bool IsField;
        public object? Type;
        public int TypeReads;
        public ulong Offset;
        public int BitValue;
    }

    public sealed class Namespace
    {
        public List<Symbol> Symbols { get; } = new();
    }

    public sealed class Node
    {
        public string Name = "";
        public object? Value;
        public object? Type;
        public string StoredKey = "";
    }

    public sealed class ProtocolScope
    {
        public string Name = "";
        public bool Expensive;
        public string PresentationHint = "";
        public int VariablesReference;
        public int NamedVariables;
        public int IndexedVariables;
    }

    public sealed class Scope
    {
        public int Id;
        public List<Namespace> Namespaces { get; } = new();
        readonly List<Node> children = new();
        public int Scans;

        public bool Serialize(ProtocolScope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));
            scope.Name = ScopeName;
            scope.Expensive = false;
            scope.PresentationHint = VmGlobalScope1400Slice.PresentationHint;
            scope.VariablesReference = Id;
            var names = new List<string>();
            GetChildNames(names);
            scope.NamedVariables = names.Count;
            scope.IndexedVariables = 0;
            return true;
        }

        public bool GetChildNames(List<string> names)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));
            foreach (string name in GlobalNames)
                names.Add(name);
            return true;
        }

        public bool TryGetChild(
            string name,
            ref Node? node,
            Func<ulong, object?, int, object?> readValue,
            Func<string, object?, object?, Node> createNode)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (readValue == null)
                throw new ArgumentNullException(nameof(readValue));
            if (createNode == null)
                throw new ArgumentNullException(nameof(createNode));
            if (children.Count == 0)
            {
                Scans++;
                var published = new List<string>();
                GetChildNames(published);
                foreach (Namespace ns in Namespaces)
                {
                    foreach (Symbol symbol in ns.Symbols)
                    {
                        if (symbol == null)
                            throw new InvalidOperationException("symbol");
                        if (!Contains(published, symbol.Key))
                            continue;
                        if (symbol.NameIsNone)
                            continue;
                        if (!symbol.IsField)
                            continue;
                        symbol.TypeReads++;
                        object? value = readValue(symbol.Offset, symbol.Type, symbol.BitValue);
                        Node created = createNode(symbol.Key, value, symbol.Type);
                        Upsert(symbol.Key, created);
                    }
                }
            }

            foreach (Node child in children)
            {
                if (Same(child.StoredKey, name))
                {
                    node = child;
                    return true;
                }
            }

            return false;
        }

        void Upsert(string key, Node created)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (Same(children[i].StoredKey, key))
                {
                    created.StoredKey = children[i].StoredKey;
                    children[i] = created;
                    return;
                }
            }

            created.StoredKey = key;
            children.Add(created);
        }
    }

    public static bool Contains(List<string> names, string key)
    {
        foreach (string name in names)
        {
            if (Same(name, key))
                return true;
        }

        return false;
    }

    public static bool Same(string left, string right)
    {
        if (left.Length != right.Length)
            return false;
        for (int i = 0; i < left.Length; i++)
        {
            if (Fold(left[i]) != Fold(right[i]))
                return false;
        }

        return true;
    }

    public static char Fold(char value)
    {
        if (value is >= 'A' and <= 'Z')
            return (char)(value + 32);
        return value;
    }
}
