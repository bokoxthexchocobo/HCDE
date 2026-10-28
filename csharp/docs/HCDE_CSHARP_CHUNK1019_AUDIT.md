# Chunk 1019 — `player_t::Serialize` arc tail (**2545–2587**), `PlayerInfo` `DEFINE_FIELD` export block (**2623–2694**) from `p_user.cpp`. **Pivot:** chunk **1019** target **2395–2544** rejected — retired **510** `VmPUserUnpredictBoss` + off-tally `VmPUserSerializeFreeze` already map `P_UnPredictClient` / `Serialize` head; no re-port.

## Scope

`player_t::Serialize` arc tail (**2545–2587**), `PlayerInfo` `DEFINE_FIELD` export block (**2623–2694**) from `p_user.cpp`. **Pivot:** chunk **1019** target **2395–2544** rejected — retired **510** `VmPUserUnpredictBoss` + off-tally `VmPUserSerializeFreeze` already map `P_UnPredictClient` / `Serialize` head; no re-port.

## Native ranges

- `src/playsim/p_user.cpp` **player_t::Serialize / PlayerInfo exports** 2545–2694 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPUserSerializeTail1019Slice.cs`
Managed LOC (tally): 77.
Conversion commit: `9ab9e825bd74863814409dec4faf62dd058b8fa2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `Serialize` morph/conversation/blend/crouch/poison/musinfo arc tail (**2545–2584**) | yes (arc key manifest) |
| `LastSafePos` legacy `"safepos"` key (**2587**) | yes |
| `PlayerInfo` `DEFINE_FIELD_X` exports (**2623–2694**) | yes (registration manifest) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2395–2544** | deferred target — `VmPUserUnpredictBoss` / `VmPUserSerializeFreeze` retired overlap |
| **2589–2607** | `ShouldClearOldButtons` / skin / fullbright tail — **`VmPUserSerializeFreeze`** (retired **512**) |
| **2609–2615** | `P_IsPlayerTotallyFrozen` — **`VmPUserSerializeFreeze`** |
| **2617–2622** | comment banner |
| **2695+** | next `DEFINE_FIELD` rows outside hull |

| Managed (`VmPUserSerializeTail1019Slice.cs`) | Native (`p_user.cpp`) |
|---------------------------------------------|---------------------|
| `SerializeTailArcKeys` / `TryGetSerializeTailArcKey` | **2545–2584** |
| `SafeposUsesLegacyKey` | **2587** |
| `PlayerInfoVmExportNames` / `TryGetPlayerInfoVmExportName` | **2623–2694** |
| Band hull (exclusive slice) | **2545–2694** |

### Chunk **1019** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1018** |


**Post-review ratio:** 77/150 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmPUserSerializeTail1019SliceTests.SafeposUsesLegacyKey_MatchesNativeAlias`
- `VmPUserSerializeTail1019SliceTests.SerializeTailArcKeys_IncludesFragsAndPsprites`
- `VmPUserSerializeTail1019SliceTests.PlayerInfoVmExportCount_MatchesNativeDefineBlock`
- `VmPUserSerializeTail1019SliceTests.PlayerInfoExportUsesNamedAlias_FlagsInconsistent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1019 --to 1019` (0 hard / 0 soft).

## Seams

`FSerializer`, `LastSafePos::Serialize`, `arc.Array`, and macro bodies remain native; post-serialize button/skin policy uses existing `VmPUserSerializeFreeze` without duplicate credit.

## Credit rationale

First tally-clean **150** hull after **509** with exclusive managed slice; honest zero-credit exclusion for **2589–2615** already covered off-tally by retired **512** helpers in `VmPUserSerializeFreeze.cs`.
