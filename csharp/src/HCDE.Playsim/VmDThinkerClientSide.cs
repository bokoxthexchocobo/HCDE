namespace HCDE.Playsim;

/// <summary>Client-side thinker tick and dynamic-light refresh (<c>dthinker.cpp</c> 210–307).</summary>
public static class VmDThinkerClientSide
{
    public const int Mf8RecreateLights = 0x00000100;
    public const int Level3LightCreated = 0x00080000;

    public const int StatFirstThinking = ThinkerStat.FirstThinking;
    public const int MaxStatNum = ThinkerStat.MaxStatNum;

    public sealed class ActorModel
    {
        public int Flags8 { get; set; }
        public int Effects { get; set; }
        public int FountainColor { get; set; }
        public Func<bool>? ShouldRenderLocally { get; set; }
        public Action? SetDynamicLights { get; set; }
        public Action<int>? RunEffect { get; set; }
    }

    public sealed class DynamicLightModel
    {
        public DynamicLightModel? Next { get; set; }
        public Action? Tick { get; set; }
    }

    public sealed class LevelModel
    {
        public int Flags3 { get; set; }
        public int LocalWorldTimer { get; set; }
        public int LocalTimer { get; set; }
        public DynamicLightModel? Lights { get; set; }
        public Func<bool>? IsFrozen { get; set; }
        public Func<IEnumerable<ActorModel>>? ActorThinkerIterator { get; set; }
        public Func<IEnumerable<ActorModel>>? ClientSideActorIterator { get; set; }
    }

    public sealed class RunClientSideContext
    {
        public LevelModel Level { get; init; } = null!;
        public VmDThinkerCollectionEarly.ThinkerListOps[] Thinkers { get; init; } =
            Array.Empty<VmDThinkerCollectionEarly.ThinkerListOps>();

        public VmDThinkerCollectionEarly.ThinkerListOps[] FreshThinkers { get; init; } =
            Array.Empty<VmDThinkerCollectionEarly.ThinkerListOps>();
        public bool GlLights { get; init; }
        public bool DynLights { get; init; }
        public int VidRenderMode { get; init; }
        public Func<bool>? WorldPaused { get; init; }
    }

    public static void RecreateDynamicLights(ActorModel mobj, bool doLights, bool frozen)
    {
        if ((mobj.Flags8 & Mf8RecreateLights) != 0)
        {
            mobj.Flags8 &= ~Mf8RecreateLights;
            if (doLights)
                mobj.SetDynamicLights?.Invoke();
        }

        if ((mobj.Effects != 0 || mobj.FountainColor != 0)
            && mobj.ShouldRenderLocally?.Invoke() == true
            && !frozen)
        {
            mobj.RunEffect?.Invoke(mobj.Effects);
        }
    }

    public static bool ComputeDoLights(RunClientSideContext ctx) =>
        (ctx.GlLights && ctx.VidRenderMode == 4) || (ctx.DynLights && ctx.VidRenderMode != 4);

    public static void RunClientSideThinkers(RunClientSideContext ctx)
    {
        var doLights = ComputeDoLights(ctx);
        var paused = ctx.WorldPaused?.Invoke() ?? false;

        ctx.Level.Flags3 &= ~Level3LightCreated;
        ctx.Level.LocalWorldTimer += paused ? 0 : 1;
        ctx.Level.LocalTimer++;

        if (!paused)
        {
            for (var i = StatFirstThinking; i <= MaxStatNum; i++)
                ctx.Thinkers[i].TickThinkers?.Invoke(null);

            int count;
            do
            {
                count = 0;
                for (var i = StatFirstThinking; i <= MaxStatNum; i++)
                    count += ctx.FreshThinkers[i].TickThinkers?.Invoke(ctx.Thinkers[i]) ?? 0;
            } while (count != 0);
        }

        var frozen = ctx.Level.IsFrozen?.Invoke() ?? false;
        foreach (var ac in ctx.Level.ActorThinkerIterator?.Invoke() ?? [])
            RecreateDynamicLights(ac, doLights, frozen);
        foreach (var ac in ctx.Level.ClientSideActorIterator?.Invoke() ?? [])
            RecreateDynamicLights(ac, doLights, frozen);

        if (doLights && !paused)
        {
            for (var light = ctx.Level.Lights; light != null;)
            {
                var next = light.Next;
                light.Tick?.Invoke();
                light = next;
            }
        }
    }
}
