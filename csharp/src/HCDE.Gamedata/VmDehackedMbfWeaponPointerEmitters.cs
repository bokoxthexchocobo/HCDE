namespace HCDE.Gamedata;

/// <summary>MBF weapon/monster emitter factories from <c>d_dehacked.cpp</c> (872–1110).</summary>
public static class VmDehackedMbfWeaponPointerEmitters
{
    public static void ValidateArgCount(int actual, int expected, string name)
    {
        if (actual != expected)
            throw new ArgumentException($"{name} expects {expected} args, got {actual}.");
    }

    public static (int special, int tag) LineEffectArgs(int misc1, int misc2) => (misc1, misc2);

    public static (int damage, int hitSound, int missSound, bool bleed) ScratchMeleeArgs(int misc1, int misc2) =>
        (misc1, misc2 - 1, 0, true);

    public static (int sound, float attenuation) PlaySoundArgs(int misc1, int misc2) =>
        (misc1 - 1, misc2 != 0 ? 0f : 1f);

    public static (int maxChance, int jumpLabel) RandomJumpArgs(int misc1, int misc2) => (misc2, misc1);

    public static (int a, float b, float c, float d, float e) MonsterProjectileArgs(
        float a, float b, float c, float d, float e) => ((int)a, b, c, d, e);

    public static (float a, float b, int spread, int numBullets, int damagePerBullet) MonsterBulletAttackArgs(
        float a, float b, int spread, int numBullets, int damagePerBullet) =>
        (a, b, spread == 0 ? 1 : spread, numBullets == 0 ? 5 : numBullets, damagePerBullet == 0 ? 3 : damagePerBullet);

    public static (int damage, int sound, float range) MonsterMeleeAttackArgs(int damage, int sound, float range) =>
        (damage == 0 ? 3 : damage, sound, range);

    public static (int damage, int distance) RadiusDamageArgs(int damage, int distance) => (damage, distance);

    public static (int state, int sound) HealChaseArgs(int state, int sound) => (state, sound);

    public static (float angle, float range) SeekTracerArgs(float angle, float range) => (angle, range);

    public static (float angle, int range) FindTracerArgs(float angle, int range) => (angle, range == 0 ? 10 : range);

    public static (int state, int health) JumpIfHealthBelowArgs(int state, int health) => (state, health);

    public static (int state, float chance) JumpIfArgs(int state, float chance) => (state, chance);

    public static (int state, int flags, int mask) JumpIfFlagSetArgs(int state, int flags, int mask) => (state, flags, mask);

    public static (int flags, int mask) FlagSetArgs(int flags, int mask) => (flags, mask);

    public static (int type, float a, float b, float c, float d, float e) WeaponProjectileArgs(
        int type, float a, float b, float c, float d, float e) => (type, a, b, c, d, e);

    public static (float spread, int numBullets, int damagePerBullet) WeaponBulletAttackArgs(
        float spread, int numBullets, int damagePerBullet) =>
        (spread, numBullets == 0 ? 5 : numBullets, damagePerBullet == 0 ? 3 : damagePerBullet);

    public static (int damage, int attackSound, float meleeRange, int soundIndex, float factor) WeaponMeleeAttackArgs(
        int damage, int attackSound, float meleeRange, int soundIndex, float factor) =>
        (damage == 0 ? 2 : damage, attackSound, meleeRange, soundIndex, factor == 0 ? 1f : factor);

    public static (int soundIndex, int flags) WeaponSoundArgs(int soundIndex, int flags) => (soundIndex, flags);

    public static (int state, int chance) WeaponJumpArgs(int state, int chance) => (state, chance);

    public static int ConsumeAmmoArg(int amount) => amount;

    public static bool SetDehParamsNative(int codepointer, int factoryCount, int aliasCount, out bool returnsState)
    {
        returnsState = codepointer == 6;
        return codepointer >= 0 && codepointer < aliasCount && codepointer < factoryCount;
    }

    public static void CreateSpawnObjectNative(int argCount) =>
        ValidateArgCount(argCount, 8, "A_SpawnObject");

    public static void CreateWeaponProjectileNative(int argCount) =>
        ValidateArgCount(argCount, 5, "A_WeaponProjectile");

    public static void CreateNoArgNative(int argCount) =>
        ValidateArgCount(argCount, 0, "MBF21 no-argument codepointer");
}
