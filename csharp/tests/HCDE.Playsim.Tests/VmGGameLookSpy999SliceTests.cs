namespace HCDE.Playsim.Tests;

public class VmGGameLookSpy999SliceTests
{
    [Fact]
    public void BuildTiccmdTail()
    {
        Assert.Equal(256, VmGGameLookSpy999Slice.ScaleMovementAxis(1));
        Assert.True(VmGGameLookSpy999Slice.ShouldWriteWeaponSelectNet(
            sendWeaponSlot: 3,
            weaponSlotNone: VmGGameCcmd998Slice.WeaponSlotNone));
        Assert.True(VmGGameLookSpy999Slice.ShouldEmitPeriodicInputTrace(
            gametic: 70,
            lastTraceTic: 0,
            ticRate: 35));
        var line = VmGGameLookSpy999Slice.FormatInputTraceLine(1, 2, 3, 4, 0x10);
        Assert.Contains("buttons=0x00000010", line);
    }

    [Fact]
    public void LookAndViewAdjust()
    {
        var scaled = VmGGameLookSpy999Slice.AdjustLookWithWeaponScales(
            look: 10,
            isAliveWithReadyWeapon: true,
            fovScale: 2.0,
            lookScale: 0.5);
        Assert.Equal(655_360, scaled);
        Assert.Equal(
            VmGGameLookSpy999Slice.PitchClampLimit,
            VmGGameLookSpy999Slice.ApplyViewPitchDelta(
                localPitch: int.MaxValue,
                look: 10,
                freelookAllowed: true,
                titleLevel: false));
        Assert.Equal(5, VmGGameLookSpy999Slice.ApplyViewAngleDelta(10, 5, titleLevel: false));
    }

    [Fact]
    public void ChangeSpyAdvance()
    {
        Assert.True(VmGGameLookSpy999Slice.ShouldCheckTeamWhenSpying(
            demoPlayback: false,
            deathmatch: true));
        Assert.True(VmGGameLookSpy999Slice.IsSpyTargetEligible(
            playerInGame: true,
            checkTeam: true,
            isTeammate: false,
            botAllowSpy: true,
            targetIsBot: true));

        var next = VmGGameLookSpy999Slice.AdvanceSpyPlayerIndexChangeSpy(
            consolePlayer: 0,
            startPlayerIndex: 0,
            spyCommand: VmGGameHead995Slice.SpyNext,
            maxPlayers: 4,
            isEligible: p => p == 2);
        Assert.Equal(2, next);
    }
}
