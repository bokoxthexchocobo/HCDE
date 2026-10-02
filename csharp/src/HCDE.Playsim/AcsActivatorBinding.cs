namespace HCDE.Playsim;

/// <summary>Mutable ACS script activator passed through <see cref="AcsCallFunctions.TryInvoke"/>.</summary>
public sealed class AcsActivatorBinding
{
    public Actor? Value { get; set; }
}
