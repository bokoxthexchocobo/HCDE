namespace HCDE.Playsim.Tests;

public class VmDThinkerClientSideTests
{
    private static VmDThinkerCollectionEarly.ThinkerListOps[] EmptyThinkerOps(int count)
    {
        var ops = new VmDThinkerCollectionEarly.ThinkerListOps[count];
        for (var i = 0; i < count; i++)
            ops[i] = new VmDThinkerCollectionEarly.ThinkerListOps();
        return ops;
    }

    [Fact]
    public void RecreateDynamicLights_ClearsFlagAndRunsEffect()
    {
        var actor = new VmDThinkerClientSide.ActorModel
        {
            Flags8 = VmDThinkerClientSide.Mf8RecreateLights,
            Effects = 3,
            ShouldRenderLocally = () => true,
            SetDynamicLights = () => { },
            RunEffect = effect => Assert.Equal(3, effect),
        };

        VmDThinkerClientSide.RecreateDynamicLights(actor, doLights: true, frozen: false);
        Assert.Equal(0, actor.Flags8 & VmDThinkerClientSide.Mf8RecreateLights);
    }

    [Fact]
    public void RunClientSideThinkers_TicksFreshUntilStableAndAdvancesTimers()
    {
        var thinkers = EmptyThinkerOps(VmDThinkerClientSide.MaxStatNum + 1);
        var fresh = EmptyThinkerOps(VmDThinkerClientSide.MaxStatNum + 1);
        var freshPasses = 0;
        fresh[ThinkerStat.Default].TickThinkers = _ =>
        {
            freshPasses++;
            return freshPasses < 2 ? 1 : 0;
        };

        var level = new VmDThinkerClientSide.LevelModel
        {
            Flags3 = VmDThinkerClientSide.Level3LightCreated,
            LocalWorldTimer = 5,
            LocalTimer = 9,
        };

        VmDThinkerClientSide.RunClientSideThinkers(new VmDThinkerClientSide.RunClientSideContext
        {
            Level = level,
            Thinkers = thinkers,
            FreshThinkers = fresh,
            GlLights = true,
            VidRenderMode = 4,
            WorldPaused = () => false,
        });

        Assert.Equal(2, freshPasses);
        Assert.Equal(6, level.LocalWorldTimer);
        Assert.Equal(10, level.LocalTimer);
        Assert.Equal(0, level.Flags3 & VmDThinkerClientSide.Level3LightCreated);
    }

    [Fact]
    public void RunClientSideThinkers_SkipsWorldTimerWhenPaused()
    {
        var level = new VmDThinkerClientSide.LevelModel { LocalWorldTimer = 1, LocalTimer = 1 };
        VmDThinkerClientSide.RunClientSideThinkers(new VmDThinkerClientSide.RunClientSideContext
        {
            Level = level,
            Thinkers = EmptyThinkerOps(VmDThinkerClientSide.MaxStatNum + 1),
            FreshThinkers = EmptyThinkerOps(VmDThinkerClientSide.MaxStatNum + 1),
            WorldPaused = () => true,
        });

        Assert.Equal(1, level.LocalWorldTimer);
        Assert.Equal(2, level.LocalTimer);
    }
}
