namespace HCDE.Playsim;

/// <summary>Native <c>FLevelLocals::PlayerSpawnPickClass</c> player class selection.</summary>
public static class PlayerSpawnClassPicker
{
    public sealed class PickContext
    {
        public bool ClassAlreadySet { get; init; }
        public int PlayerClassCount { get; init; }
        public bool Deathmatch { get; init; }
        public bool Multiplayer { get; init; }
        public int PlayerNumber { get; init; }
        public int SinglePlayerClassIndex { get; init; }
        public int UserInfoClassIndex { get; init; } = -1;
        public int RandomClassChoice { get; init; }
    }

    public sealed class PickResult
    {
        public bool Picked { get; init; }
        public int ClassIndex { get; init; }
    }

    public static PickResult Resolve(PickContext context)
    {
        if (context.ClassAlreadySet)
            return new PickResult { Picked = false, ClassIndex = 0 };

        if (context.PlayerClassCount <= 1)
            return new PickResult { Picked = true, ClassIndex = 0 };

        if (!context.Deathmatch || !context.Multiplayer)
            return new PickResult { Picked = true, ClassIndex = context.SinglePlayerClassIndex };

        if (context.UserInfoClassIndex >= 0)
            return new PickResult { Picked = true, ClassIndex = context.UserInfoClassIndex };

        var index = context.RandomClassChoice % context.PlayerClassCount;
        return new PickResult { Picked = true, ClassIndex = index };
    }
}
