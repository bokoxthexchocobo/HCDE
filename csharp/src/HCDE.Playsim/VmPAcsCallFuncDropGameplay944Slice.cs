namespace HCDE.Playsim;

/// <summary>
/// ACS drop/flag/quake/line/powerup/orientation callfuncs from <c>p_acs.cpp</c> (**6300–6449**); chunk **944**.
/// </summary>
public static class VmPAcsCallFuncDropGameplay944Slice
{
    public const int QuakeExMinArgCount = 8;
    public const int DropItemDefaultChance = 256;
    public const int DropItemMissingAmount = -1;

    public static bool MinArgCountMet(int argCount, int required) => argCount >= required;

    public static int DropItemAmountOrDefault(int argCount, int amountArg) =>
        argCount >= 3 ? amountArg : DropItemMissingAmount;

    public static int DropItemChanceOrDefault(int argCount, int chanceArg) =>
        argCount >= 4 ? chanceArg : DropItemDefaultChance;

    public static bool DropItemUsesActivatorTid(int tid) => tid == 0;

    public static bool DropItemClassFound(object? actorClass) => actorClass is not null;

    public static int IncrementDropCount(int current) => current + 1;

    public static bool DropInventoryTypeResolved(string? typeName) => typeName is not null;

    public static bool CheckFlagActorPresent(object? actor) => actor is not null;

    public static double AcsToDouble(int fixedValue) => fixedValue / 65536.0;

    public static int QuakeExOptionalInt(int argCount, int index, int stackValue, int defaultValue) =>
        argCount > index ? stackValue : defaultValue;

    public static double QuakeExOptionalDouble(int argCount, int index, int stackValue, double defaultValue) =>
        argCount > index ? AcsToDouble(stackValue) : defaultValue;

    public static int ResolveQuakeExRoll(int argCount, int rollArg) => QuakeExOptionalInt(argCount, 8, rollArg, 0);

    public static double ResolveQuakeExDamageMultiplier(int argCount, int arg) =>
        QuakeExOptionalDouble(argCount, 9, arg, 1.0);

    public static double ResolveQuakeExDamageThreshold(int argCount, int arg) =>
        QuakeExOptionalDouble(argCount, 10, arg, 1.0);

    public static double ResolveQuakeExDamageFactor(int argCount, int arg) =>
        QuakeExOptionalDouble(argCount, 11, arg, 1.0);

    public static int ResolveQuakeExWaveSpeed(int argCount, int arg) => QuakeExOptionalInt(argCount, 12, arg, 0);

    public static int ResolveQuakeExFalloff(int argCount, int arg) => QuakeExOptionalInt(argCount, 13, arg, 0);

    public static double ResolveQuakeExIntensity(int argCount, int arg) =>
        QuakeExOptionalDouble(argCount, 14, arg, 0);

    public static double ResolveQuakeExRadiusMultiplier(int argCount, int arg) =>
        QuakeExOptionalDouble(argCount, 15, arg, 0);

    public static double ResolveQuakeExTremorMultiplier(int argCount, int arg) =>
        QuakeExOptionalDouble(argCount, 16, arg, 1.0);

    public static double ResolveQuakeExHighFrequency(int argCount, int arg) =>
        QuakeExOptionalDouble(argCount, 17, arg, 0.5);

    public static int ResolveQuakeExFlags(int argCount, int arg) => QuakeExOptionalInt(argCount, 18, arg, 0);

    public static int SetLineActivationRepeatOrDefault(int argCount, int repeatArg) =>
        argCount > 2 ? repeatArg : -1;

    public static bool ShouldSetLineRepeat(int repeat) => repeat > 0;

    public static bool ShouldClearLineRepeat(int repeat) => repeat == 0;

    public static int GetLineActivationWhenFound(int lineIndex, int activation) =>
        lineIndex >= 0 ? activation : 0;

    public static bool PowerupClassIsActorType(object? powerupClass) => powerupClass is not null;

    public static bool PowerupInventoryPresent(object? powerup) => powerup is not null;

    public static bool ChangeActorAngleInterpolate(int argCount, int interpArg) =>
        argCount > 2 && interpArg != 0;

    public static bool ChangeActorPitchInterpolate(int argCount, int interpArg) =>
        argCount > 2 && interpArg != 0;

    public static bool ActivatorRequiredForDrop(int tid, object? activator) =>
        tid == 0 && activator is null;
}
