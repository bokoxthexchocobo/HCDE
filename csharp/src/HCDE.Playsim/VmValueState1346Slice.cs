using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Debugger value formatting from <c>ValueStateNode.cpp</c> (1-289). Chunk 1346.
/// The basic-type names are the table in that file. printf shapes and the
/// state-label owner walk are what those branches print.
/// </summary>
public static class VmValueState1346Slice
{
    public const int BasicNone = 0;
    public const int BasicUint32 = 1;
    public const int BasicInt32 = 2;
    public const int BasicUint16 = 3;
    public const int BasicInt16 = 4;
    public const int BasicUint8 = 5;
    public const int BasicInt8 = 6;
    public const int BasicFloat = 7;
    public const int BasicDouble = 8;
    public const int BasicBool = 9;
    public const int BasicString = 10;
    public const int BasicName = 11;
    public const int BasicSprite = 12;
    public const int BasicTexture = 13;
    public const int BasicTranslation = 14;
    public const int BasicSound = 15;
    public const int BasicColor = 16;
    public const int BasicEnum = 17;
    public const int BasicState = 18;
    public const int BasicPointer = 19;
    public const int BasicVoid = 20;

    public static readonly string[] BasicNames =
    {
        "NONE", "uint32", "int32", "uint16", "int16", "uint8", "int8", "float", "double", "bool", "string",
        "name", "SpriteID", "TextureID", "TranslationID", "Sound", "Color", "Enum", "StateLabel", "pointer", "VoidPointer",
    };

    public static readonly Type1346 UInt32 = new("uint32") { Int = true };
    public static readonly Type1346 SInt32 = new("int32") { Int = true };
    public static readonly Type1346 UInt16 = new("uint16") { Int = true };
    public static readonly Type1346 SInt16 = new("int16") { Int = true };
    public static readonly Type1346 UInt8 = new("uint8") { Int = true };
    public static readonly Type1346 SInt8 = new("int8") { Int = true };
    public static readonly Type1346 Float32 = new("float32") { Float = true };
    public static readonly Type1346 Float64 = new("float64") { Float = true };
    public static readonly Type1346 Bool = new("bool") { IntNotInt = true };
    public static readonly Type1346 String = new("string");
    public static readonly Type1346 Name = new("name") { IntNotInt = true };
    public static readonly Type1346 Sprite = new("sprite") { IntNotInt = true };
    public static readonly Type1346 Texture = new("texture") { IntNotInt = true };
    public static readonly Type1346 Translation = new("translation") { IntNotInt = true };
    public static readonly Type1346 Sound = new("sound") { IntNotInt = true };
    public static readonly Type1346 Color = new("color") { IntNotInt = true };
    public static readonly Type1346 StateLabel = new("statelabel") { IntNotInt = true };
    public static readonly Type1346 VoidPtr = new("voidptr") { Pointer = true };

    public static int BasicOf(Type1346 type)
    {
        if (ReferenceEquals(type, UInt32)) return BasicUint32;
        if (ReferenceEquals(type, SInt32)) return BasicInt32;
        if (ReferenceEquals(type, UInt16)) return BasicUint16;
        if (ReferenceEquals(type, SInt16)) return BasicInt16;
        if (ReferenceEquals(type, UInt8)) return BasicUint8;
        if (ReferenceEquals(type, SInt8)) return BasicInt8;
        if (ReferenceEquals(type, Float32)) return BasicFloat;
        if (ReferenceEquals(type, Float64)) return BasicDouble;
        if (ReferenceEquals(type, Bool)) return BasicBool;
        if (ReferenceEquals(type, String)) return BasicString;
        if (ReferenceEquals(type, Name)) return BasicName;
        if (ReferenceEquals(type, Sprite)) return BasicSprite;
        if (ReferenceEquals(type, Texture)) return BasicTexture;
        if (ReferenceEquals(type, Translation)) return BasicTranslation;
        if (ReferenceEquals(type, Sound)) return BasicSound;
        if (ReferenceEquals(type, Color)) return BasicColor;
        if (ReferenceEquals(type, StateLabel)) return BasicState;
        if (ReferenceEquals(type, VoidPtr)) return BasicVoid;
        if (type.Pointer) return BasicPointer;
        if (type.IntNotInt)
        {
            if (type.Descriptive.StartsWith("Enum", StringComparison.Ordinal)) return BasicEnum;
            if (type.Size == 4) return BasicUint32;
            if (type.Size == 2) return BasicUint16;
            if (type.Size == 1) return BasicUint8;
        }

        return BasicNone;
    }

    public static bool Valid(VmBits1346 bits)
    {
        return bits.Raw != 0 && (bits.Raw & 0xFFFFFFFF) != 0;
    }

    public static long TruncateInt(int value, int basic)
    {
        if (basic == BasicUint16) return (ushort)value;
        if (basic == BasicInt16) return (short)value;
        if (basic == BasicUint8) return (byte)value;
        if (basic == BasicInt8) return (sbyte)value;
        return value;
    }

    public static double TruncateFloat(double value, int basic)
    {
        if (basic == BasicFloat) return (double)(float)value;
        return value;
    }

    public static string FormatP(ulong address)
    {
        if (address == 0) return "(nil)";
        return "0x" + address.ToString("x", CultureInfo.InvariantCulture);
    }

    public static string FormatF(double value)
    {
        if (double.IsNaN(value))
            return BitConverter.DoubleToInt64Bits(value) < 0 ? "-nan" : "nan";
        if (double.IsPositiveInfinity(value)) return "inf";
        if (double.IsNegativeInfinity(value)) return "-inf";
        var negative = BitConverter.DoubleToInt64Bits(value) < 0;
        var abs = Math.Abs(value);
        if (abs == 0) return negative ? "-0.000000" : "0.000000";
        var scaled = (decimal)abs * 1000000m;
        var rounded = decimal.Round(scaled, 0, MidpointRounding.AwayFromZero);
        var whole = decimal.Truncate(rounded / 1000000m);
        var frac = rounded % 1000000m;
        var text = whole.ToString(CultureInfo.InvariantCulture) + "." + ((long)frac).ToString("D6", CultureInfo.InvariantCulture);
        return negative ? "-" + text : text;
    }

    public static string FormatSigned(long value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public static string FormatUnsigned(long value)
    {
        return unchecked((ulong)value).ToString(CultureInfo.InvariantCulture);
    }

    public static string FormatHex8(int value)
    {
        return unchecked((uint)value).ToString("X8", CultureInfo.InvariantCulture);
    }

    public static int TranslationType(int value)
    {
        return (int)(((uint)value & 0x00FF0000u) >> 16);
    }

    public static int TranslationIndex(int value)
    {
        return (int)((uint)value & 0xFFFFu);
    }

    public static string ToLowerAscii(string text)
    {
        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] is >= 'A' and <= 'Z') chars[i] = (char)(chars[i] + 32);
        }

        return new string(chars);
    }
}

public sealed class Type1346
{
    public string Id = "";
    public bool Pointer;
    public bool ClassPointer;
    public bool FunctionPointer;
    public bool Int;
    public bool Float;
    public bool IntNotInt;
    public bool Scalar;
    public int Size = 4;
    public string Descriptive = "";
    public Type1346? Pointed;

    public Type1346(string id)
    {
        Id = id;
    }
}

public sealed class VmBits1346
{
    public ulong Raw;
    public double Float;
    public int I32 => unchecked((int)(uint)Raw);

    public static VmBits1346 FromInt(int value)
    {
        return new VmBits1346 { Raw = (uint)value };
    }
}

public sealed class DapVar1346
{
    public string Name = "";
    public string? EvaluateName;
    public string Type = "";
    public string Value = "";
}

public sealed class FuncInfo1346
{
    public string? OwningClass;
    public string Symbol = "";
    public bool HasVariant;
    public string? Implementation;
}

public sealed class StateHit1346
{
    public bool Found;
    public string? OwnerName;
    public bool Owns;
    public int Index;
    public int LabelCount;
    public string? Label;
}

public sealed class VmValueHost1346
{
    public Func<ulong, string?>? StringChars;
    public Func<int, string?>? NameChars;
    public Func<int, string?>? TextureName;
    public int NumSounds;
    public Func<int, string>? SoundName;
    public Func<ulong, int, VmBits1346>? Deref;
    public Func<ulong, FuncInfo1346?>? FunctionAt;
    public Func<int, string?, StateHit1346>? StateAt;
    public int StringReads;
    public int DerefReads;
    public int DescriptiveReads;
}

public sealed class VmValueStateNode1346
{
    public string Name = "";
    public string EvalName = "";
    public VmBits1346 Bits = new();
    public Type1346? Type;
    public string? OwningClass;
    public VmValueHost1346 Host = new();

    public VmValueStateNode1346(string name, VmBits1346 bits, Type1346 type, string? owningClass = null, string? evalName = null)
    {
        Name = name;
        EvalName = string.IsNullOrEmpty(evalName) ? name : evalName;
        Bits = bits;
        Type = type;
        OwningClass = owningClass;
    }

    public bool Serialize(DapVar1346 variable)
    {
        var formatted = ToVariable(Bits, Type, OwningClass, Host);
        variable.Type = formatted.Type;
        variable.Value = formatted.Value;
        variable.Name = Name;
        if (EvalName != Name) variable.EvaluateName = EvalName;
        return true;
    }

    public static DapVar1346 ToVariable(VmBits1346 bits, Type1346? type, string? owningClass, VmValueHost1346 host)
    {
        if (type == null) throw new NullReferenceException("type");
        var variable = new DapVar1346();
        var basic = VmValueState1346Slice.BasicOf(type);
        variable.Type = VmValueState1346Slice.BasicNames[basic];
        if (ReferenceEquals(type, VmValueState1346Slice.String))
        {
            variable.Type = "string";
            if (VmValueState1346Slice.Valid(bits))
            {
                if (host.StringChars == null) throw new InvalidOperationException("isFStringValid");
                host.StringReads++;
                var chars = host.StringChars(bits.Raw);
                variable.Value = chars == null ? "\"\"" : "\"" + chars + "\"";
            }
            else
            {
                variable.Value = "<EMPTY>";
            }
        }
        else if (type.Pointer)
        {
            if (type.ClassPointer)
            {
                variable.Type = "ClassPointer";
                var pointed = type.Pointed ?? throw new NullReferenceException("pointed");
                host.DescriptiveReads++;
                variable.Value = pointed.Descriptive;
            }
            else if (type.FunctionPointer)
            {
                host.DescriptiveReads++;
                variable.Type = type.Descriptive;
                if (VmValueState1346Slice.Valid(bits))
                {
                    if (host.FunctionAt == null) throw new InvalidOperationException("PFunction");
                    var func = host.FunctionAt(bits.Raw) ?? throw new NullReferenceException("function");
                    if (func.OwningClass != null)
                        variable.Value += func.OwningClass + "." + func.Symbol + " (" + VmValueState1346Slice.FormatP(bits.Raw) + ")";
                    else if (func.HasVariant && func.Implementation != null)
                        variable.Value += func.Implementation + " (" + VmValueState1346Slice.FormatP(bits.Raw) + ")";
                }
                else
                {
                    variable.Value = "<NULL>";
                }
            }
            else
            {
                var pointed = type.Pointed ?? throw new NullReferenceException("pointed");
                host.DescriptiveReads++;
                variable.Type = "Pointer<" + pointed.Descriptive + ">";
                if (!VmValueState1346Slice.Valid(bits))
                {
                    variable.Value = "<NULL>";
                }
                else if (pointed.Scalar && !pointed.Pointer)
                {
                    if (host.Deref == null) throw new InvalidOperationException("DerefValue");
                    host.DerefReads++;
                    var deref = host.Deref(bits.Raw, VmValueState1346Slice.BasicOf(pointed));
                    var inner = ToVariable(deref, pointed, owningClass, host);
                    variable.Value = VmValueState1346Slice.FormatP(bits.Raw) + " {" + inner.Value + "}";
                }
                else
                {
                    variable.Value = VmValueState1346Slice.FormatP(bits.Raw);
                }
            }
        }
        else if (type.Int)
        {
            var truncated = VmValueState1346Slice.TruncateInt(bits.I32, basic);
            if (basic == VmValueState1346Slice.BasicUint32 || basic == VmValueState1346Slice.BasicUint16 || basic == VmValueState1346Slice.BasicUint8)
                variable.Value = VmValueState1346Slice.FormatUnsigned(truncated);
            else
                variable.Value = VmValueState1346Slice.FormatSigned(truncated);
        }
        else if (type.Float)
        {
            variable.Value = VmValueState1346Slice.FormatF(VmValueState1346Slice.TruncateFloat(bits.Float, basic));
        }
        else if (type.IntNotInt)
        {
            if (ReferenceEquals(type, VmValueState1346Slice.Bool))
            {
                variable.Type = "bool";
                variable.Value = bits.I32 != 0 ? "true" : "false";
            }
            else if (ReferenceEquals(type, VmValueState1346Slice.Name))
            {
                variable.Type = "Name";
                if (host.NameChars == null) throw new InvalidOperationException("FName");
                variable.Value = "Name# " + bits.I32.ToString(CultureInfo.InvariantCulture) + ": '" + (host.NameChars(bits.I32) ?? "") + "'";
            }
            else if (ReferenceEquals(type, VmValueState1346Slice.Sprite))
            {
                variable.Type = "SpriteID";
                variable.Value = "SpriteID# " + bits.I32.ToString(CultureInfo.InvariantCulture);
            }
            else if (ReferenceEquals(type, VmValueState1346Slice.Texture))
            {
                variable.Type = "TextureID";
                if (host.TextureName == null) throw new InvalidOperationException("TexMan");
                var tex = host.TextureName(bits.I32) ?? "<INVALID>";
                variable.Value = "TextureID# " + bits.I32.ToString(CultureInfo.InvariantCulture) + " (" + tex + ")";
            }
            else if (ReferenceEquals(type, VmValueState1346Slice.Translation))
            {
                variable.Type = "TranslationID";
                variable.Value = "TranslationID# " + bits.I32.ToString(CultureInfo.InvariantCulture)
                    + " {type: " + VmValueState1346Slice.TranslationType(bits.I32).ToString(CultureInfo.InvariantCulture)
                    + ", index: " + VmValueState1346Slice.TranslationIndex(bits.I32).ToString(CultureInfo.InvariantCulture) + "}";
            }
            else if (ReferenceEquals(type, VmValueState1346Slice.Sound))
            {
                variable.Type = "Sound";
                var id = bits.I32;
                string sound;
                if (id > 0 && (uint)id < (uint)host.NumSounds)
                {
                    if (host.SoundName == null) throw new InvalidOperationException("soundEngine");
                    sound = host.SoundName(id);
                }
                else
                {
                    sound = "<INVALID>";
                }

                variable.Value = "Sound# " + id.ToString(CultureInfo.InvariantCulture) + " (" + sound + ")";
            }
            else if (ReferenceEquals(type, VmValueState1346Slice.Color))
            {
                variable.Type = "Color";
                var packed = bits.I32;
                var a = (packed >> 24) & 0xFF;
                var r = (packed >> 16) & 0xFF;
                var g = (packed >> 8) & 0xFF;
                var b = packed & 0xFF;
                variable.Value = "Color #" + VmValueState1346Slice.FormatHex8(packed)
                    + " (a: " + a.ToString(CultureInfo.InvariantCulture)
                    + ", r: " + r.ToString(CultureInfo.InvariantCulture)
                    + ", g: " + g.ToString(CultureInfo.InvariantCulture)
                    + ", b: " + b.ToString(CultureInfo.InvariantCulture) + ")";
            }
            else if (ReferenceEquals(type, VmValueState1346Slice.StateLabel))
            {
                variable.Type = "StateLabel";
                variable.Value = "StateLabel# " + bits.I32.ToString(CultureInfo.InvariantCulture) + ": '" + StateLabel(bits, owningClass, host) + "'";
            }
            else
            {
                host.DescriptiveReads++;
                var descriptive = type.Descriptive;
                var folded = VmValueState1346Slice.ToLowerAscii(descriptive);
                if (folded.IndexOf("enum", StringComparison.Ordinal) == 0)
                    variable.Value = bits.I32.ToString(CultureInfo.InvariantCulture);
                else
                    variable.Value = bits.I32.ToString(CultureInfo.InvariantCulture);
                variable.Type = "Enum";
                variable.Value = bits.I32.ToString(CultureInfo.InvariantCulture);
            }
        }
        else
        {
            host.DescriptiveReads++;
            variable.Type = type.Descriptive;
            variable.Value = "<ERROR?>";
        }

        return variable;
    }

    static string StateLabel(VmBits1346 bits, string? owningClass, VmValueHost1346 host)
    {
        string? stateName = null;
        var nameVal = bits.I32;
        if (nameVal > 0)
        {
            if (nameVal > 0x10000000) nameVal -= 0x10000000;
            if (host.NameChars == null) throw new InvalidOperationException("FName");
            stateName = host.NameChars(nameVal) ?? "";
        }

        if (host.StateAt == null) throw new InvalidOperationException("GetStateFromIdx");
        var hit = host.StateAt(bits.I32, owningClass);
        if (hit.Found && hit.OwnerName != null && nameVal < 0 && hit.Owns && hit.Index < hit.LabelCount)
            stateName = hit.Label ?? "";
        if (stateName != null)
            return (hit.OwnerName ?? "<unknownOwner>") + "." + stateName;
        return "<unknown_state>";
    }
}
