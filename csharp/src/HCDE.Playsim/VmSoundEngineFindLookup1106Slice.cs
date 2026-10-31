namespace HCDE.Playsim;

/// <summary>
/// Sound lookup by resid/lump, tentative add, random cache scan, and `GetMSLength` link branch from
/// <c>s_sound.cpp</c> (**1501–1650**). Chunk **1106**.
/// </summary>
public static class VmSoundEngineFindLookup1106Slice
{
    public const int NoSound = -1;
    public const int NoLink = -1;

    public static int ScanFindByResourceId(int resid, IReadOnlyList<int> resourceIds)
    {
        for (var i = 1; i < resourceIds.Count; i++)
        {
            if (resourceIds[i] == resid)
                return i;
        }

        return NoSound;
    }

    public static int ScanFindByLump(int lump, IReadOnlyList<int> lumpNums)
    {
        if (lump == -1)
            return NoSound;

        for (var i = 1; i < lumpNums.Count; i++)
        {
            if (lumpNums[i] == lump)
                return i;
        }

        return NoSound;
    }

    public sealed class AddSoundLumpContext
    {
        public string LogicalName { get; init; } = string.Empty;
        public int Lump { get; init; }
        public int PitchMask { get; init; }
        public int ResourceId { get; init; }
        public int NearLimit { get; init; }
        public Action<AddSoundLumpContext, int>? AppendSfx { get; init; }
        public Func<int>? NextIndex { get; init; }
    }

    public static int RunAddSoundLump(AddSoundLumpContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        var id = ctx.NextIndex?.Invoke() ?? 0;
        ctx.AppendSfx?.Invoke(ctx, id);
        return id;
    }

    public static int ResolveFindSoundTentative(int existingId, Func<int>? createTentative)
    {
        if (existingId != NoSound)
            return existingId;
        return createTentative?.Invoke() ?? NoSound;
    }

    public sealed class CacheRandomSoundContext
    {
        public bool RandomHeader { get; init; }
        public IReadOnlyList<int>? ChoiceIndices { get; init; }
        public Action<int>? MarkUsed { get; init; }
        public Action<int>? CacheChoice { get; init; }
    }

    public static void RunCacheRandomSound(CacheRandomSoundContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ctx.RandomHeader)
            return;

        var choices = ctx.ChoiceIndices;
        if (choices is null)
            return;

        for (var i = 0; i < choices.Count; i++)
        {
            var index = choices[i];
            ctx.MarkUsed?.Invoke(index);
            ctx.CacheChoice?.Invoke(index);
        }
    }

    public sealed class MsLengthContext
    {
        public bool ValidSoundId { get; init; }
        public bool HasLink { get; init; }
        public bool RandomHeader { get; init; }
        public IReadOnlyList<int>? RandomChoices { get; init; }
        public Func<int, int>? ResolveLinkedLength { get; init; }
        public Func<int>? MeasureLoadedLength { get; init; }
        public int LinkedIndex { get; init; }
    }

    public static int ResolveMsLength(MsLengthContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ctx.ValidSoundId)
            return 0;

        if (!ctx.HasLink)
            return ctx.MeasureLoadedLength?.Invoke() ?? 0;

        if (ctx.RandomHeader)
        {
            var length = 0;
            var choices = ctx.RandomChoices;
            if (choices is null)
                return 0;

            for (var i = 0; i < choices.Count; i++)
            {
                var candidate = ctx.ResolveLinkedLength?.Invoke(choices[i]) ?? 0;
                if (candidate > length)
                    length = candidate;
            }

            return length;
        }

        _ = ctx.LinkedIndex;
        return ctx.ResolveLinkedLength?.Invoke(ctx.LinkedIndex) ?? ctx.MeasureLoadedLength?.Invoke() ?? 0;
    }
}
