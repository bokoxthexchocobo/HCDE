# Session audit 2026-10-20F (chunks 231–244)

## Chunks landed

| Chunk | Focus |
|-------|--------|
| 231 | g_skill + teaminfo gap lines; native export recognition tests (227b/229 tally test names fixed) |
| 232–233 | decallib core + ReadAllDecals / gap lines |
| 234 | doomfont HU constants + init gap lines |
| 235–237 | gi static schema, ParseGameInfo block, DEFINE_FIELD recognition |
| 238–240 | a_keys check helpers + remainder gaps |
| 241–244 | statistics models, parse, save, store |

**14 conversion/recognition tally chunks** this continuation (231–244), plus recognition test-name repair commit.

## Rules

- Per chunk: code (batched 232–237, 238–244) → individual tally commits with full 40-char SHAs.
- Recognition rows 227b/229 now cite `VmGSkillNativeExportRecognitionTests` / `VmTeamNativeExportRecognitionTests`.
- `dotnet build -warnaserror` and full test suite green at HEAD.

## HEAD

See bot report `git rev-parse HEAD`.
