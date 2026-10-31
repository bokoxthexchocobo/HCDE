namespace HCDE.Gamedata;

/// <summary>
/// Decal group, keyword table, animator declarations, and the library
/// constructor from <c>decallib.cpp</c> 1-222. <c>Clear</c> at 223 stays
/// chunk 197. <c>weightedlist.h</c> is the <c>AddDecal</c> callee and is
/// not a native range of this chunk.
/// </summary>
public sealed class VmDecalLib1371Slice
{
    public const string ChoiceRngName = "DecalChoice";
    public const string DecalRngName = "Decal";
    public const bool ChoiceRngClient = true;
    public const bool DecalRngClient = true;
    public const string OpaqueBloodNote = "Deprecated - use translatable instead!";
    public const int XScale = 0;
    public const int YScale = 1;
    public const int Pic = 2;
    public const int Solid = 3;
    public const int Add = 4;
    public const int Translucent = 5;
    public const int FlipX = 6;
    public const int FlipY = 7;
    public const int RandomFlipX = 8;
    public const int RandomFlipY = 9;
    public const int FullBright = 10;
    public const int Fuzzy = 11;
    public const int Shade = 12;
    public const int Colors = 13;
    public const int AnimatorKeyword = 14;
    public const int LowerDecalKeyword = 15;
    public const int OpaqueBlood = 16;
    public const int Translatable = 17;

    public static readonly string?[] Keywords =
    [
        "x-scale", "y-scale", "pic", "solid", "add", "translucent", "flipx", "flipy",
        "randomflipx", "randomflipy", "fullbright", "fuzzy", "shade", "colors",
        "animator", "lowerdecal", "opaqueblood", "translatable", null,
    ];

    readonly Dictionary<string, string> _names = new(StringComparer.OrdinalIgnoreCase);

    public VmDecalLib1371Slice(Action<Library>? clear = null)
    {
        _names["None"] = "None";
        DecalLibrary = new Library(clear);
    }

    public Library DecalLibrary { get; }
    public List<byte> DecalTranslations { get; } = [];
    public List<Animator> Animators { get; } = [];
    public List<Animator> CombinerAnimatorList { get; } = [];

    public string InternName(string? text)
    {
        if (text is null)
        {
            return "None";
        }

        if (_names.TryGetValue(text, out var found))
        {
            return found;
        }

        _names[text] = text;
        return text;
    }

    public Group CreateGroup()
    {
        return new Group();
    }

    public Library CreateLibrary(Action<Library>? clear)
    {
        return new Library(clear);
    }

    public Fader CreateFader(string? name)
    {
        return Bind(new Fader(), name);
    }

    public Colorer CreateColorer(string? name)
    {
        return Bind(new Colorer(), name);
    }

    public Stretcher CreateStretcher(string? name)
    {
        return Bind(new Stretcher(), name);
    }

    public Slider CreateSlider(string? name)
    {
        return Bind(new Slider(), name);
    }

    public Combiner CreateCombiner(string? name)
    {
        return Bind(new Combiner(), name);
    }

    T Bind<T>(T anim, string? name) where T : Animator
    {
        anim.Name = InternName(name);
        return anim;
    }

    public class Base
    {
        public string Name { get; set; } = "None";

        public virtual Template? GetDecal()
        {
            return null;
        }

        public string GetDecalName()
        {
            return Name;
        }
    }

    public sealed class Group : Base
    {
        Entry? _head;

        public Group()
        {
            RngName = ChoiceRngName;
        }

        public string RngName { get; }

        public override Template? GetDecal()
        {
            throw new InvalidOperationException("GetDecal");
        }

        public void ReplaceDecalRef(Base? from, Base? to)
        {
            for (var choice = _head; choice is not null; choice = choice.Next)
            {
                if (ReferenceEquals(choice.Value, from))
                {
                    choice.Value = to;
                }
            }
        }

        public void AddDecal(Base? decal, ushort weight)
        {
            if (weight == 0)
            {
                return;
            }

            Entry? previous = null;
            var insBefore = _head;
            while (insBefore is not null && insBefore.Weight < weight)
            {
                previous = insBefore;
                insBefore = insBefore.Next;
            }

            var created = new Entry
            {
                Weight = weight,
                Value = decal,
                Next = insBefore,
            };
            if (previous is null)
            {
                _head = created;
            }
            else
            {
                previous.Next = created;
            }

            Recalc();
        }

        public Group Assign(Group source)
        {
            _ = source;
            return this;
        }

        public List<(ushort Weight, byte RandomVal, Base? Value)> Snapshot()
        {
            var rows = new List<(ushort, byte, Base?)>();
            for (var choice = _head; choice is not null; choice = choice.Next)
            {
                rows.Add((choice.Weight, choice.RandomVal, choice.Value));
            }

            return rows;
        }

        void Recalc()
        {
            if (_head is null)
            {
                return;
            }

            var weightSums = 0;
            var choice = _head;
            while (choice.Next is not null)
            {
                weightSums += choice.Weight;
                choice = choice.Next;
            }

            weightSums += choice.Weight;
            choice.RandomVal = 255;
            var randVal = 0.0;
            var weightDenom = 1.0 / weightSums;
            choice = _head;
            while (choice.Next is not null)
            {
                randVal += choice.Weight * weightDenom;
                choice.RandomVal = (byte)(randVal * 255.0);
                choice = choice.Next;
            }
        }

        sealed class Entry
        {
            public Entry? Next;
            public ushort Weight;
            public byte RandomVal;
            public Base? Value;
        }
    }

    public sealed class Template : Base
    {
        public Base? LowerDecal { get; set; }

        public void ReplaceDecalRef(Base? from, Base? to)
        {
            if (ReferenceEquals(LowerDecal, from))
            {
                LowerDecal = to;
            }
        }
    }

    public sealed class Translation
    {
        public uint StartColor;
        public uint EndColor;
        public Translation? Next;
        public int Index;
    }

    public sealed class Library
    {
        readonly Action<Library>? _clear;

        public Library(Action<Library>? clear)
        {
            Root = null;
            Translations = null;
            _clear = clear;
        }

        public Base? Root { get; set; }
        public Translation? Translations { get; set; }

        public void Destroy()
        {
            if (_clear is null)
            {
                throw new InvalidOperationException("Clear");
            }

            _clear(this);
        }
    }

    public class Animator
    {
        public string Name { get; set; } = "None";
    }

    public sealed class Fader : Animator
    {
        public int DecayStart;
        public int DecayTime;
    }

    public sealed class Colorer : Animator
    {
        public int DecayStart;
        public int DecayTime;
        public uint GoalColor;
    }

    public sealed class Stretcher : Animator
    {
        public int StretchStart;
        public int StretchTime;
        public double GoalX;
        public double GoalY;
    }

    public sealed class Slider : Animator
    {
        public int SlideStart;
        public int SlideTime;
        public double DistY;
    }

    public sealed class Combiner : Animator
    {
        public int FirstAnimator;
        public int NumAnimators;
    }
}
