# Chunk 1174 — P_TryMove successful link, crossed-line activation, and the push-line label from p_map.cpp (2727–2898).

## Scope

P_TryMove successful link, crossed-line activation, and the push-line label from p_map.cpp (2727–2898).

## Native ranges

- `src/playsim/p_map.cpp` **P_TryMove** 2727–2898 (172 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Managed

`VmPMapTryMove1174Slice.cs`
Managed LOC (tally): 242.
Conversion commit: `68e3871c68b3252b0d412a94ac44e56051ca7b22`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Successful link and crossed lines (**2727–2794**) | yes |
| Predicting return, fake-floor eyes, portal step, sector transition (**2796–2852**) | yes |
| Push-line label (**2854–2882**) | yes |
| Short `P_TryMove` overload (**2884–2890**) | yes |
| `P_CheckMove` comment (**2891–2898**) | yes |
| Band hull (**2727–2898**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2359–2726** | crossing checks are chunk **1173** |
| **2899–2991** | `P_CheckMove` is chunk **715** |
| **6101–6522** | radius damage stays off the queue |
| **4566–4690**, **5238–5244**, **5822–5828**, **7272–7391** | short tails stay uncredited |

When no portal was crossed, the actor unlinks, copies the trace floor and ceiling fields, and links at the original destination. Floor clip runs even for a noclip or teleport actor. Crossed specials pop from the end. A predicting player predicts the cross. A player activates a cross. Monster-cross, projectile-cross, the three teleport specials, and any-cross follow in that order. A predicting player then returns true before fake-floor eyes, portal reacquire, and the sector transition.

Eyes use the current Z plus the view height captured by the crossing checks. Surface and dive compare the new fake floor with the old eye side. Ceiling eyes run unless the height sector is fake-floor only. A portal step unlinks, applies the relative position to the previous point, and stores the new sector group. The sector transition always receives the sector from function entry.

The push-line label clears the in-try-move flag and restores the Z from before the floor plane. A predicting player returns before impact damage. Otherwise a blasted actor takes melee damage of mass shifted right by five, and pushed specials are visited from the last hit back to the first. The short overload default-constructs the trace and forwards dropoff, the floor plane, and the missile check into the same path.

| Managed (`VmPMapTryMove1174Slice.cs`) | Native (`p_map.cpp`) |
|----------------------------------------|----------------------|
| Link, eyes, and push line | **2727–2898** |
| Band hull (exclusive slice) | **2727–2898** |

### Chunk **1174** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **172** |
| **Net headline** | **+172** vs post-**1173** |


**Post-review ratio:** 242/172 managed/native ≈ 141% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapTryMove1174SliceTests.Link_PlacesTheActorAndTriggersEyes`
- `VmPMapTryMove1174SliceTests.Push_RestoresZAndPushesSpecialsInReverse`
- `VmPMapTryMove1174SliceTests.ShortMove_ForwardsDropoffAndLinks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1174 --to 1174` (0 hard / 0 soft).

## Seams

P_ActivateLine, P_PredictLine, CheckForPushSpecial, and P_DamageMobj stay host-supplied. Crossing checks are chunk 1173.

## Credit rationale

UNCredited p_map.cpp 2727–2898 (+172). Chunk 1173 ends at 2726. P_CheckMove at 2899 is chunk 715. A full tally line scan shows no overlap. Radius damage 6101–6522 is not this band.
