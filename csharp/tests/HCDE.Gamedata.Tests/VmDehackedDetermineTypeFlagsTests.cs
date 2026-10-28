namespace HCDE.Gamedata.Tests;

public class VmDehackedDetermineTypeFlagsTests
{
    [Fact]
    public void DetermineTypeBinarySearch_FindsMatch()
    {
        var mappings = new List<ReadOnlyMemory<byte>>
        {
            new byte[] { (byte)'A', (byte)'B', (byte)'C', (byte)'D' },
            new byte[] { (byte)'P', (byte)'L', (byte)'A', (byte)'Y' },
        };
        var sprite = new byte[] { (byte)'P', (byte)'L', (byte)'A', (byte)'Y' };
        Assert.Equal(1, VmDehackedDetermineTypeFlags.DetermineTypeBinarySearch(0, mappings.Count - 1, sprite, mappings));
    }

    [Fact]
    public void SetNoSector_SetsFlag() =>
        Assert.NotEqual(0UL, VmDehackedDetermineTypeFlags.SetNoSector(0));

    [Fact]
    public void ClearNoSector_ClearsFlag()
    {
        var flags = VmDehackedDetermineTypeFlags.SetNoSector(0);
        Assert.Equal(0UL, VmDehackedDetermineTypeFlags.ClearNoSector(flags));
    }

    [Fact]
    public void SetNoBlockmap_SetsFlag() =>
        Assert.NotEqual(0UL, VmDehackedDetermineTypeFlags.SetNoBlockmap(0));

    [Fact]
    public void ClearNoBlockmap_ClearsFlag()
    {
        var flags = VmDehackedDetermineTypeFlags.SetNoBlockmap(0);
        Assert.Equal(0UL, VmDehackedDetermineTypeFlags.ClearNoBlockmap(flags));
    }

    [Fact]
    public void SetCountkill_AdjustsTotal()
    {
        var (flags, total) = VmDehackedDetermineTypeFlags.SetCountkill(true, 10, 5, 0);
        Assert.Equal(5, total);
        Assert.NotEqual(0UL, flags & VmDehackedDetermineTypeFlags.MfCountkill);
    }

    [Fact]
    public void ClearCountkill_AdjustsTotal()
    {
        var (flags, total) = VmDehackedDetermineTypeFlags.ClearCountkill(true, 10, 5, VmDehackedDetermineTypeFlags.MfCountkill);
        Assert.Equal(5, total);
        Assert.Equal(0UL, flags & VmDehackedDetermineTypeFlags.MfCountkill);
    }

    [Fact]
    public void SetBoss_SetsFlags()
    {
        var (f2, f3) = VmDehackedDetermineTypeFlags.SetBoss(0, 0);
        Assert.NotEqual(0UL, f2 & VmDehackedDetermineTypeFlags.Mf2Boss);
        Assert.NotEqual(0UL, f3 & VmDehackedDetermineTypeFlags.Mf3Noradiusdmg);
    }

    [Fact]
    public void ClearBoss_ClearsFlags()
    {
        var (f2, f3) = VmDehackedDetermineTypeFlags.ClearBoss(
            VmDehackedDetermineTypeFlags.Mf2Boss,
            VmDehackedDetermineTypeFlags.Mf3Noradiusdmg);
        Assert.Equal(0UL, f2 & VmDehackedDetermineTypeFlags.Mf2Boss);
        Assert.Equal(0UL, f3 & VmDehackedDetermineTypeFlags.Mf3Noradiusdmg);
    }

    [Fact]
    public void SetCountitem_IncrementsOnce()
    {
        var (flags, total) = VmDehackedDetermineTypeFlags.SetCountitem(false, 3, 0);
        Assert.Equal(4, total);
        Assert.NotEqual(0UL, flags & VmDehackedDetermineTypeFlags.MfCountitem);
    }

    [Fact]
    public void ClearCountitem_DecrementsOnce()
    {
        var (flags, total) = VmDehackedDetermineTypeFlags.ClearCountitem(true, 3, VmDehackedDetermineTypeFlags.MfCountitem);
        Assert.Equal(2, total);
        Assert.Equal(0UL, flags & VmDehackedDetermineTypeFlags.MfCountitem);
    }

    [Fact]
    public void SetFriendly_AdjustsTotal()
    {
        var (_, _, total) = VmDehackedDetermineTypeFlags.SetFriendly(true, 1, 2, 0, 0);
        Assert.Equal(2, total);
    }

    [Fact]
    public void ClearFriendly_AdjustsTotal()
    {
        var (_, _, total) = VmDehackedDetermineTypeFlags.ClearFriendly(true, 1, 2, VmDehackedDetermineTypeFlags.MfFriendly, VmDehackedDetermineTypeFlags.Mf3Noblockmonst);
        Assert.Equal(2, total);
    }

    [Fact]
    public void SetFullVol_SetsFlags()
    {
        var (f8, f3) = VmDehackedDetermineTypeFlags.SetFullVol(0, 0);
        Assert.NotEqual(0UL, f8);
        Assert.NotEqual(0UL, f3);
    }

    [Fact]
    public void ClearFullVol_ClearsFlags()
    {
        var (f8, f3) = VmDehackedDetermineTypeFlags.ClearFullVol(
            VmDehackedDetermineTypeFlags.Mf8Fullvolsee,
            VmDehackedDetermineTypeFlags.Mf3Fullvoldeath);
        Assert.Equal(0UL, f8);
        Assert.Equal(0UL, f3);
    }

    [Fact]
    public void SetTranslucent_UsesHalfAlpha()
    {
        var (alpha, _) = VmDehackedDetermineTypeFlags.SetTranslucent(VmDehackedDetermineTypeFlags.RfZdoomtrans);
        Assert.Equal(0.5, alpha);
    }

    [Fact]
    public void ClearTranslucent_ResetsAlpha()
    {
        var (alpha, _) = VmDehackedDetermineTypeFlags.ClearTranslucent(0);
        Assert.Equal(1.0, alpha);
    }

    [Fact]
    public void CheckTranslucent_IgnoresZdoomTrans() =>
        Assert.False(VmDehackedDetermineTypeFlags.CheckTranslucent(VmDehackedDetermineTypeFlags.RfZdoomtrans, 0.5));

    [Fact]
    public void SetTranslation1_PromotesToT3() =>
        Assert.Equal(2, VmDehackedDetermineTypeFlags.SetTranslation1(1, 0, 1, 2));

    [Fact]
    public void ClearTranslation1_DemotesThroughT2() =>
        Assert.Equal(1, VmDehackedDetermineTypeFlags.ClearTranslation1(2, 1, 2, -1));

    [Fact]
    public void CheckTranslation1_MatchesNative() =>
        Assert.True(VmDehackedDetermineTypeFlags.CheckTranslation1(0, 0, 2));

    [Fact]
    public void SetTranslation2_PromotesToT3() =>
        Assert.Equal(2, VmDehackedDetermineTypeFlags.SetTranslation2(0, 0, 1, 2));

    [Fact]
    public void ClearTranslation2_DemotesThroughT1() =>
        Assert.Equal(0, VmDehackedDetermineTypeFlags.ClearTranslation2(2, 0, 2, -1));

    [Fact]
    public void CheckTranslation2_MatchesNative() =>
        Assert.True(VmDehackedDetermineTypeFlags.CheckTranslation2(1, 1, 2));

    [Fact]
    public void SetBounces_MissileUsesClassic()
    {
        var (_, _, _, effects, bounce) = VmDehackedDetermineTypeFlags.SetBounces(0, 0, 0, VmDehackedDetermineTypeFlags.FxRocket, true);
        Assert.Equal(0U, effects);
        Assert.Equal(VmDehackedDetermineTypeFlags.BounceClassicDeh, bounce);
    }

    [Fact]
    public void ClearBounces_ZerosBounceFlags()
    {
        var (_, _, _, bounce) = VmDehackedDetermineTypeFlags.ClearBounces(
            VmDehackedDetermineTypeFlags.Mf6Vulnerable,
            VmDehackedDetermineTypeFlags.Mf3Noblockmonst,
            VmDehackedDetermineTypeFlags.Mf4Forceradiusdmg);
        Assert.Equal(0U, bounce);
    }

    [Fact]
    public void SetMissile_UpgradesDehBounce()
    {
        var (_, _, bounce) = VmDehackedDetermineTypeFlags.SetMissile(0, 0, VmDehackedDetermineTypeFlags.BounceDeh);
        Assert.Equal(VmDehackedDetermineTypeFlags.BounceClassicDeh, bounce);
    }

    [Fact]
    public void ClearMissile_ClearsMissileBits()
    {
        var (flags, flags2) = VmDehackedDetermineTypeFlags.ClearMissile(
            VmDehackedDetermineTypeFlags.MfMissile,
            VmDehackedDetermineTypeFlags.Mf2Noteleport);
        Assert.Equal(0UL, flags & VmDehackedDetermineTypeFlags.MfMissile);
        Assert.Equal(0UL, flags2 & VmDehackedDetermineTypeFlags.Mf2Noteleport);
    }
}
