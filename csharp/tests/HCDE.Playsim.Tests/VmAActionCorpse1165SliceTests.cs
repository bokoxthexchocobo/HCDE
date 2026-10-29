namespace HCDE.Playsim.Tests;

public class VmAActionCorpse1165SliceTests
{
    [Fact]
    public void QueueCorpse_FiltersDedupesAndEvictsTheOldest()
    {
        var kept = new VmAActionCorpse1165Slice.ActorState { LevelPresent = true };
        var queue = new List<VmAActionCorpse1165Slice.ActorState?> { kept };
        VmAActionCorpse1165Slice.QueueCorpse(null, queue, 1, 2);
        VmAActionCorpse1165Slice.QueueCorpse(
            new VmAActionCorpse1165Slice.ActorState(),
            queue,
            1,
            2);
        Assert.Single(queue);
        Assert.Same(kept, queue[0]);

        var filtered = new VmAActionCorpse1165Slice.ActorState { LevelPresent = true };
        VmAActionCorpse1165Slice.QueueCorpse(filtered, queue, 2, 0);
        Assert.False(filtered.Destroyed);
        Assert.Single(queue);

        var fresh = new VmAActionCorpse1165Slice.ActorState { LevelPresent = true };
        queue.Add(fresh);
        VmAActionCorpse1165Slice.QueueCorpse(fresh, queue, 1, 0);
        Assert.True(fresh.Destroyed);
        Assert.DoesNotContain(fresh, queue);

        var oldest = new VmAActionCorpse1165Slice.ActorState { LevelPresent = true };
        var newer = new VmAActionCorpse1165Slice.ActorState { LevelPresent = true };
        var hole = new List<VmAActionCorpse1165Slice.ActorState?> { null, oldest, newer };
        var incoming = new VmAActionCorpse1165Slice.ActorState { LevelPresent = true };
        VmAActionCorpse1165Slice.QueueCorpse(incoming, hole, 1, 2);
        Assert.True(oldest.Destroyed);
        Assert.False(newer.Destroyed);
        Assert.Equal(2, hole.Count);
        Assert.Same(newer, hole[0]);
        Assert.Same(incoming, hole[1]);

        hole.Insert(0, incoming);
        VmAActionCorpse1165Slice.QueueCorpse(incoming, hole, 1, 1);
        Assert.False(incoming.Destroyed);
        Assert.True(newer.Destroyed);
        Assert.Single(hole);
        Assert.Same(incoming, hole[0]);
    }

    [Fact]
    public void Unblock_DropsConversationAndSkipsTheQueue()
    {
        var self = new VmAActionCorpse1165Slice.ActorState
        {
            Flags = VmAActionCorpse1165Slice.MfStealth
                | VmAActionCorpse1165Slice.MfSolid
                | VmAActionCorpse1165Slice.MfCorpse,
            Alpha = 0.2,
            VisDir = -1,
            LevelPresent = true,
            ConversationPresent = true,
            ConversationHasDrop = true,
            Drops = new VmAActionCorpse1165Slice.DropLink { Name = "Clip", Amount = 4, Probability = 128 },
        };
        var queue = new List<VmAActionCorpse1165Slice.ActorState?>();
        var result = VmAActionCorpse1165Slice.Unblock(self, true, queue, 1, 4, _ => true);

        Assert.Equal(1d, self.Alpha);
        Assert.Equal(0, self.VisDir);
        Assert.Equal(VmAActionCorpse1165Slice.MfStealth | VmAActionCorpse1165Slice.MfCorpse, self.Flags);
        Assert.False(self.ConversationPresent);
        Assert.True(result.ReturnedAfterConversationDrop);
        Assert.Equal(new VmAActionCorpse1165Slice.ItemDrop("conversation", -1, 256), result.Drops[0]);
        Assert.Empty(queue);
    }

    [Fact]
    public void Unblock_WalksDropsAndQueuesAMonsterCorpse()
    {
        var clip = new VmAActionCorpse1165Slice.DropLink
        {
            Name = "Clip",
            Amount = 5,
            Probability = 200,
            Next = new VmAActionCorpse1165Slice.DropLink
            {
                Name = VmAActionCorpse1165Slice.NameNone,
                Amount = 1,
                Probability = 256,
                Next = new VmAActionCorpse1165Slice.DropLink
                {
                    Name = "Missing",
                    Amount = 9,
                    Probability = 64,
                    Next = new VmAActionCorpse1165Slice.DropLink { Name = "Shell", Amount = 2, Probability = 32 },
                },
            },
        };
        var self = new VmAActionCorpse1165Slice.ActorState
        {
            Flags = VmAActionCorpse1165Slice.MfSolid | VmAActionCorpse1165Slice.MfCorpse,
            LevelPresent = true,
            ConversationPresent = true,
            Drops = clip,
        };
        var queue = new List<VmAActionCorpse1165Slice.ActorState?>();
        var result = VmAActionCorpse1165Slice.Unblock(
            self,
            true,
            queue,
            1,
            3,
            name => name != "Missing");

        Assert.False(self.ConversationPresent);
        Assert.False(result.ReturnedAfterConversationDrop);
        Assert.Equal(2, result.Drops.Count);
        Assert.Equal(new VmAActionCorpse1165Slice.ItemDrop("Clip", 5, 200), result.Drops[0]);
        Assert.Equal(new VmAActionCorpse1165Slice.ItemDrop("Shell", 2, 32), result.Drops[1]);
        Assert.Same(self, queue[0]);

        var player = new VmAActionCorpse1165Slice.ActorState
        {
            Flags = VmAActionCorpse1165Slice.MfCorpse,
            LevelPresent = true,
            HasPlayer = true,
            Drops = clip,
        };
        VmAActionCorpse1165Slice.Unblock(player, true, queue, 1, 3, _ => true);
        Assert.Single(queue);

        var pawn = new VmAActionCorpse1165Slice.ActorState
        {
            Flags = VmAActionCorpse1165Slice.MfCorpse,
            LevelPresent = true,
            PlayerPawn = true,
            Drops = clip,
        };
        var pawnResult = VmAActionCorpse1165Slice.Unblock(pawn, true, queue, 1, 3, _ => true);
        Assert.Empty(pawnResult.Drops);
        Assert.Single(queue);
    }

    [Fact]
    public void Actions_QueueOnlyCorpsesAndDequeueTheActor()
    {
        var living = new VmAActionCorpse1165Slice.ActorState { LevelPresent = true, Flags = VmAActionCorpse1165Slice.MfSolid };
        var corpse = new VmAActionCorpse1165Slice.ActorState
        {
            LevelPresent = true,
            Flags = VmAActionCorpse1165Slice.MfCorpse,
        };
        var other = new VmAActionCorpse1165Slice.ActorState { LevelPresent = true };
        var queue = new List<VmAActionCorpse1165Slice.ActorState?> { other, corpse };

        Assert.Equal(0, VmAActionCorpse1165Slice.QueueCorpseAction(living, queue, 1, 4));
        Assert.Equal(2, queue.Count);
        Assert.Equal(0, VmAActionCorpse1165Slice.QueueCorpseAction(corpse, queue, 1, 4));
        Assert.Equal(new[] { other, corpse }, queue);

        Assert.Equal(0, VmAActionCorpse1165Slice.DeQueueCorpse(corpse, queue));
        Assert.Equal(new[] { other }, queue);
        Assert.Equal(0, VmAActionCorpse1165Slice.DeQueueCorpse(living, queue));
        Assert.Single(queue);
    }
}
