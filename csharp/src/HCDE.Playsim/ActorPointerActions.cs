namespace HCDE.Playsim;

public static class ActorPointerActions
{
    public static void RearrangePointers(Actor actor, int newTarget, int newMaster = 0, int newTracer = 0, int flags = 0)
    {
        if ((flags & ~3) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        var sim = actor.Simulation ?? throw new InvalidOperationException("Pointer rearrangement requires a simulation.");
        var writeTarget = newTarget is AcsActorPointer.Master or AcsActorPointer.Tracer or AcsActorPointer.Null;
        var writeMaster = newMaster is AcsActorPointer.Target or AcsActorPointer.Tracer or AcsActorPointer.Null;
        var writeTracer = newTracer is AcsActorPointer.Target or AcsActorPointer.Master or AcsActorPointer.Null;
        if (writeTarget) RequireWritable(actor, AcsActorPointer.Target);
        if (writeTracer) RequireWritable(actor, AcsActorPointer.Tracer);
        // Native rearrangement always reads the original three pointers before any write.
        var target = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Target);
        var master = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Master);
        var tracer = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Tracer);
        Actor? Original(int selector) => selector switch
        {
            AcsActorPointer.Target => target,
            AcsActorPointer.Master => master,
            AcsActorPointer.Tracer => tracer,
            _ => null
        };
        if (writeTarget) Assign(sim, actor, AcsActorPointer.Target, Original(newTarget), flags);
        if (writeMaster) Assign(sim, actor, AcsActorPointer.Master, Original(newMaster), flags);
        if (writeTracer) Assign(sim, actor, AcsActorPointer.Tracer, Original(newTracer), flags);
    }

    public static void TransferPointer(Actor actor, int sourceSelector = 0, int recipientSelector = 0,
        int sourceField = 2, int recipientField = 0, int flags = 0)
    {
        if ((flags & ~3) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        var sim = actor.Simulation ?? throw new InvalidOperationException("Pointer transfer requires a simulation.");
        var source = AcsActorPointer.Resolve(sim, actor, sourceSelector);
        var recipient = AcsActorPointer.Resolve(sim, actor, recipientSelector);
        if (recipient is null || recipient.Destroyed) return;
        var value = AcsActorPointer.Resolve(sim, source, sourceField);
        if (value == recipient) value = null;
        var field = recipientField == AcsActorPointer.Default ? sourceField : recipientField;
        Assign(sim, recipient, field, value, flags);
    }

    internal static void RequireWritable(Actor actor, int field)
    {
        if (field == AcsActorPointer.Target && actor.Brain is null)
            throw new NotSupportedException("Target assignment requires a managed monster brain; projectile owners are immutable.");
        if (field == AcsActorPointer.Tracer && actor is not ProjectileActor)
            throw new NotSupportedException("Tracer assignment requires a managed projectile.");
    }

    internal static void Assign(AuthoritySimulation sim, Actor recipient, int field, Actor? value, int flags)
    {
        RequireWritable(recipient, field);
        switch (field)
        {
            case AcsActorPointer.Master:
                recipient.MasterId = value?.Id;
                if ((flags & 2) == 0) VerifyMasterChain(sim, recipient);
                break;
            case AcsActorPointer.Target:
                var brain = recipient.Brain
                    ?? throw new NotSupportedException("Target assignment requires a managed monster brain; projectile owners are immutable.");
                brain.SetSpecialTarget(value);
                recipient.HasTargetMemoryOverride = true;
                break;
            case AcsActorPointer.Tracer:
                if (recipient is not ProjectileActor projectile)
                    throw new NotSupportedException("Tracer assignment requires a managed projectile.");
                projectile.RestoreTracerTarget(value?.Id);
                break;
        }
    }

    private static void VerifyMasterChain(AuthoritySimulation sim, Actor actor)
    {
        var seen = new HashSet<uint> { actor.Id };
        var next = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Master);
        while (next is not null)
        {
            if (!seen.Add(next.Id)) { actor.MasterId = null; return; }
            next = AcsActorPointer.Resolve(sim, next, AcsActorPointer.Master);
        }
    }
}
