namespace HCDE.Playsim;

/// <summary>Tail of <c>P_ActorInSpecialSector</c> from <c>p_spec.cpp</c> (478–501).</summary>
public static class VmPSpecActorSpecialFinish
{
    public static bool NegativeDamageHealTick(int damageAmount, int levelTime, int interval) =>
        damageAmount < 0 && interval != 0 && levelTime % interval == 0;

    public static int HealAmountFromNegativeSector(int damageAmount) => -damageAmount;

    public static int HealCap() => 100;

    public static bool PlayerTouchesSecret(bool hasPlayer, bool sectorIsSecret) =>
        hasPlayer && sectorIsSecret;

    public static bool ClearSecretOnDiscover() => true;

    public static bool GiveSecretWithSectorIndex() => true;
}
