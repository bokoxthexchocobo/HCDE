namespace HCDE.Playsim;

/// <summary>
/// S_AddPlayerGender through S_MarkPlayerSounds from s_advsound.cpp 1454-1759.
/// The SNDINFO command body above this band is already credited. soundlinks starts at line 1767.
/// </summary>
public static class VmAdvSound1430Slice
{
    public const int BandStart = 1454;
    public const int BandEnd = 1759;
    public const int Reserve = 1;
    public const int Silent = 4;
    public const int NoSound = 0;
    public const int NoLink = -1;
    public const int Empty = -1;
    public const int Missing = 0xffff;
    public const int GenderMax = 4;
    public const int Noskin = 0x20;
    public const int Hexen = 4;

    public sealed class Sfx
    {
        public string Name = "";
        public int User;
        public int Link = NoLink;
        public int Lump = Empty;
        public bool Random;
    }

    public sealed class Class
    {
        public string? Name;
        public int[] Lists = { Missing, Missing, Missing, Missing };
    }

    public sealed class SavedInfo
    {
        public string ClassName = "";
        public int Gender;
        public int Ref;
        public int Lump;
        public bool Alias;
    }

    public sealed class Actor
    {
        public Player? Who;
        public string? SoundClass;
        public int Flags4;
    }

    public sealed class Player
    {
        public Actor? Mo;
        public int Skin;
        public int Gender;
        public string SoundClass = "";
    }

    public sealed class State
    {
        public List<Sfx> Sounds = new();
        public List<Class> Classes = new();
        public List<Dictionary<int, int>> Tables = new();
        public List<SavedInfo> Saved = new();
        public List<SavedInfo> Calls = new();
        public List<int> Marks = new();
        public int MarkPasses;
        public Dictionary<string, int> Names = new(StringComparer.Ordinal);
        public List<string> Skins = new();
        public int PlayerClassCount;
        public int Game;
        public string? DefName = "";
        public int Def;
        public bool Sorted;
    }

    public static int AddGender(State s, int classnum, int gender)
    {
        int index = s.Classes[classnum].Lists[gender];
        if (index == Missing)
        {
            index = s.Tables.Count;
            s.Tables.Add(new Dictionary<int, int>());
            s.Classes[classnum].Lists[gender] = index & 0xffff;
        }
        return index;
    }

    public static void Shrink(State s)
    {
        s.Classes.Sort((a, b) => Compare(a.Name, b.Name));
        s.Sorted = true;
        s.Def = Find(s, s.DefName);
    }

    public static int Find(State s, string? name)
    {
        if (name == null) throw new NullReferenceException();
        if (!s.Sorted)
        {
            for (int i = 0; i < s.Classes.Count; i++)
                if (Compare(name, s.Classes[i].Name) == 0) return i;
            return -1;
        }
        int min = 0;
        int max = s.Classes.Count - 1;
        while (min <= max)
        {
            int mid = (min + max) / 2;
            int lexx = Compare(s.Classes[mid].Name, name);
            if (lexx == 0) return mid;
            if (lexx < 0) min = mid + 1;
            else max = mid - 1;
        }
        return -1;
    }

    public static int Lookup(State s, string? pclass, int gender, int refid)
    {
        Sfx? sfxp = At(s, refid);
        if (sfxp != null && (sfxp.User & Reserve) == 0) return refid;
        if (pclass == null) throw new NullReferenceException();
        return LookupIndex(s, Find(s, pclass), gender, refid);
    }

    public static int LookupIndex(State s, int classidx, int gender, int refid)
    {
        int ingender = gender;
        if (classidx == -1) classidx = s.Def;
        int listidx = s.Classes[classidx].Lists[gender];
        if (listidx == Missing)
        {
            int g;
            for (g = 0; g < GenderMax; g++)
            {
                listidx = s.Classes[classidx].Lists[g];
                if (listidx != Missing) break;
            }
            if (g == GenderMax)
            {
                if (classidx != s.Def) return LookupIndex(s, s.Def, gender, refid);
                return NoSound;
            }
            gender = g;
        }
        Sfx? sfxp = At(s, refid);
        if (sfxp == null) return NoSound;
        int sndnum = s.Tables[listidx].TryGetValue(sfxp.Link, out int found) ? found : NoSound;
        sfxp = At(s, sndnum);
        if (s.Sorted && (sfxp == null || sndnum == NoSound ||
            ((sfxp.Lump == -1 || sfxp.Lump == Empty) && sfxp.Link == NoLink && (sfxp.User & Silent) == 0)))
        {
            if (ingender != 0) return LookupIndex(s, classidx, 0, refid);
            if (classidx != s.Def) return LookupIndex(s, s.Def, gender, refid);
        }
        return sndnum;
    }

    public static void Save(State s, string pclass, int gender, int refid, int lumpnum, bool alias)
    {
        if (pclass == null) throw new NullReferenceException();
        s.Saved.Add(new SavedInfo { ClassName = pclass, Gender = gender, Ref = refid, Lump = lumpnum, Alias = alias });
    }

    public static void Restore(State s)
    {
        foreach (SavedInfo spi in s.Saved)
        {
            s.Calls.Add(new SavedInfo
            {
                ClassName = spi.ClassName,
                Gender = spi.Gender,
                Ref = spi.Ref,
                Lump = spi.Lump,
                Alias = spi.Alias,
            });
        }
    }

    public static bool Equivalent(State s, Actor? actor, int id1, int id2)
    {
        if (id1 == id2) return true;
        if (id1 <= 0 || id2 <= 0) return false;
        return Walk(s, actor, id1) == Walk(s, actor, id2);
    }

    public static string SoundClass(State s, Actor pp)
    {
        if (pp == null) throw new NullReferenceException();
        Player? player = pp.Who;
        string fallback = string.IsNullOrEmpty(pp.SoundClass) ? "player" : pp.SoundClass;
        if (player != null &&
            (player.Mo == null || (player.Mo.Flags4 & Noskin) == 0) &&
            (uint)player.Skin >= (uint)s.PlayerClassCount &&
            (uint)player.Skin < (uint)s.Skins.Count &&
            player.SoundClass.Length == 0)
            return s.Skins[player.Skin];
        return player == null || player.SoundClass.Length == 0 ? fallback : player.SoundClass;
    }

    public static int Skinned(State s, Actor? actor, int refid)
    {
        string pclass;
        int gender = 0;
        if (actor != null && actor.Who != null)
        {
            pclass = SoundClass(s, actor);
            gender = actor.Who.Gender;
        }
        else pclass = s.Game == Hexen ? "fighter" : "player";
        return Lookup(s, pclass, gender, refid);
    }

    public static int SkinnedEx(State s, Actor? actor, string? name, string? extended)
    {
        if (name == null || extended == null) throw new NullReferenceException();
        string full = name + "-" + extended;
        int id = FindSound(s, full);
        if (id <= 0) id = FindSound(s, name);
        return Skinned(s, actor, id);
    }

    public static void Mark(State s, Actor player)
    {
        string playerclass = SoundClass(s, player);
        int classidx = Find(s, playerclass);
        if (classidx < 0) classidx = s.Def;
        for (int g = 0; g < GenderMax; g++)
        {
            // Native reads gender slot 0 on every pass.
            int listidx = s.Classes[classidx].Lists[0];
            if (listidx != Missing) MarkTable(s, listidx);
        }
    }

    static int Walk(State s, Actor? actor, int id)
    {
        while (true)
        {
            Sfx sfx = At(s, id) ?? throw new NullReferenceException();
            if (sfx.Link == NoLink) return id;
            if ((sfx.User & Reserve) != 0) id = Skinned(s, actor, id);
            else if (sfx.Random) return id;
            else id = sfx.Link;
        }
    }

    static void MarkTable(State s, int list)
    {
        s.MarkPasses++;
        foreach (int value in s.Tables[list].Values) s.Marks.Add(value);
    }

    static int FindSound(State s, string name)
    {
        if (name.Length == 0) return NoSound;
        return s.Names.TryGetValue(Lower(name), out int id) ? id : NoSound;
    }

    static Sfx? At(State s, int id) => (uint)id < (uint)s.Sounds.Count ? s.Sounds[id] : null;

    static int Compare(string? a, string? b)
    {
        if (a == null || b == null) throw new NullReferenceException();
        int n = a.Length < b.Length ? a.Length : b.Length;
        for (int i = 0; i < n; i++)
        {
            int d = Fold(a[i]) - Fold(b[i]);
            if (d != 0) return d;
        }
        return a.Length - b.Length;
    }

    static int Fold(char c) => (uint)(c - 'A') <= 25u ? c + 32 : c;

    static string Lower(string name)
    {
        char[] chars = name.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
            if ((uint)(chars[i] - 'A') <= 25u) chars[i] = (char)(chars[i] + 32);
        return new string(chars);
    }
}
