namespace HCDE.Playsim;

/// <summary>Faithful port of <c>A_DoChase</c> (<c>p_enemy.cpp</c> 2397–2764).</summary>
public static class VmPEnemyDoChase
{
    public const int MfStealth = 1;
    public const int MfJustattacked = 1 << 1;
    public const int MfFriendly = 1 << 2;
    public const int MfNogravity = 1 << 3;
    public const int MfFloat = 1 << 4;
    public const int MfShootable = 1 << 5;
    public const int Mf2Cantleavefloorpic = 1;
    public const int Mf2Onmobj = 1 << 1;
    public const int Mf2Nonshootable = 1 << 2;
    public const int Mf3Nosightcheck = 1;
    public const int Mf4Incombat = 1;
    public const int Mf5Inconversation = 1;
    public const int Mf5Chasegoal = 1 << 1;
    public const int Mf7Inchase = 1;
    public const int Mf8Minvisible = 1;
    public const int RfInvisible = 1;
    public const int ChfNodirectionturn = 64;
    public const int ChfNorandomturn = 32;
    public const int ChfStopifblocked = 256;
    public const int ChfDontlookallaround = 4;
    public const int ChfDontidle = 2;
    public const int ChfNopostattackturn = 16;
    public const int CfNotarget = 1;
    public const int CfFrightening = 1 << 1;
    public const int Mf4Frightened = 1;
    public const int Mf8Frightening = 1;
    public const int DiNodir = 8;
    public const double BossStrafeRange = 64 * 10;

    public sealed class DoChaseState
    {
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int Flags5 { get; set; }
        public int Flags7 { get; set; }
        public int Flags8 { get; set; }
        public int RenderFlags { get; set; }
        public int ReactionTime { get; set; }
        public int Threshold { get; set; }
        public int Tics { get; set; }
        public int MoveCount { get; set; }
        public int Movedir { get; set; }
        public int StrafeCount { get; set; }
        public int FastChaseStrafeCount { get; set; }
        public double YawDegrees { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double Z { get; set; }
        public double FloorZ { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PrevX { get; set; }
        public double PrevY { get; set; }
        public int PrevPortalGroup { get; set; }
        public int FloorPic { get; set; }
        public bool TargetNull { get; set; }
        public int TargetHealth { get; set; }
        public int TargetRenderFlags { get; set; }
        public int TargetFlags8 { get; set; }
        public bool TargetIsGoal { get; set; }
        public bool TargetIsFriend { get; set; }
        public bool TargetHasPlayer { get; set; }
        public int TargetPlayerCheats { get; set; }
        public int TargetFlags { get; set; }
        public bool GoalNull { get; set; }
        public bool IsFast { get; set; }
        public bool AttackSoundValid { get; set; }
        public bool Multiplayer { get; set; }
        public int TidToHate { get; set; }
        public bool NightmareFast { get; set; }
        public bool FastChase { get; set; }
        public bool DontMove { get; set; }
        public int ChaseFlags { get; set; }
        public bool MeleeStateSet { get; set; }
        public bool MissileStateSet { get; set; }
        public bool PlayActive { get; set; }
        public bool NoMonsterInterpolation { get; set; }
        public int DefaultReactionTime { get; set; }
        public int MapTime { get; set; }
        public int Ticrate { get; set; } = 35;
        public bool ExitedEarly { get; set; }
        public bool LastEnemyCleared { get; set; }
        public bool LastHeardCleared { get; set; }
        public bool WanderCalled { get; set; }
        public bool IdleCalled { get; set; }
        public bool MeleeStateApplied { get; set; }
        public bool MissileStateApplied { get; set; }
        public int PrTrywalkRoll { get; set; }
        public int PrScaredycatRoll { get; set; }
        public int PrChaseRoll { get; set; }
        public Func<bool>? LookForPlayers { get; set; }
        public Action? Wander { get; set; }
        public Action? SetIdle { get; set; }
        public Func<bool>? CheckMeleeRange { get; set; }
        public Func<bool>? CheckMissileRange { get; set; }
        public Action? SetMeleeState { get; set; }
        public Action? SetMissileState { get; set; }
        public Action? PlayAttackSound { get; set; }
        public Action? PlayActiveSound { get; set; }
        public Func<bool>? CheckSight { get; set; }
        public Func<bool>? SmartMove { get; set; }
        public Action? NewChaseDir { get; set; }
        public Func<double, double, bool>? TryMove { get; set; }
        public Func<bool>? FastMonstersSkill { get; init; }
        public Func<bool>? GoalMeleeReached { get; init; }
        public Action<int>? FinishPatrolGoal { get; init; }
        public Action<double>? FaceTarget { get; init; }
        public Action<double, double>? VelFromAngle { get; init; }
        public Func<double>? Distance2DToTarget { get; init; }
        public Func<double>? AngleToTarget { get; init; }
        public Func<int>? PrNewChaseDir { get; init; }
        public Func<int>? PrChase { get; init; }
        public Func<int>? PrScaredycat { get; init; }
        public Func<int>? PrTrywalk { get; init; }
    }

    static double DeltaAngle(double yaw, double targetDir) => targetDir * 45 - yaw;

    public static bool TargetInvisibleToChaser(DoChaseState s) =>
        !s.TargetNull && !s.TargetIsGoal &&
        ((s.TargetRenderFlags & RfInvisible) != 0 || (s.TargetFlags8 & Mf8Minvisible) != 0);

    public static void ClearInvisibleTarget(DoChaseState s)
    {
        if (TargetInvisibleToChaser(s))
            s.TargetNull = true;
    }

    public static void ApplyNotargetPlayerClear(DoChaseState s)
    {
        s.TargetNull = true;
        s.Threshold = 0;
        s.LastEnemyCleared = true;
        s.LastHeardCleared = true;
    }

    public static bool MissileChaseBlockedByMoveCount(DoChaseState s) =>
        !s.IsFast && s.MoveCount != 0;

    public static bool StealthMonsterActive(DoChaseState s) => (s.Flags & MfStealth) != 0;

    public static void DoChase(DoChaseState s)
    {
        if ((s.Flags5 & Mf5Inconversation) != 0)
            return;
        if ((s.Flags7 & Mf7Inchase) != 0)
            return;
        s.Flags7 |= Mf7Inchase;

        if (StealthMonsterActive(s))
            s.RenderFlags = -1;

        if (s.ReactionTime > 0)
            s.ReactionTime--;

        ClearInvisibleTarget(s);

        if (s.Threshold > 0)
        {
            if (s.TargetNull || s.TargetHealth <= 0)
                s.Threshold = 0;
            else
                s.Threshold--;
        }

        if (s.NightmareFast && s.FastMonstersSkill?.Invoke() == true)
        {
            if (s.Tics > 3)
            {
                s.Tics -= s.Tics / 2;
                if (s.Tics < 3)
                    s.Tics = 3;
            }
        }

        if (s.StrafeCount > 0)
            s.FaceTarget?.Invoke(s.AngleToTarget?.Invoke() ?? 0);
        else if ((s.ChaseFlags & ChfNodirectionturn) == 0 && s.Movedir < DiNodir)
        {
            s.YawDegrees = Math.Floor(s.YawDegrees / 45) * 45;
            var delta = DeltaAngle(s.YawDegrees, s.Movedir);
            if (delta < 0)
                s.YawDegrees -= 45;
            else if (delta > 0)
                s.YawDegrees += 45;
        }

        if (!s.TargetNull && !s.TargetIsGoal && (s.TargetHealth <= 0 || s.TargetIsFriend))
            s.TargetNull = true;

        if (!s.TargetNull && !s.TargetIsGoal && s.TargetHasPlayer && (s.TargetPlayerCheats & CfNotarget) != 0)
            ApplyNotargetPlayerClear(s);

        if (s.TargetNull || (s.TargetFlags & MfShootable) == 0)
        {
            if (!s.TargetNull && (s.Flags2 & Mf2Nonshootable) != 0)
                s.Threshold = 0;

            if (s.LookForPlayers?.Invoke() == true && !s.TargetIsGoal)
            {
                s.Flags7 &= ~Mf7Inchase;
                s.ExitedEarly = true;
                return;
            }

            if (s.TargetNull)
            {
                if ((s.ChaseFlags & ChfDontidle) != 0 || (s.Flags & MfFriendly) != 0)
                {
                    if (s.TargetNull && !s.DontMove)
                        s.WanderCalled = true;
                    s.Wander?.Invoke();
                    s.Flags7 &= ~Mf7Inchase;
                    s.ExitedEarly = true;
                    return;
                }

                s.IdleCalled = true;
                s.SetIdle?.Invoke();
                s.Flags7 &= ~Mf7Inchase;
                s.ExitedEarly = true;
                return;
            }
        }

        if ((s.Flags & MfJustattacked) != 0)
        {
            s.Flags &= ~MfJustattacked;
            if (!s.IsFast && !s.DontMove && (s.ChaseFlags & ChfNopostattackturn) == 0 && (s.ChaseFlags & ChfStopifblocked) == 0)
                s.NewChaseDir?.Invoke();
            if ((s.ChaseFlags & ChfStopifblocked) != 0)
                s.MoveCount = (s.PrTrywalk?.Invoke() ?? s.PrTrywalkRoll) & 15;
            s.Flags7 &= ~Mf7Inchase;
            s.ExitedEarly = true;
            return;
        }

        if (s.TargetIsGoal || ((s.Flags5 & Mf5Chasegoal) != 0 && !s.GoalNull))
        {
            if (s.GoalMeleeReached?.Invoke() == true)
            {
                s.FinishPatrolGoal?.Invoke(s.MapTime);
                s.Flags |= MfJustattacked;
                s.Flags7 &= ~Mf7Inchase;
                s.ExitedEarly = true;
                return;
            }

            if (s.TargetIsGoal)
                goto Nomissile;
        }

        if (s.FastChase && !s.DontMove)
        {
            if (s.FastChaseStrafeCount > 0)
                s.FastChaseStrafeCount--;
            else
            {
                s.FastChaseStrafeCount = 0;
                s.VelX = s.VelY = 0;
                var dist = s.Distance2DToTarget?.Invoke() ?? 0;
                if (dist < BossStrafeRange && (s.PrChase?.Invoke() ?? s.PrChaseRoll) < 100)
                {
                    var ang = s.AngleToTarget?.Invoke() ?? 0;
                    if ((s.PrChase?.Invoke() ?? s.PrChaseRoll) < 128)
                        ang += 90;
                    else
                        ang -= 90;
                    s.VelFromAngle?.Invoke(13, ang);
                    s.FastChaseStrafeCount = 3;
                }
            }
        }

        var frightened = (s.TargetHasPlayer && ((s.TargetPlayerCheats & CfFrightening) != 0 || (s.TargetFlags8 & Mf8Frightening) != 0))
            || (s.Flags4 & Mf4Frightened) != 0;
        if (!frightened || (s.PrScaredycat?.Invoke() ?? s.PrScaredycatRoll) < 43)
        {
            if (s.MeleeStateSet && s.CheckMeleeRange?.Invoke() == true)
            {
                if (s.AttackSoundValid)
                    s.PlayAttackSound?.Invoke();
                s.SetMeleeState?.Invoke();
                s.MeleeStateApplied = true;
                s.Flags7 &= ~Mf7Inchase;
                s.ExitedEarly = true;
                return;
            }

            if (s.MissileStateSet)
            {
                if (MissileChaseBlockedByMoveCount(s))
                    goto Nomissile;
                if (s.CheckMissileRange?.Invoke() != true)
                    goto Nomissile;
                s.SetMissileState?.Invoke();
                s.MissileStateApplied = true;
                s.Flags |= MfJustattacked;
                s.Flags4 |= Mf4Incombat;
                s.Flags7 &= ~Mf7Inchase;
                s.ExitedEarly = true;
                return;
            }
        }

    Nomissile:
        if ((s.Multiplayer || s.TidToHate != 0) && s.Threshold == 0 && s.CheckSight?.Invoke() == false)
        {
            var lookForBetter = false;
            if ((s.Flags3 & Mf3Nosightcheck) != 0)
            {
                s.Flags3 &= ~Mf3Nosightcheck;
                lookForBetter = true;
            }

            var hadTarget = !s.TargetNull;
            var gotNew = s.LookForPlayers?.Invoke() == true;
            if (lookForBetter)
                s.Flags3 |= Mf3Nosightcheck;
            if (gotNew && hadTarget)
            {
                s.Flags7 &= ~Mf7Inchase;
                s.ExitedEarly = true;
                return;
            }
        }

        if (s.StrafeCount > 0)
            s.StrafeCount--;

        if ((!s.FastChase || s.FastChaseStrafeCount == 0) && !s.DontMove)
        {
            var oldX = s.PosX;
            var oldY = s.PosY;
            var oldGroup = s.PrevPortalGroup;
            var oldFloor = s.FloorPic;
            if ((--s.MoveCount < 0 && (s.ChaseFlags & ChfNorandomturn) == 0) || (s.SmartMove?.Invoke() != true && (s.ChaseFlags & ChfStopifblocked) == 0))
                s.NewChaseDir?.Invoke();

            if ((s.Flags2 & Mf2Cantleavefloorpic) != 0 && s.FloorPic != oldFloor)
            {
                if (s.TryMove?.Invoke(oldX, oldY) == true)
                {
                    if (s.NoMonsterInterpolation)
                    {
                        s.PrevX = oldX;
                        s.PrevY = oldY;
                        s.PrevPortalGroup = oldGroup;
                    }
                }

                if ((s.ChaseFlags & ChfStopifblocked) == 0)
                    s.NewChaseDir?.Invoke();
            }
        }
        else if (s.DontMove && s.MoveCount > 0)
            s.MoveCount--;

        if (s.PlayActive && (s.PrChase?.Invoke() ?? s.PrChaseRoll) < 3)
            s.PlayActiveSound?.Invoke();

        s.Flags7 &= ~Mf7Inchase;
    }
}
