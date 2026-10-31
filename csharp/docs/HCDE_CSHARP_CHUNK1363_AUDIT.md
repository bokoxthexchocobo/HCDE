# Chunk 1363 — Exit check, P_ActivateLine, and the P_TestActivateLine prefix through the walking floor and UseThrough condition in p_spec.cpp (1-242).

## Scope

Exit check, P_ActivateLine, and the P_TestActivateLine prefix through the walking floor and UseThrough condition in p_spec.cpp (1-242).

## Native ranges

- `src/playsim/p_spec.cpp` **SpecActivate** 1–242 (242 lines)

**Credited native LOC:** 242 (tally `nativeLoc` 242).

## Managed

`VmSpecActivate1363Slice.cs`
Managed LOC (tally): 380.
Conversion commit: `8073d4a226cbbd536f15f208768241bcd2b884d3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Banner, includes, and the rng declaration (**1–67**) | yes |
| Extern cvar declarations (**69–71**) | yes |
| CheckIfExitIsGood (**74–107**) | yes |
| Utility comment (**109–117**) | yes |
| P_ActivateLine (**119–195**) | yes |
| _Line.Activate (**197–204**) | yes |
| _Line.RemoteActivate (**206–217**) | yes |
| P_TestActivateLine prefix (**225–242**) | yes |
| Band hull (**1–242**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| p_spec.cpp **243–356** | chunk 884, including the UseThrough body that ors SPAC_Use |
| p_spec.cpp **364–399** | chunk 884 P_PredictLine |
| VmPSpecTestActivateLine.cs | already credited on 884; not edited |
| chunk 437 | retired, nativeRanges empty |

A null actor can always exit. Kill-monsters is tested before no-exit, and unequal counts return false with no damage. No-exit damages with 1000000 and the name Exit when deathmatch or alwaysapplydmflags is set, then returns false. A dead single-player actor in the same hub cluster returns false when FindClusterInfo returns a hub. Deathmatch prints the exit line only when gameaction is not 12. A null player on that print throws GetName.

P_ActivateLine returns false from the test, from a cancelled preactivation, or from a failed key check. Every other path returns true. The stored false before P_ExecuteSpecial is dead and is not kept. The special captured for the switch, the spammy print, and the diag is the low 8 bits after execute. A non-repeat success clears the full special before the switch and the rumble. Rumble reads the special after that clear. Use, Impact, and Push change the switch. Use, Push, UseThrough, and UseBack rumble when the actor is the console actor or its camera and the virtual exists. A null activator matches a null console camera and then throws GetClass. Impact with the dummy-switch flag, a non-repeat line, a non-generic special, a line id, a non-zero arg, and no tagged sector changes the switch and clears the special. Execute still returns true when the special fails. The diag's last argument is false.

P_TestActivateLine reads the level and then rejects the back of a first-side-only line or a zero special. A cross or monster-cross on a walking line rejects an actor above the floor that is not standing on another actor. A null actor on that floor check throws mo. The UseThrough condition selects the continuation. The or of SPAC_Use is not applied here. A missing continuation throws P_TestActivateLine. RemoteActivate builds the optional position. Activate passes null.

| Managed (VmSpecActivate1363Slice.cs) | Native (p_spec.cpp) |
|--------------------------------------|---------------------|
| Exit, activate, and the test prefix | **1–242** |
| Band hull (exclusive slice) | **1–242** |

### Chunk **1363** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **242** |
| **Net headline** | **+242** vs the prior chunk |


**Post-review ratio:** 380/242 managed/native ≈ 157% (src file LOC at HEAD; see tally).

## Tests

- `VmSpecActivate1363SliceTests.ExitChecksAndPrefix`
- `VmSpecActivate1363SliceTests.ActivateLineBody`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1363 --to 1363` (0 hard / 0 soft).

## Seams

The UseThrough body and the rest of P_TestActivateLine stay on chunk 884. P_ExecuteSpecial, P_CheckKeys, the world-line events, and P_ChangeSwitchTexture are called, not reimplemented.

## Credit rationale

UNCredited p_spec.cpp 1-242 (+242). A full tally line scan, including recognition ranges, shows no overlap. This is the largest remaining hole of at most 400 lines. It is not a tie. glnodes.cpp 1-240 is 240. Gaps longer than 400 lines stay whole and are not sliced. Chunk 884 already owns 243-356 and 364-399. Chunk 437 is retired and has no ranges.
