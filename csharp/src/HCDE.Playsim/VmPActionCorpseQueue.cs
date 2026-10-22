namespace HCDE.Playsim;

/// <summary>
/// Corpse queue and <c>A_Unblock</c> from <c>a_action.cpp</c> (37–169).
/// </summary>
public static class VmPActionCorpseQueue
{
    public const int MfStealth = 1 << 9;
    public const int MfSolid = 1 << 1;
    public const int MfCorpse = 1 << 7;
    public const int SvCorpseFilterMonsters = 1;

    public sealed class DropItem
    {
        public string Name { get; set; } = "";
        public int Amount { get; set; }
        public int Probability { get; set; }
        public DropItem? Next { get; set; }
    }

    public sealed class ConversationModel
    {
        public object? DropType { get; set; }
    }

    public sealed class LevelModel
    {
        public List<CorpseActor> CorpseQueue { get; } = new();
    }

    public sealed class CorpseActor
    {
        public LevelModel? Level { get; set; }
        public int Flags { get; set; }
        public double Alpha { get; set; } = 1;
        public int VisDir { get; set; }
        public ConversationModel? Conversation { get; set; }
        public object? Player { get; set; }
        public bool IsPlayerPawn { get; set; }
        public DropItem? DropItems { get; set; }
    }

    public sealed class UnblockContext
    {
        public Action<CorpseActor, object?, int, int>? DropItem { get; init; }
        public Func<string, object?>? FindActorClass { get; init; }
        public Action<CorpseActor>? DestroyCorpse { get; init; }
        public int SvCorpseFilter { get; init; }
        public int SvCorpseQueueSize { get; init; }
    }

    public static void HcdeQueueCorpse(CorpseActor? corpse, int svCorpseFilter, int svCorpseQueueSize, Action<CorpseActor>? destroy)
    {
        if (corpse is null || corpse.Level is null)
            return;

        if ((svCorpseFilter & SvCorpseFilterMonsters) == 0)
            return;

        var queue = corpse.Level!.CorpseQueue;
        var index = queue.FindIndex(element => ReferenceEquals(element, corpse));
        if (index >= 0)
            queue.RemoveAt(index);

        if (svCorpseQueueSize <= 0)
        {
            destroy?.Invoke(corpse);
            return;
        }

        while (queue.Count >= svCorpseQueueSize)
        {
            var oldCorpse = queue[0];
            if (oldCorpse is not null && !ReferenceEquals(oldCorpse, corpse))
                destroy?.Invoke(oldCorpse);
            queue.RemoveAt(0);
        }

        queue.Add(corpse);
    }

    public static void AUnblock(CorpseActor self, UnblockContext ctx, bool drop = true)
    {
        if ((self.Flags & MfStealth) != 0)
        {
            self.Alpha = 1;
            self.VisDir = 0;
        }

        self.Flags &= ~MfSolid;

        if (self.Conversation?.DropType is not null)
        {
            ctx.DropItem?.Invoke(self, self.Conversation.DropType, -1, 256);
            self.Conversation = null;
            return;
        }

        self.Conversation = null;

        if (drop && !self.IsPlayerPawn)
        {
            var di = self.DropItems;
            while (di is not null)
            {
                if (di.Name.Length > 0)
                {
                    var ti = ctx.FindActorClass?.Invoke(di.Name);
                    if (ti is not null)
                        ctx.DropItem?.Invoke(self, ti, di.Amount, di.Probability);
                }

                di = di.Next;
            }
        }

        if ((self.Flags & MfCorpse) != 0 && self.Player is null && !self.IsPlayerPawn)
        {
            HcdeQueueCorpse(self, ctx.SvCorpseFilter, ctx.SvCorpseQueueSize, ctx.DestroyCorpse);
        }
    }

    public static void AQueueCorpse(CorpseActor self, UnblockContext ctx)
    {
        if ((self.Flags & MfCorpse) != 0)
            HcdeQueueCorpse(self, ctx.SvCorpseFilter, ctx.SvCorpseQueueSize, ctx.DestroyCorpse);
    }

    public static void ADeQueueCorpse(CorpseActor self)
    {
        var queue = self.Level!.CorpseQueue;
        var index = queue.FindIndex(element => ReferenceEquals(element, self));
        if (index < queue.Count)
            queue.RemoveAt(index);
    }
}
