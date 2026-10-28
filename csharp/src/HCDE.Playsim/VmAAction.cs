namespace HCDE.Playsim;

/// <summary>Corpse queue and unblock actions from <c>a_action.cpp</c>.</summary>
public static class VmAAction
{
    public sealed class DropItemNode
    {
        public string Name { get; set; } = "";
        public int Amount { get; set; }
        public int Probability { get; set; }
        public DropItemNode? Next { get; set; }
    }

    public static void QueueCorpse(
        IList<Actor> corpseQueue,
        Actor corpse,
        int svCorpseFilter,
        int svCorpseQueueSize,
        Action<Actor> destroyActor)
    {
        if ((svCorpseFilter & 1) == 0)
            return;

        var index = corpseQueue.IndexOf(corpse);
        if (index >= 0)
            corpseQueue.RemoveAt(index);

        if (svCorpseQueueSize <= 0)
        {
            destroyActor(corpse);
            return;
        }

        while (corpseQueue.Count >= svCorpseQueueSize)
        {
            var oldCorpse = corpseQueue[0];
            if (oldCorpse != corpse)
                destroyActor(oldCorpse);
            corpseQueue.RemoveAt(0);
        }

        corpseQueue.Add(corpse);
    }

    public static void DeQueueCorpse(IList<Actor> corpseQueue, Actor self)
    {
        var index = corpseQueue.IndexOf(self);
        if (index >= 0)
            corpseQueue.RemoveAt(index);
    }

    public static void QueueCorpseAction(
        IList<Actor> corpseQueue,
        Actor self,
        int svCorpseFilter,
        int svCorpseQueueSize,
        Action<Actor> destroyActor)
    {
        if (self.Corpse)
            QueueCorpse(corpseQueue, self, svCorpseFilter, svCorpseQueueSize, destroyActor);
    }

    public static void Unblock(
        Actor self,
        bool drop,
        IList<Actor>? corpseQueue,
        int svCorpseFilter,
        int svCorpseQueueSize,
        object? conversationDropType,
        DropItemNode? dropItems,
        object? player,
        bool isPlayerPawn,
        Action<Actor, object, int, int> dropItem,
        Action<Actor> destroyActor,
        Func<string, object?> findActorClass)
    {
        if (self.Stealth)
        {
            self.Alpha = 1;
        }

        self.Solid = false;

        if (conversationDropType != null)
        {
            dropItem(self, conversationDropType, -1, 256);
            return;
        }

        if (drop && !isPlayerPawn)
        {
            var di = dropItems;
            while (di != null)
            {
                if (di.Name.Length > 0)
                {
                    var ti = findActorClass(di.Name);
                    if (ti != null)
                        dropItem(self, ti, di.Amount, di.Probability);
                }
                di = di.Next;
            }
        }

        if (self.Corpse && player == null && !isPlayerPawn && corpseQueue != null)
            QueueCorpse(corpseQueue, self, svCorpseFilter, svCorpseQueueSize, destroyActor);
    }
}
