namespace HCDE.Playsim;

/// <summary>
/// Tick tail, nightmare respawn gates, <c>CheckNoDelay</c>, and sector-transition action bits from
/// <c>p_mobj.cpp</c> (**5127–5276**). Chunk **1113**.
/// </summary>
public static class VmPMobjTickTail1113Slice
{
    public const int OfEuthanizeMe = 1;
    public const int Mf7HandleNoDelay = 2;
    public const int Mf2Dormant = 4;
    public const int Mf9DecoupledAnimations = 8;
    public const int ModelDataRecalc = 16;
    public const int Mf5AlwaysRespawn = 32;
    public const int Mf3IsMonster = 64;
    public const int Mf2DormantRespawn = 4;
    public const int Mf5NeverRespawn = 128;
    public const int SecEnter = 1;
    public const int SecHitFloor = 2;
    public const int SecHitCeiling = 4;
    public const int SecHitFakeFloor = 8;
    public const int TicRate = 35;

    public static bool ShouldApplyTerrainFlat(bool aboveFloor, int waterLevel) =>
        !aboveFloor || waterLevel != 0;

    public static bool AdvanceTicsFreesActor(ref int tics, bool setStateSucceeded)
    {
        if (tics == -1)
            return false;
        tics--;
        if (tics > 0)
            return false;
        return !setStateSucceeded;
    }

    public static bool ShouldRecalculateBones(int flags9, bool hasModelData, int modelFlags) =>
        (flags9 & Mf9DecoupledAnimations) == 0
        && hasModelData
        && (modelFlags & ModelDataRecalc) == 0;

    public static int ResolveRespawnMonsterTics(int skillRespawn, int minRespawnTics)
    {
        if (minRespawnTics > 0)
            return minRespawnTics;
        if (minRespawnTics < 0)
            return -minRespawnTics * TicRate;
        return skillRespawn;
    }

    public sealed class NightmareRespawnContext
    {
        public int Tics { get; init; }
        public bool StateCanRaise { get; init; }
        public int Flags5 { get; init; }
        public int Flags3 { get; init; }
        public int Flags2 { get; init; }
        public int RespawnMonsters { get; init; }
        public int RespawnLimit { get; init; }
        public int SkillRespawnCount { get; init; }
        public int MoveCount { get; set; }
        public int LevelTime { get; init; }
        public int DiceRoll { get; init; }
        public int RespawnDice { get; init; }
        public bool Spawn { get; set; }
    }

    public static bool TryNightmareRespawn(NightmareRespawnContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.Tics != -1 && !ctx.StateCanRaise)
            return false;

        if ((ctx.Flags5 & Mf5AlwaysRespawn) == 0)
        {
            if (ctx.RespawnMonsters == 0
                || (ctx.Flags3 & Mf3IsMonster) == 0
                || (ctx.Flags2 & Mf2DormantRespawn) != 0
                || (ctx.Flags5 & Mf5NeverRespawn) != 0)
            {
                return false;
            }

            if (ctx.RespawnLimit > 0 && ctx.SkillRespawnCount >= ctx.RespawnLimit)
                return false;
        }

        ctx.MoveCount++;
        if (ctx.MoveCount < ctx.RespawnMonsters)
            return false;
        if ((ctx.LevelTime & 31) != 0)
            return false;
        if (ctx.DiceRoll > ctx.RespawnDice)
            return false;

        ctx.Spawn = true;
        return true;
    }

    public sealed class CheckNoDelayContext
    {
        public int Flags7 { get; set; }
        public int Flags2 { get; init; }
        public bool StateNoDelay { get; init; }
        public bool ActionCalled { get; init; }
        public bool Euthanized { get; init; }
        public bool HasNewState { get; init; }
        public bool SetStateResult { get; init; } = true;
        public bool ClearedHandleFlag { get; set; }
    }

    public static bool RunCheckNoDelay(CheckNoDelayContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if ((ctx.Flags7 & Mf7HandleNoDelay) == 0 || (ctx.Flags2 & Mf2Dormant) != 0)
            return true;

        ctx.Flags7 &= ~Mf7HandleNoDelay;
        ctx.ClearedHandleFlag = true;
        if (!ctx.StateNoDelay || !ctx.ActionCalled)
            return true;
        if (ctx.Euthanized)
            return false;
        if (ctx.HasNewState)
            return ctx.SetStateResult;
        return true;
    }

    public static int BuildSectorEnterActions(bool onFloor, bool hitsCeiling, bool onFakeFloor)
    {
        var act = SecEnter;
        if (onFloor)
            act |= SecHitFloor;
        if (hitsCeiling)
            act |= SecHitCeiling;
        if (onFakeFloor)
            act |= SecHitFakeFloor;
        return act;
    }
}
