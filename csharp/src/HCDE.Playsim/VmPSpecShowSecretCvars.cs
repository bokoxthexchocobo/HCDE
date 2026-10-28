namespace HCDE.Playsim;

/// <summary>showsecretsector cvar gate from <c>p_spec.cpp</c> (593–594) — sole surviving band on chunk 442.</summary>
public static class VmPSpecShowSecretCvars
{
    public static bool PrintSecretSector(bool showSector, int sectorNum) => showSector && sectorNum >= 0;
}
