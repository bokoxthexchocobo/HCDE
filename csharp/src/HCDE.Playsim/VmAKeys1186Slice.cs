namespace HCDE.Playsim;

/// <summary>
/// Lock definition data and key checks from <c>a_keys.cpp</c> (<b>1–163</b>). Chunk <b>1186</b>.
/// <c>AddOneKey</c> starts at line 164.
/// </summary>
public static class VmAKeys1186Slice
{
    public static readonly string?[] LockKeywords =
    {
        "ANY",
        "MESSAGE",
        "REMOTEMESSAGE",
        "MAPCOLOR",
        "LOCKEDSOUND",
        null,
    };

    public static bool KeysDone;
    public static readonly List<string> KeyTypes = new();

    public sealed class PlayerPawn
    {
        public Actor? Mo;
    }

    public sealed class Actor
    {
        public string ClassName = "";
        public string Species = "";
        public string SpeciesName = "";
        public bool DehackedPickup;
        public string Replacee = "";
        public bool IsKey;
        public Actor? Inventory;
        public PlayerPawn? Player;
    }

    public sealed class KeyClass
    {
        public string Name = "";
        public string TypeName = "";
    }

    public sealed class OneKey
    {
        public KeyClass? Key;
        public int Count;

        public bool Check(Actor owner)
        {
            var key = Key!;
            if (owner.ClassName == key.Name || owner.SpeciesName == key.TypeName)
                return true;
            if (owner.DehackedPickup)
            {
                if (owner.Replacee == key.Name || owner.Species == key.TypeName)
                    return true;
            }

            for (var item = owner.Inventory; item != null; item = item.Inventory)
            {
                if (item.ClassName == key.Name)
                    return true;
                if (item.SpeciesName == key.TypeName)
                    return true;
            }

            return false;
        }
    }

    public sealed class KeyGroup
    {
        public List<OneKey> AnyKeyList { get; } = new();

        public bool Check(Actor owner)
        {
            for (var i = 0; i < AnyKeyList.Count; i++)
            {
                if (AnyKeyList[i].Check(owner))
                    return true;
            }

            return false;
        }
    }

    public sealed class Lock
    {
        public List<KeyGroup> KeyList { get; } = new();
        public List<string> LockSounds { get; } = new();
        public string Message = "";
        public string RemoteMessage = "";
        public int Rgb;

        public bool Check(Actor owner)
        {
            if (owner.Player != null && owner.Player.Mo != null && !ReferenceEquals(owner.Player.Mo, owner))
                owner = owner.Player.Mo;
            if (KeyList.Count == 0)
            {
                for (var item = owner.Inventory; item != null; item = item.Inventory)
                {
                    if (item.IsKey)
                        return true;
                }

                return false;
            }

            for (var i = 0; i < KeyList.Count; i++)
            {
                if (!KeyList[i].Check(owner))
                    return false;
            }

            return true;
        }
    }
}
