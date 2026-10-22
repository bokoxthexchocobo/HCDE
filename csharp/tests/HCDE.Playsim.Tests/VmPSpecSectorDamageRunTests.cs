namespace HCDE.Playsim.Tests;

public class VmPSpecSectorDamageRunTests
{
    [Fact]
    public void DoSectorDamage_SkipsNonShootable()
    {
        var hit = false;
        VmPSpecSectorDamageRun.DoSectorDamage(
            new VmPSpecSectorDamageRun.Actor { Flags = 0 },
            new VmPSpecSectorDamageRun.Sector(),
            5,
            null,
            new VmPSpecSectorDamageRun.DamageContext { Damage = (_, _, _, _) => hit = true },
            0);
        Assert.False(hit);
    }

    [Fact]
    public void GiveSecret_IncrementsFound()
    {
        var found = 0;
        VmPSpecSectorDamageRun.GiveSecret(new object(), true, false, 1, new VmPSpecSectorDamageRun.GiveSecretContext
        {
            ShowSecretMessage = false,
            LocalView = _ => false,
            IncrementFoundSecrets = () => found++,
        });
        Assert.Equal(1, found);
    }
}
