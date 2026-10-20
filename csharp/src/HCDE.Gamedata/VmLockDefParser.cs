namespace HCDE.Gamedata;

/// <summary><c>ParseLock</c> / LOCKDEFS subset from <c>a_keys.cpp</c>.</summary>
public static class VmLockDefParser
{
    private static readonly string[] LockKeywords = ["ANY", "MESSAGE", "REMOTEMESSAGE", "MAPCOLOR", "LOCKEDSOUND"];

    public static bool TryParseDocument(string text, VmLockDefDatabase db, ILockDefActorCatalog actors, ILockDefGameFilter games, List<string> errors)
    {
        var tokens = VmLockDefTokenize.Tokenize(text);
        var i = 0;
        var currentNumber = 0;
        while (i < tokens.Count)
        {
            var cmd = tokens[i++];
            if (cmd.Equals("CLEARLOCKS", StringComparison.OrdinalIgnoreCase))
            {
                db.Locks.Clear();
                db.KeyNumbersByClass.Clear();
                currentNumber = 0;
                continue;
            }

            if (!cmd.Equals("LOCK", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Unknown command '{cmd}' in LockDef.");
                return false;
            }

            if (!TryParseLock(ref i, tokens, db, actors, games, ref currentNumber, errors))
                return false;
        }

        return errors.Count == 0;
    }

    private static bool TryParseLock(ref int i, IReadOnlyList<string> tokens, VmLockDefDatabase db, ILockDefActorCatalog actors,
        ILockDefGameFilter games, ref int currentNumber, List<string> errors)
    {
        if (i >= tokens.Count || !int.TryParse(tokens[i], out var keynum))
        {
            errors.Add("Expected lock index.");
            return false;
        }

        i++;
        if (i < tokens.Count && tokens[i] != "{")
        {
            if (!games.IsActiveGame(tokens[i]))
                keynum = -1;
            i++;
        }

        if (i >= tokens.Count || tokens[i] != "{")
        {
            errors.Add("Expected '{' after lock index.");
            return false;
        }

        i++;
        if (keynum == 0 || keynum < -1)
        {
            errors.Add($"Lock index {keynum} out of range.");
            return false;
        }

        var ignoreKey = keynum == -1;
        var lockDef = keynum == -1 ? new VmLockDefLock() : db.Locks.GetValueOrDefault(keynum) ?? new VmLockDefLock();
        if (keynum != -1)
            db.Locks[keynum] = lockDef;

        if (lockDef.LockSounds.Count == 0)
        {
            lockDef.LockSounds.Add("*keytry");
            lockDef.LockSounds.Add("misc/keytry");
        }

        while (i < tokens.Count && tokens[i] != "}")
        {
            var word = tokens[i++];
            var kw = MatchKeyword(word);
            switch (kw)
            {
                case 0:
                {
                    var group = new VmLockDefKeyGroup();
                    if (!TryParseKeyGroup(ref i, tokens, group, db, actors, ignoreKey, ref currentNumber, errors))
                        return false;
                    if (group.AnyKeyList.Count > 0)
                        lockDef.KeyGroups.Add(group);
                    break;
                }
                case 1:
                    lockDef.Message = RequireBareString(ref i, tokens, errors);
                    break;
                case 2:
                    lockDef.RemoteMessage = RequireBareString(ref i, tokens, errors);
                    break;
                case 3:
                    lockDef.MapColorRgb = RequireRgb(ref i, tokens, errors);
                    break;
                case 4:
                    lockDef.LockSounds.Clear();
                    if (!TryParseSoundList(ref i, tokens, lockDef.LockSounds, errors))
                        return false;
                    break;
                default:
                    if (!actors.TryGet(word, out var isInventory, out var isKey))
                    {
                        errors.Add($"Unknown item '{word}'.");
                        return false;
                    }

                    if (!isInventory)
                    {
                        errors.Add($"'{word}' is not an inventory item.");
                        return false;
                    }

                    var singleGroup = new VmLockDefKeyGroup();
                    singleGroup.AnyKeyList.Add(new VmLockDefKeyRequirement { ActorClass = word, Count = 1 });
                    if (isKey && !ignoreKey && !db.KeyNumbersByClass.ContainsKey(word))
                        db.KeyNumbersByClass[word] = ++currentNumber;
                    lockDef.KeyGroups.Add(singleGroup);
                    break;
            }
        }

        if (i < tokens.Count && tokens[i] == "}") i++;

        if (string.IsNullOrEmpty(lockDef.RemoteMessage) && !string.IsNullOrEmpty(lockDef.Message))
            lockDef.RemoteMessage = lockDef.Message;
        if (string.IsNullOrEmpty(lockDef.Message) && !string.IsNullOrEmpty(lockDef.RemoteMessage))
            lockDef.Message = lockDef.RemoteMessage;

        return true;
    }

    private static bool TryParseKeyGroup(ref int i, IReadOnlyList<string> tokens, VmLockDefKeyGroup group, VmLockDefDatabase db,
        ILockDefActorCatalog actors, bool ignoreKey, ref int currentNumber, List<string> errors)
    {
        if (i >= tokens.Count || tokens[i] != "{")
        {
            errors.Add("Expected '{' after ANY.");
            return false;
        }

        i++;
        while (i < tokens.Count && tokens[i] != "}")
        {
            var className = tokens[i++];
            if (!actors.TryGet(className, out var isInventory, out var isKey))
            {
                errors.Add($"Unknown item '{className}'.");
                return false;
            }

            if (!isInventory)
            {
                errors.Add($"'{className}' is not an inventory item.");
                return false;
            }

            group.AnyKeyList.Add(new VmLockDefKeyRequirement { ActorClass = className, Count = 1 });
            if (isKey && !ignoreKey && !db.KeyNumbersByClass.ContainsKey(className))
                db.KeyNumbersByClass[className] = ++currentNumber;
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        return true;
    }

    private static bool TryParseSoundList(ref int i, IReadOnlyList<string> tokens, List<string> sounds, List<string> errors)
    {
        while (i < tokens.Count && tokens[i] != "}")
        {
            sounds.Add(RequireBareString(ref i, tokens, errors));
            if (i < tokens.Count && tokens[i] == ",") i++;
            else break;
        }

        return true;
    }

    private static int RequireRgb(ref int i, IReadOnlyList<string> tokens, List<string> errors)
    {
        var r = RequireInt(ref i, tokens, errors);
        var g = RequireInt(ref i, tokens, errors);
        var b = RequireInt(ref i, tokens, errors);
        return (r & 0xFF) | ((g & 0xFF) << 8) | ((b & 0xFF) << 16);
    }

    private static int RequireInt(ref int i, IReadOnlyList<string> tokens, List<string> errors)
    {
        if (i >= tokens.Count || !int.TryParse(tokens[i], out var n))
        {
            errors.Add("Expected integer.");
            return 0;
        }

        i++;
        return n;
    }

    private static string RequireBareString(ref int i, IReadOnlyList<string> tokens, List<string> errors)
    {
        if (i >= tokens.Count)
        {
            errors.Add("Expected string.");
            return "";
        }

        return tokens[i++];
    }

    private static int MatchKeyword(string word)
    {
        for (var k = 0; k < LockKeywords.Length; k++)
        {
            if (word.Equals(LockKeywords[k], StringComparison.OrdinalIgnoreCase))
                return k;
        }

        return -1;
    }
}

internal static class VmLockDefTokenize
{
    public static List<string> Tokenize(string text)
    {
        var tokens = new List<string>();
        var i = 0;
        while (i < text.Length)
        {
            if (char.IsWhiteSpace(text[i])) { i++; continue; }
            if (text[i] is '{' or '}' or ',')
            {
                tokens.Add(text[i].ToString());
                i++;
                continue;
            }

            if (text[i] == '"')
            {
                i++;
                var start = i;
                while (i < text.Length && text[i] != '"') i++;
                tokens.Add(text[start..i]);
                if (i < text.Length) i++;
                continue;
            }

            var j = i;
            while (j < text.Length && !char.IsWhiteSpace(text[j]) && text[j] is not '{' and not '}' and not ',' and not '"')
                j++;
            tokens.Add(text[i..j]);
            i = j;
        }

        return tokens;
    }
}
