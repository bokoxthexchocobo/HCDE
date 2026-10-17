namespace HCDE.Playsim;

[Flags]
public enum ActorLookFlags
{
    NoSightCheck = 1, NoSoundCheck = 2, DontChaseGoal = 4,
    NoSeeSound = 8, FullVolumeSeeSound = 16, NoJump = 32,
}

public static class ActorLookActions
{
    public static void LookEx(Actor actor, ActorLookFlags flags = 0, double minSeeDistance = 0,
        double maxSeeDistance = 0, double maxHearDistance = 0, double fieldOfView = 0,
        int? seeState = null, bool? allAround = null)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if ((flags & ~(ActorLookFlags)63) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        if (!double.IsFinite(minSeeDistance) || !double.IsFinite(maxSeeDistance)
            || !double.IsFinite(maxHearDistance) || !double.IsFinite(fieldOfView))
            throw new ArgumentOutOfRangeException(nameof(minSeeDistance));
        if (actor.Destroyed || actor.InConversation) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("LookEx requires a simulation.");
        var brain = actor.Brain ?? throw new NotSupportedException("LookEx requires a represented monster brain.");
        if (seeState is { } requestedState && !actor.States.HasState(requestedState))
            throw new ArgumentOutOfRangeException(nameof(seeState));
        if (actor.Special == 229 && actor.SpecialArgs[0] == 0)
        {
            actor.Special = 0;
            actor.GoalId = AcsActorTid.AllFromTid(sim, actor.SpecialArgs[1])
                .FirstOrDefault(a => a.ClassDoomEdNum == PatrolMarkers.PointType)?.Id;
            actor.ReactionTime = unchecked(actor.SpecialArgs[2] * 35 + sim.Thinkers.Clock.Tic);
            actor.ChaseGoal = actor.SpecialArgs[3] != 0;
        }
        brain.SetChaseThreshold(0, false);
        var parameters = new ActorLookParameters(maxSeeDistance, minSeeDistance, fieldOfView == 0 ? 180 : fieldOfView);
        Actor? Find() => brain.FindForLook(sim, actor, parameters, allAround ?? actor.LookAllAround, (flags & ActorLookFlags.DontChaseGoal) == 0);
        Actor? heard = null;
        if (actor.TidToHate != 0)
            heard = sim.Actors.FirstOrDefault(a => !a.Destroyed && a.Id == actor.TargetId);
        else if ((flags & ActorLookFlags.NoSoundCheck) == 0)
        {
            var id = actor.NoSector || sim.Compat.HasFlag(CompatSurface.SoundTarget)
                ? sim.SectorSoundTargetId(actor.SectorIndex) : actor.LastHeardTargetId;
            heard = sim.Actors.FirstOrDefault(a => !a.Destroyed && !a.IsDead && a.Id == id);
            if (heard is not null && maxHearDistance != 0 && Distance(actor, heard) > maxHearDistance)
            { heard = null; actor.LastHeardTargetId = null; }
            if (heard is PlayerPawn { NoTargetCheat: true }) return;
        }
        if (actor.Stealth) actor.VisibilityDirection = -1;
        Actor? target = null;
        if (heard is { Shootable: true })
        {
            if (actor.IsFriend(heard))
            {
                if (!actor.StandStill)
                {
                    if ((flags & ActorLookFlags.NoSightCheck) == 0) target = Find();
                    if (target is null && !brain.IsInChase) EnterSeeState(wanderIfMissing: true);
                }
            }
            else
            {
                actor.SetTargetPointer(heard == actor ? null : heard);
                var distance = Distance(actor, heard);
                if (heard != actor && (!actor.Ambush || (sim.NativeMonsterMovement
                    ? CombatTrace.CheckSight(sim, actor, heard, ActorSightFlags.SeePastBlockEverything)
                    : CombatTrace.HasLineOfSight(sim, actor, heard))
                    && (minSeeDistance == 0 || distance > minSeeDistance)
                    && (maxSeeDistance == 0 || distance < maxSeeDistance))) target = heard;
            }
        }
        if (target is null)
        {
            if ((flags & ActorLookFlags.NoSightCheck) != 0) return;
            target = Find();
            if (target is null) return;
        }
        actor.SetTargetPointer(target);
        if (target.Id == actor.GoalId)
        {
            if (actor.ReactionTime > sim.Thinkers.Clock.Tic) actor.SetTargetPointer(null);
        }
        else if ((flags & ActorLookFlags.NoSeeSound) == 0
            && actor.ActorSounds.TryGetValue(ActorSoundType.See, out var sound) && !string.IsNullOrEmpty(sound))
            ActorSoundActions.StartSound(actor, sound, 2, attenuation: (flags & ActorLookFlags.FullVolumeSeeSound) != 0 ? 0 : 1);
        if (actor.TargetId is not null && !brain.IsInChase && (flags & ActorLookFlags.NoJump) == 0) EnterSeeState();

        void EnterSeeState(bool wanderIfMissing = false)
        {
            var state = seeState ?? actor.SeeState;
            if (actor.States.HasState(state)) actor.States.Enter(actor, state);
            else if (wanderIfMissing && seeState is null) ActorWanderActions.Wander(actor);
        }
    }

    private static double Distance(Actor a, Actor b)
    {
        var dx = a.X.ToDouble() - b.X.ToDouble(); var dy = a.Y.ToDouble() - b.Y.ToDouble();
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
