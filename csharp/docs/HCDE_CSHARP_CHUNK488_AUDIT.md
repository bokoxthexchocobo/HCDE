# Chunk 488 — SpawnMapThing ShouldSpawn gate and actor placement UDMF tail

## Scope

SpawnMapThing ShouldSpawn gate and actor placement UDMF tail

## Native ranges

- `src/playsim/p_mobj.cpp` **FLevelLocals::SpawnMapThing** 6871–6881 (11 lines)
- `src/playsim/p_mobj.cpp` **FLevelLocals::SpawnMapThing** 6942–7094 (153 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmPMobjSpawnMapThingCompose.cs`, `VmPMobjSpawnMapThingComposeTests.cs`
Managed LOC (tally): 100.
Conversion commit: `f56f49a695e2a46785f0e82a9d947ae93591e186`.

## Tests

- `VmPMobjSpawnMapThingComposeTests.Gate_RequiresVirtualTrue`
- `VmPMobjSpawnMapThingComposeTests.Z_CeilingWinsOverFloat`
- `VmPMobjSpawnMapThingComposeTests.Conversation_NeedsValidRoot`
- `VmPMobjSpawnMapThingComposeTests.Udmf_AlphaWhenNonNegative`
- `VmPMobjSpawnMapThingComposeTests.Light_OverrideClearsFlag`
- `VmPMobjSpawnMapThingComposeTests.Health_PositiveScales`
- `VmPMobjSpawnMapThingComposeTests.Health_DieFromSpawnWhenZero`
- `VmPMobjSpawnMapThingComposeTests.Spawn_FloatbobCompatRecordsSpecial`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 488 --to 488` (0 hard / 0 soft).

## Seams

`VMCall` ShouldSpawn, `StaticSpawn`, `CallBeginPlay`, and `GetConversation` are not hosted.

## Credit rationale

Credits virtual spawn gate, mapthing Z selection, conversation/UDMF/light/health placement policy from `p_mobj.cpp` (6871–6881, 6942–7094).
