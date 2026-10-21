namespace HCDE.Playsim;

/// <summary><c>SoundEngine::LoadSound</c> policy from <c>s_sound.cpp</c> (718–792).</summary>
public static class VmSoundEngineLoadSound
{
    public const int SfxEmpty = -1;

    public static bool AudioDeviceNull(bool isNull) => isNull;

    public static bool SfxAlreadyValid(bool valid) => valid;

    public static bool ShouldReturnEmptyLump(int lumpnum, int sfxEmpty) => lumpnum == sfxEmpty;

    public static bool CanShareLumpBuffer(
        bool otherValid,
        int otherLink,
        int otherLump,
        int lumpnum,
        bool loadRaw,
        int rawRate,
        int otherRawRate) =>
        otherValid && otherLink < 0 && otherLump == lumpnum
        && (!loadRaw || rawRate == otherRawRate);

    public static bool ShouldApplyDefaultRolloff(float minDistance) => minDistance == 0;

    public static bool IsVocHeader(ReadOnlySpan<byte> data) =>
        data.Length > 19 && data.StartsWith("Creative Voice File"u8);

    public static bool IsDmxHeader(ReadOnlySpan<byte> data, int dmxLen, int size) =>
        data.Length > 1 && data[0] == 3 && data[1] == 0 && dmxLen <= size - 8;

    public static int DmxFrequency(ushort rawFrequency) => rawFrequency == 0 ? 11025 : rawFrequency;

    public static bool ShouldFallbackToEmpty(int lumpnum, int sfxEmpty, bool valid) =>
        !valid && lumpnum != sfxEmpty;
}
