namespace HCDE.Playsim;

/// <summary>
/// Deprecated actor flags, string match, pointer check, and the early actor
/// info properties from <c>thingdef_properties.cpp</c> (249-616). Chunk 1329.
/// <c>tag</c> at line 617 is already credited. Class lookup, baggage reset,
/// and the default-actor tail stay callees.
/// </summary>
public sealed class VmDeprecatedFlags1329
{
    public const int DepfFireDamage = 1;
    public const int DepfIceDamage = 2;
    public const int DepfLowGravity = 3;
    public const int DepfLongMeleeRange = 4;
    public const int DepfShortMissileRange = 5;
    public const int DepfPickupFlash = 6;
    public const int DepfQuarterGravity = 7;
    public const int DepfFireResist = 8;
    public const int DepfHereticBounce = 9;
    public const int DepfHexenBounce = 10;
    public const int DepfDoomBounce = 11;
    public const int DepfInterHubStrip = 12;
    public const int DepfHigherMProb = 13;
    public const int DepfMissileMore = 14;
    public const int DepfMissileEvenMore = 15;
    public const int DepfRangeHalf = 16;
    public const int DepfFullVolSounds = 17;

    public const int BounceWalls = 1 << 0;
    public const int BounceFloors = 1 << 1;
    public const int BounceCeilings = 1 << 2;
    public const int BounceActors = 1 << 3;
    public const int BounceAutoOff = 1 << 5;
    public const int BounceHereticType = 1 << 6;
    public const int BounceUseSeeSound = 1 << 7;
    public const int BounceQuiet = 1 << 9;
    public const int BounceMbf = 1 << 12;
    public const int BounceTypeMask = BounceWalls | BounceFloors | BounceCeilings | BounceActors | BounceAutoOff | BounceHereticType | BounceMbf;
    public const int BounceClear = BounceTypeMask | BounceUseSeeSound;
    public const int BounceHeretic = BounceFloors | BounceCeilings | BounceHereticType;
    public const int BounceDoom = BounceWalls | BounceFloors | BounceCeilings | BounceActors | BounceAutoOff;
    public const int BounceHexen = BounceWalls | BounceFloors | BounceCeilings | BounceActors;
    public const int BounceHereticCompat = BounceHeretic | BounceUseSeeSound;
    public const int BounceDoomCompat = BounceDoom | BounceUseSeeSound;
    public const int BounceHexenCompat = BounceHexen | BounceUseSeeSound;

    public const int Mf3FullVolActive = 0x00001000;
    public const int Mf3FullVolDeath = 0x00400000;
    public const int Mf8FullVolSee = 0x00010000;
    public const int MsgOptError = 3;

    public const int GameAny = 0;
    public const int GameDoom = 1;
    public const int GameHeretic = 2;
    public const int GameHexen = 4;
    public const int GameStrife = 8;
    public const int GameChex = 16;
    public const int GameRaven = GameHeretic | GameHexen;

    public readonly List<string> Errors = new();
    public readonly List<Message> Messages = new();
    public int BaggageResets;
    public Func<string, string, string?>? FindTentative;
    public Func<string, string?>? FindClass;

    public readonly record struct Message(int Level, string Text);

    public sealed class DamageFactor
    {
        public string Name = "";
        public double Value;
    }

    public sealed class ClassPointer
    {
        public string Name = "";
        public readonly HashSet<string> Ancestors = new(StringComparer.Ordinal);

        public bool IsDescendantOf(ClassPointer other)
        {
            if (string.Equals(Name, other.Name, StringComparison.Ordinal))
                return true;
            return Ancestors.Contains(other.Name);
        }
    }

    public sealed class ActorInfo
    {
        public int GameFilter;
        public int SpawnId = -1;
        public int ConversationId = -1;
        public bool SkipSuperSet;
        public int DefaultStateUsage;
        public string TypeName = "";
        public int Size;
        public int ActorSize;
        public bool StateSet;
    }

    public sealed class Actor
    {
        public string DamageType = "";
        public double Gravity = 1;
        public double MaxTargetRange;
        public double MeleeThreshold;
        public double MissileChanceMult = 1;
        public int BounceFlags;
        public string? PickupFlash;
        public int InterHubAmount = -1;
        public int MinMissileChance = 200;
        public int Flags3;
        public int Flags8;
        public readonly List<DamageFactor> DamageFactors = new();
        public byte[] Tail = Array.Empty<byte>();
        public byte[] DefaultTail = Array.Empty<byte>();
        public readonly List<int> ConversationExtras = new();
        public ActorInfo Info = new();
    }

    public int CallHandle(Actor actor, int set, int index)
    {
        HandleDeprecatedFlags(actor, set, index);
        return 0;
    }

    public int CallCheck(Actor actor, int index)
    {
        return CheckDeprecatedFlags(actor, index);
    }

    public void HandleDeprecatedFlags(Actor actor, int set, int index)
    {
        bool on = set != 0;
        switch (index)
        {
            case DepfFireDamage:
                actor.DamageType = on ? "Fire" : "None";
                break;
            case DepfIceDamage:
                actor.DamageType = on ? "Ice" : "None";
                break;
            case DepfLowGravity:
                actor.Gravity = on ? 1.0 / 8.0 : 1.0;
                break;
            case DepfShortMissileRange:
                actor.MaxTargetRange = on ? 896.0 : 0.0;
                break;
            case DepfLongMeleeRange:
                actor.MeleeThreshold = on ? 196.0 : 0.0;
                break;
            case DepfQuarterGravity:
                actor.Gravity = on ? 1.0 / 4.0 : 1.0;
                break;
            case DepfFireResist:
                SetDamageFactor(actor, "Fire", on ? 0.5 : 1.0);
                break;
            case DepfHereticBounce:
                SetBounce(actor, on, BounceHereticCompat);
                break;
            case DepfHexenBounce:
                SetBounce(actor, on, BounceHexenCompat);
                break;
            case DepfDoomBounce:
                SetBounce(actor, on, BounceDoomCompat);
                break;
            case DepfPickupFlash:
                if (on)
                    actor.PickupFlash = FindTentative == null ? null : FindTentative("PickupFlash", "Actor");
                else
                    actor.PickupFlash = null;
                break;
            case DepfInterHubStrip:
                actor.InterHubAmount = on ? 0 : 1;
                break;
            case DepfHigherMProb:
                actor.MinMissileChance = on ? 160 : 200;
                break;
            case DepfMissileMore:
                if (on)
                    actor.MissileChanceMult = actor.MissileChanceMult == 0.125 ? 0.0625 : 0.5;
                else
                    actor.MissileChanceMult = actor.MissileChanceMult == 0.0625 ? 0.125 : 1;
                break;
            case DepfMissileEvenMore:
                if (on)
                    actor.MissileChanceMult = actor.MissileChanceMult == 0.5 ? 0.0625 : 0.125;
                else
                    actor.MissileChanceMult = actor.MissileChanceMult == 0.0625 ? 0.5 : 1;
                break;
            case DepfRangeHalf:
                if (on)
                {
                    actor.MissileChanceMult = 0.5;
                    actor.MeleeThreshold = 196;
                }
                else
                {
                    actor.MissileChanceMult = 1;
                    actor.MeleeThreshold = 0;
                }
                break;
            case DepfFullVolSounds:
                if (on)
                {
                    actor.Flags8 |= Mf8FullVolSee;
                    actor.Flags3 |= Mf3FullVolDeath;
                    actor.Flags3 |= Mf3FullVolActive;
                }
                else
                {
                    actor.Flags8 &= ~Mf8FullVolSee;
                    actor.Flags3 &= ~Mf3FullVolDeath;
                    actor.Flags3 &= ~Mf3FullVolActive;
                }
                break;
            default:
                break;
        }
    }

    public int CheckDeprecatedFlags(Actor actor, int index)
    {
        switch (index)
        {
            case DepfFireDamage:
                return SameName(actor.DamageType, "Fire") ? 1 : 0;
            case DepfIceDamage:
                return SameName(actor.DamageType, "Ice") ? 1 : 0;
            case DepfLowGravity:
                return actor.Gravity == 1.0 / 8.0 ? 1 : 0;
            case DepfShortMissileRange:
                return actor.MaxTargetRange == 896.0 ? 1 : 0;
            case DepfLongMeleeRange:
                return actor.MeleeThreshold == 196.0 ? 1 : 0;
            case DepfQuarterGravity:
                return actor.Gravity == 1.0 / 4.0 ? 1 : 0;
            case DepfFireResist:
                return FireResist(actor);
            case DepfHereticBounce:
                return BounceIs(actor, BounceHereticCompat);
            case DepfHexenBounce:
                return BounceIs(actor, BounceHexenCompat);
            case DepfDoomBounce:
                return BounceIs(actor, BounceDoomCompat);
            case DepfPickupFlash:
                string? found = FindClass == null ? null : FindClass("PickupFlash");
                return actor.PickupFlash == found ? 1 : 0;
            case DepfInterHubStrip:
                return actor.InterHubAmount == 0 ? 1 : 0;
            case DepfHigherMProb:
                return actor.MinMissileChance <= 160 ? 1 : 0;
            case DepfMissileMore:
                return actor.MissileChanceMult == 0.5 || actor.MissileChanceMult == 0.0625 ? 1 : 0;
            case DepfMissileEvenMore:
                return actor.MissileChanceMult == 0.125 || actor.MissileChanceMult == 0.0625 ? 1 : 0;
            case DepfRangeHalf:
                return actor.MissileChanceMult == 0.5 && actor.MeleeThreshold == 196 ? 1 : 0;
            case DepfFullVolSounds:
                bool see = (actor.Flags8 & Mf8FullVolSee) != 0;
                bool death = (actor.Flags3 & Mf3FullVolDeath) != 0;
                return see && death ? 1 : 0;
            default:
                return 0;
        }
    }

    public static int MatchString(string text, IReadOnlyList<string?> strings)
    {
        for (int i = 0; i < strings.Count; i++)
        {
            string? item = strings[i];
            if (item == null)
                return -1;
            if (StrIcmp(text, item) == 0)
                return i;
        }
        return -1;
    }

    public static bool PointerCheck(ClassPointer? sym, ClassPointer? check)
    {
        if (sym == null || check == null)
            return false;
        return sym.IsDescendantOf(check);
    }

    public void ApplyGame(ActorInfo info, string name)
    {
        if (StrIcmp(name, "Doom") == 0)
            info.GameFilter |= GameDoom;
        else if (StrIcmp(name, "Heretic") == 0)
            info.GameFilter |= GameHeretic;
        else if (StrIcmp(name, "Hexen") == 0)
            info.GameFilter |= GameHexen;
        else if (StrIcmp(name, "Raven") == 0)
            info.GameFilter |= GameRaven;
        else if (StrIcmp(name, "Strife") == 0)
            info.GameFilter |= GameStrife;
        else if (StrIcmp(name, "Chex") == 0)
            info.GameFilter |= GameChex;
        else if (StrIcmp(name, "Any") == 0)
            info.GameFilter = GameAny;
        else
        {
            string message = "Unknown game type " + name;
            Errors.Add(message);
            throw new InvalidOperationException(message);
        }
    }

    public void ApplySpawnId(ActorInfo info, int id)
    {
        if (id < 0 || id > 65535)
        {
            const string message = "SpawnID must be in the range [0,65535]";
            Errors.Add(message);
            throw new InvalidOperationException(message);
        }
        info.SpawnId = id;
    }

    public void ApplyConversationId(Actor actor, int convid, int id1, int id2)
    {
        actor.ConversationExtras.Add(id1);
        actor.ConversationExtras.Add(id2);
        if (convid <= 0 || convid > 65535)
            return;
        actor.Info.ConversationId = convid;
    }

    public void ApplySkipSuper(Actor actor)
    {
        ActorInfo info = actor.Info;
        if (info.Size != info.ActorSize)
        {
            Messages.Add(new Message(MsgOptError, "'skip_super' is only allowed in subclasses of Actor with no additional fields and will be ignored in type " + info.TypeName + "."));
            return;
        }
        if (info.StateSet)
        {
            Messages.Add(new Message(MsgOptError, "'skip_super' must appear before any state definitions."));
            return;
        }
        actor.Tail = actor.DefaultTail.ToArray();
        BaggageResets++;
        info.SkipSuperSet = true;
    }

    public static void ApplyDefaultStateUsage(ActorInfo info, int use)
    {
        info.DefaultStateUsage = use;
    }

    static void SetBounce(Actor actor, bool on, int compat)
    {
        actor.BounceFlags &= ~BounceClear;
        if (on)
            actor.BounceFlags |= compat;
    }

    static int BounceIs(Actor actor, int compat)
    {
        return (actor.BounceFlags & BounceClear) == compat ? 1 : 0;
    }

    static void SetDamageFactor(Actor actor, string name, double value)
    {
        for (int i = 0; i < actor.DamageFactors.Count; i++)
        {
            if (SameName(actor.DamageFactors[i].Name, name))
            {
                actor.DamageFactors[i].Value = value;
                return;
            }
        }
        actor.DamageFactors.Add(new DamageFactor { Name = name, Value = value });
    }

    static int FireResist(Actor actor)
    {
        for (int i = 0; i < actor.DamageFactors.Count; i++)
        {
            if (SameName(actor.DamageFactors[i].Name, "Fire"))
                return actor.DamageFactors[i].Value == 0.5 ? 1 : 0;
        }
        return 0;
    }

    static bool SameName(string left, string right)
    {
        return StrIcmp(left, right) == 0;
    }

    static int StrIcmp(string left, string right)
    {
        int n = left.Length < right.Length ? left.Length : right.Length;
        for (int i = 0; i < n; i++)
        {
            int a = Fold(left[i]);
            int b = Fold(right[i]);
            if (a != b)
                return a - b;
        }
        return left.Length - right.Length;
    }

    static int Fold(char c)
    {
        if (c >= 'A' && c <= 'Z')
            return c + 32;
        return c;
    }
}
