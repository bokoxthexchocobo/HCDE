namespace HCDE.Playsim;

/// <summary>Native <c>COPY_AAPTREX</c> subset for ACS pointer selectors.</summary>
internal static class AcsActorPointer
{
    public const int Default = 0;
    public const int Null = 0x1;
    public const int Target = 0x2;
    public const int Master = 0x4;
    public const int Tracer = 0x8;
    public const int PlayerGetTarget = 0x10;
    public const int PlayerGetConversation = 0x20;
    public const int FriendPlayer = 0x4000;
    public const int LineTarget = 0x8000;
    public const int Player1 = 0x40;

    public static Actor? Resolve(AuthoritySimulation sim, Actor? origin, int selector)
    {
        if (selector == Default)
            return origin;

        if (origin is PlayerPawn player)
        {
            switch (selector & (PlayerGetTarget | PlayerGetConversation))
            {
                case PlayerGetTarget:
                    return CombatTrace.FindTarget(sim, player, HitscanCombat.HitscanRange);
                case PlayerGetConversation:
                    return null;
            }
        }

        if (origin is { Destroyed: false })
        {
            switch (selector & (Target | Master | Tracer | FriendPlayer | LineTarget))
            {
                case Target: return ActorFromId(sim, origin.TargetId);
                case Master: return ActorFromId(sim, origin.MasterId);
                case Tracer: return ActorFromId(sim, origin.TracerId);
                case FriendPlayer:
                    return origin.FriendPlayer > 0 ? PlayerByNum(sim, origin.FriendPlayer - 1) : null;
                case LineTarget:
                    return CombatTrace.FindTarget(sim, origin, HitscanCombat.HitscanRange);
            }
        }

        var staticSelector = selector & (0x3fc0 | Null);
        for (var index = 0; index < 8; index++)
        {
            if (staticSelector == (Player1 << index))
                return PlayerByNum(sim, index);
        }
        if (staticSelector == Null) return null;

        return origin;
    }

    public static bool PointersEqual(Actor? left, Actor? right) =>
        ReferenceEquals(left, right);

    private static Actor? PlayerByNum(AuthoritySimulation sim, int playerNum) =>
        sim.Players.FirstOrDefault(player => !player.Destroyed && player.PlayerNum == playerNum);

    private static Actor? ActorFromId(AuthoritySimulation sim, uint? id) =>
        id is { } actorId
            ? sim.Actors.FirstOrDefault(actor => actor.Id == actorId && !actor.Destroyed)
            : null;
}
