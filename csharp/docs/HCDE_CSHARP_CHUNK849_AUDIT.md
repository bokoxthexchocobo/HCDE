# Chunk 849 — `FParser::SF_SpawnShot2` … `SaveCurrentScript` from `fragglescript/t_func.cpp` (3700–3849). Retired (2026-10-23): out-of-body credit removed; honest **102** < **150**.

## Scope

`FParser::SF_SpawnShot2` … `SaveCurrentScript` from `fragglescript/t_func.cpp` (3700–3849). Retired (2026-10-23): out-of-body credit removed; honest **102** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `fd1d19f8ac22191c339ce0107d502594970bd4a0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_SpawnShot2` | yes |
| `SF_KillInSector` | yes |
| `SF_SetLineTrigger` | yes |
| `RunLineSpecial` | yes |
| `SaveCurrentScript` | yes |

| Managed | Native (`t_func.cpp` 3700–3849) |
|---------|-------------------------|
| `SfSpawnShot2` | **3707–3743** |
| `SfKillInSector` | **3753–3766** |
| `SfSetLineTrigger` | **3776–3799** |
| `SfRunLineSpecial` | **3808–3821** |
| `SaveCurrentScript` | **3830–3842** |
| (separators) | **3700–3706**, **3744–3752**, **3767–3775**, **3800–3807**, **3822–3829**, **3843–3849** |

### Out-of-body uncredit (849)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **48** |
| **Honest** | **102** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 849 --to 849` (0 hard / 0 soft).

## Seams

Uses `SpawnProjectile`, `TagManager` sector tags, line id lookup, `LineSpecials.TryActivate`, and `VmFScriptThinker.AddRunningScript`.

## Credit rationale

GZDoom spawn/kill/line band after **847** `setcolor`; script wait tail in **850**.
