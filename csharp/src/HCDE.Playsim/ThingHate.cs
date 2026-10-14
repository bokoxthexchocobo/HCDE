namespace HCDE.Playsim;

public static class ThingHate
{
    internal static bool? ExecuteSpecial(AuthoritySimulation sim, int special, Actor? activator, int haterTid, int hateeTid, int mode)
        => special == 177 ? Execute(sim, activator, haterTid, hateeTid, mode) : null;

    public static bool Execute(AuthoritySimulation sim, Actor? activator, int haterTid, int hateeTid, int mode)
    {
        if (haterTid == 0 && activator is PlayerPawn) return false;
        var haters = haterTid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : AcsActorTid.AllFromTid(sim, haterTid).Where(a => a.Health > 0 && a.Shootable).ToArray();
        var hatees = AcsActorTid.AllFromTid(sim, hateeTid).Where(a => a.Shootable && a.Health > 0 && !a.Dormant).ToArray();
        var index = 0;
        foreach (var hater in haters)
        {
            // Built-in managed monsters represent chase through their brain without a See label.
            if (!hater.States.HasState(hater.SeeState) && hater.Brain is null) continue;
            if (mode != 0)
            {
                hater.TidToHate = hateeTid;
                hater.LastLookActorId = null;
                if (hateeTid != 0)
                {
                    var target = sim.Actors.FirstOrDefault(a => a.Id == hater.TargetId && !a.Destroyed);
                    if (target is not null && target.ThingId != hateeTid) hater.SetTargetPointer(null);
                    hater.Brain?.FilterHateMemory(sim, hateeTid);
                }
            }
            hater.NoSightCheck = mode is 2 or 4 or 6;
            hater.HuntPlayers = mode is 3 or 4;
            hater.NoHatePlayers = mode is 5 or 6;
            Actor? hatee = hateeTid == 0 ? activator : hatees.FirstOrDefault();
            if (hateeTid != 0 && mode != 0 && hatees.Length != 0)
            {
                hatee = null;
                for (var checkedActors = 0; checkedActors < hatees.Length; checkedActors++)
                {
                    index = (index + 1) % hatees.Length;
                    if (hatees[index] == hater) continue;
                    hatee = hatees[index];
                    break;
                }
            }
            if (hatee is null || hatee.Destroyed || hatee == hater || mode != 0 && (hater.GoalId is null || hater.TargetId == hater.GoalId)) continue;
            hater.Brain?.RememberHateTarget(hater.TargetId);
            hater.SetTargetPointer(hatee);
            if (!hater.Dormant && hater.Health > 0 && hater.States.HasState(hater.SeeState)) hater.States.Enter(hater, hater.SeeState);
        }
        return true;
    }
}
