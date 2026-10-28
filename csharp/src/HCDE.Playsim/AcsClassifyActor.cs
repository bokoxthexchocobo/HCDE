namespace HCDE.Playsim;

/// <summary>Native ACS <c>ClassifyActor</c> / <c>DoClassifyActor</c> bit flags.</summary>
internal static class AcsClassifyActor
{
    public const int None = 0;
    public const int World = 0x00000001;
    public const int Player = 0x00000002;
    public const int Bot = 0x00000004;
    public const int VoodooDoll = 0x00000008;
    public const int Monster = 0x00000010;
    public const int Alive = 0x00000020;
    public const int Dead = 0x00000040;
    public const int Missile = 0x00000080;
    public const int Generic = 0x00000100;

    public static int Classify(AuthoritySimulation sim, Actor? activator, int tid)
    {
        Actor? actor;
        if (tid == 0)
        {
            actor = activator is { Destroyed: false } ? activator : null;
            if (actor is null) return World;
        }
        else
        {
            actor = AcsActorTid.SingleFromTid(sim, tid);
            if (actor is null || actor.Destroyed) return None;
        }

        var classify = 0;
        if (actor is PlayerPawn player)
        {
            classify |= Player;
            if (player.IsDead) classify |= Dead;
            else classify |= Alive;
            if (sim.IsPlayerBot(player.PlayerNum)) classify |= Bot;
        }
        else if (actor.IsMonster)
        {
            classify |= Monster;
            if (actor.IsDead) classify |= Dead;
            else classify |= Alive;
        }
        else if (actor is ProjectileActor) classify |= Missile;
        else classify |= Generic;
        return classify;
    }
}
