namespace HCDE.Playsim;

/// <summary><c>SoundEngine::RestartChannel</c> from <c>s_sound.cpp</c> (649–708).</summary>
public static class VmSoundEngineRestartChannel
{
    public sealed class RestartChannelContext
    {
        public int ChanFlags { get; set; }
        public int SoundId { get; set; }
        public bool SfxSingular { get; set; }
        public int Lumpnum { get; set; }
        public int SfxEmpty { get; set; }
        public int NearLimit { get; set; }
        public float DistanceScale { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public int EntChannel { get; set; }
        public int Priority { get; set; }

        public Func<int, bool>? CheckSingular { get; init; }
        public Func<int, int>? LoadSound { get; init; }
        public Func<bool>? ValidatePosVel { get; init; }
        public Func<bool>? CheckSoundLimit { get; init; }
        public Func<int, object?>? StartSound3D { get; init; }
        public Func<int, object?>? StartSound2D { get; init; }
    }

    public const int ChanfEvicted = 1;
    public const int ChanfLoop = 2;
    public const int ChanfArea = 4;
    public const int ChanfUi = 8;
    public const int ChanfNopause = 16;
    public const int ChanfAbstime = 32;
    public const int ChanfIs3D = 64;

    public static bool RestartChannel(RestartChannelContext ctx)
    {
        if ((ctx.ChanFlags & ChanfEvicted) == 0)
            return false;

        if (ctx.SfxSingular && ctx.CheckSingular?.Invoke(ctx.SoundId) == true)
            return false;

        ctx.Lumpnum = ctx.LoadSound?.Invoke(ctx.SoundId) ?? ctx.Lumpnum;
        if (ctx.Lumpnum == ctx.SfxEmpty)
            return false;

        var oldFlags = ctx.ChanFlags;
        var startflags = 0;
        if ((ctx.ChanFlags & ChanfLoop) != 0) startflags |= 1;
        if ((ctx.ChanFlags & ChanfArea) != 0) startflags |= 2;
        if ((ctx.ChanFlags & (ChanfUi | ChanfNopause)) != 0) startflags |= 4;
        if ((ctx.ChanFlags & ChanfAbstime) != 0) startflags |= 8;

        if ((ctx.ChanFlags & ChanfIs3D) != 0)
        {
            if (ctx.ValidatePosVel?.Invoke() != true)
                return false;

            if (ctx.NearLimit > 0 && ctx.CheckSoundLimit?.Invoke() == true)
                return false;

            ctx.ChanFlags &= ~(ChanfEvicted | ChanfAbstime);
            var ochan = ctx.StartSound3D?.Invoke(startflags);
            if (ochan is null)
                ctx.ChanFlags = oldFlags;
            return true;
        }

        ctx.ChanFlags &= ~(ChanfEvicted | ChanfAbstime);
        var ochan2D = ctx.StartSound2D?.Invoke(startflags);
        if (ochan2D is null)
            ctx.ChanFlags = oldFlags;
        return true;
    }
}
