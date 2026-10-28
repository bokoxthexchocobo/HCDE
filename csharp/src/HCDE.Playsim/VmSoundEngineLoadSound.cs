namespace HCDE.Playsim;

/// <summary><c>SoundEngine::LoadSound</c> from <c>s_sound.cpp</c> (718–792).</summary>
public static class VmSoundEngineLoadSound
{
    public sealed class LoadSoundContext
    {
        public int Lumpnum { get; set; }
        public int SfxEmpty { get; set; }
        public bool DataValid { get; set; }
        public bool LoadRaw { get; set; }
        public int RawRate { get; set; }
        public float RolloffMinDistance { get; set; }
        public float DefaultRolloffMin { get; set; }
        public byte[] LumpBytes { get; set; } = Array.Empty<byte>();
        public IReadOnlyList<(bool Valid, int Link, int Lump, int RawRate)> Catalog { get; set; }
            = Array.Empty<(bool, int, int, int)>();
        public int LinkIndex { get; set; } = -1;

        public Func<bool>? DeviceIsNull { get; init; }
        public Func<int, byte[]>? ReadSound { get; init; }
        public Func<byte[], bool>? LoadVoc { get; set; }
        public Func<byte[], int, int, int, int, int, bool>? LoadRawSamples { get; init; }
        public Func<byte[], int, int, int, int, bool>? LoadDmxRaw { get; init; }
        public Func<byte[], int, int, int, bool>? LoadGeneric { get; init; }
        public Action<string, string>? PrintfDecodeFailure { get; init; }
        public Func<int, string>? LumpShortName { get; init; }
        public string SfxName { get; set; } = string.Empty;
    }

    public static int LoadSound(LoadSoundContext ctx)
    {
        if (ctx.DeviceIsNull?.Invoke() == true)
            return ctx.Lumpnum;

        while (!ctx.DataValid)
        {
            if (ctx.Lumpnum == ctx.SfxEmpty)
                return ctx.Lumpnum;

            for (var i = 0; i < ctx.Catalog.Count; i++)
            {
                var entry = ctx.Catalog[i];
                if (!entry.Valid || entry.Link >= 0 || entry.Lump != ctx.Lumpnum)
                    continue;
                if (ctx.LoadRaw && entry.RawRate != ctx.RawRate)
                    continue;

                ctx.LinkIndex = i;
                if (ctx.RolloffMinDistance == 0)
                    ctx.RolloffMinDistance = ctx.DefaultRolloffMin;
                return entry.Lump;
            }

            ctx.LumpBytes = ctx.ReadSound?.Invoke(ctx.Lumpnum) ?? ctx.LumpBytes;
            var size = ctx.LumpBytes.Length;
            if (size > 8)
            {
                var dmxLen = LittleLongDmxLength(ctx.LumpBytes);
                if (IsVocHeader(ctx.LumpBytes))
                {
                    if (ctx.LoadVoc?.Invoke(ctx.LumpBytes) != true)
                        ctx.DataValid = false;
                }
                else if (ctx.LoadRaw)
                {
                    ctx.LoadRawSamples?.Invoke(ctx.LumpBytes, size, ctx.RawRate, 1, 8, 0);
                }
                else if (IsDmxHeader(ctx.LumpBytes, dmxLen, size))
                {
                    var frequency = DmxFrequency(
                        (ushort)(ctx.LumpBytes.Length >= 4 ? (ctx.LumpBytes[2] << 8) | ctx.LumpBytes[3] : 0));
                    ctx.LoadDmxRaw?.Invoke(ctx.LumpBytes, dmxLen, frequency, 1, 8);
                }
                else
                {
                    ctx.LoadGeneric?.Invoke(ctx.LumpBytes, size, 0, 0);
                }
            }

            if (!ctx.DataValid && ctx.Lumpnum != ctx.SfxEmpty)
            {
                ctx.PrintfDecodeFailure?.Invoke(
                    ctx.SfxName,
                    ctx.LumpShortName?.Invoke(ctx.Lumpnum) ?? string.Empty);
                ctx.Lumpnum = ctx.SfxEmpty;
                continue;
            }

            break;
        }

        return ctx.Lumpnum;
    }

    public static bool IsVocHeader(ReadOnlySpan<byte> data) =>
        data.Length > 19 && data.StartsWith("Creative Voice File"u8);

    public static bool IsDmxHeader(ReadOnlySpan<byte> data, int dmxLen, int size) =>
        data.Length > 1 && data[0] == 3 && data[1] == 0 && dmxLen <= size - 8;

    public static int DmxFrequency(ushort rawFrequency) => rawFrequency == 0 ? 11025 : rawFrequency;

    public static int LittleLongDmxLength(ReadOnlySpan<byte> data) =>
        data.Length >= 8 ? data[4] | (data[5] << 8) | (data[6] << 16) | (data[7] << 24) : 0;
}
