namespace HCDE.Playsim;

/// <summary>
/// State duration, <c>SetState</c>, and inventory teardown from <c>p_mobj.cpp</c>
/// (<b>823–1044</b>). Chunk <b>1152</b>. <c>DropInventory</c> begins at <b>1045</b>.
/// </summary>
public static class VmPMobjState1152Slice
{
    public const int SprFixed = 1;
    public const int SprNoChange = 2;
    public const int ActorUse = 1;
    public const int FullBright = 0x0010;
    public const int NoSkin = 0x00000020;
    public const int Euthanize = 1 << 5;
    public const int RecreateLights = 0x00000100;
    public const int LightCreated = 0x00080000;
    public const int StateLoopLimit = 300000;

    public readonly record struct ActorFrame(
        int Sprite,
        int Frame,
        int Tics,
        int ClientTics,
        bool Fast,
        bool Slow,
        bool SameFrame,
        bool FullBright,
        int UseFlags,
        int Light,
        int NextIndex,
        bool CallsAction,
        int JumpIndex,
        bool Euthanize);

    public readonly record struct StateStep(
        bool Alive,
        int Sprite,
        int Frame,
        int Tics,
        int RenderFlags,
        int Flags8,
        int LevelFlags3,
        bool Destroyed);

    public static int GetTics(bool clientSide, int tics, int clientTics, bool actorFast, bool stateFast, bool actorSlow, bool stateSlow)
    {
        int duration = clientSide ? clientTics : tics;
        if (actorFast && stateFast)
            return duration - (duration >> 1);
        if (actorSlow && stateSlow)
            return duration << 1;
        return duration;
    }

    public static StateStep SetState(
        IReadOnlyList<ActorFrame> states,
        int startIndex,
        bool noFunction,
        bool clientSide,
        bool actorFast,
        bool actorSlow,
        bool hasCurrentState,
        int currentStateSprite,
        int sprite,
        int frame,
        int renderFlags,
        int flags4,
        int flags8,
        int spawnSprite,
        bool hasPlayer,
        int skinCount,
        int skinSprite,
        int lightAssociations,
        int oldStateLight,
        int levelFlags3)
    {
        int limit = StateLoopLimit;
        int index = startIndex;
        bool hasState = hasCurrentState;
        int stateSprite = currentStateSprite;
        int tics = 0;
        int stateLight = 0;
        while (true)
        {
            if (--limit == 0)
                return new StateStep(false, sprite, frame, tics, renderFlags, flags8, levelFlags3, true);
            if (index < 0 || index >= states.Count)
                return new StateStep(false, sprite, frame, tics, renderFlags, flags8, levelFlags3, true);

            ActorFrame next = states[index];
            int previousSprite = hasState ? stateSprite : -1;
            if ((next.UseFlags & ActorUse) == 0)
                return new StateStep(false, sprite, frame, tics, renderFlags, flags8, levelFlags3, true);

            hasState = true;
            stateSprite = next.Sprite;
            stateLight = next.Light;
            tics = GetTics(clientSide, next.Tics, next.ClientTics, actorFast, next.Fast, actorSlow, next.Slow);
            renderFlags = (renderFlags & ~FullBright) | (next.FullBright ? FullBright : 0);
            if (next.Sprite != SprFixed)
            {
                if (!next.SameFrame)
                    frame = next.Frame;
                if (next.Sprite != SprNoChange)
                {
                    if ((flags4 & NoSkin) == 0 && next.Sprite == spawnSprite)
                    {
                        if (hasPlayer && skinCount > 0)
                            sprite = skinSprite;
                        else if (next.Sprite != previousSprite)
                            sprite = next.Sprite;
                    }
                    else
                    {
                        sprite = next.Sprite;
                    }
                }
            }

            if (!noFunction && next.CallsAction)
            {
                if (next.Euthanize)
                    return new StateStep(false, sprite, frame, tics, renderFlags, flags8, levelFlags3, true);
                if (next.JumpIndex >= 0)
                {
                    index = next.JumpIndex;
                    tics = 0;
                    continue;
                }
            }

            index = next.NextIndex;
            if (tics != 0)
                break;
        }

        if (lightAssociations > 0 || (hasState && stateLight > 0) || oldStateLight > 0)
        {
            flags8 |= RecreateLights;
            levelFlags3 |= LightCreated;
        }

        return new StateStep(true, sprite, frame, tics, renderFlags, flags8, levelFlags3, false);
    }

    public static IReadOnlyList<int> DestroyInventory(IReadOnlyList<(int Id, bool AlreadyDead)> items)
    {
        var destroyed = new List<int>();
        foreach ((int id, bool alreadyDead) in items)
        {
            if (!alreadyDead)
                destroyed.Add(id);
        }
        return destroyed;
    }

    public static bool UseInventory(bool hasVm, bool vmUsed)
    {
        if (hasVm)
            return vmUsed;
        return false;
    }
}
