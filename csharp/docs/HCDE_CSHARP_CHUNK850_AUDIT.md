# Chunk 850 — `FParser::SF_Wait` … `SF_ScriptRunning` from `fragglescript/t_func.cpp` (3850–4008). Retired (2026-10-23): out-of-body credit removed; honest **124** < **150**.

## Scope

`FParser::SF_Wait` … `SF_ScriptRunning` from `fragglescript/t_func.cpp` (3850–4008). Retired (2026-10-23): out-of-body credit removed; honest **124** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `68b1174374bdd3943d9bb099efa28170ebd059f9`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_Wait` | yes |
| `SF_TagWait` | yes |
| `SF_ScriptWait` | yes |
| `SF_ScriptWaitPre` | yes |
| `SF_StartScript` | yes |
| `SF_ScriptRunning` | yes |

| Managed | Native (`t_func.cpp` 3850–4008) |
|---------|-------------------------|
| `SfWait` | **3850–3866** |
| `SfTagWait` | **3874–3889** |
| `SfScriptWait` | **3897–3912** |
| `SfScriptWaitPre` | **3920–3934** |
| `SfStartScript` | **3942–3973** |
| `SfScriptRunning` | **3981–4008** |
| (separators) | **3867–3873**, **3890–3896**, **3913–3919**, **3935–3941**, **3974–3980** |

### Chunk **850** gross vs net (849/850 parallel WIP)

**Incident:** **850** WIP committed wait bodies into **`VmFScriptParserSfGzd.cs`** while **849** still owned that file (same parallel-WIP class as **846**/**847**). First tally hit **`managedLoc_mismatch`** / MigrationMatrix exclusivity.

**Remedy:** **`996516d0`** split wait ports to **`VmFScriptParserSfWait.cs`**; **`land_chunk_fast.sh --resume 850`** at **`582adc71`** refreshed rows via **`fix_chunk_tally.py`** (no hand-edited `managedLoc`).

| Row | `nativeLoc` | `managedLoc` | File |
|-----|------------:|-------------:|------|
| **849** | **150** | **87** | `VmFScriptParserSfGzd.cs` |
| **850** | **159** | **122** | `VmFScriptParserSfWait.cs` |

Reaudit **849** and **850** each **0/0** at **`582adc71`**. Net **848–850** headline: **100,383** → **100,882** (**+499**). |

### Out-of-body uncredit (850)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **159** |
| **Out-of-body removed** | **35** |
| **Honest** | **124** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 850 --to 850` (0 hard / 0 soft).

## Seams

Uses `SaveCurrentScript`, `VmFScriptThinker.AddRunningScript`, and `FsTerminatorException` for script yield.

## Credit rationale

Script wait/start band after **849** `SaveCurrentScript`; **4017+** `InitFunctions` in **851**.
