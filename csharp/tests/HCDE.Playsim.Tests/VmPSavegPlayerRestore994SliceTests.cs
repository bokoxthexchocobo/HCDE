namespace HCDE.Playsim.Tests;

public class VmPSavegPlayerRestore994SliceTests
{
    [Fact]
    public void ReadMultiplePlayersAssignment()
    {
        var copied = false;
        Assert.True(VmPSavegPlayerRestore994Slice.TryAssignNamedTempPlayer(
            fromHub: false,
            tempAlreadyUsed: false,
            namesMatch: true,
            copyFullPlayer: () => copied = true,
            assignMobjOnly: () => { }));
        Assert.True(copied);

        var mobjOnly = false;
        Assert.True(VmPSavegPlayerRestore994Slice.TryAssignFirstUnusedTempPlayer(
            fromHub: true,
            tempAlreadyUsed: false,
            copyFullPlayer: () => { },
            assignMobjOnly: () => mobjOnly = true));
        Assert.True(mobjOnly);

        Assert.True(VmPSavegPlayerRestore994Slice.ShouldDestroyUnusedTempActor(tempUsed: false));
    }

    [Fact]
    public void CopyPlayerPredicates()
    {
        Assert.Equal(5, VmPSavegPlayerRestore994Slice.MergeChaseCamCheat(cheats: 1, chaseCamFlag: 4));
        var botCount = 0;
        Assert.Equal(
            VmPSavegPlayerRestore994Slice.UserinfoRestoreKind.BotFromSave,
            VmPSavegPlayerRestore994Slice.ResolveUserinfoRestoreAfterCopy(
                isBot: true,
                botMatched: true,
                ref botCount));
        Assert.Equal(1, botCount);
        Assert.True(VmPSavegPlayerRestore994Slice.TryMarkMatchingBotInUse(
            ["Alpha", "Beta"],
            savedName: "beta",
            startIndex: 0,
            out var index));
        Assert.Equal(1, index);

        var owners = new List<object?>();
        VmPSavegPlayerRestore994Slice.RelinkPlayerOwnedPsprites(
            firstPsprite: "a",
            player: "player",
            (ps, owner) => owners.Add($"{ps}:{owner}"),
            _ => null);
        Assert.Single(owners);

        var attack = false;
        var use = false;
        VmPSavegPlayerRestore994Slice.RestoreStickyInputFlags(
            ref attack,
            ref use,
            savedAttackDown: true,
            savedUseDown: true);
        Assert.True(attack && use);
    }

    [Fact]
    public void SpawnExtraPlayersGate()
    {
        Assert.True(VmPSavegPlayerRestore994Slice.ShouldSpawnExtraPlayers(
            deathmatch: false,
            isPrimaryLevel: true));
        Assert.False(VmPSavegPlayerRestore994Slice.ShouldSpawnExtraPlayers(
            deathmatch: true,
            isPrimaryLevel: true));
        Assert.True(VmPSavegPlayerRestore994Slice.ShouldSpawnMissingPlayerPawn(
            playerInGame: true,
            mobj: null));
    }
}
