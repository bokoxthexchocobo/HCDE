# Chunk 1206 — M3U and PLS playlist load, shuffle, and cursor from s_playlist.cpp (1–198).

## Scope

M3U and PLS playlist load, shuffle, and cursor from s_playlist.cpp (1–198).

## Native ranges

- `src/common/utility/s_playlist.cpp` **FPlayList::ChangeList** 1–198 (198 lines)

**Credited native LOC:** 198 (tally `nativeLoc` 198).

## Managed

`VmPlaylist1206Slice.cs`
Managed LOC (tally): 189.
Conversion commit: `216c10087f6833fc7c29ccffb34539a161197aa7`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FPlayList destructor (**33–35**) | yes |
| ChangeList (**37–116**) | yes |
| NextLine (**118–136**) | yes |
| Shuffle (**139–149**) | yes |
| GetNumSongs through GetSong (**151–198**) | yes |
| Band hull (**1–198**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A failed open returns false and keeps the current songs. A file that opens is cleared first. Comment and blank lines are skipped. The first real line `[playlist]` selects PLS mode, and only `File` plus digits plus `=` is kept. Reading past the end of that token throws the string-bounds abort. A path that starts with `/`, or whose first slash begins `://`, stays absolute. Every other path is joined to the playlist directory, including an empty `File=` value, which becomes that directory. The destructor body is empty. SetPosition and GetSong treat a negative index as out of range. Advance and Backup wrap, and Backup of an empty list returns -1. Shuffle is an in-place Fisher-Yates over rand and then rewinds to the first song.

| Managed (`VmPlaylist1206Slice.cs`) | Native (`s_playlist.cpp`) |
|------------------------------------|---------------------------|
| Playlist | **1–198** |
| Band hull (exclusive slice) | **1–198** |

### Chunk **1206** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **198** |
| **Net headline** | **+198** vs post-**1205** |


**Post-review ratio:** 189/198 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmPlaylist1206SliceTests.ChangeList_JoinsRelativeSongsAndKeepsAFailedOpen`
- `VmPlaylist1206SliceTests.Cursor_WrapsAndShuffleUsesRand`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1206 --to 1206` (0 hard / 0 soft).

## Seams

The playlist text stands in for FileReader. A null text is a failed open and leaves the previous songs in place. NextLine uses a 511-character fgets chunk and signed-char leading skip. Backslashes become slashes after that skip. The directory prefix is ExtractFilePath and includes the trailing slash. This host is not Windows, so a drive letter is not absolute. Shuffle draws from the host rand.

## Credit rationale

UNCredited s_playlist.cpp 1–198 (+198). The file ends at 198. A full tally line scan shows no overlap.
