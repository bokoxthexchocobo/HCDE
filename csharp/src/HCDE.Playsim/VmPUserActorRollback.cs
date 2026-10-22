namespace HCDE.Playsim;

/// <summary>Rollback backup types from <c>p_user.cpp</c> (389–616).</summary>
public static class VmPUserActorRollback
{
    public const int MfPickup = 1;
    public const int Mf2Pushwall = 2;
    public const int IntMax = int.MaxValue;
    public const int Maxplayers = 64;

    public sealed class ObjectBackup
    {
        public object? Obj { get; private set; }

        public ObjectBackup()
        {
        }

        public ObjectBackup(object obj) => Obj = obj;
    }

    public sealed class ActorBackup : ObjectBackup
    {
        public VmPUserHcdeWeaponPhysics.PhysicsLinkBackup Link { get; }
        public int StatNum { get; private set; } = -1;
        public int Tid { get; private set; } = IntMax;
        public bool HadPickup { get; private set; }
        public bool HadPushwall { get; private set; }
        public bool SavedFlagsValid { get; private set; }

        public ActorBackup(object actor, VmPUserHcdeWeaponPhysics.PhysicsLinkBackup link) : base(actor)
        {
            Link = link;
        }

        public void PostBackup(Func<uint> readFlags, Func<uint> readFlags2, Action<uint> writeFlags, Action<uint> writeFlags2)
        {
            if (Obj == null)
                return;
            HadPickup = (readFlags() & MfPickup) != 0;
            HadPushwall = (readFlags2() & Mf2Pushwall) != 0;
            SavedFlagsValid = true;
            writeFlags(readFlags() & ~MfPickup);
            writeFlags2(readFlags2() & ~Mf2Pushwall);
        }

        public void PreRollback(int statNum, int tid, Action unlinkFromWorld)
        {
            if (Obj == null)
                return;
            StatNum = statNum;
            Tid = tid;
            unlinkFromWorld();
        }

        public void PostRollback(
            int liveStatNum,
            int liveTid,
            Action<int> rollbackStatNum,
            Action changeStatNum,
            Action<int> setTid,
            Action relinkToWorld,
            Action<object> restorePhysicsLink,
            Action<uint> writeFlags,
            Action<uint> writeFlags2,
            Func<uint> readFlags,
            Func<uint> readFlags2)
        {
            if (Obj == null)
                return;

            if (StatNum == -1)
                changeStatNum();
            else
                rollbackStatNum(StatNum);

            if (Tid == IntMax)
            {
                Tid = liveTid;
                setTid(0);
                setTid(Tid);
            }
            else
                setTid(Tid);

            relinkToWorld();
            restorePhysicsLink(Obj);

            if (!SavedFlagsValid)
                return;

            writeFlags(HadPickup ? readFlags() | MfPickup : readFlags() & ~MfPickup);
            writeFlags2(HadPushwall ? readFlags2() | Mf2Pushwall : readFlags2() & ~Mf2Pushwall);
        }

        public void CaptureStatTid(int statNum, int tid)
        {
            StatNum = statNum;
            Tid = tid;
        }
    }

    public sealed class PlayerBob
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class PredictionRollbackState
    {
        public bool WeaponBobBackupValid { get; set; }
        public int WeaponBobBackupPlayer { get; set; } = -1;
        public PlayerBob WeaponBobBackup { get; } = new();
        public bool WeaponPredictStateValid { get; set; }
        public ushort SavedWeaponState { get; set; }
        public short SavedRefire { get; set; }
        public bool SavedAttackDown { get; set; }
        public bool SavedUseDown { get; set; }
        public object? SavedReadyWeapon { get; set; }
        public object? SavedPendingWeapon { get; set; }
        public List<object?> RollbackObjectRefs { get; } = new();
        public List<ObjectBackup> RollbackObjects { get; } = new();
        public List<ActorBackup> RollbackActors { get; } = new();
        public List<int> RollbackPlayers { get; } = new();
        public double LastPosX { get; set; }
        public double LastPosY { get; set; }
        public double LastPosZ { get; set; }
        public int LastPortalGroup { get; set; }
        public int LastPosTic { get; set; } = -1;

        public void ResetPos()
        {
            LastPosX = LastPosY = LastPosZ = 0;
            LastPortalGroup = 0;
            LastPosTic = -1;
        }

        public void CaptureWeaponPredictState(
            ushort weaponState,
            short refire,
            bool attackDown,
            bool useDown,
            object? readyWeapon,
            object? pendingWeapon)
        {
            SavedWeaponState = weaponState;
            SavedRefire = refire;
            SavedAttackDown = attackDown;
            SavedUseDown = useDown;
            SavedReadyWeapon = readyWeapon;
            SavedPendingWeapon = pendingWeapon;
            WeaponPredictStateValid = true;
        }

        public void RestoreWeaponPredictState(
            Action<ushort> setWeaponState,
            Action<short> setRefire,
            Action<bool> setAttackDown,
            Action<bool> setUseDown,
            Action<object?> setReadyWeapon,
            Action<object?> setPendingWeapon)
        {
            if (!WeaponPredictStateValid)
                return;
            setWeaponState(SavedWeaponState);
            setRefire(SavedRefire);
            setAttackDown(SavedAttackDown);
            setUseDown(SavedUseDown);
            setReadyWeapon(SavedReadyWeapon);
            setPendingWeapon(SavedPendingWeapon);
            WeaponPredictStateValid = false;
        }

        public void ClearBackup(Action restoreWeaponBob)
        {
            restoreWeaponBob();
            RollbackObjectRefs.Clear();
            RollbackObjects.Clear();
            RollbackActors.Clear();
            RollbackPlayers.Clear();
        }

        public void BackupWeaponBob(int player, PlayerBob[] playerBobs)
        {
            if (player < 0 || player >= Maxplayers || WeaponBobBackupValid)
                return;
            WeaponBobBackup.X = playerBobs[player].X;
            WeaponBobBackup.Y = playerBobs[player].Y;
            WeaponBobBackupPlayer = player;
            WeaponBobBackupValid = true;
        }

        public void RestoreWeaponBob(PlayerBob[] playerBobs)
        {
            if (!WeaponBobBackupValid)
                return;
            if (WeaponBobBackupPlayer >= 0 && WeaponBobBackupPlayer < Maxplayers)
            {
                playerBobs[WeaponBobBackupPlayer].X = WeaponBobBackup.X;
                playerBobs[WeaponBobBackupPlayer].Y = WeaponBobBackup.Y;
            }

            WeaponBobBackup.X = WeaponBobBackup.Y = 0;
            WeaponBobBackupPlayer = -1;
            WeaponBobBackupValid = false;
        }

        public void MarkRollbackRefs(Action<object?> markObject)
        {
            foreach (var obj in RollbackObjectRefs)
                markObject(obj);
        }
    }
}
