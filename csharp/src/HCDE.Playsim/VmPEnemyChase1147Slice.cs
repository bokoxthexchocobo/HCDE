namespace HCDE.Playsim;

/// <summary>
/// Random chase direction from <c>p_enemy.cpp</c> (<b>1100–1257</b>). Chunk <b>1147</b>.
/// The commented-out second search is not live. <c>P_IsVisible</c> begins at <b>1268</b>.
/// </summary>
public static class VmPEnemyChase1147Slice
{
    public const int East = 0;
    public const int NorthEast = 1;
    public const int North = 2;
    public const int NorthWest = 3;
    public const int West = 4;
    public const int SouthWest = 5;
    public const int South = 6;
    public const int SouthEast = 7;
    public const int NoDir = 8;
    public const int Friendly = 0x08000000;
    public const int DontFollowPlayers = 0x04000000;
    public const int MaxPlayers = 64;
    public const int AxisThreshold = 128;

    public static readonly int[] Opposite =
    {
        West, SouthWest, South, SouthEast, East, NorthEast, North, NorthWest, NoDir
    };

    public static readonly int[] Diags = { NorthWest, NorthEast, SouthWest, SouthEast };

    public readonly record struct ChaseStep(int MoveDir, int? MoveCount);

    public static ChaseStep RandomChaseDir(
        int moveDir,
        int flags,
        int flags8,
        int friendPlayer,
        bool multiplayer,
        Func<int, bool>? playerInGame,
        Func<int, bool>? playerPawn,
        bool hasSight,
        double deltaX,
        double deltaY,
        Func<int, bool> tryWalk,
        IReadOnlyList<int> rolls)
    {
        int oldDir = moveDir;
        if (oldDir < East || oldDir > NoDir)
            oldDir = NoDir;
        int turnaround = Opposite[oldDir];
        int roll = 0;

        int Next()
        {
            int value = rolls[roll];
            roll++;
            return value;
        }

        bool follow = (flags & Friendly) != 0 && (flags8 & DontFollowPlayers) == 0;
        if (follow)
        {
            int slot;
            bool haveSlot = true;
            if (friendPlayer != 0)
            {
                slot = friendPlayer - 1;
            }
            else if (!multiplayer)
            {
                slot = 0;
            }
            else
            {
                slot = Next() & (MaxPlayers - 1);
                int seen = 0;
                while (playerInGame == null || !playerInGame(slot))
                {
                    slot = (slot + 1) & (MaxPlayers - 1);
                    seen++;
                    if (seen >= MaxPlayers)
                    {
                        haveSlot = false;
                        break;
                    }
                }
            }

            bool pawn = haveSlot && playerPawn != null && playerPawn(slot) && playerInGame != null && playerInGame(slot);
            if (pawn)
            {
                int steer = Next();
                if ((steer & 1) != 0 || !hasSight)
                {
                    int directX = deltaX > AxisThreshold ? East : deltaX < -AxisThreshold ? West : NoDir;
                    int directY = deltaY < -AxisThreshold ? South : deltaY > AxisThreshold ? North : NoDir;
                    if (directX != NoDir && directY != NoDir)
                    {
                        int ySouth = deltaY < 0 ? 1 : 0;
                        int xEast = deltaX > 0 ? 1 : 0;
                        int diag = Diags[(ySouth << 1) + xEast];
                        if (diag != turnaround && tryWalk(diag))
                            return new ChaseStep(diag, null);
                    }

                    int order = Next();
                    if (order > 200 || Math.Abs(deltaY) > Math.Abs(deltaX))
                    {
                        int swap = directX;
                        directX = directY;
                        directY = swap;
                    }

                    if (directX == turnaround)
                        directX = NoDir;
                    if (directY == turnaround)
                        directY = NoDir;
                    if (directX != NoDir && tryWalk(directX))
                        return new ChaseStep(directX, null);
                    if (directY != NoDir && tryWalk(directY))
                        return new ChaseStep(directY, null);
                }
            }
        }

        if (Next() < 150 && tryWalk(oldDir))
            return new ChaseStep(oldDir, null);

        int turn = (Next() & 1) != 0 ? -1 : 1;
        if (oldDir == NoDir)
            oldDir = Next() & 7;

        for (int tdir = (oldDir + turn) & 7; tdir != oldDir; tdir = (tdir + turn) & 7)
        {
            if (tdir != turnaround && tryWalk(tdir))
                return new ChaseStep(tdir, null);
        }

        if (turnaround != NoDir && tryWalk(turnaround))
            return new ChaseStep(turnaround, Next() & 15);

        return new ChaseStep(NoDir, null);
    }
}
