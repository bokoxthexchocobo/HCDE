# Chunk 1436 — Text responder through cast drawer in intermission.cpp (440-537 and 545-770).

## Scope

Text responder through cast drawer in intermission.cpp (440-537 and 545-770).

## Native ranges

- `src/intermission/intermission.cpp` **DIntermissionScreenText::Responder** 440–537 (98 lines)
- `src/intermission/intermission.cpp` **DIntermissionScreenCast::Init** 545–770 (226 lines)

**Credited native LOC:** 324 (tally `nativeLoc` 324).

## Managed

`VmIntermission1436Slice.cs`
Managed LOC (tally): 471.
Conversion commit: `33eabe01ceb51670a99040c8719fd9ce3feda18b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Text responder and drawer (**440–537**) | yes |
| Cast init, input, attack, ticker, and drawer (**545–770**) | yes |
| Band hull (**440–770**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Comment banner | lines 538-544 stay uncredited |
| Scroller banner | lines 771-777 stay uncredited |
| Super init, responder, ticker, and drawer | callees |
| GetCharFromString | callee, reimplemented for the text drawer |

A key during the typewriter jumps the ticker to the end of the text and returns 1. A key after that returns -1. Any other event returns 0. The drawer scales from the 320 by 200 or 640 by 400 reference, counts newline rows, and shrinks positive row padding until the block fits. A default origin recenters when the space below is smaller than the top. A fixed origin only shrinks padding. A character that does not fit the width is consumed and not drawn. Speed 0 divides. Bytes 128 through 159 use the Windows-1252 map. Other non-ASCII bytes use the UTF-8 decoder, including its dead four-byte continuation check.

Cast init copies the class, the attack sounds, and the see state. A missing class clears the body and state and returns. A player advances through the missile state and uses the console player translation. Any other actor uses its own translation when that value is not zero. The see sound plays only when its id is greater than zero. A key that is not a key-down returns 0. Death already in progress returns 1. Death plus Cast is preferred, and Death alone is kept when Cast is absent. A missing label returns -1. A player death sound comes from the skin. A monster uses its death sound when that id is greater than zero. An attack sound matches the melee flag and the state index, stops every channel, and plays on the weapon channel. The ticker decrements cast tics first. A null state, a -1 tic state, a missing next state, or a self-loop during death returns -1. Reaching the advance state stops the attack and returns to the see state. Frame 12 starts an attack, alternating melee and missile for monsters and always using missile for a player. A null choice tries the other state. Frame 24 or a return to the see state while attacking also stops. A -1 tic result after the advance becomes 15. The cast title resolves a leading dollar string and centers it. A player skin replaces the sprite when the spawn sprite matches, the class is still listed, and the no-skin flag is clear. The last matching class wins.

| Managed (`VmIntermission1436Slice.cs`) | Native (`intermission.cpp`) |
|----------------------------------------|------------------------------|
| Text and cast screens | **440–537**, **545–770** |
| Band hull (exclusive slice) | **440–770** |

### Chunk **1436** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **324** |
| **Net headline** | **+324** vs the prior chunk |


**Post-review ratio:** 471/324 managed/native ≈ 145% (src file LOC at HEAD; see tally).

## Tests

- `VmIntermission1436SliceTests.TextKeysAndTypewriter`
- `VmIntermission1436SliceTests.TextDelayAndSpeed`
- `VmIntermission1436SliceTests.CastAdvanceSkinAndAttack`
- `VmIntermission1436SliceTests.CastMissingDeath`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1436 --to 1436` (0 hard / 0 soft).

## Seams

Super init, responder, ticker, and drawer are callees. GetCharFromString is reimplemented for the text drawer. The scroller starts at line 778.

## Credit rationale

UNCredited intermission.cpp 440-770 with the comment banner 538-544 left out (+324). This is the text typewriter and the cast call screen. The band is shorter than 400 lines and is not sliced.
