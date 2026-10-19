using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::ChangeFlat</c>, <c>SetLineTexture</c>, and <c>DoSpawn*</c> ACS helpers.</summary>
internal static class AcsMapMutations
{
    private const int TextureTop = 0;
    private const int TextureMiddle = 1;
    private const int TextureBottom = 2;

    public static void ChangeFlat(
        AuthoritySimulation sim,
        int tag,
        int nameStringId,
        bool ceiling,
        string[] stringTable,
        AcsGlobalStrings globalStrings)
    {
        var flatName = AcsStringIds.LookupTerminated(nameStringId, stringTable, globalStrings);
        if (string.IsNullOrEmpty(flatName)) return;
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag)) continue;
            if (ceiling) sector.CeilingPic = flatName;
            else sector.FloorPic = flatName;
        }
    }

    public static void SetLineTexture(
        AuthoritySimulation sim,
        int lineId,
        int side,
        int position,
        int nameStringId,
        string[] stringTable,
        AcsGlobalStrings globalStrings)
    {
        var textureName = AcsStringIds.LookupTerminated(nameStringId, stringTable, globalStrings);
        if (string.IsNullOrEmpty(textureName) || lineId == 0) return;
        side = side != 0 ? 1 : 0;
        foreach (var line in AcsLineActivation.LinesFromId(sim, lineId))
        {
            var sideIndex = side == 0 ? line.SideFront : line.SideBack;
            if ((uint)sideIndex >= (uint)sim.Level.Sides.Count) continue;
            var sidedef = sim.Level.Sides[sideIndex];
            switch (position)
            {
                case TextureTop: sidedef.TopTexture = textureName; break;
                case TextureMiddle: sidedef.MidTexture = textureName; break;
                case TextureBottom: sidedef.BottomTexture = textureName; break;
            }
        }
    }

    public static void ClearLineSpecial(LevelLine? triggerLine)
    {
        if (triggerLine is not null) triggerLine.Special = 0;
    }

    public static int Spawn(
        AuthoritySimulation sim,
        int typeStringId,
        int x,
        int y,
        int z,
        int tid,
        int angleByte,
        bool force,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        Actor? activator)
    {
        var className = AcsStringIds.LookupTerminated(typeStringId, stringTable, globalStrings);
        if (string.IsNullOrEmpty(className)) return 0;
        return sim.SpawnAcsActor(className, x / 65536.0, y / 65536.0, z / 65536.0, tid, angleByte, force);
    }

    public static int SpawnSpot(
        AuthoritySimulation sim,
        int typeStringId,
        int spotTid,
        int tid,
        int angleByte,
        bool force,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        Actor? activator)
    {
        var className = AcsStringIds.LookupTerminated(typeStringId, stringTable, globalStrings);
        if (string.IsNullOrEmpty(className)) return 0;
        var spawned = 0;
        if (spotTid != 0)
        {
            foreach (var actor in sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == spotTid).ToArray())
                spawned += sim.SpawnAcsActor(className, actor.X.ToDouble(), actor.Y.ToDouble(), actor.Z.ToDouble(),
                    tid, angleByte, force, actor.Angle);
        }
        else if (activator is { Destroyed: false })
        {
            spawned += sim.SpawnAcsActor(className, activator.X.ToDouble(), activator.Y.ToDouble(), activator.Z.ToDouble(),
                tid, angleByte, force, activator.Angle);
        }

        return spawned;
    }

    public static int SpawnSpotFacing(
        AuthoritySimulation sim,
        int typeStringId,
        int spotTid,
        int tid,
        bool force,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        Actor? activator)
    {
        var className = AcsStringIds.LookupTerminated(typeStringId, stringTable, globalStrings);
        if (string.IsNullOrEmpty(className)) return 0;
        var spawned = 0;
        if (spotTid != 0)
        {
            foreach (var actor in sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == spotTid).ToArray())
                spawned += sim.SpawnAcsActor(className, actor.X.ToDouble(), actor.Y.ToDouble(), actor.Z.ToDouble(),
                    tid, 0, force, actor.Angle);
        }
        else if (activator is { Destroyed: false })
        {
            spawned += sim.SpawnAcsActor(className, activator.X.ToDouble(), activator.Y.ToDouble(), activator.Z.ToDouble(),
                tid, 0, force, activator.Angle);
        }

        return spawned;
    }

    public static int ResolveTaggedTypeStringId(int stringId, string[] stringTable, AcsGlobalStrings globalStrings) =>
        globalStrings.Add(AcsStringIds.LookupTerminated(stringId, stringTable, globalStrings));
}
