namespace HCDE.Playsim;

/// <summary>
/// Corpse queue and unblock actions from <c>a_action.cpp</c> (<b>1–169</b>). Chunk <b>1165</b>.
/// </summary>
public static class VmAActionCorpse1165Slice
{
    public const int MfSolid = 0x2;
    public const int MfCorpse = 0x00100000;
    public const int MfStealth = 0x40000000;
    public const string NameNone = "None";

    public sealed class DropLink
    {
        public string Name = "";
        public int Amount;
        public int Probability;
        public DropLink? Next;
    }

    public sealed class ActorState
    {
        public int Flags;
        public double Alpha;
        public int VisDir;
        public bool LevelPresent;
        public bool HasPlayer;
        public bool PlayerPawn;
        public bool ConversationPresent;
        public bool ConversationHasDrop;
        public DropLink? Drops;
        public bool Destroyed;
    }

    public readonly record struct ItemDrop(string Kind, int Amount, int Probability);

    public sealed class UnblockResult
    {
        public List<ItemDrop> Drops { get; } = new();
        public bool ReturnedAfterConversationDrop;
    }

    public static void QueueCorpse(
        ActorState? corpse,
        List<ActorState?> queue,
        int filter,
        int queueSize)
    {
        if (corpse == null || !corpse.LevelPresent)
        {
            return;
        }

        if ((filter & 1) == 0)
        {
            return;
        }

        var index = queue.IndexOf(corpse);
        if (index >= 0)
        {
            queue.RemoveAt(index);
        }

        if (queueSize <= 0)
        {
            corpse.Destroyed = true;
            return;
        }

        while (queue.Count >= queueSize)
        {
            var oldCorpse = queue[0];
            if (oldCorpse != null && !ReferenceEquals(oldCorpse, corpse))
            {
                oldCorpse.Destroyed = true;
            }

            queue.RemoveAt(0);
        }

        queue.Add(corpse);
    }

    public static int QueueCorpseAction(
        ActorState self,
        List<ActorState?> queue,
        int filter,
        int queueSize)
    {
        if ((self.Flags & MfCorpse) != 0)
        {
            QueueCorpse(self, queue, filter, queueSize);
        }

        return 0;
    }

    public static int DeQueueCorpse(ActorState self, List<ActorState?> queue)
    {
        var index = queue.IndexOf(self);
        if (index >= 0)
        {
            queue.RemoveAt(index);
        }

        return 0;
    }

    public static UnblockResult Unblock(
        ActorState self,
        bool drop,
        List<ActorState?> queue,
        int filter,
        int queueSize,
        Func<string, bool> classFound)
    {
        var result = new UnblockResult();
        if ((self.Flags & MfStealth) != 0)
        {
            self.Alpha = 1d;
            self.VisDir = 0;
        }

        self.Flags &= ~MfSolid;

        if (self.ConversationPresent && self.ConversationHasDrop)
        {
            result.Drops.Add(new ItemDrop("conversation", -1, 256));
            self.ConversationPresent = false;
            result.ReturnedAfterConversationDrop = true;
            return result;
        }

        self.ConversationPresent = false;

        if (drop && !self.PlayerPawn)
        {
            var link = self.Drops;
            while (link != null)
            {
                if (!string.Equals(link.Name, NameNone, StringComparison.Ordinal))
                {
                    if (classFound(link.Name))
                    {
                        result.Drops.Add(new ItemDrop(link.Name, link.Amount, link.Probability));
                    }
                }

                link = link.Next;
            }
        }

        if ((self.Flags & MfCorpse) != 0 && !self.HasPlayer && !self.PlayerPawn)
        {
            QueueCorpse(self, queue, filter, queueSize);
        }

        return result;
    }
}
