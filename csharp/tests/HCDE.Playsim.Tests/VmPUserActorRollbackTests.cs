namespace HCDE.Playsim.Tests;

public class VmPUserActorRollbackTests
{
    [Fact]
    public void PostBackup_ClearsPickupAndPushwallFlags()
    {
        var link = new VmPUserHcdeWeaponPhysics.PhysicsLinkBackup();
        var bak = new VmPUserActorRollback.ActorBackup(new object(), link);
        uint flags = VmPUserActorRollback.MfPickup;
        uint flags2 = VmPUserActorRollback.Mf2Pushwall;
        bak.PostBackup(() => flags, () => flags2, v => flags = v, v => flags2 = v);
        Assert.Equal(0u, flags);
        Assert.True(bak.SavedFlagsValid);
    }

    [Fact]
    public void RestoreWeaponBob_RestoresPlayerSlot()
    {
        var state = new VmPUserActorRollback.PredictionRollbackState();
        var bobs = new[] { new VmPUserActorRollback.PlayerBob(), new VmPUserActorRollback.PlayerBob() };
        bobs[1].X = 5;
        state.BackupWeaponBob(1, bobs);
        bobs[1].X = 0;
        state.RestoreWeaponBob(bobs);
        Assert.Equal(5, bobs[1].X);
        Assert.False(state.WeaponBobBackupValid);
    }

    [Fact]
    public void CaptureRestoreWeaponPredictState_RoundTrips()
    {
        var state = new VmPUserActorRollback.PredictionRollbackState();
        state.CaptureWeaponPredictState(3, 2, true, false, new object(), null);
        ushort ws = 0;
        state.RestoreWeaponPredictState(v => ws = v, _ => { }, _ => { }, _ => { }, _ => { }, _ => { });
        Assert.Equal((ushort)3, ws);
        Assert.False(state.WeaponPredictStateValid);
    }
}
