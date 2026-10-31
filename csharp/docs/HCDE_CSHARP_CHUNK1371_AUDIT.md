# Chunk 1371 — FDecalGroup, the keyword table, animator declarations, base getters, and the FDecalLib constructor from decallib.cpp (1-222).

## Scope

FDecalGroup, the keyword table, animator declarations, base getters, and the FDecalLib constructor from decallib.cpp (1-222).

## Native ranges

- `src/gamedata/decallib.cpp` **DecalLibHead** 1–222 (222 lines)

**Credited native LOC:** 222 (tally `nativeLoc` 222).

## Managed

`VmDecalLib1371Slice.cs`
Managed LOC (tally): 325.
Conversion commit: `4c96f5d97a7cfe4493307d6334e0c333f8febe63`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Banner and includes (**1–40**) | yes |
| `DecalLibrary` and the `ReadScale` declaration (**41–44**) | yes |
| `pr_decalchoice` and `pr_decal` (**52–53**) | yes |
| `FDecalGroup` construct, replace, add, and assign (**55–75**) | yes |
| `FTranslation` fields (**77–85**) | yes |
| Animator declarations and derived constructors (**87–144**) | yes |
| `Animators` and `AnimatorList` (**146–147**) | yes |
| `DecalKeywords` including the null sentinel (**149–170**) | yes |
| `DECAL_*` enumerators (**172–192**) | yes |
| `FDecalBase::GetDecal` and `GetDecalName` (**194–202**) | yes |
| `FDecalTemplate::ReplaceDecalRef` (**204–210**) | yes |
| `FDecalLib` construct and destroy (**212–221**) | yes |
| Blank line before `Clear` (**222**) | yes |
| Band hull (**1–222**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| decallib.cpp **223–247** | `Clear` and `DelTree`, chunk 197 |
| decallib.cpp **956–963** | `FDecalBase` construct and destroy |
| decallib.cpp **995–1078** | translation construct, group `GetDecal` body, animator construct and destroy |
| decallib.cpp **1173** | `ReadScale` body; line 43 is only the declaration |
| `weightedlist.h` | callee of `AddDecal`; not a line of this file |

`FName(NULL)` is name index 0, `None`. Any other spelling is stored on first use and later lookups are case-insensitive. An empty string is its own name.

`FDecalGroup` binds `pr_decalchoice` (`DecalChoice`, a client rng). `pr_decal` is the separate client rng named `Decal`. Assignment returns this and does not copy the choice list. `AddDecal` skips weight 0. A positive weight inserts before the first entry whose weight is not strictly less, so an equal weight lands in front of the existing equals, then the random values are rebuilt. The last entry is 255. Each earlier entry stores `(uint8_t)(accumulated * 255.0)`. `ReplaceDecalRef` replaces every reference-equal value and does not rebuild those values. A null decal pointer is a legal entry.

`FDecalBase::GetDecal` returns null. `GetDecalName` returns `Name`, which the base constructor sets to `None`. The group override of `GetDecal` is only declared here; calling it is rejected because the body at 1053 is outside this band.

`FDecalTemplate::ReplaceDecalRef` uses pointer equality, so a null lower decal matches a null `from`. The library constructor sets `Root` and `Translations` to null. The destructor calls `Clear` and does not itself clear those fields. A missing `Clear` hook throws. `DecalTranslations` starts empty. `Animators` and `FDecalCombinerAnim::AnimatorList` are distinct empty lists. The slider has `DistY` only. Derived animator constructors pass the name through `FDecalAnimator`.

The keyword table is 18 names and a null sentinel, in enumerator order. `DECAL_OPAQUEBLOOD` is 16 and `DECAL_TRANSLATABLE` is 17. The `opaqueblood` comment is `Deprecated - use translatable instead!`.

| Managed (`VmDecalLib1371Slice.cs`) | Native (`decallib.cpp`) |
|------------------------------------|------------------------|
| Group, keywords, animators, getters, library constructor | **1–222** |
| Band hull (exclusive slice) | **1–222** |

### Chunk **1371** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **222** |
| **Net headline** | **+222** vs the prior chunk |


**Post-review ratio:** 325/222 managed/native ≈ 146% (src file LOC at HEAD; see tally).

## Tests

- `VmDecalLib1371SliceTests.GroupKeywordsAndReplace`
- `VmDecalLib1371SliceTests.LibraryAnimatorsAndNames`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1371 --to 1371` (0 hard / 0 soft).

## Seams

FDecalLib::Clear at 223 stays chunk 197 and is injected. FDecalGroup::GetDecal's body at 1053 stays outside this band. weightedlist.h is the AddDecal callee and is not a native range.

## Credit rationale

UNCredited decallib.cpp 1-222 (+222). A full tally line scan, including recognition ranges, shows no overlap. This is the largest remaining hole of at most 400 lines after codegen.cpp 2017-2240. It is not a tie. hcde_edf.cpp 757-976 is 220. Gaps longer than 400 lines stay whole and are not sliced. Clear starts at 223 and is already chunk 197.
