namespace HCDE.Playsim;

/// <summary>Numeric HealthPickup metadata. Negative health is percentage healing in manual-only mode 0; positive modes 1/2 are emergency use and 3 is Strife use.</summary>
public readonly record struct HealthPickupStack(int Health, int Amount, int AutoUseMode, string? TypeName = null);
