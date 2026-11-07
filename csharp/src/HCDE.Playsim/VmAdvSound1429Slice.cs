using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// S_CheckSound through S_AddLocalSndInfo from s_advsound.cpp 251-644.
/// Declarations above line 251 stay outside this slice. Include paths start at line 657.
/// </summary>
public static class VmAdvSound1429Slice
{
    public const int BandStart = 251;
    public const int BandEnd = 644;
    public const int Reserve = 1;
    public const int Compat = 2;
    public const int NoSound = 0;
    public const int NoLink = -1;
    public const int Global = 0;
    public const int Voices = 9;
    public const int Sounds = 13;
    public const string Red = "\u001cG";
    public const string Orange = "\u001cI";
    public const string Reserved = "Sounds that are reserved for players cannot be reassigned";
    public const string LocalLocked = "Local SNDINFO cannot be combined with DSDHacked sounds!";

    public sealed class Sfx
    {
        public string Name = "";
        public int User;
        public bool Random;
        public bool ListNull;
        public int[] Choices = Array.Empty<int>();
        public int Owner = NoSound;
        public int Link = NoLink;
        public int Lump = NoLink;
        public bool Tentative;
        public int Near = -1;
        public int Range;
        public int Ms;
        public bool LoadNull;
        public bool DataNull;
    }

    public sealed class Entry
    {
        public int Space;
        public string Name = "";
    }

    public sealed class Save
    {
        public string ClassName = "";
        public int Gender;
        public int Ref;
        public int Lump;
        public bool Alias;
    }

    public sealed class State
    {
        public bool EngineNull;
        public List<Sfx?> Sounds = new();
        public Dictionary<string, int> Ids = new(StringComparer.Ordinal);
        public Dictionary<string, int> FullNames = new(StringComparer.Ordinal);
        public List<string> Looks = new();
        public List<string> Lines = new();
        public bool Script;
        public int PitchMask;
        public List<int> Masks = new();
        public List<string> ClassNames = new();
        public List<int> ClassReturns = new();
        public int ClassAt;
        public List<(int Class, int Gender)> Genders = new();
        public List<int> GenderReturns = new();
        public int GenderAt;
        public List<List<(int Player, int Sfx)>> PlayerSounds = new();
        public List<Save> Saves = new();
        public List<int> LookupReturns = new();
        public int LookupAt;
        public List<(string? ClassName, int Gender, int Ref)> Lookups = new();
        public List<int> SkinReturns = new();
        public int SkinAt;
        public int Loads;
        public int RumbleReady;
        public int RumbleResets;
        public int EngineClears;
        public List<string> Ambients = new();
        public List<string> Volumes = new();
        public int Reserves = 4;
        public bool Sorted = true;
        public List<string> Classes = new() { "old" };
        public int DefClass = 3;
        public string DefName = "Doom";
        public List<string> Aliases = new() { "d_e1m1" };
        public List<string> Midi = new() { "timidity" };
        public List<string> Hexen = new() { "winnow" };
        public List<string> Mods = new() { "mod" };
        public List<Save> Saved = new() { new Save { ClassName = "skin" } };
        public bool Locked;
        public bool EntriesNull;
        public List<Entry> Entries = new();
        public List<int> InfoLumps = new();
        public List<int> VoiceLumps = new();
        public int Restores;
        public int Hashes;
        public int Shrinks;
    }

    public static bool Check(State s, Sfx start, Sfx sfx, List<Sfx> chain)
    {
        int siz = chain.Count;
        if ((sfx.User & Reserve) != 0) return true;
        if (chain.Contains(sfx)) return true;
        chain.Add(sfx);
        bool success = true;
        if (sfx.Random)
        {
            if (sfx.ListNull) throw new NullReferenceException();
            foreach (int choice in sfx.Choices)
            {
                var next = Writable(s, choice) ?? throw new NullReferenceException();
                if (ReferenceEquals(next, start))
                {
                    s.Lines.Add(Red + "recursive sound $random found for " + start.Name + ":\n");
                    success = false;
                    for (int i = 1; i < chain.Count; i++)
                        s.Lines.Add(Orange + "    -> " + chain[i].Name + "\n");
                }
                else success &= Check(s, start, next, chain);
            }
        }
        else if (sfx.Link != NoLink)
        {
            var next = Writable(s, sfx.Link) ?? throw new NullReferenceException();
            if (ReferenceEquals(next, start))
            {
                s.Lines.Add(Red + "recursive sound $alias found for " + start.Name + ":\n");
                success = false;
                for (int i = 1; i < chain.Count; i++)
                    s.Lines.Add(Orange + "    -> " + chain[i].Name + "\n");
                chain.RemoveRange(siz, chain.Count - siz);
            }
            else success &= Check(s, start, next, chain);
        }
        if (chain.Count > 0) chain.RemoveAt(chain.Count - 1);
        return success;
    }

    public static void Integrity(State s)
    {
        if (s.EngineNull) throw new NullReferenceException();
        var chain = new List<Sfx>();
        var broken = new bool[s.Sounds.Count];
        for (int i = 0; i < s.Sounds.Count; i++)
        {
            var sfx = s.Sounds[i] ?? throw new NullReferenceException();
            broken[i] = !Check(s, sfx, sfx, chain);
        }
        for (int i = 0; i < s.Sounds.Count; i++)
        {
            if (!broken[i]) continue;
            var sfx = s.Sounds[i] ?? throw new NullReferenceException();
            s.Lines.Add(Red + "Sound " + sfx.Name + " has been disabled\n");
            sfx.Random = false;
            sfx.Link = NoSound;
        }
        s.RumbleReady++;
    }

    public static int Milliseconds(State s, int id)
    {
        var sfx = Writable(s, id);
        if (sfx == null) return 0;
        if (sfx.Link != NoLink)
        {
            if ((sfx.User & Reserve) != 0)
            {
                if (s.SkinAt >= s.SkinReturns.Count) throw new NullReferenceException();
                sfx = Writable(s, s.SkinReturns[s.SkinAt++]) ?? throw new NullReferenceException();
            }
            else if (sfx.Random)
            {
                if (sfx.ListNull) throw new NullReferenceException();
                int length = 0;
                foreach (int choice in sfx.Choices)
                {
                    int one = Milliseconds(s, choice);
                    if (one > length) length = one;
                }
                return length;
            }
            else sfx = Writable(s, sfx.Link) ?? throw new NullReferenceException();
        }
        if (sfx.LoadNull) return 0;
        if (sfx.DataNull) throw new NullReferenceException();
        s.Loads++;
        return sfx.Ms;
    }

    public static double LengthSeconds(State s, int id) => Milliseconds(s, id) / 1000d;

    public static int AddName(State s, string? name, string? lumpName)
    {
        if (lumpName == null) throw new NullReferenceException();
        s.Looks.Add(lumpName);
        int lump = s.FullNames.TryGetValue(lumpName, out int found) ? found : -1;
        return AddLump(s, name, lump, false);
    }

    public static int AddLump(State s, string? name, int lump, bool script)
    {
        if (name == null || s.EngineNull) throw new NullReferenceException();
        if (!s.Ids.TryGetValue(name, out int id)) return Append(s, name, lump);
        var sfx = Writable(s, id) ?? throw new NullReferenceException();
        if ((sfx.User & Reserve) != 0)
        {
            s.Script = script;
            s.Lines.Add(Reserved);
            throw new InvalidOperationException(Reserved);
        }
        if ((sfx.User & Compat) != 0) sfx = Writable(s, sfx.Link) ?? throw new NullReferenceException();
        if (sfx.Random)
        {
            if (sfx.ListNull) throw new NullReferenceException();
            sfx.Choices = Array.Empty<int>();
            sfx.Owner = NoSound;
        }
        sfx.Lump = lump;
        sfx.Random = false;
        sfx.Link = NoLink;
        sfx.Tentative = false;
        if (sfx.Near == -1)
        {
            sfx.Near = 2;
            sfx.Range = 256 * 256;
        }
        return id;
    }

    public static int AddPlayerName(State s, string? pclass, int gender, int refid, string? lumpName)
    {
        int lump = -1;
        if (lumpName != null)
        {
            s.Looks.Add(lumpName);
            if (s.FullNames.TryGetValue(lumpName, out int found)) lump = found;
        }
        return AddPlayer(s, pclass, gender, refid, lump, false);
    }

    public static int AddPlayer(State s, string? pclass, int gender, int refid, int lump, bool fromSkin)
    {
        if (pclass == null || s.EngineNull) throw new NullReferenceException();
        var sfx = Writable(s, refid);
        if (refid == NoSound || sfx == null) return NoSound;
        int playerSound = sfx.Link;
        int id = Append(s, pclass + "\"" + (char)('0' + gender) + "\"" + sfx.Name, lump);
        int list = TakeGender(s, TakeClass(s, pclass), gender);
        if ((uint)list >= (uint)s.PlayerSounds.Count) throw new NullReferenceException();
        s.PlayerSounds[list].Add((playerSound, id));
        if (fromSkin) s.Saves.Add(new Save { ClassName = pclass, Gender = gender, Ref = refid, Lump = lump, Alias = false });
        return id;
    }

    public static int AddExisting(State s, string? pclass, int gender, int refid, int alias, bool fromSkin)
    {
        if (pclass == null) throw new NullReferenceException();
        int list = TakeGender(s, TakeClass(s, pclass), gender);
        var sfx = Writable(s, refid);
        if (refid == NoSound || sfx == null) return NoSound;
        if ((uint)list >= (uint)s.PlayerSounds.Count) throw new NullReferenceException();
        s.PlayerSounds[list].Add((sfx.Link, alias));
        if (fromSkin) s.Saves.Add(new Save { ClassName = pclass, Gender = gender, Ref = refid, Lump = alias, Alias = true });
        return alias;
    }

    public static int Dup(State s, string? pclass, int gender, int refid, int aliasRef)
    {
        if (pclass == null) throw new NullReferenceException();
        if (s.LookupAt >= s.LookupReturns.Count) throw new NullReferenceException();
        int alias = s.LookupReturns[s.LookupAt++];
        s.Lookups.Add((pclass, gender, aliasRef));
        return AddExisting(s, pclass, gender, refid, alias, false);
    }

    public static void Clear(State s)
    {
        if (!s.EngineNull)
        {
            s.EngineClears++;
            s.Sounds.Clear();
            s.Ids.Clear();
        }
        s.Ambients.Clear();
        s.Volumes.Clear();
        s.Reserves = 0;
        s.Sorted = false;
        s.Classes.Clear();
        s.PlayerSounds.Clear();
        s.DefClass = 0;
        s.DefName = "";
        s.Aliases.Clear();
        s.Midi.Clear();
        s.Hexen.Clear();
        s.Mods.Clear();
        s.RumbleResets++;
    }

    public static void Parse(State s, bool redefine)
    {
        if (redefine && s.Locked) return;
        s.Saved.Clear();
        Clear(s);
        s.PitchMask = 0;
        AddName(s, "{ no sound }", "DSEMPTY");
        if (s.EntriesNull) throw new NullReferenceException();
        for (int lump = 0; lump < s.Entries.Count; lump++)
        {
            var entry = s.Entries[lump];
            if (entry.Space == Global && entry.Name == "SNDINFO") s.InfoLumps.Add(lump);
            else if (entry.Space == Voices) s.VoiceLumps.Add(lump);
        }
        s.Restores++;
        if (s.EngineNull) throw new NullReferenceException();
        s.Hashes++;
        s.Shrinks++;
        Integrity(s);
    }

    public static void Lock(State s) => s.Locked = true;

    public static void AddLocal(State s, int lump)
    {
        if (s.Locked)
        {
            s.Lines.Add(LocalLocked);
            return;
        }
        s.InfoLumps.Add(lump);
        if (s.EngineNull) throw new NullReferenceException();
        s.Hashes++;
        s.Shrinks++;
        Integrity(s);
    }

    static Sfx? Writable(State s, int id)
    {
        if (s.EngineNull) throw new NullReferenceException();
        if ((uint)id >= (uint)s.Sounds.Count) return null;
        return s.Sounds[id];
    }

    static int Append(State s, string name, int lump)
    {
        int id = s.Sounds.Count;
        s.Sounds.Add(new Sfx { Name = name, Lump = lump });
        s.Ids[name] = id;
        s.Masks.Add(s.PitchMask);
        return id;
    }

    static int TakeClass(State s, string name)
    {
        if (s.ClassAt >= s.ClassReturns.Count) throw new NullReferenceException();
        s.ClassNames.Add(name);
        return s.ClassReturns[s.ClassAt++];
    }

    static int TakeGender(State s, int classNum, int gender)
    {
        if (s.GenderAt >= s.GenderReturns.Count) throw new NullReferenceException();
        s.Genders.Add((classNum, gender));
        return s.GenderReturns[s.GenderAt++];
    }
}
