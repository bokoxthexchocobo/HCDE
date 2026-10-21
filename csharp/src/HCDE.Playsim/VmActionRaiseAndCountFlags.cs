namespace HCDE.Playsim;

/// <summary>
/// Count-flag changes, raise helpers, orientation setters, and monster refire gating from
/// <c>p_actionfunctions.cpp</c> (2767–3028, 2938–2958).
/// </summary>
public static class VmActionRaiseAndCountFlags
{
    public const int MfCountkill = 0x400000;
    public const int MfCountitem = 0x80;
    public const int Mf5Countsecret = 0x2000000;

    public static bool RefNull(AActorRaiseRef? actor) => actor is null;

    public static (int Total, int Flags) ApplyKillCount(int total, bool countsAsKill, int killParam, int flags, bool alive)
    {
        if (countsAsKill && alive) total--;
        if (killParam != -1)
            flags = killParam == 0 ? flags & ~MfCountkill : flags | MfCountkill;
        if (CountsAsKill(flags) && alive) total++;
        return (total, flags);
    }

    public static bool CountsAsKill(int flags) => (flags & MfCountkill) != 0;

    public static (int Total, int Flags) ApplyItemCount(int total, int flags, int itemParam)
    {
        if ((flags & MfCountitem) != 0) total--;
        if (itemParam != -1)
            flags = itemParam == 0 ? flags & ~MfCountitem : flags | MfCountitem;
        if ((flags & MfCountitem) != 0) total++;
        return (total, flags);
    }

    public static (int Total, int Flags5) ApplySecretCount(int total, int flags5, int secretParam)
    {
        if ((flags5 & Mf5Countsecret) != 0) total--;
        if (secretParam != -1)
            flags5 = secretParam == 0 ? flags5 & ~Mf5Countsecret : flags5 | Mf5Countsecret;
        if ((flags5 & Mf5Countsecret) != 0) total++;
        return (total, flags5);
    }

    public static (int Monsters, int Items, int Secrets, int Flags, int Flags5) PlanChangeCountFlags(
        int monsters,
        int items,
        int secrets,
        int flags,
        int flags5,
        bool countsAsKill,
        bool alive,
        int kill,
        int item,
        int secret)
    {
        (monsters, flags) = ApplyKillCount(monsters, countsAsKill, kill, flags, alive);
        (items, flags) = ApplyItemCount(items, flags, item);
        (secrets, flags5) = ApplySecretCount(secrets, flags5, secret);
        return (monsters, items, secrets, flags, flags5);
    }

    public static bool ShouldRaiseMaster(bool hasMaster) => hasMaster;

    public static bool ChildUsesMaster(AActorRaiseRef child, AActorRaiseRef parent) =>
        child.MasterId == parent.ActorId;

    public static bool SiblingSharesMaster(AActorRaiseRef self, AActorRaiseRef other, AActorRaiseRef master) =>
        other.MasterId == master.ActorId && other.ActorId != self.ActorId;

    public static bool SiblingsNeedMaster(bool hasMaster) => hasMaster;

    public static bool RaiseSelfReturnsBool(bool raiseResult) => raiseResult;

    public static bool CanRaiseActor(bool canRaise) => canRaise;

    public static bool RefireProbabilityFails(int roll, int prob) => roll < prob;

    public static bool RefireTargetInvalid(bool targetNull, bool hitFriend, int targetHealth, bool sightOk) =>
        targetNull || hitFriend || targetHealth <= 0 || !sightOk;

    public static bool RefireJumpsToState(bool refireInvalid) => refireInvalid;

    public static bool RefireContinuesFiring(bool probFails, bool targetInvalid) => !probFails && !targetInvalid;

    public static bool FaceTargetBeforeRefire() => true;

    public static bool IteratorRaisesChild(bool masterMatch) => masterMatch;

    public static bool IteratorRaisesSibling(bool siblingMatch) => siblingMatch;

    public static bool SetAngleOnRef(bool refNull) => !refNull;

    public static bool SetPitchOnRef(bool refNull) => !refNull;

    public static bool SetRollOnRef(bool refNull) => !refNull;

    public static bool SetViewAngleOnRef(bool refNull) => !refNull;

    public static bool SetViewPitchOnRef(bool refNull) => !refNull;

    public static bool CheckFlagOwnerNull(bool ownerNull) => ownerNull;

    public static bool CheckFlagResult(bool ownerNull, bool flagSet) => !ownerNull && flagSet;

    public enum CountFlagsPlan
    {
        AdjustTotals,
        NoOpKill,
        NoOpItem,
        NoOpSecret,
    }

    public static CountFlagsPlan PlanCountFlags(int kill, int item, int secret) =>
        kill == -1 && item == -1 && secret == -1
            ? CountFlagsPlan.NoOpKill
            : CountFlagsPlan.AdjustTotals;

    public readonly record struct AActorRaiseRef(int ActorId, int MasterId, int Health, int Flags, int Flags5, bool CountsAsKill);
}
