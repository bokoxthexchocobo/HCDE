using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmPActionCorpseQueueTests
{
    [Fact]
    public void HcdeQueueCorpse_EvictsOldestWhenFull()
    {
        var level = new VmPActionCorpseQueue.LevelModel();
        var destroyed = new List<VmPActionCorpseQueue.CorpseActor>();
        var a = new VmPActionCorpseQueue.CorpseActor { Level = level };
        var b = new VmPActionCorpseQueue.CorpseActor { Level = level };
        var c = new VmPActionCorpseQueue.CorpseActor { Level = level };

        VmPActionCorpseQueue.HcdeQueueCorpse(a, 1, 2, destroyed.Add);
        VmPActionCorpseQueue.HcdeQueueCorpse(b, 1, 2, destroyed.Add);
        VmPActionCorpseQueue.HcdeQueueCorpse(c, 1, 2, destroyed.Add);

        Assert.Single(destroyed);
        Assert.Same(a, destroyed[0]);
        Assert.Equal(2, level.CorpseQueue.Count);
        Assert.DoesNotContain(a, level.CorpseQueue);
    }

    [Fact]
    public void AUnblock_ClearsSolidAndQueuesCorpse()
    {
        var level = new VmPActionCorpseQueue.LevelModel();
        var actor = new VmPActionCorpseQueue.CorpseActor
        {
            Level = level,
            Flags = VmPActionCorpseQueue.MfSolid | VmPActionCorpseQueue.MfCorpse,
        };
        var ctx = new VmPActionCorpseQueue.UnblockContext { SvCorpseFilter = 1, SvCorpseQueueSize = 4 };
        VmPActionCorpseQueue.AUnblock(actor, ctx);
        Assert.Equal(0, actor.Flags & VmPActionCorpseQueue.MfSolid);
        Assert.Single(level.CorpseQueue);
    }

    [Fact]
    public void ADeQueueCorpse_RemovesSelf()
    {
        var level = new VmPActionCorpseQueue.LevelModel();
        var actor = new VmPActionCorpseQueue.CorpseActor { Level = level };
        level.CorpseQueue.Add(actor);
        VmPActionCorpseQueue.ADeQueueCorpse(actor);
        Assert.Empty(level.CorpseQueue);
    }
}
