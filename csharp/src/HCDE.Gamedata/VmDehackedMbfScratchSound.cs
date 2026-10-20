namespace HCDE.Gamedata;

/// <summary>MBF `CreateScratchFunc` / `CreatePlaySoundFunc` from <c>d_dehacked.cpp</c> (829–848).</summary>
public static class VmDehackedMbfScratchSound
{
    public static (int damage, int hitSoundIndex, int missSound, bool bleed) ScratchMeleeArgs(
        int misc1,
        int misc2,
        Func<int, bool, int> resolveSound) =>
        (misc1, resolveSound(misc2 - 1, true), 0, true);

    public static (int soundIndex, float attenuation) PlaySoundArgs(
        int misc1,
        int misc2,
        Func<int, bool, int> resolveSound) =>
        (resolveSound(misc1 - 1, false), misc2 != 0 ? 0f : 1f);
}
