using HCDE.Gamedata;

namespace HCDE.Playsim;

public static class PatrolMarkers
{
    public const int PointType = 9024;
    public const int SpecialType = 9047;

    internal static bool IsMarker(int type) => type is PointType or SpecialType;

    internal static void Apply(Actor actor, int type, DehackedActor? patch)
    {
        if (!IsMarker(type)) return;
        if (patch is null) actor.Health = 1000;
        if (patch is not { WidthPatched: true }) actor.Radius = Fixed.FromInt(8);
        if (patch is not { HeightPatched: true }) actor.Height = Fixed.FromInt(8);
        if (patch is not { BitsPatched: true })
        {
            actor.Solid = actor.Shootable = false;
            actor.NoGravity = actor.NoBlockmap = true;
        }
        if (patch?.RenderStyle is null)
            actor.RenderStyle = actor.DefaultRenderStyle = ActorRenderStyle.FromNative(0);
        actor.HasOnlySpawnLabel = true;
    }
}
