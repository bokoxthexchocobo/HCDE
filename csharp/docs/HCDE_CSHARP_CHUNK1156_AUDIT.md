# Chunk 1156 — Thing_Hate from `p_lnspec.cpp` (**1565–1727**).

## Scope

Thing_Hate from `p_lnspec.cpp` (**1565–1727**).

## Native ranges

- `src/playsim/p_lnspec.cpp` **Thing_Hate** 1565–1727 (163 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmPLnspecHate1156Slice.cs`
Managed LOC (tally): 256.
Conversion commit: `3b4371006adde319e2f7005bfc04802e51e6cc5a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `LS_Thing_Hate` (**1565–1726**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1728+** | `Thing_ProjectileAimed` is outside this hole |

`MF_SHOOTABLE` is **4**. `MF2_DORMANT` is **0x10000000**. `MF3_NOSIGHTCHECK` is **0x80**. `MF3_HUNTPLAYERS` is **0x80000000**. `MF4_NOHATEPLAYERS` is **1**. A non-zero hatee tid keeps the first shootable living non-dormant actor. Tid **0** for the hater uses the activator, and a player activator returns false before any write. Other hater tids take each living shootable actor, including a dormant one. Without a see state the actor is left unchanged. Mode **0** keeps that first hatee for every hater and does not record a tid. Any other mode records the hatee tid, clears the last-look actor, and drops a target or last enemy whose tid differs. Modes **2**, **4**, and **6** set no-sight-check; **3** and **4** set hunt-players; **5** and **6** set no-hate-players. Every other mode clears the bit. A non-zero mode then continues the hatee iterator, wrapping once through the host list and skipping itself, the dead, the unshootable, and the dormant. The native do-while restarts on a null iterator and can spin when the only acceptable actor is the hater; this slice stops after one pass. The target is replaced only when the hatee exists, is not the hater, and either the mode is **0** or the hater has a goal that is not already the target. The previous target becomes the last enemy. A living non-dormant hater then enters its see state. Hater tid **0** handles only the activator.

| Managed (`VmPLnspecHate1156Slice.cs`) | Native (`p_lnspec.cpp`) |
|---------------------------------------|--------------------------|
| Thing_Hate | **1565–1727** |
| Band hull (exclusive slice) | **1565–1727** |

### Chunk **1156** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **163** |
| **Net headline** | **+163** vs post-**1155** |


**Post-review ratio:** 256/163 managed/native ≈ 157% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecHate1156SliceTests.SpecificHate_SkipsAPlayerActivatorAndRemembersTheOldTarget`
- `VmPLnspecHate1156SliceTests.GroupModes_SetTheHuntBitsAndStillRequireAGoal`
- `VmPLnspecHate1156SliceTests.Rescan_WrapsPastTheFirstHateeAndSharesASpecificOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1156 --to 1156` (0 hard / 0 soft).

## Seams

Tid iterators stay host-supplied lists. A hatee scan that would spin on the hater alone stops after one pass.

## Credit rationale

UNCredited **`p_lnspec.cpp`** **1565–1727** (+163), inside the file, with no conversion or recognition overlap.
