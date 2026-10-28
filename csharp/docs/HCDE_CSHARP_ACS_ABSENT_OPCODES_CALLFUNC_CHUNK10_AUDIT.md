# ACS absent opcodes + CallFunc gap fill — chunk 10 audit

## Scope

Closed all **12** matrix-absent ACS word opcodes and **26** of **44** absent `EACSFunctions` CallFunc wires (remaining **18** HUD/named-ACS/warp/OpenGL helpers).

### ACS pcodes (now executed)

| Opcode | Implementation |
| --- | --- |
| SectorSound, AmbientSound, SoundSequence | `AcsAbsentOpcodeActions` + sound request queue |
| SetLineBlocking, SetLineSpecial | Line id iteration via `AcsLineActivation.LinesFromId` |
| SetMugshotState | `PlayerPawn.MugshotState` |
| TranslationRange5 | Stack consume no-op (palette tint deferred) |
| CheckActorFloorTexture, GetPlayerInput, UnmorphActor | Sector flat compare, `ReadPlayerInput`, `IsMorphForm` |
| PrintBinary, PrintHex | `AuthoritySimulation.AcsPrintBuffer` |

### CallFunc wires added

| Wire | Name |
| ---: | --- |
| 18 | SetSkyScrollSpeed |
| 53–59 | SetCVar, GetUserCVar, SetUserCVar, GetCVarString, SetCVarString, GetUserCVarString, SetUserCVarString |
| 19620–19621 | GetTeamScore, SetTeamScore |
| 19700–19715 | Invasion query/control API (via `InvasionDirector.QueryAcs` + `TryInvoke`) |

### Still absent (18)

AnnouncerSound, SetPointer, ACS_NamedExecute/Suspend/Terminate/Locked*/ExecuteWithResult/ExecuteAlways, SetHUDClipRect/WrapWidth, Warp, SetTranslation, ScriptCall, StartSlideshow, SetSubtitleNumber, SetSectorGlow, SetFogDensity.

## Tests

`AcsAbsentOpcodeChunk10Tests` (11 cases).

## Verified

`dotnet build -warnaserror`; `HCDE.Playsim.Tests` (17060); `HCDE.MigrationMatrix.Tests` (2). Matrix: **273/273** ACS pcode executed; CallFunc **120 executed / 18 absent**.

**Conversion native LOC credited:** 842.
