# Chunk 1170 — Named `P_LineAttack`, `P_LineTrace`, and `P_LinePickActor` from `p_map.cpp` (**5051–5237**).

## Scope

Named `P_LineAttack`, `P_LineTrace`, and `P_LinePickActor` from `p_map.cpp` (**5051–5237**).

## Native ranges

- `src/playsim/p_map.cpp` **P_LineTrace** 5051–5237 (187 lines)

**Credited native LOC:** 187 (tally `nativeLoc` 187).

## Managed

`VmPMapLineTrace1170Slice.cs`
Managed LOC (tally): 419.
Conversion commit: `9b4704527b354832abb83b914d69a20942ae3672`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Named `P_LineAttack` (**5052–5067**) | yes |
| `CheckLineTrace` (**5083–5100**) | yes |
| `P_LineTrace` placement and masks (**5102–5157**) | yes |
| Trace output and wall tier (**5158–5200**) | yes |
| `P_LinePickActor` (**5208–5237**) | yes |
| Band hull (**5051–5237**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **4691–5050** | class-puff `P_LineAttack` is chunk **1169** |
| **5238–5244** | comment banner before `P_TraceBleed` |
| **5245–5327** | `P_TraceBleed` is already conversion credit |
| **6101–6522** | radius damage stays off the queue |
| `Trace` / `FindActor` / `PointInSector` | the trace and the lookups stay host-supplied |

An unknown puff name clears the victim when one was passed, prints `Attempt to spawn unknown actor type`, and returns null. A known name calls the class-puff attack.

A portal crossing increments the portal count and returns skip (**2**). Any other non-actor hit returns stop. Thru-actors, or thru-species with the same species, skips an actor. A different species stops.

Start Z is actor Z minus floor clip plus the Z argument. Absolute position uses the three offsets and that Z argument. Absolute offset and a rotated offset use the start Z. A zero offset uses the actor at the start Z. Angle **0** with forward **3** and side **1** places the shot at offset `(3, -1, startZ)`. All-actors uses mask `0xFFFFFFFF`; otherwise the mask is shootable (**4**), with solid (**2**) added when requested. The wall mask starts as block-everything plus block-hitscan. Block-use adds that bit. Block-self adds projectile when the actor is a missile or has MBF bounce, blocking when it is not a projectile or blocks as a player, monsters unless the no-block-monster or friendly-compat bit applies, blocking and block-players for a player or block-as-player, and block-floaters for a float. Thru-block clears block-everything. Thru-hitscan clears block-hitscan. Trace flags are report-portals, plus no-sky when requested. The sector lookup receives the start position.

The output is cleared before it is filled. A ceiling sets the sector plane to **1**. A wall copies the side and the tier texture: middle part **1** texture **1**, upper part **0** texture **0**, lower part **2** texture **2**. A 3D floor acts as a middle part and takes the upper master texture when that flag is set, otherwise the lower, otherwise the middle. Any other hit copies the trace texture. Portal count is the callback count minus **2**. A null output still returns the trace result.

Pick uses center minus floor clip plus attack offset, no extra Z. Trace flags are no-sky plus portal-restrict. Ghosts are pierced, and the species checks are off with puff species `None`. An actor hit returns that actor. A miss or any other hit returns null.

| Managed (`VmPMapLineTrace1170Slice.cs`) | Native (`p_map.cpp`) |
|------------------------------------------|----------------------|
| Named attack, line trace, and pick | **5051–5237** |
| Band hull (exclusive slice) | **5051–5237** |

### Chunk **1170** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **187** |
| **Net headline** | **+187** vs post-**1169** |


**Post-review ratio:** 419/187 managed/native ≈ 224% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapLineTrace1170SliceTests.Name_RejectsAnUnknownPuffAndForwardsAKnownOne`
- `VmPMapLineTrace1170SliceTests.Callback_SkipsPortalsAndSameSpecies`
- `VmPMapLineTrace1170SliceTests.Trace_PlacesTheShotAndFillsTheWall`
- `VmPMapLineTrace1170SliceTests.Pick_ReturnsOnlyAnActorHit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1170 --to 1170` (0 hard / 0 soft).

## Seams

The trace, the sector lookup, and the named class lookup stay host-supplied. The class-puff `P_LineAttack` is chunk **1169**. `P_TraceBleed` starts at **5245**.

## Credit rationale

UNCredited **`p_map.cpp`** **5051–5237** (+187). Chunk **1169** ends at **5050**. `P_TraceBleed` credit starts at **5245**. **5238–5244** is the bleed comment and stays uncredited. A full tally line scan shows no overlap. Radius damage **6101–6522** is not this band.
