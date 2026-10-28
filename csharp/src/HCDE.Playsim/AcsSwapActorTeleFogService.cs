namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::SwapActorTeleFog</c>.</summary>
public static class AcsSwapActorTeleFogService
{
    public static int Swap(AuthoritySimulation sim, Actor? activator, int tid)
        => AcsActorTeleFog.Swap(sim, tid, activator);
}
