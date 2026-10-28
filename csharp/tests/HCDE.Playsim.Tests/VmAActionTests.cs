using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmAActionTests
{
    [Fact]
    public void QueueCorpse_SkipsWhenFilterDisabled()
    {
        var q = new List<Actor>();
        var a = new Actor();
        VmAAction.QueueCorpse(q, a, svCorpseFilter: 0, svCorpseQueueSize: 2, _ => { });
        Assert.Empty(q);
    }

    [Fact]
    public void QueueCorpse_EvictsOldestAtCapacity()
    {
        var q = new List<Actor>();
        var destroyed = new List<Actor>();
        var a = new Actor();
        var b = new Actor();
        var c = new Actor();
        VmAAction.QueueCorpse(q, a, 1, 2, x => destroyed.Add(x));
        VmAAction.QueueCorpse(q, b, 1, 2, x => destroyed.Add(x));
        VmAAction.QueueCorpse(q, c, 1, 2, x => destroyed.Add(x));
        Assert.Equal(2, q.Count);
        Assert.Contains(a, destroyed);
        Assert.Equal([b, c], q);
    }

    [Fact]
    public void DeQueueCorpse_RemovesSelf()
    {
        var q = new List<Actor> { new(), new() };
        VmAAction.DeQueueCorpse(q, q[1]);
        Assert.Single(q);
    }

    [Fact]
    public void Unblock_ClearsSolidAndQueuesCorpse()
    {
        var self = new Actor { Corpse = true, Solid = true };
        var q = new List<Actor>();
        VmAAction.Unblock(self, drop: false, q, 1, 4, null, null, null, false, (_, _, _, _) => { }, _ => { }, _ => null);
        Assert.False(self.Solid);
        Assert.Single(q);
        Assert.Same(self, q[0]);
    }

    [Fact]
    public void Unblock_StealthRestoresAlpha()
    {
        var self = new Actor { Stealth = true, Alpha = 0.25 };
        VmAAction.Unblock(self, false, null, 0, 0, null, null, null, false, (_, _, _, _) => { }, _ => { }, _ => null);
        Assert.Equal(1, self.Alpha);
        Assert.False(self.Solid);
    }

    [Fact]
    public void QueueCorpseAction_RequiresCorpseFlag()
    {
        var q = new List<Actor>();
        var live = new Actor { Corpse = false };
        VmAAction.QueueCorpseAction(q, live, 1, 2, _ => { });
        Assert.Empty(q);
    }
}
