namespace HCDE.Playsim.Tests;

public class VmPMapTeleportMoveTests
{
    [Fact]
    public void TeleportMove_FailsWhenSolidActorBlocks()
    {
        var mover = new VmPMapTeleportMove.MapActor
        {
            Flags = VmPMapTeleportMove.MfShootable,
            Radius = 16,
            Height = 41,
            Level = new VmPMapTeleportMove.MapLevel(),
        };
        var blocker = new VmPMapTeleportMove.MapActor
        {
            Flags = VmPMapTeleportMove.MfShootable,
            X = 0,
            Y = 0,
            Z = 0,
            Radius = 16,
            Height = 41,
        };
        var ctx = new VmPMapTeleportMove.TeleportContext
        {
            Thing = mover,
            GetFloorCeilingZ = (tmf, _) => { tmf.FloorZ = 0; tmf.CeilingZ = 128; },
            IterateThingsAtDest = _ => new[] { blocker },
            CanCollideWith = (_, _) => true,
        };
        Assert.False(VmPMapTeleportMove.TeleportMove(ctx, 0, 0, 0, false, true));
    }

    [Fact]
    public void TeleportMove_TelefragsWhenFlagged()
    {
        var mover = new VmPMapTeleportMove.MapActor
        {
            Flags2 = VmPMapTeleportMove.Mf2Telestomp,
            Radius = 16,
            Height = 41,
            Level = new VmPMapTeleportMove.MapLevel(),
        };
        var victim = new VmPMapTeleportMove.MapActor
        {
            Flags = VmPMapTeleportMove.MfShootable,
            X = 0,
            Y = 0,
            Z = 0,
            Radius = 16,
            Height = 41,
        };
        var damaged = false;
        var ctx = new VmPMapTeleportMove.TeleportContext
        {
            Thing = mover,
            GetFloorCeilingZ = (tmf, _) => tmf.FloorZ = 0,
            IterateThingsAtDest = _ => new[] { victim },
            CanCollideWith = (_, _) => true,
            DamageMobj = (_, _, _, _, _, _) => damaged = true,
        };
        Assert.True(VmPMapTeleportMove.TeleportMove(ctx, 0, 0, 0, true, true));
        Assert.True(damaged);
    }

    [Fact]
    public void PlayerStartStomp_DamagesMonsterUnderfoot()
    {
        var player = new VmPMapTeleportMove.MapActor
        {
            Flags = VmPMapTeleportMove.MfShootable,
            Z = 0,
            Height = 41,
            Radius = 16,
            Player = new VmPMapTeleportMove.PlayerState(),
        };
        var monster = new VmPMapTeleportMove.MapActor
        {
            Flags = VmPMapTeleportMove.MfShootable,
            Flags3 = VmPMapTeleportMove.Mf3Ismonster,
            X = 0,
            Y = 0,
            Z = 0,
            Height = 41,
            Radius = 16,
        };
        var hit = false;
        VmPMapTeleportMove.PlayerStartStomp(
            player,
            false,
            _ => new[] { monster },
            (_, _, _, _, _, _) => hit = true);
        Assert.True(hit);
    }
}
