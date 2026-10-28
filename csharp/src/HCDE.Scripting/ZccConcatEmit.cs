namespace HCDE.Scripting;

/// <summary><c>FxConcat::Emit</c> cast and string-load policy from <c>codegen.cpp</c> (4847–4926).</summary>
public static class ZccConcatEmit
{
    public const int RegtString = 1;
    public const int RegtInt = 2;
    public const int RegtFloat = 3;
    public const int RegtPointer = 4;

    public const int CastI2s = 1;
    public const int CastU2s = 3;
    public const int CastF2s = 6;
    public const int CastP2s = 7;
    public const int CastN2s = 9;
    public const int CastCo2s = 10;
    public const int CastSo2s = 11;
    public const int CastV22s = 12;
    public const int CastV32s = 13;
    public const int CastV42s = 14;
    public const int CastSid2s = 15;
    public const int CastTid2s = 16;

    public static bool UseStringKonstReload(int regType, bool isKonst) =>
        regType == RegtString && isKonst;

    public static bool ReuseStringRegister(int regType) =>
        regType == RegtString;

    public static int SelectCastOpcode(
        int regType,
        int regCount,
        bool isUInt32ValueType,
        bool isNameValueType,
        bool isSoundValueType,
        bool isColorValueType,
        bool isSpriteIdValueType,
        bool isTextureIdValueType,
        bool isTranslationIdValueType)
    {
        if (regType == RegtFloat)
        {
            return regCount switch
            {
                1 => CastF2s,
                2 => CastV22s,
                3 => CastV32s,
                _ => CastV42s,
            };
        }

        if (isUInt32ValueType)
            return CastU2s;
        if (isNameValueType)
            return CastN2s;
        if (isSoundValueType)
            return CastSo2s;
        if (isColorValueType)
            return CastCo2s;
        if (isSpriteIdValueType)
            return CastSid2s;
        if (isTextureIdValueType)
            return CastTid2s;
        if (isTranslationIdValueType)
            return CastU2s;
        if (regType == RegtPointer)
            return CastP2s;
        if (regType == RegtInt)
            return CastI2s;
        return -1;
    }
}
