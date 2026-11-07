namespace HCDE.Playsim;

/// <summary>
/// Interpolation class declarations and <c>IMPLEMENT_CLASS</c> registration from
/// <c>r_interpolate.cpp</c> 1-169. Chunk 1397.
/// <c>CountInterpolations</c> starts at line 170 and stays outside this band.
/// </summary>
public static class VmInterpolate1397Slice
{
    public const int BandStart = 1;
    public const int BandEnd = 169;
    public const int CountInterpolationsLine = 170;
    public const int PointerProbeAddress = 8;
    public const ulong PointerSentinel = ulong.MaxValue;
    public const string BaseClass = "DInterpolation";

    public static readonly string[] Headers =
    {
        "p_3dmidtex.h",
        "stats.h",
        "r_data/r_interpolate.h",
        "p_local.h",
        "po_man.h",
        "serializer_doom.h",
        "serialize_obj.h",
        "g_levellocals.h",
    };

    public readonly record struct MethodSpec(string Name, string Kind);

    public sealed class ClassReg
    {
        public object? MyClass { get; set; }
        public string Name { get; init; } = "";
        public string? ParentName { get; init; }
        public bool Abstract { get; init; }
        public bool HasPointers { get; init; }
        public bool DefaultConstructorInBand { get; init; }
        public int ParameterizedLine { get; init; }
        public string[] PointerFields { get; init; } = Array.Empty<string>();
        public string[] Fields { get; init; } = Array.Empty<string>();
        public string[] DefaultInitializedMembers { get; init; } = Array.Empty<string>();
        public MethodSpec[] Methods { get; init; } = Array.Empty<MethodSpec>();
    }

    public static IReadOnlyList<ClassReg> CreateRegistrations()
    {
        return new[]
        {
            new ClassReg
            {
                Name = BaseClass,
                Abstract = true,
                HasPointers = true,
                PointerFields = new[] { "Next", "Prev" },
            },
            Subclass(
                "DSectorPlaneInterpolation",
                379,
                new[] { "sector", "oldheight", "oldtexz", "bakheight", "baktexz", "ceiling", "attached" },
                new[] { "attached" },
                new[]
                {
                    new MethodSpec("UnlinkFromMap", "override"),
                    new MethodSpec("UpdateInterpolation", "plain"),
                    new MethodSpec("Restore", "plain"),
                    new MethodSpec("Interpolate", "plain"),
                    new MethodSpec("Serialize", "virtual"),
                    new MethodSpec("PropagateMark", "plain"),
                }),
            Subclass(
                "DSectorScrollInterpolation",
                547,
                new[] { "sector", "oldx", "oldy", "bakx", "baky", "ceiling" },
                Array.Empty<string>(),
                ScrollMethods()),
            Subclass(
                "DWallScrollInterpolation",
                654,
                new[] { "side", "part", "oldx", "oldy", "bakx", "baky" },
                Array.Empty<string>(),
                ScrollMethods()),
            Subclass(
                "DPolyobjInterpolation",
                753,
                new[] { "poly", "oldverts", "bakverts", "oldcx", "oldcy", "bakcx", "bakcy" },
                new[] { "oldverts", "bakverts" },
                ScrollMethods()),
        };
    }

    public static ClassReg Find(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("class name", nameof(name));
        foreach (var reg in CreateRegistrations())
        {
            if (reg.Name == name)
                return reg;
        }
        throw new InvalidOperationException(name);
    }

    public static ulong FieldOffset(ulong addressAtProbe)
    {
        return addressAtProbe - (ulong)PointerProbeAddress;
    }

    public static ulong[] PointerOffsets(ulong nextAddress, ulong prevAddress)
    {
        return new[]
        {
            FieldOffset(nextAddress),
            FieldOffset(prevAddress),
            PointerSentinel,
        };
    }

    public static ulong[]? PublishedPointerTable(string className, ulong nextAddress, ulong prevAddress)
    {
        var reg = Find(className);
        if (!reg.HasPointers)
            return null;
        if (reg.PointerFields.Length != 2 || reg.PointerFields[0] != "Next" || reg.PointerFields[1] != "Prev")
            throw new InvalidOperationException(className);
        return PointerOffsets(nextAddress, prevAddress);
    }

    public readonly record struct Constructed(string ClassName, string[] InitializedMembers, string[] IndeterminateMembers);

    public static Constructed ConstructInPlace(string className)
    {
        var reg = Find(className);
        if (!CanConstruct(reg))
            throw new InvalidOperationException(className);
        var indeterminate = new List<string>();
        foreach (var field in reg.Fields)
        {
            if (Array.IndexOf(reg.DefaultInitializedMembers, field) < 0)
                indeterminate.Add(field);
        }
        return new Constructed(
            reg.Name,
            reg.DefaultInitializedMembers.ToArray(),
            indeterminate.ToArray());
    }

    public static object? StaticType(ClassReg reg)
    {
        if (reg == null)
            throw new ArgumentNullException(nameof(reg));
        return reg.MyClass;
    }

    public static ClassReg? ParentRegistration(string className)
    {
        var reg = Find(className);
        if (reg.ParentName == null)
            return null;
        return Find(reg.ParentName);
    }

    public static bool CanConstruct(ClassReg reg)
    {
        if (reg == null)
            throw new ArgumentNullException(nameof(reg));
        return !reg.Abstract;
    }

    public static bool Declares(string className, string method)
    {
        if (string.IsNullOrEmpty(method))
            throw new ArgumentException("method", nameof(method));
        foreach (var spec in Find(className).Methods)
        {
            if (spec.Name == method)
                return true;
        }
        return false;
    }

    public static string MethodKind(string className, string method)
    {
        foreach (var spec in Find(className).Methods)
        {
            if (spec.Name == method)
                return spec.Kind;
        }
        throw new InvalidOperationException(method);
    }

    public static bool ParameterizedBodyInBand(string className)
    {
        int line = Find(className).ParameterizedLine;
        if (line <= 0)
            throw new InvalidOperationException(className);
        return line >= BandStart && line <= BandEnd;
    }

    public static bool HeaderSetMatches(IReadOnlyList<string> headers)
    {
        if (headers == null)
            throw new ArgumentNullException(nameof(headers));
        if (headers.Count != Headers.Length)
            return false;
        for (int i = 0; i < Headers.Length; i++)
        {
            if (!string.Equals(headers[i], Headers[i], StringComparison.Ordinal))
                return false;
        }
        return true;
    }

    private static MethodSpec[] ScrollMethods()
    {
        return new[]
        {
            new MethodSpec("UnlinkFromMap", "override"),
            new MethodSpec("UpdateInterpolation", "plain"),
            new MethodSpec("Restore", "plain"),
            new MethodSpec("Interpolate", "plain"),
            new MethodSpec("Serialize", "virtual"),
        };
    }

    private static ClassReg Subclass(string name, int parameterizedLine, string[] fields, string[] defaultInitialized, MethodSpec[] methods)
    {
        foreach (var member in defaultInitialized)
        {
            if (Array.IndexOf(fields, member) < 0)
                throw new InvalidOperationException(member);
        }
        return new ClassReg
        {
            Name = name,
            ParentName = BaseClass,
            Abstract = false,
            HasPointers = false,
            DefaultConstructorInBand = true,
            ParameterizedLine = parameterizedLine,
            Fields = fields,
            DefaultInitializedMembers = defaultInitialized,
            Methods = methods,
        };
    }
}
