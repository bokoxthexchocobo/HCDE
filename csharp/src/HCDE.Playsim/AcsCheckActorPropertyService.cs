namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::CheckActorProperty</c> ACS comparisons.</summary>
public static class AcsCheckActorPropertyService
{
    public static bool Check(AuthoritySimulation sim, Actor? activator, int tid, int property, int value)
        => AcsActorProperties.Check(sim, activator, tid, property, value);
}
