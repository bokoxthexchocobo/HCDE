namespace HCDE.Playsim;

/// <summary>Negative-damage heal branch of <c>P_ActorInSpecialSector</c> from <c>p_spec.cpp</c> (488–495).</summary>
public static class VmPSpecActorSpecialFinish
{
    public static bool NegativeDamageHealTick(int damageAmount, int levelTime, int interval) =>
        damageAmount < 0 && interval != 0 && levelTime % interval == 0;

    public static int HealAmountFromNegativeSector(int damageAmount) => -damageAmount;

    public static int HealCap() => 100;

}
