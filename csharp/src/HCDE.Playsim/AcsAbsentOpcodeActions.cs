using System.Linq;
using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native ACS pcodes that were matrix-absent before chunk 10 (p_acs.cpp).</summary>
internal static class AcsAbsentOpcodeActions
{
    private const int BlockNothing = 0;
    private const int BlockCreatures = 1;
    private const int BlockEverything = 2;
    private const int BlockRailing = 3;
    private const int BlockPlayers = 4;

    public static void SectorSound(
        AuthoritySimulation sim,
        int volume,
        int soundId,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        LevelLine? triggerLine,
        Actor? activator)
    {
        var sound = AcsStringIds.LookupTerminated(soundId, stringTable, globalStrings);
        if (string.IsNullOrEmpty(sound)) return;
        var vol = volume / 127f;
        if (triggerLine is not null && triggerLine.SideFront >= 0
            && triggerLine.SideFront < sim.Level.Sides.Count)
        {
            var sector = sim.Level.Sides[triggerLine.SideFront].Sector;
            sim.QueueSectorAcsSound(sector, sound, vol);
            return;
        }

        if (activator is { Destroyed: false })
            sim.QueueSoundRequest(new(activator.Id, sound, 0, 0, vol, false, 1, false));
    }

    public static void AmbientSound(int volume, int soundId, string[] stringTable, AcsGlobalStrings globalStrings, AuthoritySimulation sim)
    {
        var sound = AcsStringIds.LookupTerminated(soundId, stringTable, globalStrings);
        if (string.IsNullOrEmpty(sound)) return;
        sim.QueueAmbientAcsSound(sound, volume / 127f);
    }

    public static void SoundSequence(
        AuthoritySimulation sim,
        int sequenceId,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        LevelLine? triggerLine)
    {
        if (triggerLine is null || triggerLine.SideFront < 0 || triggerLine.SideFront >= sim.Level.Sides.Count)
            return;
        var name = AcsStringIds.LookupTerminated(sequenceId, stringTable, globalStrings);
        if (string.IsNullOrEmpty(name)) return;
        sim.SetSectorAcsSoundSequence(sim.Level.Sides[triggerLine.SideFront].Sector, name);
    }

    public static void SetLineBlocking(AuthoritySimulation sim, int lineId, int mode)
    {
        foreach (var line in AcsLineActivation.LinesFromId(sim, lineId))
        {
            var flags = line.Flags;
            flags &= ~(LevelLine.BlockingFlag | LevelLine.BlockEverythingFlag | LevelLine.RailingFlag | LevelLine.BlockPlayersFlag);
            switch (mode)
            {
                case BlockNothing:
                    break;
                case BlockEverything:
                    flags |= LevelLine.BlockingFlag | LevelLine.BlockEverythingFlag;
                    break;
                case BlockRailing:
                    flags |= LevelLine.RailingFlag | LevelLine.BlockingFlag;
                    break;
                case BlockPlayers:
                    flags |= LevelLine.BlockPlayersFlag;
                    break;
                default:
                    flags |= LevelLine.BlockingFlag;
                    break;
            }

            line.Flags = flags;
        }
    }

    public static void SetLineSpecial(
        AuthoritySimulation sim,
        int lineId,
        int special,
        int arg0,
        int arg1,
        int arg2,
        int arg3,
        int arg4)
    {
        foreach (var line in AcsLineActivation.LinesFromId(sim, lineId))
        {
            line.Special = special;
            line.Arg0 = arg0;
            line.Arg1 = arg1;
            line.Arg2 = arg2;
            line.Arg3 = arg3;
            line.Arg4 = arg4;
        }
    }

    public static void SetMugshotState(AuthoritySimulation sim, int stateId, string[] stringTable, AcsGlobalStrings globalStrings, Actor? activator)
    {
        var state = AcsStringIds.LookupTerminated(stateId, stringTable, globalStrings);
        if (activator is PlayerPawn { Destroyed: false } player)
            player.MugshotState = state;
        else if (sim.Players.Count() > 0)
            sim.Players.First().MugshotState = state;
    }

    public static void TranslationRange5() { }

    public static int CheckActorFloorTexture(
        AuthoritySimulation sim,
        int tid,
        int textureId,
        string[] stringTable,
        AcsGlobalStrings globalStrings,
        Actor? activator)
    {
        var expected = AcsStringIds.LookupTerminated(textureId, stringTable, globalStrings);
        var actor = tid == 0 ? activator : AcsActorTid.AllFromTid(sim, tid).FirstOrDefault();
        if (actor is null || actor.SectorIndex < 0 || actor.SectorIndex >= sim.Level.Sectors.Count)
            return 0;
        var floor = sim.Level.Sectors[actor.SectorIndex].FloorPic;
        return string.Equals(floor, expected, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
    }

    public static int UnmorphActor(AuthoritySimulation sim, int tid, bool force, Actor? activator)
    {
        _ = force;
        if (tid == 0 && activator is { Destroyed: false, IsMorphForm: true })
        {
            activator.IsMorphForm = false;
            return 1;
        }

        var changes = 0;
        foreach (var actor in (tid == 0 && activator is { Destroyed: false }
            ? new[] { activator }
            : AcsActorTid.AllFromTid(sim, tid)))
        {
            if (actor.IsMorphForm)
            {
                actor.IsMorphForm = false;
                changes++;
            }
        }

        return changes;
    }

    public static int GetPlayerInput(AuthoritySimulation sim, int playerNum, int inputNum, Actor? activator)
    {
        if (!sim.IsPlayerInGame(playerNum)) return 0;
        return sim.Players.FirstOrDefault(p => p.PlayerNum == playerNum)?.GetPlayerInput(inputNum) ?? 0;
    }

    public static void AppendPrintBinary(AuthoritySimulation sim, int value) =>
        sim.AppendAcsPrint(Convert.ToString(value, 2));

    public static void AppendPrintHex(AuthoritySimulation sim, int value) =>
        sim.AppendAcsPrint(value.ToString("X", System.Globalization.CultureInfo.InvariantCulture));
}
