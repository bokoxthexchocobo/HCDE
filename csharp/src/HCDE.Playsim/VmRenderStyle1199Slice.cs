namespace HCDE.Playsim;

/// <summary>
/// Legacy render styles, alpha, visibility, and fuzz from <c>renderstyle.cpp</c> (<b>1–150</b>).
/// Chunk <b>1199</b>. <c>r_drawtrans</c> and <c>r_drawfuzz</c> are the two cvars in this file.
/// </summary>
public static class VmRenderStyle1199Slice
{
    public const int Archive = 1;
    public const int StyleCount = 19;

    public const int OpNone = 0;
    public const int OpAdd = 1;
    public const int OpSub = 2;
    public const int OpRevSub = 3;
    public const int OpFuzz = 4;
    public const int OpFuzzOrAdd = 5;
    public const int OpFuzzOrSub = 6;
    public const int OpFuzzOrRevSub = 7;
    public const int OpShadow = 8;

    public const int AlphaZero = 0;
    public const int AlphaOne = 1;
    public const int AlphaSrc = 2;
    public const int AlphaInvSrc = 3;
    public const int AlphaSrcCol = 4;
    public const int AlphaInvSrcCol = 5;
    public const int AlphaDstCol = 6;
    public const int AlphaInvDstCol = 7;

    public const int FlagTransSouls = 1;
    public const int FlagAlpha1 = 2;
    public const int FlagRedIsAlpha = 4;
    public const int FlagColorIsFixed = 8;

    public static bool DrawTrans = true;
    public static int DrawTransFlags;
    public static int DrawFuzz = 1;
    public static int DrawFuzzFlags = Archive;

    public struct Style
    {
        public byte BlendOp, SrcAlpha, DestAlpha, Flags;

        public Style(byte op, byte src, byte dest, byte flags)
        {
            BlendOp = op;
            SrcAlpha = src;
            DestAlpha = dest;
            Flags = flags;
        }
    }

    public static readonly Style[] Legacy =
    {
        new(OpNone, AlphaZero, AlphaZero, 0),
        new(OpAdd, AlphaSrc, AlphaInvSrc, FlagAlpha1),
        new(OpFuzz, AlphaSrc, AlphaInvSrc, 0),
        new(OpAdd, AlphaSrc, AlphaInvSrc, FlagTransSouls),
        new(OpFuzzOrAdd, AlphaSrc, AlphaInvSrc, 0),
        new(OpAdd, AlphaSrc, AlphaInvSrc, (byte)(FlagAlpha1 | FlagColorIsFixed)),
        new(OpAdd, AlphaSrc, AlphaInvSrc, 0),
        new(OpAdd, AlphaSrc, AlphaOne, 0),
        new(OpAdd, AlphaSrc, AlphaInvSrc, (byte)(FlagRedIsAlpha | FlagColorIsFixed)),
        new(OpAdd, AlphaSrc, AlphaInvSrc, FlagColorIsFixed),
        new(OpShadow, 0, 0, 0),
        new(OpRevSub, AlphaSrc, AlphaOne, 0),
        new(OpAdd, AlphaSrc, AlphaOne, FlagColorIsFixed),
        new(OpAdd, AlphaSrc, AlphaOne, (byte)(FlagRedIsAlpha | FlagColorIsFixed)),
        new(OpAdd, AlphaDstCol, AlphaZero, 0),
        new(OpAdd, AlphaInvDstCol, AlphaZero, 0),
        new(OpAdd, AlphaSrcCol, AlphaInvSrcCol, 0),
        new(OpAdd, AlphaOne, AlphaZero, 0),
        new(OpAdd, AlphaSrcCol, AlphaOne, 0),
    };

    public static void Reset()
    {
        DrawTrans = true;
        DrawTransFlags = 0;
        DrawFuzz = 1;
        DrawFuzzFlags = Archive;
    }

    public static double GetAlpha(int type, double alpha)
    {
        switch (type)
        {
            case AlphaZero: return 0;
            case AlphaOne: return 1;
            case AlphaSrc: return alpha;
            case AlphaInvSrc: return 1 - alpha;
            default: return 0.5;
        }
    }

    public static bool IsVisible(Style style, double alpha)
    {
        if (style.BlendOp == OpNone)
            return false;
        if (style.BlendOp == OpAdd || style.BlendOp == OpRevSub)
        {
            if ((style.Flags & FlagAlpha1) != 0)
                alpha = 1;
            else
                alpha = Math.Clamp(alpha, 0, 1);
            return GetAlpha(style.SrcAlpha, alpha) != 0 || GetAlpha(style.DestAlpha, alpha) != 1;
        }

        return true;
    }

    public static Style CheckFuzz(Style style)
    {
        switch (style.BlendOp)
        {
            default:
                return style;
            case OpFuzzOrAdd:
                if (DrawTrans && DrawFuzz == 0)
                {
                    style.BlendOp = OpAdd;
                    return style;
                }

                break;
            case OpFuzzOrSub:
                if (DrawTrans && DrawFuzz == 0)
                {
                    style.BlendOp = OpSub;
                    return style;
                }

                break;
            case OpFuzzOrRevSub:
                if (DrawTrans && DrawFuzz == 0)
                {
                    style.BlendOp = OpRevSub;
                    return style;
                }

                break;
        }

        style.BlendOp = DrawFuzz == 2 ? (byte)OpShadow : (byte)OpFuzz;
        return style;
    }
}
