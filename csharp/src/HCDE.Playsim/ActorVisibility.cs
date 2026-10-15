namespace HCDE.Playsim;

public sealed record ActorLookParameters(double MaxDistance = 0, double MinDistance = 0, double FieldOfView = 180)
{
    internal bool NativeSight { get; init; }
}

/// <summary>Represented flat-sector P_IsVisible range, view cone and sight checks.</summary>
internal static class ActorVisibility
{
    internal static bool IsVisible(AuthoritySimulation sim, Actor observer, Actor target, bool allAround,
        ActorLookParameters? parameters = null)
    {
        if (target.MVisBlocked) return false;
        var maximum = parameters?.MaxDistance ?? 0;
        var minimum = parameters?.MinDistance ?? 0;
        var fieldOfView = allAround ? 0 : parameters?.FieldOfView ?? 180;
        if (!double.IsFinite(maximum) || !double.IsFinite(minimum) || !double.IsFinite(fieldOfView))
            throw new ArgumentOutOfRangeException(nameof(parameters));
        var dx = target.X.ToDouble() - observer.X.ToDouble();
        var dy = target.Y.ToDouble() - observer.Y.ToDouble();
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (maximum != 0 && distance > maximum || minimum != 0 && distance < minimum) return false;
        if (fieldOfView != 0)
        {
            var angle = Math.Atan2(dy, dx) * 180 / Math.PI;
            if (Math.Abs(Math.IEEERemainder(angle - observer.Angle.ToDegrees(), 360)) > fieldOfView / 2
                && (minimum != 0 || distance > observer.MeleeRange.ToDouble() + observer.Radius.ToDouble())) return false;
        }
        return sim.NativeMonsterMovement || parameters?.NativeSight == true
            ? CombatTrace.CheckSight(sim, observer, target, ActorSightFlags.SeePastShootableLines)
            : CombatTrace.HasLineOfSight(sim, observer, target);
    }
}
