# Chunk 714 — Whole `AActor::Serialize` (`p_mobj.cpp` 210–421). `PostSerialize` and `DBehavior::Serialize` excluded.

## Scope

Whole `AActor::Serialize` (`p_mobj.cpp` 210–421). `PostSerialize` and `DBehavior::Serialize` excluded.

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::Serialize** 210–421 (212 lines)

**Credited native LOC:** 212 (tally `nativeLoc` 212).

## Managed

`VmPMobjActorSerialize.cs`
Managed LOC (tally): 218.
Conversion commit: `84f379c45dcb8cfad11958a3ab5c8065cf797214`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SerializeSuperCalled` / `PersistsScalarField` / `PersistsReferenceField` / `RecordField` | `AActor::Serialize` 210–421 |
| `ShouldSerializeUserLights` / `RollbackSkipsUserLights` / `NonRollbackInventoryTics` | 417–420 (`if (!arc.IsRollback())`) |
| `SerializeTerrainRoundTrip` / `SerializeArgsWhenSpecial` / `ResolveArgsCount` / `ApplySerializeTail` | 414–415 |
| `ShouldPersistGhostBits` / `ShouldPersistThruSpecies` / `ShouldPersistMthruSpecies` / `ShouldPersistNonshootable` / `ShouldPersistHitOwner` | 251–258 (`flags`–`flags8`) |
| `MorphBlockSkippedOnRollback` / `DamageSourcePersisted` / `UnmorphTimePersist` / `BehaviorsArrayPersist` / `ModelDataPersist` | 404–409 |
| `PoisonFieldsPersist` / `ConversationFieldsPersist` / `RenderHiddenRequired` / `StealthAlphaPersist` | 360–392 |
| `ViewAnglePersist` / `SpriteOffsetPersist` / `WorldOffsetPersist` / `LandingSpeedPersist` / `CameraPersist` / `VisibleConePersist` | 377–401 |
| `FloatBobPersist` / `WeaveIndexPersist` / `FreezeTicsPersist` / `DamageFactorPersist` / `PushFactorPersist` / `RipLevelPersist` / `TeleFogTypesPersist` | 295–374 |

### Out-of-body uncredit (714)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **212** |
| **Out-of-body removed** | **0** |
| **Honest** | **212** |


**Post-review ratio:** 218/212 managed/native ≈ 103% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjActorSerializeTests.Serialize_SkipsUserLightsOnRollback`
- `VmPMobjActorSerializeTests.Serialize_PersistsGhostFlags`
- `VmPMobjActorSerializeTests.Serialize_ArgsOnlyWhenSpecial`
- `VmPMobjActorSerializeTests.Serialize_TailInvokesUserLightsWhenNotRollback`
- `VmPMobjActorSerializeTests.Serialize_ThruSpeciesGate`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 714 --to 714` (0 hard / 0 soft).

## Seams

`FSerializer`, `Super::Serialize`, `SerializeTerrain`, `SerializeArgs`, and live `UserLights` I/O remain native.

## Credit rationale

Fresh conversion credit for actor savegame field archive band; pose-persistence tests reference this native span.
