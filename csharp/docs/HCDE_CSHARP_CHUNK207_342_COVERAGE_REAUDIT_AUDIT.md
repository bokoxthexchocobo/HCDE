# Chunks 207–342 coverage re-audit (2026-10-20)

Strengthened `reaudit_chunks.py` flagged **21** conversion rows with `thin_managed_coverage`. Trimmed to tested symbol ranges; **46** other soft warnings (`thin_test_band`) left for optional follow-up.

## Summary

| Metric | Before | After |
|--------|-------:|------:|
| Thin-chunk native sum (18 trimmed rows) | 2,728 | **601** |
| `convertedBehaviorNative` | 33,139 | **31,012** |
| `recognitionBehaviorNative` | 5,591 | 5,591 |
| `remainingNative` | 469,356 | **471,483** |

**Dropped tally rows** (fully superseded after overlap-safe trim): chunks **328**, **333**, **337** (shadow MAPINFO, EMAPINFO dispatch, episode teaser — already credited in 271, 286, 297).

## Per-chunk trims

### Chunk 212 — `sprite-init-hash-chunk212` (63 → **14** native, commit src **26**)

Credits `SpriteNameId` hash bucket linking in `R_InitSpriteDefs` — not the full voxel lump name scan loop.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| R_InitSpriteDefs hash | 328–341 | `VmSpriteTextureHasherTests.BuildSpriteTextureChains_LinksSamePrefix` |

### Chunk 216 — `doomfont-planner-chunk216` (112 → **62** native, commit src **32**)

Credits `HU` font size constants and small-font lump preference — not the full fallback chain.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| HU font constants | 30–34 | `VmDoomFontPlannerTests.HuFontSize_MatchesNativeRange` |
| InitDoomFonts small font | 36–92 | `VmDoomFontPlannerTests.PlanSmallFont_PrefersFontAWhenPresent` |

### Chunk 222 — `umapinfo-schema-doomed-spawn-chunk222` (265 → **43** native, commit src **37**)

Credits small UMAPINFO parse branches and Strife spawnable clear — not full schema tables or CCMD bodies.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| ParseMultiString clear branch | 112–118 | `VmUMapInfoTokenParserTests.TryParseMultiString_Clear` |
| ParseLumpName validation | 130–136 | `VmUMapInfoTokenParserTests.TryParseLumpName_RejectsLongName` |
| ParseStandardProperty header | 156–164 | `VmUMapInfoStandardPropertyParserTests.TryApplyProperty_SetsLevelName` |
| Strife type clear | 406–419 | `VmDoomEdSpawnInitTests.ClearStrifeTypes_EmptiesMap` |
| Spawn CCMD tail | 452–457 | `VmDoomEdSpawnInitTests.InitSpawnablesFromMapinfo_ResolvesClasses` |

### Chunk 228 — `g-skill-globals-teaminfo-accessors-chunk228` (165 → **57** native, commit src **31**)

Credits skill list globals, name token assign, and team option table — not `FTeam` accessor surface or `teamlist` CCMD.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| AllSkills and DefaultSkill | 35–45 | `VmGSkillGlobalsTests.CreateEmptySkillList_StartsEmpty` |
| ParseSkill name assignment | 83–87 | `VmSkillMapinfoParserTests.AssignSkillNameFromToken_SetsName` |
| TeamInfoOptions table | 57–97 | `VmTeamInfoParserTests.ParseTeam_ReadsOptions` |

### Chunk 230 — `p-terrain-keywords-init-chunk230` (162 → **78** native, commit src **47**)

Credits the `OuterKeywords` catalog mirrored by `VmTerrainKeywordSchema` — not splash prototype bodies.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| Terrain enums and file header | 1–78 | `VmTerrainKeywordSchemaTests.OuterKeywords_MatchesNativeTable` |

### Chunk 235 — `gi-static-schema-chunk235` (218 → **39** native, commit src **184**)

Credits `GameNames[]` and border preset lookup — not the `GAMEINFOKEY` macro catalog or file banner.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| GameNames and border tables | 73–111 | `VmGameInfoStaticSchemaTests.GameNames_IncludesDoom`, `VmGameInfoStaticSchemaTests.ResolveBorderPreset_HereticUsesFourBySixteen` |

### Chunk 313 — `dehacked-dehsupp-map-sections-chunk313` (124 → **69** native, commit src **44**)

Credits secondary DehSupp sections and lump error gates — not the full `StateMap` actor/state expansion loop.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| SoundMap | 3653–3663 | `VmDehackedDehSuppSoundInfoTests.ParseSoundMapSection_ResolvesEachSound` |
| InfoNames | 3664–3679 | `VmDehackedDehSuppSoundInfoTests.TryParseInfoNamesSection_AcceptsKnownActors`, `VmDehackedDehSuppSoundInfoTests.TryParseInfoNamesSection_RejectsUnknownActor` |
| ThingBits | 3680–3707 | `VmDehackedDehSuppThingBitsTests.TryParseEntry_ValidatesBitRange`, `VmDehackedDehSuppThingBitsTests.TryParseEntry_TruncatesLongName`, … |
| LoadDehSupp unknown section | 3781–3787 | `VmDehackedDehSuppLumpGateTests.FormatUnknownSectionError_MatchesNativeMessage` |
| LoadDehSupp recoverable error | 3790–3796 | `VmDehackedDehSuppLumpGateTests.TryHandleRecoverableLoadError_ReturnsFalse` |

### Chunk 321 — `g-mapinfo-options-media-env-chunk321` (169 → **23** native, commit src **54**)

Credits skymist/skymistyscale/wavelimit/music and environment scalars exercised by map-option tests — not full DEFINE blocks through sucktime.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| skymist speed hexen hack | 1186–1190 | `VmMapInfoMapOptionsScalarsTests.ClampSkyMistYScale_Bounds` |
| skymistyscale clamp | 1197–1198 | `VmMapInfoMapOptionsScalarsTests.ClampSkyMistYScale_Bounds` |
| wavelimit clamp | 1254–1256 | `VmMapInfoMapOptionsScalarsTests.ClampWaveLimit_CapsAt3000` |
| music pair parse | 1273–1276 | `VmMapInfoMapOptionsScalarsTests.ParseMusicPair_PreservesOrder` |
| evenlighting | 1310–1312 | `VmMapInfoMapOptionsEnvironmentTests.ApplyEvenLighting_ClearsSkySpeeds` |
| cdid hex parse | 1324–1325 | `VmMapInfoMapOptionsEnvironmentTests.ParseCdIdHex_ReadsHex` |
| wallshade halve clamp | 1338–1339 | `VmMapInfoMapOptionsEnvironmentTests.ClampWallShade_DividesByTwo` |
| nogravity | 1357–1358 | `VmMapInfoMapOptionsEnvironmentTests.ApplyNoGravity_ReturnsMax` |

### Chunk 322 — `g-mapinfo-options-pics-chunk322` (46 → **18** native, commit src **17**)

Credits string assignment for inter/exit/enter pics — not anim/specialaction tails.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| interpic | 1375–1380 | `VmMapInfoMapOptionsPicsTests.ApplyInterPic_SetsLabel` |
| exitpic | 1382–1387 | `VmMapInfoMapOptionsPicsTests.ApplyExitPic_SetsNextMap` |
| enterpic | 1389–1394 | `VmMapInfoMapOptionsPicsTests.ApplyInterPic_SetsLabel` |

### Chunk 325 — `hcde-emapinfo-primitives-head-chunk325` (185 → **42** native, commit src **25**)

Credits `NormalizeResourceName`, `SplitValueList`, and empty-true `ParseBool` — not the full primitive block through `SetFlag`/`ParseDouble`.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| NormalizeResourceName | 104–112 | `VmEMapInfoLineParserTests.NormalizeResourceName_StripsLeadingSlashes` |
| SplitValueList | 174–201 | `VmEMapInfoLineParserTests.SplitValueList_SplitsOnComma` |
| ParseBool empty true | 205–209 | `VmEMapInfoLineParserTests.TryParseBool_TreatsEmptyAsTrue` |

### Chunk 327 — `hcde-emapinfo-apply-block-chunk327` (128 → **31** native, commit src **41**)

Credits typed apply wrappers, `E1M8` boss-special bit, and `NormalizeResourceName` head used by lump resolve tests.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| ApplyIntProperty | 405–413 | `VmEMapInfoApplyHelpersTests.ApplyIntProperty_RejectsBadNumber` |
| ApplyDoubleProperty | 415–423 | `VmEMapInfoApplyHelpersTests.ApplyDoubleProperty_RejectsBadToken` |
| ApplyBossSpecials E1M8 | 436–439 | `VmEMapInfoApplyHelpersTests.ApplyBossSpecials_ParsesE1M8` |
| NormalizeResourceName | 104–112 | `VmEMapInfoLumpResolveTests.NormalizeLumpLookupName_NormalizesSlashes` |

### Chunk 328 — `g-mapinfo-hcde-shadow-map-chunk328` (76 → **12** native, commit src **23**)

Credits profile/quality/max-lights clamp assignments only — not surrounding light-blend parser.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| hcde_shadowprofile clamp | 1779–1782 | `VmMapInfoHcdeShadowMapOptionsTests.ApplyShadowProfile_ClampsHigh` |
| hcde_shadowqualitycap negative clear | 1793–1796 | `VmMapInfoHcdeShadowMapOptionsTests.ApplyShadowQualityCap_ClearsWhenNegative` |
| hcde_shadowmaxlightscap clamp | 1815–1818 | `VmMapInfoHcdeShadowMapOptionsTests.ApplyShadowMaxLightsCap_CapsAt1024` |

### Chunk 329 — `g-mapinfo-script-flags-head-chunk329` (79 → **11** native, commit src **24**)

Credits `edata`/`intro` string assigns and the `e1m8special` handler catalog row — not the full `MapFlagHandlers` head.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| edata assign | 1892–1897 | `VmMapInfoMapOptionsScriptTests.ApplyEdName_SetsField` |
| intro cutscene | 1906–1909 | `VmMapInfoMapOptionsScriptTests.ApplyIntroCutscene_SetsLump` |
| e1m8special handler row | 1960–1960 | `VmMapInfoFlagHandlersTests.TryGetHandler_FindsE1M8Special` |

### Chunk 330 — `g-mapinfo-flag-table-mid-chunk330` (116 → **2** native, commit src **17**)

Credits only handler rows referenced by mid-table lookup tests.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| strifefallingdamage row | 1983–1983 | `VmMapInfoFlagHandlersMidTests.TryGetHandler_FindsStrifeFallingDamage` |
| compat_noid24 row | 2091–2091 | `VmMapInfoFlagHandlersMidTests.TryGetHandler_FindsCompatNoid24` |

### Chunk 331 — `g-mapinfo-flag-apply-switch-chunk331` (138 → **22** native, commit src **39**)

Credits `MITYPE_SETFLAG2`, `MITYPE_SCFLAGS2`, and `MITYPE_CLRCOMPATFLAG` switch arms only.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| MITYPE_SETFLAG2 | 2159–2171 | `VmMapInfoFlagApplyTests.ApplySetFlag2_TogglesWithAssign` |
| MITYPE_SCFLAGS2 | 2178–2180 | `VmMapInfoFlagApplyTests.ApplyScFlags2_MergesTeamplayOn` |
| MITYPE_CLRCOMPATFLAG | 2205–2210 | `VmMapInfoFlagApplyTests.ApplyClearCompat_SetsMask` |

### Chunk 332 — `g-mapinfo-parse-header-chunk332` (149 → **23** native, commit src **59**)

Credits MAP/E#M# default levelnum, Hexen numeric map token, `$` title prefix, and default levelnum assign — not full `ParseMapHeader` body.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| GetDefaultLevelNum MAP branch | 2297–2305 | `VmMapInfoParseMapHeaderTests.AssignDefaultLevelNum_ParsesMap01` |
| ParseMapHeader Hexen number token | 2341–2345 | `VmMapInfoParseMapHeaderTests.ResolveMapNameToken_OldHexenNumber` |
| ParseMapHeader dollar title | 2382–2387 | `VmMapInfoParseMapHeaderTests.ApplyLevelTitle_DollarPrefixSetsLookup` |
| default levelnum assign | 2430–2432 | `VmMapInfoParseMapHeaderTests.AssignDefaultLevelNum_ParsesMap01` |

### Chunk 333 — `hcde-emapinfo-property-dispatch-chunk333` (125 → **31** native, commit src **62**)

Credits short lump-name fallback, `levelname`, `disable-jump`, and zero-gravity apply — not the full `ApplyEMapInfoProperty` dispatch tail.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| ResolveSiblingLump short name | 547–554 | `VmEMapInfoPropertyDispatchTests.ShouldTryShortLumpName_RejectsLongNames` |
| levelname assign | 573–578 | `VmEMapInfoPropertyDispatchTests.TryApplyLevelName_SetsTitle` |
| disable-jump | 639–647 | `VmEMapInfoPropertyDispatchTests.TryApplyDisableJump_SetsFlag` |
| gravity zero | 657–664 | `VmEMapInfoPropertyDispatchTests.TryApplyGravity_ZeroIsMax` |

### Chunk 336 — `dehacked-mbf-scratch-sound-chunk336` (86 → **27** native, commit src **17**)

Credits `CreateScratchFunc`, `CreateRandomJumpFunc`, and `CreateLineEffectFunc` argument packing only.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| CreateScratchFunc | 829–837 | `VmDehackedMbfScratchSoundTests.ScratchMeleeArgs_ResolvesHitSound` |
| CreateRandomJumpFunc | 851–858 | `VmDehackedMbfScratchSoundTests.RandomJumpArgs_ResolvesStateLabel` |
| CreateLineEffectFunc | 861–870 | `VmDehackedMbfScratchSoundTests.LineEffectArgs_PassesSpecialAndTag` |

### Chunk 337 — `g-mapinfo-episode-head-chunk337` (41 → **8** native, commit src **39**)

Credits shareware `teaser` map override only — not `ParseMapDefinition` comment header.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| ParseEpisodeInfo teaser | 2476–2483 | `VmMapInfoEpisodeParserTests.TryResolveTeaserMap_UsesTeaserWhenShareware` |

### Chunk 339 — `dehacked-mbf-pointer-emitters-chunk339` (163 → **22** native, commit src **17**)

Credits default-argument packing for tracer-family emitters and weapon bullet counts — not the full emitter table through melee attack.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| CreateFindTracerFunc | 936–941 | `VmDehackedMbfPointerEmittersTests.FindTracerArgs_DefaultsRange` |
| CreateSeekerMissileFunc | 943–952 | `VmDehackedMbfPointerEmittersTests.SeekerMissileArgs_AppliesDefaults` |
| CreateTracer2Func | 954–959 | `VmDehackedMbfPointerEmittersTests.Tracer2Angle_UsesDefaultWhenZero` |

### Chunk 340 — `am-map-cvar-head-chunk340` (118 → **24** native, commit src **20**)

Credits automap numeric constants, net-game `am_cheat` gate, and `am_emptyspacemargin` clamp — not CVAR registration blocks.

| Native symbol | Lines | Named test(s) |
|---------------|------:|---------------|
| Automap numeric constants | 66–76 | `VmAutomapCvarModelTests.ClampEmptySpaceMargin_Bounds` |
| am_cheat net gate | 135–139 | `VmAutomapCvarModelTests.ShouldBlockAutomapCheat_WhenNetNoCheats` |
| am_emptyspacemargin clamp | 170–177 | `VmAutomapCvarModelTests.ClampEmptySpaceMargin_Bounds` |

## Verified
`apply_chunks_207_342_trim.py`; sync; matrix; `reaudit_chunks.py audit --from 207 --to 342` (0 hard, 0 thin_managed_coverage).
