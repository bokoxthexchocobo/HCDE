namespace HCDE.Playsim.Tests;

public class VmPInteractionDamageMobjTests
{
    [Fact]
    public void DamageMobj_DormantTarget_ReturnsCancel()
    {
        var target = new VmPInteractionDamageMobj.DamageActor
        {
            Health = 50,
            Flags = VmPInteractionDamageMobj.MfShootable,
            Flags2 = VmPInteractionDamageMobj.Mf2Dormant, // distinct from MF2_INVULNERABLE test bit
        };
        var need = true;
        var ret = VmPInteractionDamageMobj.DamageMobj(new() { Target = target, Mod = 1 }, 10, ref need);
        Assert.Equal(-1, ret);
    }

    [Fact]
    public void DamageMobj_PlayerInvuln_ReturnsZero()
    {
        var target = new VmPInteractionDamageMobj.DamageActor
        {
            Health = 50,
            PlayerHealth = 50,
            HasPlayer = true,
            Flags = VmPInteractionDamageMobj.MfShootable,
            Flags2 = VmPInteractionDamageMobj.Mf2Invulnerable,
        };
        var need = true;
        var ret = VmPInteractionDamageMobj.DamageMobj(new() { Target = target, Mod = 1 }, 20, ref need);
        Assert.Equal(0, ret);
        Assert.Equal(50, target.PlayerHealth);
    }

    [Fact]
    public void DamageMobj_MonsterBuddha_SavesAtOne()
    {
        var target = new VmPInteractionDamageMobj.DamageActor
        {
            Health = 10,
            Flags = VmPInteractionDamageMobj.MfShootable,
            Flags7 = VmPInteractionDamageMobj.Mf7Buddha,
        };
        var need = true;
        var ret = VmPInteractionDamageMobj.DamageMobj(new() { Target = target, Mod = 1 }, 50, ref need);
        Assert.Equal(50, ret);
        Assert.Equal(1, target.Health);
        Assert.False(target.Died);
    }

    [Fact]
    public void DamageMobj_Death_CallsDie()
    {
        var died = false;
        var target = new VmPInteractionDamageMobj.DamageActor
        {
            Health = 10,
            Flags = VmPInteractionDamageMobj.MfShootable,
        };
        var need = true;
        var ret = VmPInteractionDamageMobj.DamageMobj(new()
        {
            Target = target,
            Mod = 2,
            CallDie = (_, _, _, _) => died = true,
        }, 50, ref need);
        Assert.Equal(50, ret);
        Assert.True(died);
        Assert.False(need);
    }

    [Fact]
    public void DamageMobj_Reflection_AppliesToSource()
    {
        var reflected = 0;
        var target = new VmPInteractionDamageMobj.DamageActor
        {
            Health = 100,
            PlayerHealth = 100,
            HasPlayer = true,
            Flags = VmPInteractionDamageMobj.MfShootable,
        };
        var need = true;
        VmPInteractionDamageMobj.DamageMobj(new()
        {
            Target = target,
            Mod = 1,
            SourceNull = false,
            SameTargetSource = false,
            PickReflection = () => (25, false),
            ReflectDamage = (d, _) => reflected = d,
            AbsorbDamage = d => d,
        }, 10, ref need);
        Assert.Equal(25, reflected);
    }

    [Fact]
    public void ScaleSkillDamage_HalvesPlayerDamage()
    {
        Assert.Equal(50, VmPInteractionDamageMobj.ScaleSkillDamage(true, false, 100, 0.5, 1, 1));
    }
}
