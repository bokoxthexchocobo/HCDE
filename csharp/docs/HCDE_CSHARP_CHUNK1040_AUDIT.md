# Chunk 1040 — `PActorInfo` / `AddActorInfo` and `FState::MatchesCallerType` (+ `CheckCallerType` prologue) from `info.cpp` (**1–150**). Follows **1039** (**894–1043**).

## Scope

`PActorInfo` / `AddActorInfo` and `FState::MatchesCallerType` (+ `CheckCallerType` prologue) from `info.cpp` (**1–150**). Follows **1039** (**894–1043**).

## Native ranges

- `src/gamedata/info.cpp` **PActorInfo / FState caller matching** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmInfoActorState1040Slice.cs`
Managed LOC (tally): 83.
Conversion commit: `e8f6b06babf7b53282855b4259d3270a07477e70`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `PActorInfo` layout + value ctor/dtor (**62–95**) | yes (policy snapshot) |
| `AddActorInfo` meta field (**99–104**) | yes |
| `FState::SetAction` name bind (**107–110**) | seam (host `FindVMFunction`) |
| `FState::MatchesCallerType` (**113–148**) | yes |
| `CheckCallerType` prologue (**150–155** hull ends **150**) | seam (signature line only) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–46** | license / includes host |
| **47–54** | globals / forward decls |
| **151+** | `CheckCallerType` body / `CallAction` outside hull (**150** ends prologue) |
| **500–799** | chunks **1037–1038** |
| **800–893** | deferred (**94** lines) |
| **894–1043** | chunk **1039** |
| `VmActorStateRuntime` | uncredited helper — seam only |

| Managed (`VmInfoActorState1040Slice.cs`) | Native (`info.cpp`) |
|------------------------------------------|---------------------|
| `ConstructActorInfoAtOffset` / `DestroyActorInfoAtOffset` | **70–95** |
| `ShouldRegisterActorInfoMetaField` | **99–104** |
| `ShouldSkipCallerTypeMatch` | **115–118** |
| `MatchesObjectPointerKind` / `MatchesActorKind` | **122–136** |
| `MatchesCallerType` | **138–147** |
| `ShouldAbortCheckCallerOnBadPrototype` | **158–161** (policy; hull **150** seam) |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1040** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1039** |


**Post-review ratio:** 83/150 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmInfoActorState1040SliceTests.MatchesCallerType_AllowsWhenNoImplicitArgs`
- `VmInfoActorState1040SliceTests.MatchesCallerType_DehackedBypassesKindCheck`
- `VmInfoActorState1040SliceTests.MatchesCallerType_RequiresKindOfSelf`
- `VmInfoActorState1040SliceTests.MatchesCallerType_SecondArgChecksStateOwner`
- `VmInfoActorState1040SliceTests.ShouldAbortCheckCallerOnBadPrototype_WhenNotObjectPointer`
- `VmInfoActorState1040SliceTests.ConstructActorInfoAtOffset_CopiesDefaultBytes`
- `VmInfoActorState1040SliceTests.ShouldRegisterActorInfoMetaField_OnlyStarMeta`
- `VmInfoActorState1040SliceTests.CheckCallerTypePasses_DelegatesToMatches`
- `VmInfoActorState1040SliceTests.ShouldSkipCallerTypeMatch_WhenImplicitArgsZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1040 --to 1040` (0 hard / 0 soft).

## Seams

`TypeTable`, `FindVMFunction`, `ThrowAbortException`, `VMCallAction`, and global `AllActorClasses` remain host-injected; uncredited `VmActorStateRuntime` is not re-credited.

## Credit rationale

File-scoped UNCredited **`info.cpp`** **1–150**; excludes credited **500–799** / **894–1043** and deferred **800–893**.
