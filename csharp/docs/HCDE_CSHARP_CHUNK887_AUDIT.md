# Chunk 887 — Retired (2026-10-23): invalid land — banned seams (`RotatePolyobj`/`MovePolyobj` call-recording stubs, `EnvironmentCatalog` `S_FindEnvironment` stand-in). Shrinking band to drop poly/reverb yields **131** `nativeLoc` (<**150** floor). Matrix credit **0**.

## Scope

Retired (2026-10-23): invalid land — banned seams (`RotatePolyobj`/`MovePolyobj` call-recording stubs, `EnvironmentCatalog` `S_FindEnvironment` stand-in). Shrinking band to drop poly/reverb yields **131** `nativeLoc` (<**150** floor). Matrix credit **0**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmPSavegSubsectorsPortals.cs`, `VmPSavegSubsectorsPortalsTests.cs`
Managed LOC (tally): 0.
Conversion commit: `d7870cf36389cf0acfa739f73efd037fe0d7c683`.

## Padding review (2026-10-21)

### `land_chunk_fast.sh` incident (single agent; no parallel land/verify)

| Step | Outcome |
|------|--------|
| `land_chunk_fast.sh 887` (first invocation) | **`uncredited_range_check --band` OK** → staged port → **`fatal: cannot lock ref 'HEAD'`** (exit **128**): concurrent **`git commit`** moved HEAD from **`b8abca74`** to **`e1d05f3b`** (session-audit commit) while land still expected prior HEAD — **not** a second `land_chunk_fast` or manual verify run. |
| `land_chunk_fast.sh 887` (immediate retry) | Port commit **`56537f4f`** created; **`sync_loc_tally` / `generate_native_managed_matrix`** ran; **`reaudit_chunks.py`** hard fail **`managed_padding_signature`** on empty `SavegArchive.EndObject` → exit **1** **before** tally commit/verify/push; **`loc_tally.json` / matrix .md left modified unstaged** on working tree. |
| Fix commit **`5f8d0b29`** | `EndObject` stack pop (reaudit hygiene). |
| `land_chunk_fast.sh --resume 887` | Tally **`60c4c0e2`**, verify **14/14**, push — **valid script land** but port later rejected for banned seams → **this retire** reverses matrix credit. |

### Seam review (why port is invalid)

| Native callee | Rejected approach | Required for faithful credit |
|---------------|-------------------|------------------------------|
| `FPolyObj::RotatePolyobj` / `MovePolyobj` on save read | `RotateCalls`/`MoveCalls` counters | Direct use of ported `VmPoManMoveRotate` (or equivalent whole-function `po_man.cpp` credit) on poly geometry |
| `S_FindEnvironment` | `EnvironmentCatalog` dictionary | Faithful `S_FindEnvironment` port or remove **539–553** from band |

### Behavior ported (matrix credit)

| Native | Behavior ported |
|--------|-----------------|
| `FLevelLocals::RecalculateDrawnSubsectors` **346–359** | **no** (retired) |
| `FLevelLocals::SerializeSubsectors` **367–451** | **no** |
| `Serialize(FLinePortal)` **459–473** | **no** |
| `Serialize(FSectorPortal)` **481–497** | **no** |
| `Serialize(FPolyObj)` **505–531** | **no** |
| `Serialize(ReverbContainer*)` **539–548** | **no** |
| `Serialize(zone_t)` **550–553** | **no** |

### Padding map (rejected land — not credited)

| Pattern | Verdict |
|---------|--------|
| `PolyObj` methods that increment call counters instead of moving vertices | **recorded-call queue** — banned |
| `EnvironmentCatalog` + `FindEnvironment` | **lookup-table stand-in** for `S_FindEnvironment` — banned |
| Hull **346–553** with seamful poly/reverb serializers | **invalid** — retired to **0** |
| Subsector+portal-only subset **346–497** | **131** `nativeLoc` — below **150** floor if poly/reverb removed |

### Gross vs net (**887** retire)

| | Value |
|--|------:|
| Gross (invalid land tally **`60c4c0e2`**) | **172** |
| Net behavior-ported (matrix after retire) | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 887 --to 887` (0 hard / 0 soft).

## Seams

Rejected port used `PolyObj.RotatePolyobj`/`MovePolyobj` counters instead of `VmPoManMoveRotate` (or other faithful `po_man.cpp` callees), and `FindEnvironment` + static `EnvironmentCatalog` instead of `S_FindEnvironment` (`s_environment.cpp`). `SavegArchive` FSerializer mirror is acceptable only when serializers are faithful end-to-end; poly/reverb branches were not.

## Credit rationale

Standing rules ban recorded-call queues and lookup-table stand-ins. Faithful fix would require wiring `Serialize(FPolyObj)` read path to real `VmPoManMoveRotate.RotatePolyobj`/`MovePolyobj` on poly state and porting or directly calling `S_FindEnvironment` for reverb/zone — not done. Remaining uncredited whole functions (**346–497** subsector + line/sector portals) sum **131** native lines on their own row (<**150**); cannot expand contiguously without poly (**505–531**) or reverb (**539–553**) without those callees.
