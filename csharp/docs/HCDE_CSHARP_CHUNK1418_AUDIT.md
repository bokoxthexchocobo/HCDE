# Chunk 1418 — Nerve wad renames, Mac Hexen extra-lump clearing, and Strife teaser voice namespaces in d_main.cpp (3096-3281).

## Scope

Nerve wad renames, Mac Hexen extra-lump clearing, and Strife teaser voice namespaces in d_main.cpp (3096-3281).

## Native ranges

- `src/d_main.cpp` **RenameNerve** 3096–3281 (186 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`VmDMain1418Slice.cs`
Managed LOC (tally): 249.
Conversion commit: `88efe3a80bccb5a3312e33057c3ba3894be171fa`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| RenameNerve (**3096–3164**) | yes |
| FixMacHexen banner and body (**3165–3246**) | yes |
| FindStrifeTeaserVoices (**3248–3281**) | yes |
| Band hull (**3096–3281**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| RenameSprites | ends at line 3083 |
| DoomButtons | starts at line 3283 |
| MD5 implementation and file-reader primitives | callees |

RenameNerve returns immediately unless the game type is Doom. It scans wad indexes after the IWAD. A missing reader is skipped. Four Nerve sizes are checked, and the last matching size index is kept. A size miss skips the digest. A size hit seeks to the start and hashes the bytes actually read, 8192 at a time. The digest is compared with the checksum paired to that size index. The first match stops the scan. Inside that wad, a CWIL name changes its first character to N. A MAP0 name copies character 4 into character 6, writes 0 at character 5 and L at character 4, then writes LEVE into the first four characters. Other names stay. Entries outside that wad's first-to-last range stay.

FixMacHexen returns immediately unless the game type is Hexen. A missing IWAD reader is a null dereference. The IWAD length must be the demo size 13596228, the beta size 13749984, or the full size 21078584, or the function returns before hashing. The digest is then compared with the beta checksum first. A beta digest keeps the beta path even when the size that passed was the demo or full size. Otherwise the demo or full checksum must match. An unrecognized digest returns after the hash. The deletion window is 299 lumps ending at the IWAD's last entry. A beta digest moves that end 12 entries earlier so the trailing map stays. Each deleted lump clears only its first name byte. The native assert does not change the window.

FindStrifeTeaserVoices runs only when the game type is exactly Strife and the shareware flag bit 2 is set. It visits every entry. A global lump owned by the IWAD whose name starts with VOC is moved to the strife-voices namespace when characters 3 through 7 are each either 0 or a decimal digit. Any other byte in that span leaves the lump in the global namespace. The strife-voices namespace is 9.

The Nerve checksums are 967d5ae23daf45196212ae1b605da3b0, 4214c47651b63ee2257b1c2490a518c9, 3544e1903091c50ba50049db74cd7d25, and 9143b392392a7ac870c2ca36ac65af45, paired with sizes 3819855, 3821966, 3821885, and 4003409. The Mac Hexen checksums are 925f9f5000e17dc84b0a6a3bed3a6f31, 2af1b27cd11fb159e608472a1b53e40e, and b68140a796f6fd7f3a5d3226a32b93be.

| Managed (`VmDMain1418Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Nerve renames, Mac Hexen clearing, and teaser voices | **3096–3281** |
| Band hull (exclusive slice) | **3096–3281** |

### Chunk **1418** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **186** |
| **Net headline** | **+186** vs the prior chunk |


**Post-review ratio:** 249/186 managed/native ≈ 134% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1418SliceTests.NerveAndMacHexen`
- `VmDMain1418SliceTests.StrifeTeaserVoices`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1418 --to 1418` (0 hard / 0 soft).

## Seams

The file-system walk, the 8192-byte MD5 update, and the C digit test are modeled in this slice. DoomButtons starts at line 3283.

## Credit rationale

UNCredited d_main.cpp 3096-3281 (+186). RenameNerve, FixMacHexen, and FindStrifeTeaserVoices are whole functions. The comment banner between RenameNerve and FixMacHexen sits inside the span. Each function alone is under 150 lines, so they share this band. DoomButtons starts at line 3283. hq4x and hq4x_asm stay later because they are generated tables. The band is not sliced further.
