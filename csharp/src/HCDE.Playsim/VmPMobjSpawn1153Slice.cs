namespace HCDE.Playsim;

/// <summary>
/// Map-thing spawn tail from <c>p_mobj.cpp</c> (<b>6942–7094</b>). Chunk <b>1153</b>.
/// The yaw line at <b>6941</b> and the indexed-spawn body from <b>7095</b> are already credited.
/// </summary>
public static class VmPMobjSpawn1153Slice
{
    public const int StyleCount = 19;
    public const int NoShadowMap = 16;
    public const int DontLightActors = 32;
    public const int Spot = 64;
    public const int DontLightMap = 256;
    public const int Euthanize = 1 << 5;

    public readonly record struct ThingTail(
        int Conversation,
        int LookedUpRoot,
        double Alpha,
        uint RenderStyle,
        double ScaleX,
        double ScaleY,
        short Pitch,
        short Roll,
        int Score,
        uint FillColor,
        int PaletteIndex,
        bool DynamicLight,
        bool HasColorName,
        int ColorR,
        int ColorG,
        int ColorB,
        int Arg0,
        int Arg1,
        int Arg2,
        bool NoShadowDefined,
        bool NoShadow,
        bool DontActorsDefined,
        bool DontActors,
        bool DontMapDefined,
        bool DontMap,
        double SoftShadow,
        double Linearity,
        int ShadowQuality,
        double Health,
        bool HasDieState);

    public readonly record struct ActorTail(
        int ConversationRoot,
        bool DialogueAssigned,
        double Alpha,
        int RenderStyle,
        double ScaleX,
        double ScaleY,
        double Pitch,
        double Roll,
        int Score,
        uint FillColor,
        int Arg0,
        int Arg1,
        int Arg2,
        int LightFlags,
        float SoftShadow,
        float Linearity,
        int ShadowQuality,
        int ObjectFlags,
        int Health,
        int StartHealth);

    public readonly record struct LightSlot(bool Present, float Soft, float Linearity, int Quality);

    public readonly record struct SpawnTail(
        int ConversationRoot,
        bool DialogueAssigned,
        double Alpha,
        int RenderStyle,
        double ScaleX,
        double ScaleY,
        double Pitch,
        double Roll,
        int Score,
        uint FillColor,
        int Arg0,
        int Arg1,
        int Arg2,
        int LightFlags,
        float SoftShadow,
        float Linearity,
        int ShadowQuality,
        bool SpotAngles,
        double SpotInner,
        double SpotOuter,
        bool BeginPlay,
        bool LevelSpawned,
        int Health,
        int StartHealth,
        bool StartHealthSet,
        bool DieFromSpawn,
        IReadOnlyList<LightSlot> Lights);

    public readonly record struct IndexOpen(bool Register, bool SpawnCalled);

    public static SpawnTail Finish(ThingTail thing, ActorTail actor, IReadOnlyList<LightSlot> lights)
    {
        int root = actor.ConversationRoot;
        bool dialogue = actor.DialogueAssigned;
        if (thing.Conversation > 0 && thing.LookedUpRoot != -1)
        {
            root = thing.LookedUpRoot;
            dialogue = true;
        }

        double alpha = actor.Alpha;
        if (thing.Alpha >= 0)
            alpha = thing.Alpha;

        int renderStyle = actor.RenderStyle;
        if (thing.RenderStyle != StyleCount)
            renderStyle = (int)thing.RenderStyle;

        double scaleX = actor.ScaleX;
        double scaleY = actor.ScaleY;
        if (thing.ScaleX != 0)
            scaleX *= thing.ScaleX;
        if (thing.ScaleY != 0)
            scaleY *= thing.ScaleY;

        double pitch = actor.Pitch;
        double roll = actor.Roll;
        if (thing.Pitch != 0)
            pitch = thing.Pitch;
        if (thing.Roll != 0)
            roll = thing.Roll;

        int score = actor.Score;
        if (thing.Score != 0)
            score = thing.Score;

        uint fill = actor.FillColor;
        if (thing.FillColor != 0)
            fill = (thing.FillColor & 0x00ffffffu) | ((uint)thing.PaletteIndex << 24);

        int arg0 = actor.Arg0;
        int arg1 = actor.Arg1;
        int arg2 = actor.Arg2;
        int lightFlags = actor.LightFlags;
        float soft = actor.SoftShadow;
        float linearity = actor.Linearity;
        int quality = actor.ShadowQuality;
        bool spotAngles = false;
        double spotInner = 0;
        double spotOuter = 0;
        if (thing.DynamicLight)
        {
            if (thing.HasColorName)
            {
                arg0 = thing.ColorR;
                arg1 = thing.ColorG;
                arg2 = thing.ColorB;
            }
            else if ((lightFlags & Spot) != 0)
            {
                arg0 = Red(thing.Arg0);
                arg1 = Green(thing.Arg0);
                arg2 = Blue(thing.Arg0);
            }

            if (thing.NoShadowDefined)
            {
                if (thing.NoShadow)
                    lightFlags |= NoShadowMap;
                else
                    lightFlags &= ~NoShadowMap;
            }

            if (thing.DontActorsDefined)
            {
                if (thing.DontActors)
                    lightFlags |= DontLightActors;
                else
                    lightFlags &= ~DontLightActors;
            }

            if (thing.DontMapDefined)
            {
                if (thing.DontMap)
                    lightFlags |= DontLightMap;
                else
                    lightFlags &= ~DontLightMap;
            }

            if (thing.SoftShadow >= 0)
                soft = (float)thing.SoftShadow;
            if (thing.Linearity >= 0)
                linearity = (float)thing.Linearity;
            if (thing.ShadowQuality >= 0)
                quality = thing.ShadowQuality;

            if ((lightFlags & Spot) != 0)
            {
                spotAngles = true;
                spotInner = thing.Arg1;
                spotOuter = thing.Arg2;
            }
        }

        var copied = new List<LightSlot>(lights.Count);
        if (thing.DynamicLight)
        {
            foreach (LightSlot light in lights)
            {
                if (!light.Present)
                {
                    copied.Add(light);
                    continue;
                }

                float nextSoft = light.Soft;
                float nextLine = light.Linearity;
                int nextQuality = light.Quality;
                if (soft >= 0f)
                    nextSoft = soft;
                if (linearity >= 0f)
                    nextLine = linearity;
                if (quality >= 0)
                    nextQuality = quality;
                copied.Add(new LightSlot(true, nextSoft, nextLine, nextQuality));
            }
        }
        else
        {
            copied.AddRange(lights);
        }

        int health;
        if (thing.Health > 0)
            health = (int)(actor.Health * thing.Health);
        else
            health = -((int)thing.Health);

        bool die = false;
        int startHealth = actor.StartHealth;
        bool startSet = false;
        if (thing.Health == 0)
        {
            if (thing.HasDieState)
                die = true;
        }
        else if (thing.Health != 1)
        {
            startHealth = health;
            startSet = true;
        }

        bool levelSpawned = (actor.ObjectFlags & Euthanize) == 0;
        return new SpawnTail(
            root,
            dialogue,
            alpha,
            renderStyle,
            scaleX,
            scaleY,
            pitch,
            roll,
            score,
            fill,
            arg0,
            arg1,
            arg2,
            lightFlags,
            soft,
            linearity,
            quality,
            spotAngles,
            spotInner,
            spotOuter,
            true,
            levelSpawned,
            health,
            startHealth,
            startSet,
            die,
            copied);
    }

    public static IndexOpen OpenIndex(int index)
    {
        bool register = false;
        if (index == 0)
            register = true;
        return new IndexOpen(register, true);
    }

    private static int Red(int packed)
    {
        return (packed >> 16) & 255;
    }

    private static int Green(int packed)
    {
        return (packed >> 8) & 255;
    }

    private static int Blue(int packed)
    {
        return packed & 255;
    }
}
