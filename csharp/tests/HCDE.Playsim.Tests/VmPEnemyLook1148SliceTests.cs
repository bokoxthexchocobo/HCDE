namespace HCDE.Playsim.Tests;

public class VmPEnemyLook1148SliceTests
{
    [Fact]
    public void IsVisible_RejectsDistanceAndFovAfterTheBlockFlag()
    {
        Assert.False(Visible(otherExists: false));
        Assert.False(Visible(otherFlags8: VmPEnemyLook1148Slice.MonsterVisBlocked));
        Assert.False(Visible(hasParams: true, paramMaxDist: 100, distance: 101));
        Assert.False(Visible(hasParams: true, paramMinDist: 64, distance: 63));
        Assert.True(Visible(hasParams: true, paramFov: 90, allAround: true, angleDelta: 170));
        Assert.False(Visible(angleDelta: 91, distance: 300, meleeRange: 64, radius: 16));
        Assert.True(Visible(angleDelta: 90, distance: 300));
        Assert.True(Visible(angleDelta: 170, distance: 70, meleeRange: 64, radius: 16));
        Assert.False(Visible(hasParams: true, paramMinDist: 1, angleDelta: 170, distance: 20, meleeRange: 64, radius: 16));
        Assert.False(Visible(hasSight: false));
        Assert.Equal(2, VmPEnemyLook1148Slice.SeePastShootableLines);
        Assert.Equal(1280, VmPEnemyLook1148Slice.MonsLookRange);
        Assert.Equal(64, VmPEnemyLook1148Slice.MonsLookLimit);
    }

    [Fact]
    public void IsTargetablePlayer_MissesSneakingShadowsUnlessTheRollClears()
    {
        Assert.False(Player(pawnExists: false));
        Assert.False(Player(pawnFlags: 0));
        Assert.False(Player(isFriend: true));
        Assert.False(Player(cheats: VmPEnemyLook1148Slice.NoTarget));
        Assert.False(Player(playerHealth: 0));
        Assert.False(Player(visible: false));
        Assert.False(Player(pawnFlags: VmPEnemyLook1148Slice.Shootable | VmPEnemyLook1148Slice.Shadow, distance: 129, speedSquared: 24, lookRoll: 255));
        Assert.False(Player(pawnFlags: VmPEnemyLook1148Slice.Shootable | VmPEnemyLook1148Slice.Shadow, distance: 129, speedSquared: 25, lookRoll: 224));
        Assert.True(Player(pawnFlags: VmPEnemyLook1148Slice.Shootable | VmPEnemyLook1148Slice.Shadow, distance: 129, speedSquared: 25, lookRoll: 225));
        Assert.True(Player(pawnFlags: VmPEnemyLook1148Slice.Shootable | VmPEnemyLook1148Slice.Shadow, distance: 128, speedSquared: 0, lookRoll: 225));
        Assert.True(Player(
            pawnFlags: VmPEnemyLook1148Slice.Shootable | VmPEnemyLook1148Slice.Shadow,
            lookerFlags6: VmPEnemyLook1148Slice.SeeInvisible,
            distance: 400,
            speedSquared: 0,
            lookRoll: 0));
        Assert.False(Player(pawnFlags: VmPEnemyLook1148Slice.Shootable, pawnFlags3: VmPEnemyLook1148Slice.Ghost, distance: 200, speedSquared: 0, lookRoll: 0));
        Assert.True(Player(
            pawnFlags: VmPEnemyLook1148Slice.Shootable | VmPEnemyLook1148Slice.Shadow,
            compatFlags: VmPEnemyLook1148Slice.CompatInvisibility,
            distance: 400,
            speedSquared: 0,
            lookRoll: 0));
    }

    [Fact]
    public void ValidEnemy_KeepsHostileTargetsAndDoesNotAdoptAFriendsTarget()
    {
        int monster = VmPEnemyLook1148Slice.Shootable | VmPEnemyLook1148Slice.IsMonster;
        Assert.False(Enemy(otherFlags3: 0));
        Assert.False(Enemy(isSelf: true));
        Assert.False(Enemy(otherFlags7: VmPEnemyLook1148Slice.NeverTarget));
        Assert.True(Enemy(lookerFlags: VmPEnemyLook1148Slice.Friendly, otherFlags: monster));
        Assert.False(Enemy(
            lookerFlags: VmPEnemyLook1148Slice.Friendly,
            otherFlags: monster | VmPEnemyLook1148Slice.Friendly,
            otherIsFriend: true,
            otherHasTarget: true,
            targetFlags: VmPEnemyLook1148Slice.Shootable,
            targetHealth: 100,
            visible: true));
        Assert.True(Enemy(
            lookerFlags: VmPEnemyLook1148Slice.Friendly,
            otherFlags: monster | VmPEnemyLook1148Slice.Friendly,
            otherIsFriend: false));
        Assert.True(Enemy(
            lookerFlags8: VmPEnemyLook1148Slice.SeeFriendlyMonsters,
            otherFlags: monster | VmPEnemyLook1148Slice.Friendly));
        Assert.False(Enemy(lookerFlags8: VmPEnemyLook1148Slice.SeeFriendlyMonsters, otherFlags: monster));
        Assert.False(Enemy(
            lookerFlags: VmPEnemyLook1148Slice.Friendly,
            otherHasTarget: true,
            targetTargetsOther: true,
            lookerIsFriendOfTarget: true,
            targetHealth: 50,
            targetSpawnHealth: 100,
            skipRoll: 101));
        Assert.True(Enemy(
            lookerFlags: VmPEnemyLook1148Slice.Friendly,
            otherHasTarget: true,
            targetTargetsOther: true,
            lookerIsFriendOfTarget: true,
            targetHealth: 50,
            targetSpawnHealth: 100,
            skipRoll: 100));
    }

    [Fact]
    public void LookForEnemies_UsesSeeBlocksAndSkipsPlayersWhenAsked()
    {
        var aborted = VmPEnemyLook1148Slice.LookForEnemies(false, 32, 4, false, 2, Array.Empty<VmPEnemyLook1148Slice.LookCandidate>());
        Assert.True(aborted.Aborted);
        Assert.Equal(0, aborted.Accepted);

        var scan = VmPEnemyLook1148Slice.LookForEnemies(true, -1, 4, true, 2, new[]
        {
            new VmPEnemyLook1148Slice.LookCandidate(false, true, false),
            new VmPEnemyLook1148Slice.LookCandidate(true, false, true),
            new VmPEnemyLook1148Slice.LookCandidate(false, false, false),
        });
        Assert.False(scan.Aborted);
        Assert.Equal(512d, scan.Range);
        Assert.Equal(3, scan.Accepted);

        scan = VmPEnemyLook1148Slice.LookForEnemies(true, 40, 4, false, 0, new[]
        {
            new VmPEnemyLook1148Slice.LookCandidate(true, true, true),
        });
        Assert.Equal(40d, scan.Range);
        Assert.Equal(1, scan.Accepted);
    }

    private static bool Visible(
        bool otherExists = true,
        int otherFlags8 = 0,
        bool hasParams = false,
        double paramMaxDist = 0,
        double paramMinDist = 0,
        double paramFov = 180,
        bool allAround = false,
        double distance = 100,
        double angleDelta = 0,
        double meleeRange = 64,
        double radius = 32,
        bool hasSight = true)
    {
        return VmPEnemyLook1148Slice.IsVisible(
            otherExists, otherFlags8, hasParams, paramMaxDist, paramMinDist, paramFov, allAround,
            distance, angleDelta, meleeRange, radius, hasSight);
    }

    private static bool Player(
        bool pawnExists = true,
        int pawnFlags = VmPEnemyLook1148Slice.Shootable,
        int pawnFlags3 = 0,
        bool isFriend = false,
        int cheats = 0,
        int playerHealth = 100,
        bool visible = true,
        int lookerFlags6 = 0,
        int compatFlags = 0,
        double distance = 64,
        double speedSquared = 100,
        int lookRoll = 255)
    {
        return VmPEnemyLook1148Slice.IsTargetablePlayer(
            pawnExists, pawnFlags, pawnFlags3, isFriend, cheats, playerHealth, visible,
            lookerFlags6, compatFlags, distance, speedSquared, lookRoll);
    }

    private static bool Enemy(
        int lookerFlags = 0,
        int lookerFlags8 = 0,
        int otherFlags = VmPEnemyLook1148Slice.Shootable,
        int otherFlags2 = 0,
        int otherFlags3 = VmPEnemyLook1148Slice.IsMonster,
        int otherFlags7 = 0,
        int otherHealth = 100,
        bool isSelf = false,
        bool otherIsFriend = false,
        bool otherHasTarget = false,
        int targetFlags = 0,
        int targetFlags2 = 0,
        int targetHealth = 0,
        double targetSpawnHealth = 100,
        bool targetTargetsOther = false,
        bool lookerIsFriendOfTarget = false,
        int skipRoll = 0,
        bool visible = true)
    {
        return VmPEnemyLook1148Slice.ValidEnemy(
            lookerFlags, lookerFlags8, otherFlags, otherFlags2, otherFlags3, otherFlags7, otherHealth, isSelf,
            otherIsFriend, otherHasTarget, targetFlags, targetFlags2, targetHealth, targetSpawnHealth,
            targetTargetsOther, lookerIsFriendOfTarget, skipRoll, visible);
    }
}
