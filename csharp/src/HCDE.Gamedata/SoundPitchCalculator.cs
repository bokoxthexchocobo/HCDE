namespace HCDE.Gamedata;

/// <summary>Calculates represented sound pitch using externally supplied random draws.</summary>
public static class SoundPitchCalculator
{
    public static float Calculate(SndInfoSoundSettings settings, bool pitched,
        Func<byte>? nextByte = null, Func<float>? nextFloat = null)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (!float.IsFinite(settings.DefPitch) || !float.IsFinite(settings.DefPitchMax))
            throw new ArgumentException("Sound pitch settings must be finite.", nameof(settings));
        if (settings.PitchMask is < 0 or > 127)
            throw new ArgumentException("Sound pitch mask must be between zero and 127.", nameof(settings));
        if (settings.DefPitch > 0)
        {
            if (settings.DefPitchMax > 0 && settings.DefPitch != settings.DefPitchMax)
            {
                if (nextFloat is null) throw new ArgumentException("Ranged pitch requires a float random provider.", nameof(nextFloat));
                var draw = nextFloat();
                if (!float.IsFinite(draw) || draw < 0 || draw >= 1)
                    throw new ArgumentOutOfRangeException(nameof(nextFloat), "Pitch random draw must be in [0, 1).");
                return draw * (settings.DefPitchMax - settings.DefPitch) + settings.DefPitch;
            }
            return settings.DefPitch;
        }
        if (settings.PitchMask == 0 || !pitched) return 1;
        if (nextByte is null) throw new ArgumentException("Shifted pitch requires a byte random provider.", nameof(nextByte));
        var lower = nextByte() & settings.PitchMask;
        var upper = nextByte() & settings.PitchMask;
        return (128 - lower + upper) / 128f;
    }
}
