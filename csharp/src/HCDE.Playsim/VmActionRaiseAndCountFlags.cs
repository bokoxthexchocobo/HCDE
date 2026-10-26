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

    public static bool ChildUsesMaster(AActorRaiseRef child, AActorRaiseRef parent) =>
        child.MasterId == parent.ActorId;

    public static bool SiblingSharesMaster(AActorRaiseRef self, AActorRaiseRef other, AActorRaiseRef master) =>
        other.MasterId == master.ActorId && other.ActorId != self.ActorId;

    public static bool RefireProbabilityFails(int roll, int prob) => roll < prob;

    public static bool RefireTargetInvalid(bool targetNull, bool hitFriend, int targetHealth, bool sightOk) =>
        targetNull || hitFriend || targetHealth <= 0 || !sightOk;

    public static bool RefireContinuesFiring(bool probFails, bool targetInvalid) => !probFails && !targetInvalid;

    public static bool CheckFlagResult(bool ownerNull, bool flagSet) => !ownerNull && flagSet;

    public readonly record struct LevelCountTotals(int Monsters, int Items, int Secrets);

    public static (LevelCountTotals Totals, AActorRaiseRef Actor) RunChangeCountFlags(
        LevelCountTotals totals,
        AActorRaiseRef actor,
        int kill,
        int item,
        int secret)
    {
        var (monsters, items, secrets, flags, flags5) = PlanChangeCountFlags(
            totals.Monsters,
            totals.Items,
            totals.Secrets,
            actor.Flags,
            actor.Flags5,
            actor.CountsAsKill,
            actor.Health > 0,
            kill,
            item,
            secret);
        actor = actor with { Flags = flags, Flags5 = flags5 };
        return (new LevelCountTotals(monsters, items, secrets), actor);
    }

    public delegate bool RaiseActorDelegate(AActorRaiseRef victim, AActorRaiseRef raiser, int flags);

    public static bool TryRaiseMaster(AActorRaiseRef self, AActorRaiseRef? master, int flags, RaiseActorDelegate raise) =>
        master is not null && raise(master.Value, self, flags);

    public static int RaiseChildrenOnLevel(
        AActorRaiseRef parent,
        ReadOnlySpan<AActorRaiseRef> levelActors,
        int flags,
        RaiseActorDelegate raise)
    {
        var raised = 0;
        foreach (var mo in levelActors)
        {
            if (!ChildUsesMaster(mo, parent))
                continue;
            if (raise(mo, parent, flags))
                raised++;
        }

        return raised;
    }

    public static int RaiseSiblingsOnLevel(
        AActorRaiseRef self,
        AActorRaiseRef? master,
        ReadOnlySpan<AActorRaiseRef> levelActors,
        int flags,
        RaiseActorDelegate raise)
    {
        if (master is null)
            return 0;
        var raised = 0;
        foreach (var mo in levelActors)
        {
            if (!SiblingSharesMaster(self, mo, master.Value))
                continue;
            if (raise(mo, self, flags))
                raised++;
        }

        return raised;
    }

    public static bool RaiseSelf(AActorRaiseRef self, int flags, RaiseActorDelegate raise) =>
        raise(self, self, flags);

    public enum MonsterRefireOutcome
    {
        KeepFiring,
        Jump,
    }

    public static MonsterRefireOutcome DecideMonsterRefire(
        int roll,
        int prob,
        bool targetNull,
        bool hitFriend,
        int targetHealth,
        bool sightOk)
    {
        if (RefireProbabilityFails(roll, prob))
            return MonsterRefireOutcome.KeepFiring;
        if (RefireTargetInvalid(targetNull, hitFriend, targetHealth, sightOk))
            return MonsterRefireOutcome.Jump;
        return MonsterRefireOutcome.KeepFiring;
    }

    public static bool TrySetAngle(AActorRaiseRef? refActor, double angleDegrees, int flags, Action<AActorRaiseRef, double, int>? setAngle) =>
        TrySetOrientation(refActor, setAngle, angleDegrees, flags);

    public static bool TrySetPitch(AActorRaiseRef? refActor, double pitchDegrees, int flags, Action<AActorRaiseRef, double, int>? setPitch) =>
        TrySetOrientation(refActor, setPitch, pitchDegrees, flags);

    public static bool TrySetRoll(AActorRaiseRef? refActor, double rollDegrees, int flags, Action<AActorRaiseRef, double, int>? setRoll) =>
        TrySetOrientation(refActor, setRoll, rollDegrees, flags);

    public static bool TrySetViewAngle(AActorRaiseRef? refActor, double angleDegrees, int flags, Action<AActorRaiseRef, double, int>? setViewAngle) =>
        TrySetOrientation(refActor, setViewAngle, angleDegrees, flags);

    public static bool TrySetViewPitch(AActorRaiseRef? refActor, double pitchDegrees, int flags, Action<AActorRaiseRef, double, int>? setViewPitch) =>
        TrySetOrientation(refActor, setViewPitch, pitchDegrees, flags);

    static bool TrySetOrientation<T>(
        AActorRaiseRef? refActor,
        Action<AActorRaiseRef, T, int>? setter,
        T value,
        int flags)
    {
        if (refActor is null || setter is null)
            return false;
        setter(refActor.Value, value, flags);
        return true;
    }

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
