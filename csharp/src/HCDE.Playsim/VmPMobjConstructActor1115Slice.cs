namespace HCDE.Playsim;

/// <summary>
/// <c>UpdateWaterLevel</c> tail and <c>ConstructActor</c> spawn setup through float-rand height from
/// <c>p_mobj.cpp</c> (**5427–5576**). Chunk **1115**.
/// </summary>
public static class VmPMobjConstructActor1115Slice
{
    public const int MfNoGravity = 1;
    public const int Mf5AlwaysFast = 2;
    public const int Mf3IsMonster = 4;
    public const int Mf5NoInteraction = 8;
    public const int RfFullBright = 16;

    public enum SpawnZMode
    {
        Absolute,
        OnFloor,
        OnCeiling,
        FloatRandom
    }

    public static bool RunUpdateWaterLevel(bool doSplash, bool hasPlayer, Action? splash, Action? refreshDepth, Action? playDive)
    {
        if (doSplash)
            splash?.Invoke();
        refreshDepth?.Invoke();
        if (hasPlayer)
            playDive?.Invoke();
        return false;
    }

    public sealed class ConstructContext
    {
        public bool ClientSide { get; init; }
        public int ConversationRoot { get; init; } = -1;
        public bool AssignConversation { get; set; }
        public bool ClearConversation { get; set; }
        public double Gravity { get; init; } = 1;
        public int Flags { get; set; }
        public int Flags3 { get; init; }
        public int Flags5 { get; init; }
        public bool InstantReaction { get; init; }
        public bool DmInstantReaction { get; init; }
        public int ReactionTime { get; set; } = 8;
        public int LastLookPlayer { get; set; } = -1;
        public int TidToHate { get; set; } = -1;
        public int MaxPlayers { get; init; } = 8;
        public int Rng { get; init; }
        public int ServerTics { get; init; }
        public int ClientTics { get; init; }
        public int Tics { get; set; }
        public int RenderFlags { get; set; }
        public bool StateFullBright { get; init; }
        public bool FastMonsters { get; init; }
        public double FastSpeed { get; init; } = -1;
        public double Speed { get; set; }
        public SpawnZMode ZMode { get; init; }
        public double FloorZ { get; init; }
        public double CeilingZ { get; init; }
        public double Height { get; init; }
        public double SpawnZ { get; set; }
        public bool SpawningMapThing { get; init; }
        public bool IsPlayerPawn { get; init; }
        public bool CopySectorFloorCeiling { get; set; }
        public bool FindFloorCeiling { get; set; }
        public bool CopyNoInteractionPics { get; set; }
        public bool CopySpawnXY { get; set; } = true;
        public bool CopySpawnZ { get; set; }
        public bool ApplyFloatRandom { get; set; }
    }

    public static void RunConstructActor(ConstructContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.ConversationRoot != -1)
            ctx.AssignConversation = true;
        else
            ctx.ClearConversation = true;

        if (ctx.Gravity == 0)
            ctx.Flags |= MfNoGravity;

        if ((ctx.InstantReaction || (ctx.Flags5 & Mf5AlwaysFast) != 0 || ctx.DmInstantReaction)
            && (ctx.Flags3 & Mf3IsMonster) != 0)
        {
            ctx.ReactionTime = 0;
        }

        if ((ctx.Flags3 & Mf3IsMonster) != 0 && ctx.MaxPlayers > 0)
        {
            ctx.LastLookPlayer = ctx.Rng % ctx.MaxPlayers;
            ctx.TidToHate = 0;
        }

        ctx.Tics = ctx.ClientSide ? ctx.ClientTics : ctx.ServerTics;
        if (ctx.StateFullBright)
            ctx.RenderFlags |= RfFullBright;
        else
            ctx.RenderFlags &= ~RfFullBright;

        if (ctx.FastMonsters && ctx.FastSpeed >= 0)
            ctx.Speed = ctx.FastSpeed;

        ApplySpawnZ(ctx);

        if (ctx.SpawningMapThing || !ctx.IsPlayerPawn)
        {
            if (!ctx.SpawningMapThing)
                ctx.FindFloorCeiling = true;
            else
                ctx.CopySectorFloorCeiling = true;
        }
        else if ((ctx.Flags5 & Mf5NoInteraction) == 0)
        {
            ctx.FindFloorCeiling = true;
        }
        else
        {
            ctx.CopyNoInteractionPics = true;
        }

        ctx.CopySpawnXY = true;
        ctx.CopySpawnZ = false;
        ApplySpawnZ(ctx);
        if (ctx.ZMode == SpawnZMode.FloatRandom)
        {
            var space = ctx.CeilingZ - ctx.Height - ctx.FloorZ;
            ctx.ApplyFloatRandom = space > 48;
        }
    }

    private static void ApplySpawnZ(ConstructContext ctx)
    {
        ctx.SpawnZ = ctx.ZMode switch
        {
            SpawnZMode.OnFloor or SpawnZMode.FloatRandom => ctx.FloorZ,
            SpawnZMode.OnCeiling => ctx.CeilingZ - ctx.Height,
            _ => ctx.SpawnZ
        };
    }
}
