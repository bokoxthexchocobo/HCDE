namespace HCDE.Gamedata;

/// <summary>Native <c>SI_*</c> directive names handled by <see cref="SndInfoMapMusicParser"/>.</summary>
public static class VmSndInfoDirectiveIndex
{
    public static IReadOnlyList<string> SoundSettingDirectives =>
    [
        "$alias", "$limit", "$singular", "$pitchshift", "$pitchset", "$pitchshiftrange",
        "$volume", "$attenuation", "$rolloff", "$random"
    ];
}
