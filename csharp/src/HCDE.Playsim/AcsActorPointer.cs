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
    public const int FriendPlayer = 0x4000;
    public const int LineTarget = 0x8000;
    public const int Player1 = 0x40;

    public static Actor? Resolve(AuthoritySimulation sim, Actor? origin, int selector)
    {
        if (selector == Default)
            return origin;

        if (origin is PlayerPawn player)
        {
            if ((selector & PlayerGetTarget) != 0)
                return CombatTrace.FindTarget(sim, player, HitscanCombat.HitscanRange);
            if ((selector & 0x20) != 0)
                return null;
        }

        if (origin is { Destroyed: false })
        {
            if ((selector & Target) != 0)
                return origin is ProjectileActor missile ? ActorFromId(sim, missile.Owner.Id) : ActorFromId(sim, origin.Brain?.TargetId);
            if ((selector & Master) != 0)
                return ActorFromId(sim, origin.MasterId);
            if ((selector & Tracer) != 0)
                return origin is ProjectileActor tracer ? ActorFromId(sim, tracer.TracerTargetId) : null;
            if ((selector & FriendPlayer) != 0)
                return origin.FriendPlayer > 0 ? PlayerByNum(sim, origin.FriendPlayer - 1) : null;
            if ((selector & LineTarget) != 0)
                return origin is PlayerPawn pawn
                    ? CombatTrace.FindTarget(sim, pawn, HitscanCombat.HitscanRange)
                    : null;
        }

        for (var index = 0; index < 8; index++)
        {
            if ((selector & (Player1 << index)) != 0)
                return PlayerByNum(sim, index);
        }

        if ((selector & Null) != 0)
            return null;

        return origin;
    }

    public static bool PointersEqual(Actor? left, Actor? right) =>
        left is null ? right is null : right is not null && left.Id == right.Id;

    private static Actor? PlayerByNum(AuthoritySimulation sim, int playerNum) =>
        sim.Players.FirstOrDefault(player => !player.Destroyed && player.PlayerNum == playerNum);

    private static Actor? ActorFromId(AuthoritySimulation sim, uint? id) =>
        id is { } actorId
            ? sim.Actors.FirstOrDefault(actor => actor.Id == actorId && !actor.Destroyed)
            : null;
}
