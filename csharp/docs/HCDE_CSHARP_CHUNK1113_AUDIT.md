# Chunk 1113 — Actor tick tail, nightmare respawn, `CheckNoDelay`, and `CheckSectorTransition` action bits from `p_mobj.cpp` (**5127–5276**). Skips credited movement/tick hulls through **5075**.

## Scope

Actor tick tail, nightmare respawn, `CheckNoDelay`, and `CheckSectorTransition` action bits from `p_mobj.cpp` (**5127–5276**). Skips credited movement/tick hulls through **5075**.

## Native ranges

- `src/playsim/p_mobj.cpp` **Tick tail CheckNoDelay sector transition** 5127–5276 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjTickTail1113Slice.cs`
Managed LOC (tally): 140.
Conversion commit: `ad927b91ae0a0bd46a2c2ee63f889841859a7b6b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Terrain flat touch gate (**5126–5128**) | yes |
| Null state destroy (**5132–5135**) | yes |
| Tic countdown (**5142–5151**) | yes |
| Bone recalc skip (**5154–5157**) | yes |
| Nightmare respawn gates (**5159–5190**) | yes |
| `CheckNoDelay` (**5200–5224**) | yes |
| Sector enter/hit bits (**5251–5262**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **5076–5126** | inside credited tick / outside hull |
| **5121–5125**, **5140**, **5190** | host damage/render/respawn calls |
| **5227–5231** | VM thunk |
| **5277+** | transition tail outside hull |

| Managed (`VmPMobjTickTail1113Slice.cs`) | Native (`p_mobj.cpp`) |
|----------------------------------------|---------------------|
| Tick tail + no-delay + sector bits | **5127–5276** |
| Band hull (exclusive slice) | **5127–5276** |

### Chunk **1113** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1112** |


**Post-review ratio:** 140/150 managed/native ≈ 93% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjTickTail1113SliceTests.ShouldApplyTerrainFlat_WhenTouchingOrInWater`
- `VmPMobjTickTail1113SliceTests.ResolveRespawnMonsterTics_ScalesNegativeMinimum`
- `VmPMobjTickTail1113SliceTests.TryNightmareRespawn_SpawnsWhenDiceAndCadenceAllow`
- `VmPMobjTickTail1113SliceTests.RunCheckNoDelay_ReturnsFalseWhenActionFreesActor`
- `VmPMobjTickTail1113SliceTests.BuildSectorEnterActions_OrsFloorAndCeiling`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1113 --to 1113` (0 hard / 0 soft).

## Seams

`P_ActorInSpecialSector`, `SetState`, `CalcBones`, `P_NightmareRespawn`, and sector triggers remain host-injected.

## Credit rationale

First ≥150 UNCredited window after **1112**: **5127–5276**. **2251–2400** is **1112**; **2421–5075** overlaps **724**/**725**/**728**.
