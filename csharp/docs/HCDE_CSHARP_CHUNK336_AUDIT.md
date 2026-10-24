# Chunk 336 — MBF CreateScratchFunc and CreatePlaySoundFunc parameter packing

## Scope

MBF CreateScratchFunc and CreatePlaySoundFunc parameter packing

## Native ranges

- `src/gamedata/d_dehacked.cpp` **CreateRandomJump CreateLineEffect emitters** 849–934 (86 lines)

**Credited native LOC:** 86 (tally `nativeLoc` 86).

## Managed

`VmDehackedMbfScratchSound.cs`, `VmDehackedMbfScratchSoundTests.cs`
Managed LOC (tally): 22.
Conversion commit: `af911e170e4a161a67570a8748e9905b68f6d162`.

## Tests

- `VmDehackedMbfScratchSoundTests.RandomJumpArgs_ResolvesStateLabel`
- `VmDehackedMbfScratchSoundTests.LineEffectArgs_PassesSpecialAndTag`
- `VmDehackedMbfScratchSoundTests.ScratchMeleeArgs_ResolvesHitSound`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 336 --to 336` (0 hard / 0 soft).

## Seams

`FunctionCallEmitter` and `DehFindSound` are injected via a sound-index callback; only the misc1/misc2 argument shaping is modeled.

## Credit rationale

Credits uncredited `d_dehacked.cpp` 849–934 between earlier MBF rows and the credited emitter tail: random jump state label packing, Boom line-effect special/tag args, and the spawn/monster projectile emitter parameter lists.
