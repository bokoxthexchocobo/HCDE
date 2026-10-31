# Chunk 1181 — Doom actor-name table and the static script RNG from t_func.cpp (1–218).

## Scope

Doom actor-name table and the static script RNG from t_func.cpp (1–218).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **ActorNames_init** 1–218 (218 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

`VmTFuncActorNames1181Slice.cs`
Managed LOC (tally): 167.
Conversion commit: `dd5feb0553eb3bb75e26e908beec7d4f040e14fe`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| pr_script FScript (**48–48**) | yes |
| ActorNames_init (**60–201**) | yes |
| ActorTypes null slots (**203–203**) | yes |
| Band hull (**1–218**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **219–245** | T_GetMobjType stays chunk 837 |
| **4017–4231** | InitFunctions fill stays chunk 851 |

The table is the original Doom actor order, 139 names, index 0 DoomPlayer through index 138 PointPuller. Cyberdemon is index 21 and Slalagtite is index 97. ActorTypes has one slot per name and static storage starts null. The script RNG stream is named FScript.

| Managed (`VmTFuncActorNames1181Slice.cs`) | Native (`t_func.cpp`) |
|------------------------------------------------|----------------------|
| Actor names, null slots, and FScript | **1–218** |
| Band hull (exclusive slice) | **1–218** |

### Chunk **1181** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **218** |
| **Net headline** | **+218** vs post-**1180** |


**Post-review ratio:** 167/218 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmTFuncActorNames1181SliceTests.ActorNames_KeepTheOriginalDoomOrder`
- `VmTFuncActorNames1181SliceTests.ActorTypes_StartNullWithOneSlotPerName`
- `VmTFuncActorNames1181SliceTests.ScriptRandom_UsesTheFScriptStream`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1181 --to 1181` (0 hard / 0 soft).

## Seams

T_GetMobjType at 219 is chunk 837. The actor-class fill loop at 4019 is chunk 851. This slice only records the name table, the null class slots, and the FScript RNG name.

## Credit rationale

UNCredited t_func.cpp 1–218 (+218). The table closes at 201. ActorTypes is declared at 203. T_GetMobjType starts at 219. A full tally line scan shows no overlap.
