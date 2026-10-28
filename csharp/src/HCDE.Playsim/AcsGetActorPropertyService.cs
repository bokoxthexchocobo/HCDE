namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::GetActorProperty</c> ACS property reads.</summary>
public static class AcsGetActorPropertyService
{
    public static int Get(AuthoritySimulation sim, Actor? activator, int tid, int property)
        => AcsActorProperties.Get(sim, activator, tid, property);

    public static bool Check(AuthoritySimulation sim, Actor? activator, int tid, int property, int value)
        => AcsActorProperties.Check(sim, activator, tid, property, value);
}
