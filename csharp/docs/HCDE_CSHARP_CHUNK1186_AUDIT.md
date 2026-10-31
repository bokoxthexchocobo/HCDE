# Chunk 1186 — LOCKDEFS key, group, and lock checks from a_keys.cpp (1–163).

## Scope

LOCKDEFS key, group, and lock checks from a_keys.cpp (1–163).

## Native ranges

- `src/gamedata/a_keys.cpp` **OneKey::check** 1–163 (163 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmAKeys1186Slice.cs`
Managed LOC (tally): 121.
Conversion commit: `90741d1bab1fa448b0578efab096e6b20eda1285`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| OneKey::check (**51–79**) | yes |
| Keygroup::check (**91–98**) | yes |
| Lock::check (**114–138**) | yes |
| keywords_lock (**150–157**) | yes |
| Band hull (**1–163**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **164–585** | lock parsing and the later key queries stay outside this band |

One key matches the carrier class, the carrier species name, a dehacked replacee, or the dehacked species field. Otherwise it walks the inventory for the class or the species name. A group matches if any key matches. An empty group does not. A lock with no groups accepts any inventory item that is a key. A lock with groups requires every group. A voodoo doll is redirected to the real player body before that test. The keyword list ends with a null slot. Keys are not initialized yet.

| Managed (`VmAKeys1186Slice.cs`) | Native (`a_keys.cpp`) |
|----------------------------------|----------------------|
| Lock checks and keywords | **1–163** |
| Band hull (exclusive slice) | **1–163** |

### Chunk **1186** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **163** |
| **Net headline** | **+163** vs post-**1185** |


**Post-review ratio:** 121/163 managed/native ≈ 74% (src file LOC at HEAD; see tally).

## Tests

- `VmAKeys1186SliceTests.OneKey_MatchesTheCarrierThePickupAndTheInventory`
- `VmAKeys1186SliceTests.Locks_RequireEveryGroupUnlessTheListIsEmpty`
- `VmAKeys1186SliceTests.Keywords_ListTheLockdefWordsAndStartUninitialized`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1186 --to 1186` (0 hard / 0 soft).

## Seams

Class identity, species, dehacked replacee, and the key-kind test are fields on the host actor. AddOneKey at 164 stays outside this slice.

## Credit rationale

UNCredited a_keys.cpp 1–163 (+163). The keyword table closes at 157. The AddOneKey banner is 159–163 and the function starts at 164. A full tally line scan shows no overlap.
