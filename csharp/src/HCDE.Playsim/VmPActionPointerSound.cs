namespace HCDE.Playsim;

/// <summary>Pointer and legacy sound actions from <c>p_actionfunctions.cpp</c> (531–728).</summary>
public static class VmPActionPointerSound
{
    public const int AaptrTarget = 1;
    public const int AaptrMaster = 2;
    public const int AaptrTracer = 3;
    public const int AaptrNull = 0;
    public const int AaptrDefault = 2;
    public const int PtropUnsafetarget = 1;
    public const int PtropUnsafemaster = 2;
    public const int AttnStatic = -1;
    public const int AttnNorm = 0;
    public const int AttnNone = 1;
    public const int NameAuto = 100;
    public const int NameSoundslot7 = 107;

    public sealed class PointerActor
    {
        public object? Target { get; set; }
        public object? Master { get; set; }
        public object? Tracer { get; set; }
        public bool HasPlayer { get; set; }
    }

    public sealed class PointerContext
    {
        public Func<object?, int, object?>? CopyAaptr { get; init; }
        public Action<object?, int>? VerifyTargetChain { get; init; }
        public Action<object?, int>? VerifyMasterChain { get; init; }
        public Action<object?, object?, bool, bool>? CopyFriendliness { get; init; }
    }

    public static void RearrangePointers(
        PointerContext ctx,
        PointerActor self,
        int ptrTarget,
        int ptrMaster,
        int ptrTracer,
        int flags)
    {
        var getTarget = self.Target;
        var getMaster = self.Master;
        var getTracer = self.Tracer;

        switch (ptrTarget)
        {
            case AaptrMaster:
                self.Target = getMaster;
                if ((flags & PtropUnsafetarget) == 0)
                    ctx.VerifyTargetChain?.Invoke(self, flags);
                break;
            case AaptrTracer:
                self.Target = getTracer;
                if ((flags & PtropUnsafetarget) == 0)
                    ctx.VerifyTargetChain?.Invoke(self, flags);
                break;
            case AaptrNull:
                self.Target = null;
                break;
        }

        switch (ptrMaster)
        {
            case AaptrTarget:
                self.Master = getTarget;
                if ((flags & PtropUnsafemaster) == 0)
                    ctx.VerifyMasterChain?.Invoke(self, flags);
                break;
            case AaptrTracer:
                self.Master = getTracer;
                if ((flags & PtropUnsafemaster) == 0)
                    ctx.VerifyMasterChain?.Invoke(self, flags);
                break;
            case AaptrNull:
                self.Master = null;
                break;
        }

        switch (ptrTracer)
        {
            case AaptrTarget:
                self.Tracer = getTarget;
                break;
            case AaptrMaster:
                self.Tracer = getMaster;
                break;
            case AaptrNull:
                self.Tracer = null;
                break;
        }
    }

    public static void TransferPointer(
        PointerContext ctx,
        PointerActor self,
        int ptrSource,
        int ptrRecipient,
        int ptrSourceField,
        int ptrRecipientField,
        int flags)
    {
        var sourceActor = ctx.CopyAaptr?.Invoke(self, ptrSource);
        var recipient = ctx.CopyAaptr?.Invoke(self, ptrRecipient);
        if (recipient == null)
            return;

        var source = ctx.CopyAaptr?.Invoke(sourceActor, ptrSourceField);
        if (ReferenceEquals(source, recipient))
            source = null;

        if (ptrRecipientField == AaptrDefault)
            ptrRecipientField = ptrSourceField;

        AssignPointer(ctx, recipient, ptrRecipientField, source, flags);
    }

    private static void AssignPointer(PointerContext ctx, object recipient, int field, object? value, int flags)
    {
        if (recipient is not PointerActor actor)
            return;
        switch (field)
        {
            case AaptrTarget:
                actor.Target = value;
                if ((flags & PtropUnsafetarget) == 0)
                    ctx.VerifyTargetChain?.Invoke(recipient, flags);
                break;
            case AaptrMaster:
                actor.Master = value;
                if ((flags & PtropUnsafemaster) == 0)
                    ctx.VerifyMasterChain?.Invoke(recipient, flags);
                break;
            case AaptrTracer:
                actor.Tracer = value;
                break;
        }
    }

    public static void CopyFriendlinessFromPointer(PointerContext ctx, PointerActor self, int ptrSource)
    {
        if (self.HasPlayer)
            return;
        var source = ctx.CopyAaptr?.Invoke(self, ptrSource);
        if (source != null)
            ctx.CopyFriendliness?.Invoke(self, source, false, false);
    }

    public sealed class SoundContext
    {
        public Action<object?, int, int, string, bool, float>? PlaySound { get; init; }
        public Func<object?, int, string, bool>? IsPlaying { get; init; }
        public Action<object?, int>? StopSound { get; init; }
    }

    public static float AttenuationFromRaw(int raw) =>
        raw switch
        {
            AttnStatic => 3f,
            1 or 2 => 0f,
            _ => 1f,
        };

    public static int NormalizeChannel(int channel) =>
        channel < NameAuto || channel > NameSoundslot7 ? NameAuto : channel;

    public static void PlaySoundEx(
        SoundContext ctx,
        object actor,
        string soundId,
        int channel,
        bool looping,
        int attenuationRaw)
    {
        var attenuation = AttenuationFromRaw(attenuationRaw);
        channel = NormalizeChannel(channel);
        var slot = channel - NameAuto;
        if (!looping)
        {
            ctx.PlaySound?.Invoke(actor, slot, 0, soundId, true, attenuation);
            return;
        }

        if (ctx.IsPlaying?.Invoke(actor, slot, soundId) != true)
            ctx.PlaySound?.Invoke(actor, slot, 1, soundId, true, attenuation);
    }

    public static void StopSoundEx(SoundContext ctx, object actor, int channel)
    {
        channel = NormalizeChannel(channel);
        if (channel > NameAuto && channel <= NameSoundslot7)
            ctx.StopSound?.Invoke(actor, channel - NameAuto);
    }
}
