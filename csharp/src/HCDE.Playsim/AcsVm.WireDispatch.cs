using HCDE.MapLoader;

namespace HCDE.Playsim;

public sealed partial class AcsVm
{
    /// <summary>Wire indices that collide with mistaken <see cref="AcsPcode"/> enum values (e.g. PrintBind enum 335 vs PCD_SETACTORSTATE wire 335).</summary>
    private bool TryDispatchCollidingWireOpcodes(AuthoritySimulation sim, Fiber fiber, int opcode)
    {
        switch (opcode)
        {
            case AcsPcodeWire.PrintBind:
            {
                if (fiber.Stack.Count < 1) { fiber.Done = true; return true; }
                var bindId = Pop(fiber);
                if (!fiber.Done)
                {
                    var command = AcsPrintHudOpcodes.ResolveString(bindId, fiber.StringTable, _globalStrings);
                    AcsPrintHudOpcodes.AppendBind(fiber.PrintWork, command);
                }
                return true;
            }
            case AcsPcodeWire.SetActorState:
                return ExecuteSetActorState(sim, fiber);
            case AcsPcodeWire.ThingDamage2:
            {
                if (fiber.Stack.Count < 3) { fiber.Done = true; return true; }
                var damageString = Pop(fiber);
                var damageAmount = Pop(fiber);
                var damageTid = Pop(fiber);
                var damageName = damageString >= 0 && damageString < fiber.StringTable.Length
                    ? fiber.StringTable[damageString] : null;
                fiber.Stack.Add(ThingDamage.Apply(sim, fiber.Activator, damageTid, damageAmount, damageName));
                return true;
            }
            default:
                return false;
        }
    }

    private bool ExecuteSetActorState(AuthoritySimulation sim, Fiber fiber)
    {
        if (fiber.Stack.Count < 3) { fiber.Done = true; return true; }
        var exactState = Pop(fiber) != 0;
        var stateString = Pop(fiber);
        var stateTid = Pop(fiber);
        var stateName = AcsStringIds.IsGlobalPool(stateString) ? GlobalStringAt(stateString)
            : stateString >= 0 && stateString < fiber.StringTable.Length ? fiber.StringTable[stateString] : null;
        var targets = stateTid == 0
            ? fiber.Activator is { Destroyed: false } stateActivator ? new[] { stateActivator } : Array.Empty<Actor>()
            : sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == stateTid).ToArray();
        var changedStates = 0;
        foreach (var target in targets)
            if (AcsActorStates.TryFindNamedState(target, stateName, exactState, out var stateIndex))
            {
                target.States.Enter(target, stateIndex);
                changedStates++;
            }
        fiber.Stack.Add(changedStates);
        return true;
    }
}
