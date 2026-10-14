namespace HCDE.Playsim;

public static class ThingSetGoal
{
    internal static bool? ExecuteSpecial(AuthoritySimulation sim, int special, int tid, int goalTid, int delay, int chaseGoal)
        => special == 229 ? Execute(sim, tid, goalTid, delay, chaseGoal) : null;

    public static bool Execute(AuthoritySimulation sim, int tid, int goalTid, int delay, int chaseGoal)
    {
        var goal = AcsActorTid.AllFromTid(sim, goalTid).FirstOrDefault(a => a.ClassDoomEdNum == PatrolMarkers.PointType);
        var actors = AcsActorTid.AllFromTid(sim, tid).ToArray();
        foreach (var actor in actors)
        {
            if (!actor.Shootable) continue;
            if (actor.TargetId == actor.GoalId) actor.SetTargetPointer(null);
            actor.GoalId = goal?.Id;
            actor.ChaseGoal = chaseGoal != 0;
            if (actor.TargetId is null) actor.ReactionTime = unchecked(delay * 35);
        }
        return actors.Length != 0;
    }
}
