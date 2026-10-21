namespace HCDE.Playsim;

/// <summary><c>SoundEngine::StartSound</c> from <c>s_sound.cpp</c> (398–639).</summary>
public static class VmSoundEngineStartSound
{
    public static object? StartSound(StartSoundContext ctx)
    {
        if (ctx.IsValidSoundId?.Invoke(ctx.SoundId) != true || ctx.Volume <= 0 || ctx.NoSfx
            || ctx.SoundEnabled?.Invoke() != true || ctx.BlockNewSounds)
        {
            return null;
        }

        if (ctx.Type == StartSoundContext.SourceUnattached && !ctx.HasPoint)
            ctx.Type = StartSoundContext.SourceNone;

        ctx.OrgSoundId = ctx.SoundId;

        if (ctx.ValidatePosVel?.Invoke() != true)
            return null;

        ctx.Volume = Math.Min(ctx.Volume * ctx.SfxVolume, 1f);
        if (ctx.Volume <= 0)
            return null;

        for (var i = 0; i < ctx.SfxLinkFrames.Count; i++)
        {
            var frame = ctx.SfxLinkFrames[i];
            if (!frame.ValidSoundId)
                return null;
            if (!frame.SfxChanged)
                return null;
            if (ctx.NearLimit < 0)
            {
                ctx.NearLimit = frame.NearLimit;
                ctx.LimitRange = frame.LimitRange;
                ctx.DefPitch = frame.DefPitch;
                ctx.DefPitchMax = frame.DefPitchMax;
            }

            if (ctx.RolloffMinDistance == 0)
                ctx.RolloffMinDistance = frame.RolloffMinDistance;
        }

        ctx.Attenuation *= ctx.SfxAttenuation;

        if (ctx.ForcedRolloffPresent && ctx.ForcedRolloffMin != 0)
            ctx.RolloffMinDistance = ctx.ForcedRolloffMin;
        else if (ctx.RolloffMinDistance == 0)
            ctx.RolloffMinDistance = ctx.GlobalRolloffMin;

        if ((ctx.SfxSingular || (ctx.ChanFlags & 0x20) != 0) && ctx.CheckSingular?.Invoke() == true)
            ctx.ChanFlags |= StartSoundContext.ChanfEvicted;

        if (ctx.Type == StartSoundContext.SourceNone || ctx.SourceIsListener)
            ctx.NearLimit = 0;

        if (ctx.NearLimit > 0 && ctx.CheckSoundLimit?.Invoke() == true)
            ctx.ChanFlags |= StartSoundContext.ChanfEvicted;

        if ((ctx.ChanFlags & (StartSoundContext.ChanfEvicted | StartSoundContext.ChanfLoop))
            == StartSoundContext.ChanfEvicted)
        {
            return null;
        }

        ctx.LoadedLump = ctx.LoadSoundLump?.Invoke(ctx.SoundId) ?? ctx.LoadedLump;
        if (ctx.LoadedLump == ctx.SfxEmptyLump)
            return null;

        var basePriority = ctx.Type == StartSoundContext.SourceNone || ctx.SourceIsListener ? 80 : 0;

        if (ctx.Source is not null && ctx.Channel == StartSoundContext.ChanAuto)
            ctx.ChanFlags |= StartSoundContext.ChanfOverlap;

        if ((ctx.ChanFlags & StartSoundContext.ChanfOverlap) == 0
            && ctx.Type != StartSoundContext.SourceNone
            && ctx.IsChannelUsed?.Invoke() == true)
        {
            ctx.StopMatchingChannel?.Invoke(ctx.Channel);
        }

        if ((ctx.ChanFlags & StartSoundContext.ChanfLoop) == 0
            && (ctx.ChanFlags & 0x300) == 0
            && ctx.SoundPaused)
        {
            return null;
        }

        var pitch = ctx.Spitch > 0 ? ctx.Spitch : ctx.CalcPitch?.Invoke(0, ctx.DefPitch, ctx.DefPitchMax) ?? 1f;
        object? chan;
        if ((ctx.ChanFlags & StartSoundContext.ChanfEvicted) != 0)
        {
            chan = null;
        }
        else
        {
            var startflags = 0;
            if ((ctx.ChanFlags & StartSoundContext.ChanfLoop) != 0) startflags |= 1;
            if ((ctx.ChanFlags & 0x4) != 0) startflags |= 2;
            if ((ctx.ChanFlags & 0x18) != 0) startflags |= 4;
            if ((ctx.ChanFlags & 0x8) != 0) startflags |= 8;

            var sfxLength = (ctx.GetMsLength?.Invoke(ctx.LoadedLump) ?? 0f) / 1000f;
            ctx.StartTime = (startflags & 1) != 0
                ? sfxLength > 0 ? ctx.StartTime % sfxLength : 0f
                : Math.Clamp(ctx.StartTime, 0f, sfxLength);

            chan = ctx.Attenuation > 0 && ctx.Type != StartSoundContext.SourceNone
                ? ctx.StartSound3D?.Invoke()
                : ctx.StartSound2D?.Invoke(startflags);
        }

        if (chan is null && (ctx.ChanFlags & StartSoundContext.ChanfLoop) != 0)
        {
            chan = ctx.GetChannel?.Invoke(null);
            ctx.MarkStartTime?.Invoke(chan);
            ctx.ChanFlags |= StartSoundContext.ChanfEvicted;
        }

        if (ctx.Attenuation > 0 && ctx.Type != StartSoundContext.SourceNone)
            ctx.ChanFlags |= 0x30;
        else
            ctx.ChanFlags |= 0x60;

        if ((ctx.ChanFlags & 0x80) != 0 && (ctx.ChanFlags & 0x100) == 0)
            ctx.JoyRumble?.Invoke(ctx.GetSoundName?.Invoke(ctx.OrgSoundId) ?? string.Empty);

        if (chan is not null)
        {
            ctx.ResultChannelId = chan.GetHashCode();
        }

        return chan;
    }
}
