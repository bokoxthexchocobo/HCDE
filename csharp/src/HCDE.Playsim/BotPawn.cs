using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>
/// Walks forward one thrust per tic. Used by invasion waves and by <see cref="AuthoritySimulation.AddBot"/>.
/// </summary>
public sealed class BotPawn : Actor
{
    public const short WalkCommand = 2048;

    public override void Tick()
    {
        RememberPosition();
        if (Level != null)
        {
            var (dx, dy) = Movement.Thrust(Angle, WalkCommand, 0);
            var moved = LineSlide.Move(Level, X.ToDouble(), Y.ToDouble(), Radius.ToDouble(), dx, dy);
            X = Fixed.FromDouble(moved.X);
            Y = Fixed.FromDouble(moved.Y);
        }

        base.Tick();
    }
}
