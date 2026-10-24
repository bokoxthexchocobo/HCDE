# ACS CallFunc sound sequences and spawn-facing — chunk 6 audit

## Scope

| Native | Lines | Managed |
| --- | --- | --- |
| `DLevelScript::DoSpawnSpotFacing` | 3895–3914 | `AcsMapMutations.SpawnSpotFacing`, `ACSF_SpawnSpotFacingForced`, `PCD_SpawnSpotFacing` |
| `ACSF_SoundSequenceOnActor` | 5816–5841 | `AcsSoundSequenceActions.StartOnActor` |
| `ACSF_SoundSequenceOnSector` | 5843–5858 | `AcsSoundSequenceActions.StartOnSectors` |
| `ACSF_SoundSequenceOnPolyobj` | 5860–5873 | `AcsSoundSequenceActions.StartOnPolyobj` |

**Behavior native LOC credited:** 175 (scripting / `p_acs.cpp` subsystem).

## Tests

`AcsSoundSequenceCallFuncTests`, `AcsHotCallFuncBehaviorTests.SpawnSpotFacingForcedUsesActivatorYaw`.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
