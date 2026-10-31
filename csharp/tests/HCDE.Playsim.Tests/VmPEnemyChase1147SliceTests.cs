namespace HCDE.Playsim.Tests;

public class VmPEnemyChase1147SliceTests
{
    [Fact]
    public void RandomChaseDir_TurnsAwayFromTheReverseAndKeepsAClearHeading()
    {
        var tried = new List<int>();
        var step = Chase(0, 0, _ => false, new[] { 100 }, 0, walk: dir =>
        {
            tried.Add(dir);
            return true;
        });
        Assert.Equal(new[] { VmPEnemyChase1147Slice.East }, tried);
        Assert.Equal(VmPEnemyChase1147Slice.East, step.MoveDir);
        Assert.Null(step.MoveCount);

        tried.Clear();
        step = Chase(VmPEnemyChase1147Slice.East, 0, dir => dir == VmPEnemyChase1147Slice.SouthWest, new[] { 200, 0 }, 0, walk: dir =>
        {
            tried.Add(dir);
            return dir == VmPEnemyChase1147Slice.SouthWest;
        });
        Assert.Equal(new[] { 1, 2, 3, 5 }, tried);
        Assert.Equal(VmPEnemyChase1147Slice.SouthWest, step.MoveDir);
        Assert.Null(step.MoveCount);

        tried.Clear();
        step = Chase(VmPEnemyChase1147Slice.East, 0, dir => dir == VmPEnemyChase1147Slice.West, new[] { 200, 0, 10 }, 0, walk: dir =>
        {
            tried.Add(dir);
            return dir == VmPEnemyChase1147Slice.West;
        });
        Assert.Equal(VmPEnemyChase1147Slice.West, tried[^1]);
        Assert.Equal(VmPEnemyChase1147Slice.West, step.MoveDir);
        Assert.Equal(10, step.MoveCount);
        Assert.DoesNotContain(VmPEnemyChase1147Slice.West, tried.Take(tried.Count - 1));
    }

    [Fact]
    public void RandomChaseDir_SteersAFriendlyTowardThePlayer()
    {
        var step = Chase(
            VmPEnemyChase1147Slice.East,
            VmPEnemyChase1147Slice.Friendly,
            dir => dir == VmPEnemyChase1147Slice.NorthEast,
            new[] { 0 },
            friendPlayer: 1,
            deltaX: 200,
            deltaY: 200,
            hasSight: false);
        Assert.Equal(VmPEnemyChase1147Slice.NorthEast, step.MoveDir);

        step = Chase(
            VmPEnemyChase1147Slice.West,
            VmPEnemyChase1147Slice.Friendly,
            dir => dir == VmPEnemyChase1147Slice.North,
            new[] { 0, 201 },
            friendPlayer: 1,
            deltaX: 400,
            deltaY: 200,
            hasSight: false);
        Assert.Equal(VmPEnemyChase1147Slice.North, step.MoveDir);

        step = Chase(
            VmPEnemyChase1147Slice.East,
            VmPEnemyChase1147Slice.Friendly,
            dir => dir == VmPEnemyChase1147Slice.NorthEast,
            new[] { 62, 1 },
            friendPlayer: 0,
            multiplayer: true,
            playerInGame: slot => slot == 0,
            deltaX: 200,
            deltaY: 200,
            hasSight: true);
        Assert.Equal(VmPEnemyChase1147Slice.NorthEast, step.MoveDir);

        var tried = new List<int>();
        step = Chase(
            VmPEnemyChase1147Slice.East,
            VmPEnemyChase1147Slice.Friendly,
            dir => dir == VmPEnemyChase1147Slice.SouthWest,
            new[] { 200, 0 },
            friendPlayer: 1,
            flags8: VmPEnemyChase1147Slice.DontFollowPlayers,
            deltaX: 200,
            deltaY: 200,
            walk: dir =>
            {
                tried.Add(dir);
                return dir == VmPEnemyChase1147Slice.SouthWest;
            });
        Assert.Equal(new[] { 1, 2, 3, 5 }, tried);
        Assert.Equal(VmPEnemyChase1147Slice.SouthWest, step.MoveDir);
    }

    [Fact]
    public void RandomChaseDir_ReplacesNoDirBeforeTheSearchAndSkipsABlockedReverse()
    {
        var tried = new List<int>();
        var step = Chase(VmPEnemyChase1147Slice.NoDir, 0, dir => dir == VmPEnemyChase1147Slice.North, new[] { 200, 1, 3 }, 0, walk: dir =>
        {
            tried.Add(dir);
            return dir == VmPEnemyChase1147Slice.North;
        });
        Assert.Equal(new[] { VmPEnemyChase1147Slice.North }, tried);
        Assert.Equal(VmPEnemyChase1147Slice.North, step.MoveDir);
        Assert.Null(step.MoveCount);

        step = Chase(VmPEnemyChase1147Slice.NoDir, 0, dir => dir == VmPEnemyChase1147Slice.NoDir, new[] { 100 }, 0);
        Assert.Equal(VmPEnemyChase1147Slice.NoDir, step.MoveDir);
    }

    private static VmPEnemyChase1147Slice.ChaseStep Chase(
        int moveDir,
        int flags,
        Func<int, bool> success,
        int[] rolls,
        int friendPlayer,
        int flags8 = 0,
        bool multiplayer = false,
        Func<int, bool>? playerInGame = null,
        double deltaX = 0,
        double deltaY = 0,
        bool hasSight = true,
        Func<int, bool>? walk = null)
    {
        return VmPEnemyChase1147Slice.RandomChaseDir(
            moveDir,
            flags,
            flags8,
            friendPlayer,
            multiplayer,
            playerInGame ?? (slot => true),
            slot => true,
            hasSight,
            deltaX,
            deltaY,
            walk ?? success,
            rolls);
    }
}
