using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client;

/// <summary>
/// One-ray software view. Blocking lines become solid columns. Ceiling and floor fill the rest.
/// Not <c>rendering/swrenderer</c>.
/// </summary>
public static class SoftwareView
{
    public const byte CeilingR = 40;
    public const byte CeilingG = 40;
    public const byte CeilingB = 80;
    public const byte FloorR = 40;
    public const byte FloorG = 30;
    public const byte FloorB = 20;
    public const byte WallR = 180;
    public const byte WallG = 40;
    public const byte WallB = 40;
    public const double FovRadians = Math.PI / 2;

    public static void Draw(AuthoritySimulation sim, SoftwareFrame frame)
    {
        var horizon = frame.Height / 2;
        for (var y = 0; y < frame.Height; y++)
        {
            var ceiling = y < horizon;
            for (var x = 0; x < frame.Width; x++)
            {
                if (ceiling)
                    frame.Plot(x, y, CeilingR, CeilingG, CeilingB);
                else
                    frame.Plot(x, y, FloorR, FloorG, FloorB);
            }
        }

        var player = sim.Players.FirstOrDefault();
        if (player == null)
            return;

        var yaw = player.Angle.ToDegrees() * (Math.PI / 180.0);
        var originX = player.X.ToDouble();
        var originY = player.Y.ToDouble();
        for (var x = 0; x < frame.Width; x++)
        {
            var ndc = (x + 0.5) / frame.Width - 0.5;
            var ray = yaw + ndc * FovRadians;
            var dx = Math.Cos(ray);
            var dy = Math.Sin(ray);
            var nearest = double.PositiveInfinity;
            foreach (var line in sim.Level.Lines)
            {
                if (!line.BlocksMovement)
                    continue;
                if (TryHit(originX, originY, dx, dy, line, out var distance) && distance < nearest)
                    nearest = distance;
            }

            if (double.IsPositiveInfinity(nearest))
                continue;

            var column = (int)Math.Clamp(frame.Height * 16.0 / nearest, 1, frame.Height);
            var top = (frame.Height - column) / 2;
            for (var y = top; y < top + column; y++)
                frame.Plot(x, y, WallR, WallG, WallB);
        }
    }

    internal static bool TryHit(double ox, double oy, double dx, double dy, LevelLine line, out double distance)
    {
        distance = 0;
        var sx = line.X2 - line.X1;
        var sy = line.Y2 - line.Y1;
        var denom = dx * sy - dy * sx;
        if (Math.Abs(denom) < 1e-8)
            return false;

        var qx = line.X1 - ox;
        var qy = line.Y1 - oy;
        distance = (qx * sy - qy * sx) / denom;
        var along = (qx * dy - qy * dx) / denom;
        return distance > 0.05 && along >= 0 && along <= 1;
    }
}
