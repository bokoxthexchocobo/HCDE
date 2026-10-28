namespace HCDE.Gamedata;

/// <summary>Shadow handlers and MBF21 flag opcodes from <c>d_dehacked.cpp</c> (4213–4393).</summary>
public static class VmDehackedShadowMbf21
{
    public const ulong MfShadow = 0x00040000UL;
    public const uint RfZdoomtrans = 0x10000000U;

    public static (ulong Flags, int RenderStyle, uint RenderFlags) SetShadow(ulong flags, int renderStyle, uint renderFlags) =>
        (flags | MfShadow, renderStyle, renderFlags & ~RfZdoomtrans);

    public static (ulong Flags, int RenderStyle, uint RenderFlags) ClearShadow(
        ulong flags,
        float alpha,
        int legacyNormalStyle,
        int legacyTranslucentStyle,
        uint renderFlags) =>
        (
            flags & ~MfShadow,
            alpha >= 1f - float.Epsilon ? legacyNormalStyle : legacyTranslucentStyle,
            renderFlags & ~RfZdoomtrans);

    public static bool Mbf21JumpIfFlagsSet(
        int standardMask,
        int mbf21Mask,
        IReadOnlyList<Func<bool>?> standardCheckers,
        IReadOnlyList<Func<bool>?> mbf21Checkers)
    {
        for (var i = 0; i < 32; i++)
        {
            if ((standardMask & (1 << i)) != 0)
            {
                var check = i < standardCheckers.Count ? standardCheckers[i] : null;
                if (check is null || !check())
                    return false;
            }

            if ((mbf21Mask & (1 << i)) != 0)
            {
                var check = i < mbf21Checkers.Count ? mbf21Checkers[i] : null;
                if (check is null || !check())
                    return false;
            }
        }

        return true;
    }

    public static void Mbf21AddFlags(
        int standardMask,
        int mbf21Mask,
        IReadOnlyList<Action?> standardSetters,
        IReadOnlyList<Action?> mbf21Setters)
    {
        for (var i = 0; i < 32; i++)
        {
            if ((standardMask & (1 << i)) != 0 && i < standardSetters.Count)
                standardSetters[i]?.Invoke();
            if ((mbf21Mask & (1 << i)) != 0 && i < mbf21Setters.Count)
                mbf21Setters[i]?.Invoke();
        }
    }

    public static void Mbf21RemoveFlags(
        int standardMask,
        int mbf21Mask,
        IReadOnlyList<Action?> standardClearers,
        IReadOnlyList<Action?> mbf21Clearers)
    {
        for (var i = 0; i < 32; i++)
        {
            if ((standardMask & (1 << i)) != 0 && i < standardClearers.Count)
                standardClearers[i]?.Invoke();
            if ((mbf21Mask & (1 << i)) != 0 && i < mbf21Clearers.Count)
                mbf21Clearers[i]?.Invoke();
        }
    }
}
