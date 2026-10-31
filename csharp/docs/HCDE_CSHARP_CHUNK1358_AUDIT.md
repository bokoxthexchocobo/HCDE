# Chunk 1358 — Crouch through RunScript net commands from d_net.cpp (9009-9260).

## Scope

Crouch through RunScript net commands from d_net.cpp (9009-9260).

## Native ranges

- `src/d_net.cpp` **NetDoCommand** 9009–9260 (252 lines)

**Credited native LOC:** 252 (tally `nativeLoc` 252).

## Managed

`VmNetDoCommand1358Slice.cs`
Managed LOC (tally): 663.
Conversion commit: `8f026735b8aa79af87ba73d352a2bbcd8958dbd8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Crouch (**9010–9017**) | yes |
| Morph (**9019–9026**) | yes |
| Controllers (**9028–9048**) | yes |
| Kill and remove (**9050–9091**) | yes |
| Conversation (**9093–9097**) | yes |
| Weapon slots (**9099–9133**) | yes |
| Pitch, camera, finish (**9135–9147**) | yes |
| Net event (**9149–9169**) | yes |
| Screen, ready, zscript, skill (**9171–9199**) | yes |
| Kick, weapon, flechette, default (**9201–9229**) | yes |
| RunScript (**9232–9248**) | yes |
| Skip-contract comment (**9250–9260**) | yes |
| Band hull (**9009–9260**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Net_DoCommand head | opcodes above DEM_CROUCH stay in the earlier cases |
| Net_TrySkipCommand | the skip parser starts at 9261 |
| cht_Morph | the morph cheat lives outside this file |
| Net_ReadWeapon | the weapon index lives outside this file |
| a_weapons slot printf | Could not add is printed by AddSlot, not this tail |

Crouch runs only in GS_LEVEL. A bad player index is not touched when the game state is something else. A null body, a state other than PST_LIVE, or BT_JUMP in oldbuttons skips the freeze check. The freeze hook can change crouchdir before the assignment. A true result leaves crouching alone. A false result stores 1 when crouchdir is negative and -1 otherwise.

Morph reads the C string first. A missing NUL throws before FindActor. FindActor runs before the player index is checked, and cht_Morph receives that class, including null, with quickundo false. An out-of-range player throws after the lookup. The console prints the message plus a newline. An empty message or a message whose first character is NUL prints Morph failed. Any other player still morphs and prints nothing.

Add and delete controller consume one player byte, then store the flag, then print. The local console player gets the you-can / you-can-no-longer line and does not ask the authority. Anyone else asks the authority, and a true result prints the name as it stands after that call. A false result keeps the flag and prints nothing. A bad index throws after the byte is consumed.

Kill and remove do not index the sender. An unknown name, or a remove class that is not an actor, prints the not-an-actor line and does not touch the level. A real class massacres or removes the class type name, then the replacement when the pointer differs, then prints the original string. A null replacement throws after the first call. A null primary level throws after the lookup.

The three conversation opcodes forward the opcode, the sender, and the stream. Weapon slot commands read the optional target, the slot, and the count before they index the player. Clear runs only when the slot is below 10. Each count calls Net_ReadWeapon once, and that hook may consume a continuation byte. AddSlot stores a non-null weapon in range and faults on a non-null weapon out of range. A null weapon does not index. AddSlotDefault applies that fault only for a weapon descendant. Feedback is the target compared with the console player. The could-not-add printf stays in a_weapons.cpp.

Pitch reads the up byte and stores its negation before it reads the down byte. A short stream keeps the new minimum. A bad player index throws after the up byte. Revert camera assigns the body, including null. Finish calls ChangeLevel with a null name and two zeros and does not read the stream. A bad sender still finishes. A null primary throws.

Net event reads the name, one ignored count, three ints, and a nonzero manual byte. The count is not how many ints are read. Predator runs before the level is touched. A true result skips the console. A false result with a null level throws after predator. A short stream throws before predator. ZScript reads a C string and an int16 stored in a full unsigned width. A negative int16 becomes a wide size and the first missing payload byte throws before NetCommand. Size 0 still submits an empty buffer. A null level is touched only after the bytes are in hand.

End screen, readied, weapon select, and flechette record the sender. Weapon select reads one slot byte first. Skill stores one big-endian int and leaves the previous skill when the read throws. Kick reads the target, errors when that target is the console, and does not ask InGame. Otherwise it asks InGame, and a true result prints the name, warns with gametic, client tic, and room, then disconnects. A false result stops. An in-game index that is not a player throws before the warning. Unknown opcodes, including ones handled above this band, throw Unknown net command and do not read.

RunScript returns on a null pawn without reading. Otherwise it reads every arg and keeps four. The script count is the minimum of 4 and the requested count, which stays negative when the request is negative. Flags are ACS_NET combined with the always bits. The map name is read after the args. A null primary throws after those reads.

| Managed (VmNetDoCommand1358Slice.cs) | Native (d_net.cpp) |
|--------------------------------------|--------------------|
| Crouch through RunScript | **9009–9260** |
| Band hull (exclusive slice) | **9009–9260** |

### Chunk **1358** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **252** |
| **Net headline** | **+252** vs the prior chunk |


**Post-review ratio:** 663/252 managed/native ≈ 263% (src file LOC at HEAD; see tally).

## Tests

- `VmNetDoCommand1358SliceTests.CrouchThroughCheats`
- `VmNetDoCommand1358SliceTests.SlotsEventsAndRunScript`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1358 --to 1358` (0 hard / 0 soft).

## Seams

cht_Morph, FindActor, Massacre, GetReplacement, RemoveClass, P_ConversationCommand, Net_ReadWeapon, P_IsPlayerTotallyFrozen, HCDEPredator_HandleNetEvent, and NetworkClients.InGame stay injected. The byte readers are the stream this tail consumes.

## Credit rationale

UNCredited d_net.cpp 9009-9260 (+252). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines. It is not a tie.
