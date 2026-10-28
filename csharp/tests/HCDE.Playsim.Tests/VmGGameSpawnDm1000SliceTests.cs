namespace HCDE.Playsim.Tests;

public class VmGGameSpawnDm1000SliceTests
{
    [Fact]
    public void SpawnPlayerBotTail()
    {
        Assert.Equal(5, VmGGameSpawnDm1000Slice.ResolveReadyWeaponAfterDefaultInventory(
            gameState: 0,
            gsTitleLevel: 3,
            pendingWeapon: 5));
        Assert.Null(VmGGameSpawnDm1000Slice.ResolveReadyWeaponAfterDefaultInventory(
            gameState: 3,
            gsTitleLevel: 3,
            pendingWeapon: 5));
        Assert.True(VmGGameSpawnDm1000Slice.TryPreserveBotSkill(new object(), skill: 2, out var bot));
        Assert.Equal(2, bot.Skill);
    }

    [Fact]
    public void CheckSpotAndRange()
    {
        Assert.True(VmGGameSpawnDm1000Slice.IsInvalidMapthingType(0));
        Assert.Equal(5.0, VmGGameSpawnDm1000Slice.NearestLivingPlayerDistance(
            spotX: 0,
            spotY: 0,
            players: [(true, true, 1, 3.0, 4.0)]));
        Assert.True(VmGGameSpawnDm1000Slice.AnotherLivingPlayerAtSpot(
            playernum: 1,
            spotX: 1,
            spotY: 2,
            players: [(0, true, 1.0, 2.0)]));
    }

    [Fact]
    public void DeathmatchSelection()
    {
        var farthest = VmGGameSpawnDm1000Slice.SelectFarthestDeathmatchIndex(
            starts: new[] { 1.0, 9.0, 3.0 },
            distanceFromPlayers: d => d);
        Assert.Equal(1, farthest);

        var index = VmGGameSpawnDm1000Slice.SelectRandomOpenDeathmatchIndex(
            selectionCount: 4,
            randomRoll: () => 1,
            spotIsOpen: i => i == 1,
            out var last);
        Assert.Equal(1, index);

        var empty = VmGGameSpawnDm1000Slice.ResolvePickDeathmatchStart(
            selectionCount: 0,
            randomIndex: 0,
            startAngle: 90,
            startX: 1,
            startY: 2,
            startZ: 3);
        Assert.Equal(VmGGameSpawnDm1000Slice.EmptyDeathmatchAngle, empty.Angle);
        Assert.Equal(2, VmGGameSpawnDm1000Slice.CountPickDeathmatchReturns(
            provideVector: true,
            provideAngle: true));
    }
}
