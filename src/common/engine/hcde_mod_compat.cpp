/*
** hcde_mod_compat.cpp
**
** HCDE-managed compatibility resources for known gameplay mods.
**
**---------------------------------------------------------------------------
**
** SPDX-License-Identifier: GPL-3.0-or-later
**
*/

#include "hcde_mod_compat.h"

#include "cmdlib.h"
#include "findfile.h"
#include "fs_findfile.h"
#include "printf.h"

#include <vector>

// Keep compatibility resources discoverable even when users install them from the
// optional compat zip in separate folders, instead of the bundled base package.

struct HCDEModCompatEntry
{
	const char* Label;
	const char* ResourceFile;
	const char* StartupMapOverride;
	const char* const* Patterns;
	unsigned int Flags;
	// Null-terminated list of resources that must be parsed BEFORE the matched
	// mod (e.g. DECORATE/ZScript parent classes the mod inherits from). Unlike
	// ResourceFile - which is appended after the mod, the right place for
	// `replaces`/override shims - these are inserted ahead of the mod in the
	// load order, because parent-class lookup is resolved by load order at
	// parse time (PClass::FindActor in CreateNewActor). Trailing field, so
	// existing 5-field initializers leave it null via aggregate value-init.
	const char* const* PreloadFiles;
};

static const char* const BrutalDoomRailgunPatterns[] =
{
	"brutal*.pk3",
	"brutaldoom*.pk3",
	nullptr
};

static const char* const AliensEradicationPatterns[] =
{
	"ALIENS_ERADICATION_TC*",
	"ALIENS_ERADICATION_TC*.pk3",
	nullptr
};

static const char* const AliensEradicationMapsetPatterns[] =
{
	"ERADICATION_MAPSET*",
	"ERADICATION_MAPSET*.wad",
	nullptr
};

static const char* const TheIslandPatterns[] =
{
	"theisland*.pk3",
	"the_island*.pk3",
	nullptr
};

static const char* const PinkValleyPatterns[] =
{
	"THE_PINK_VALLEY - ENG*",
	"the_pink_valley*",
	nullptr
};

static const char* const Armageddon2Patterns[] =
{
	"armageddon2*",
	"Armageddon2*",
	nullptr
};

static const char* const BeheadedKamikaziPatterns[] =
{
	"armageddon2*",
	"Armageddon2*",
	"BeheadedKamikazi*",
	"beheadedkamikazi*",
	nullptr
};

static const char* const SkulltagContentPatterns[] =
{
	"armageddon2*",
	"Armageddon2*",
	"skulltag_content*",
	"skulltagcontent*",
	nullptr
};

static const char* const SkulltagFlamePatchPatterns[] =
{
	"armageddon2*",
	"Armageddon2*",
	"skulltag_content*",
	"skulltagcontent*",
	"skulltag_flame_patches*",
	nullptr
};

static const char* const MonstersAndAddonsPatterns[] =
{
	"Monstersandaddons*.pk3",
	"Monsters_and_addons*.pk3",
	nullptr
};

static const char* const BladeOfAgonyPatterns[] =
{
	"boa_c31.4*",
	"boa_c31_4*",
	"boa.ipk3",
	"Blade of Agony*",
	nullptr
};

static const char* const DoomCenterPatterns[] =
{
	"doomcenter*.pk3",
	"doomcenter*",
	"DoomCenter*",
	nullptr
};

// Loaded ahead of DoomCenter so the DECORATE parents it inherits from exist
// when its lumps are parsed (parent lookup is load-order sensitive).
static const char* const DoomCenterPreloadFiles[] =
{
	// Skulltag base monsters (Abaddon, Belphegor, BloodDemon, Cacolantern,
	// DarkImp, Hectebus, SuperShotgunGuy) that DoomCenter's holographic display
	// actors inherit from. Third-party; the user supplies the file (the same one
	// HCDE already pulls in for Armageddon2). Resolved best-effort - if it is
	// absent DoomCenter will still fail on those parents, which is a genuine
	// missing-dependency situation we cannot legally bundle.
	"skulltag_content-4.0.pk3",
	// HCDE-authored stub for FloatyIcon, a Skulltag/Zandronum engine built-in
	// (present in neither skulltag_content nor skulltag_actors) that DoomCenter
	// inherits from + replaces. Ours to ship.
	"hcde_mod_compat_doomcenter.pk3",
	nullptr
};

static unsigned int ActiveCompatFlags = 0u;
static const char* ActiveStartupMapOverride = nullptr;

static const char* const HCDEModCompatSearchFolders[] =
{
	"",
	"compat",
	"compat-mods",
	"compatibility",
	"Mod Compatibilities",
	"mods",
	"mod_compat",
	nullptr
};

static FString HCDE_ModCompat_NormalizedProgDir()
{
	FString base = progdir;
	FixPathSeperator(base);
	if (base.IsNotEmpty() && (base.Back() == '/' || base.Back() == '\\'))
	{
		base.DeleteLastCharacter();
	}
	return base;
}

static FString HCDE_ModCompat_ParentDir(const FString& path)
{
	FString parent = path;
	if (parent.IsEmpty())
	{
		return parent;
	}
	if (parent.Back() == '/' || parent.Back() == '\\')
	{
		parent.DeleteLastCharacter();
	}

	ptrdiff_t slash = parent.LastIndexOfAny("/\\");
	if (slash < 0)
	{
		return parent;
	}

	return parent.Left(static_cast<size_t>(slash + 1));
}

// True for a path that is a filesystem/drive root (e.g. "C:/", "C:", "/").
// We must never launch a directory scan from such a location: a root contains
// the whole drive, and walking it for a possibly-absent file is exactly the
// multi-minute startup hang this resolver was rewritten to eliminate.
static bool HCDE_ModCompat_IsFilesystemRoot(const FString& path)
{
	if (path.IsEmpty())
	{
		return true;
	}

	FString p = path;
	FixPathSeperator(p);
	while (p.Len() > 0 && p.Back() == '/')
	{
		p.DeleteLastCharacter();
	}

	if (p.IsEmpty())
	{
		// Was "/" - the POSIX filesystem root.
		return true;
	}

	// "C:" (a Windows drive letter with nothing after it) is a drive root.
	if (p.Len() == 2 && p[1] == ':')
	{
		return true;
	}

	return false;
}

// Directory portion (with trailing '/') of a file path, normalized to '/'.
// Empty if the path has no directory component.
static FString HCDE_ModCompat_FileDir(const char* filepath)
{
	if (filepath == nullptr || filepath[0] == '\0')
	{
		return FString();
	}

	FString p = filepath;
	FixPathSeperator(p);
	ptrdiff_t slash = p.LastIndexOfAny("/\\");
	if (slash < 0)
	{
		return FString();
	}
	return p.Left(static_cast<size_t>(slash + 1));
}

// Look for `resourceFile` directly in `dir` and in each IMMEDIATE subdirectory
// of `dir` - one level only, never a recursive descent. Returns the full path
// or an empty string. This is the bounded replacement for the old whole-drive
// crawl: it is only ever pointed at the specific folders that hold the user's
// loaded mods, so the cost is a single directory listing per mod folder.
static FString HCDE_ModCompat_ShallowFind(const FString& dir, const char* resourceFile)
{
	if (dir.IsEmpty() || HCDE_ModCompat_IsFilesystemRoot(dir))
	{
		return FString();
	}

	FString base = dir;
	if (base.Back() != '/' && base.Back() != '\\')
	{
		base << '/';
	}

	FString direct = base;
	direct << resourceFile;
	if (DirEntryExists(direct.GetChars()))
	{
		return direct;
	}

	// nosubdir = true keeps this to the immediate children of `dir`.
	FileSys::FileList list;
	if (FileSys::ScanDirectory(list, dir.GetChars(), "*", true))
	{
		for (auto& entry : list)
		{
			if (entry.isDirectory && !entry.isHidden && !entry.isSystem)
			{
				FString candidate = entry.FilePath.c_str();
				FixPathSeperator(candidate);
				if (candidate.IsNotEmpty() && candidate.Back() != '/')
				{
					candidate << '/';
				}
				candidate << resourceFile;
				if (FileExists(candidate))
				{
					return candidate;
				}
			}
		}
	}

	return FString();
}

static const char* HCDE_ModCompat_ResolveCompatFile(const char* resourceFile, FConfigFile* config,
	const std::vector<FileSys::ResourceName>& searchWads)
{
	if (resourceFile == nullptr || resourceFile[0] == '\0')
	{
		return nullptr;
	}

	const char* found = BaseFileSearch(resourceFile, nullptr, true, config);
	if (found != nullptr)
	{
		return found;
	}

	static FString fallback;
	fallback = "";

	const FString normalizedProgDir = HCDE_ModCompat_NormalizedProgDir();
	const FString parentProgDir = HCDE_ModCompat_ParentDir(normalizedProgDir);

	// Build the bounded set of root directories to probe. Compat PK3s are
	// normally shipped alongside the mods they patch - e.g. the user's
	// ".../Monstersandaddons/Mod Compatibilities/" folder, nowhere near
	// hcde.exe - so the directories of the already-loaded WAD/PK3 files (which
	// HCDE_ModCompat_AppendFiles hands us in searchWads) are the key roots.
	// We also include the engine folder, its parent, and the working directory.
	// Any root that resolves to a filesystem/drive root is dropped, because the
	// old code's whole-drive crawl from "C:\" was the 5-minute launch hang.
	std::vector<FString> roots;
	auto addRoot = [&roots](FString candidate)
	{
		if (candidate.IsEmpty())
		{
			return;
		}
		FixPathSeperator(candidate);
		while (candidate.Len() > 1 && candidate.Back() == '/')
		{
			candidate.DeleteLastCharacter();
		}
		if (candidate.IsEmpty() || HCDE_ModCompat_IsFilesystemRoot(candidate))
		{
			return;
		}
		for (const FString& existing : roots)
		{
			if (!stricmp(existing.GetChars(), candidate.GetChars()))
			{
				return;
			}
		}
		roots.push_back(candidate);
	};

	addRoot(normalizedProgDir);
	addRoot(parentProgDir);
	for (const auto& wad : searchWads)
	{
		addRoot(HCDE_ModCompat_FileDir(wad.Name.c_str()));
	}

	// Pass 1: cheap direct probes - each root combined with the known compat
	// subfolder names. No directory enumeration, just one stat() per candidate.
	for (const FString& root : roots)
	{
		for (size_t j = 0; HCDEModCompatSearchFolders[j] != nullptr; ++j)
		{
			FString basePath = root;
			if (basePath.IsNotEmpty() && basePath.Back() != '/' && basePath.Back() != '\\')
			{
				basePath << '/';
			}

			const char* folder = HCDEModCompatSearchFolders[j];
			if (*folder != '\0')
			{
				basePath << folder;
				basePath << '/';
			}

			FString candidate = basePath;
			candidate << resourceFile;
			if (DirEntryExists(candidate.GetChars()))
			{
				fallback = std::move(candidate);
				Printf("HCDE: compatibility resource '%s' resolved from fallback path '%s'.\n", resourceFile, fallback.GetChars());
				return fallback.GetChars();
			}
		}
	}

	// Pass 2: one-level scan of each root's immediate subdirectories. This
	// catches arbitrarily-named drop folders (the working logs show users keep
	// patches in a "Mod Compatibilities" folder) without crawling the whole
	// tree. Filesystem roots were already excluded in addRoot(), so this can
	// never walk an entire drive the way the old RecursiveFileExists() path did.
	for (const FString& root : roots)
	{
		FString hit = HCDE_ModCompat_ShallowFind(root, resourceFile);
		if (hit.IsNotEmpty())
		{
			fallback = std::move(hit);
			Printf("HCDE: compatibility resource '%s' resolved from mod-folder scan '%s'.\n", resourceFile, fallback.GetChars());
			return fallback.GetChars();
		}
	}

	Printf("HCDE: compatibility resource '%s' not found near the engine or the loaded mods; "
		"skipping (place it next to hcde.exe, in a 'compat' subfolder, or beside the mod it patches).\n",
		resourceFile);
	return nullptr;
}

static const HCDEModCompatEntry ModCompatEntries[] =
{
	{
		"Brutal Doom railgun server compatibility",
		"hcde_mod_compat_combined.pk3",
		nullptr,
		BrutalDoomRailgunPatterns,
		0u
	},
	{
		"Aliens Eradication dedicated player input compatibility",
		nullptr,
		nullptr,
		AliensEradicationPatterns,
		HCDE_MODCOMPAT_ALIENS_PLAYER0_INPUT
	},
	{
		"The Island MAPINFO and sound compatibility",
		"hcde_mod_compat_combined.pk3",
		nullptr,
		TheIslandPatterns,
		HCDE_MODCOMPAT_MAPINFO_TRAILING_TEXT_COMMA
	},
	{
		"Pink Valley map compatibility",
		"hcde_mod_compat_pink_valley_eng.pk3",
		"A_NEW_DAY",
		PinkValleyPatterns,
		HCDE_MODCOMPAT_MAPINFO_SKY_SPEED_NO_COMMA
	},
	{
		"Armageddon2 compatibility",
		"hcde_mod_compat_armageddon2_test.pk3",
		nullptr,
		Armageddon2Patterns,
		0u
	},
	{
		"Beheaded Kamikazi monster pack",
		"BeheadedKamikazi.pk3",
		nullptr,
		BeheadedKamikaziPatterns,
		0u
	},
	{
		"Skulltag content resources",
		"skulltag_content-4.0.pk3",
		nullptr,
		SkulltagContentPatterns,
		0u
	},
	{
		"Skulltag flame patch sprites",
		"skulltag_flame_patches.pk3",
		nullptr,
		SkulltagFlamePatchPatterns,
		0u
	},
	{
		"Monsters and Addons settings controller script compatibility",
		nullptr,
		nullptr,
		MonstersAndAddonsPatterns,
		HCDE_MODCOMPAT_SETTINGS_CONTROLLER_NONNET_SCRIPTS
	},
	{
		"Blade of Agony compatibility",
		"hcde_mod_compat_boa_c31_4.pk3",
		nullptr,
		BladeOfAgonyPatterns,
		0u
	},
	{
		"DoomCenter Skulltag actor + hub map compatibility",
		nullptr,
		"MAP55",
		DoomCenterPatterns,
		0u,
		DoomCenterPreloadFiles
	}
};

static bool HCDE_ModCompat_FileAlreadyListed(const std::vector<FileSys::ResourceName>& wadfiles, const char* file)
{
	FString targetBase = ExtractFileBase(file, true);

	for (const auto& wad : wadfiles)
	{
		if (!stricmp(wad.Name.c_str(), file))
		{
			return true;
		}

		FString wadBase = ExtractFileBase(wad.Name.c_str(), true);
		if (!stricmp(wadBase.GetChars(), targetBase.GetChars()))
		{
			return true;
		}
	}

	return false;
}

static bool HCDE_ModCompat_FileMatchesPattern(const FileSys::ResourceName& wad, const char* pattern)
{
	FString base = ExtractFileBase(wad.Name.c_str(), true);
	return CheckWildcards(pattern, base.GetChars()) || CheckWildcards(pattern, wad.Name.c_str());
}

static bool HCDE_ModCompat_EntryMatches(const std::vector<FileSys::ResourceName>& pwads, const HCDEModCompatEntry& entry)
{
	for (const auto& wad : pwads)
	{
		for (const char* const* pattern = entry.Patterns; *pattern != nullptr; ++pattern)
		{
			if (HCDE_ModCompat_FileMatchesPattern(wad, *pattern))
			{
				return true;
			}
		}
	}

	return false;
}

static int HCDE_ModCompat_FindFirstMatch(const std::vector<FileSys::ResourceName>& pwads, const char* const* patterns)
{
	for (size_t i = 0; i < pwads.size(); ++i)
	{
		for (const char* const* pattern = patterns; *pattern != nullptr; ++pattern)
		{
			if (HCDE_ModCompat_FileMatchesPattern(pwads[i], *pattern))
			{
				return static_cast<int>(i);
			}
		}
	}

	return -1;
}

static void HCDE_ModCompat_NormalizeAliensEradicationOrder(std::vector<FileSys::ResourceName>& pwads)
{
	const int mapsetIndex = HCDE_ModCompat_FindFirstMatch(pwads, AliensEradicationMapsetPatterns);
	if (mapsetIndex < 0)
	{
		return;
	}

	const int tcIndex = HCDE_ModCompat_FindFirstMatch(pwads, AliensEradicationPatterns);
	if (tcIndex < 0)
	{
		Printf("HCDE: Aliens Eradication mapset detected without ALIENS_ERADICATION_TC; load the TC before the mapset.\n");
		return;
	}

	if (mapsetIndex < tcIndex)
	{
		// The mapset replaces actors defined by the TC. Some launchers preserve UI order,
		// others sort paths; normalize here so the known pair parses in dedicated servers.
		FileSys::ResourceName tc = pwads[tcIndex];
		pwads.erase(pwads.begin() + tcIndex);
		pwads.insert(pwads.begin() + mapsetIndex, tc);
		Printf("HCDE: reordered Aliens Eradication add-ons so the TC loads before the mapset.\n");
	}
}

void HCDE_ModCompat_AppendFiles(std::vector<FileSys::ResourceName>& pwads, FConfigFile* config, const std::vector<FileSys::ResourceName>* iwads)
{
	ActiveCompatFlags = 0u;
	ActiveStartupMapOverride = nullptr;

	if (pwads.empty() && (iwads == nullptr || iwads->empty()))
	{
		return;
	}

	HCDE_ModCompat_NormalizeAliensEradicationOrder(pwads);

	// Most compat resources are keyed by files passed through -file, but some
	// total conversions (notably Blade of Agony) can be selected as the IWAD.
	// Include the chosen IWADs in detection while still appending compat PK3s
	// to the normal PWAD list so load order remains explicit and late.
	std::vector<FileSys::ResourceName> matchWads = pwads;
	if (iwads != nullptr)
	{
		matchWads.insert(matchWads.end(), iwads->begin(), iwads->end());
	}

	for (const auto& entry : ModCompatEntries)
	{
		if (!HCDE_ModCompat_EntryMatches(matchWads, entry))
		{
			continue;
		}

		if (entry.Flags != 0u)
		{
			ActiveCompatFlags |= entry.Flags;
			Printf("HCDE: enabled mod compatibility '%s'.\n", entry.Label);
		}
		if (ActiveStartupMapOverride == nullptr && entry.StartupMapOverride != nullptr && entry.StartupMapOverride[0] != '\0')
		{
			ActiveStartupMapOverride = entry.StartupMapOverride;
			Printf("HCDE: startup map compatibility override is '%s'.\n", ActiveStartupMapOverride);
		}

		if (entry.PreloadFiles != nullptr)
		{
			// Insert each dependency immediately before the matched mod so its
			// DECORATE/ZScript parents are parsed first. Appending (the
			// ResourceFile path below) would leave the parents undefined and
			// abort the mod's parse. modIndex tracks the mod's current position;
			// each successful insert shifts the mod down by one, so we advance
			// modIndex to keep the preload files in their listed order and all
			// ahead of the mod.
			int modIndex = HCDE_ModCompat_FindFirstMatch(pwads, entry.Patterns);
			for (const char* const* preload = entry.PreloadFiles; *preload != nullptr; ++preload)
			{
				const char* preloadFile = HCDE_ModCompat_ResolveCompatFile(*preload, config, matchWads);
				if (preloadFile == nullptr)
				{
					Printf("HCDE: mod compatibility '%s' dependency '%s' was not found near the engine or the loaded mods; "
						"the mod may fail to load.\n", entry.Label, *preload);
					continue;
				}
				if (HCDE_ModCompat_FileAlreadyListed(pwads, preloadFile))
				{
					continue;
				}
				const int insertAt = (modIndex >= 0) ? modIndex : -1;
				if (D_AddFile(pwads, preloadFile, true, insertAt, config, false))
				{
					Printf("HCDE: preloaded dependency '%s' for mod compatibility '%s'.\n", preloadFile, entry.Label);
					if (modIndex >= 0)
					{
						++modIndex;
					}
				}
			}
		}

		if (entry.ResourceFile == nullptr || entry.ResourceFile[0] == '\0')
		{
			continue;
		}

		const char* compatFile = HCDE_ModCompat_ResolveCompatFile(entry.ResourceFile, config, matchWads);
		if (compatFile == nullptr)
		{
			Printf("HCDE: mod compatibility '%s' matched, but '%s' was not found.\n", entry.Label, entry.ResourceFile);
			continue;
		}

		if (HCDE_ModCompat_FileAlreadyListed(pwads, compatFile))
		{
			continue;
		}

		if (D_AddFile(pwads, compatFile, true, -1, config, false))
		{
			Printf("HCDE: loaded mod compatibility '%s'.\n", entry.Label);
		}
	}
}

bool HCDE_ModCompat_IsActive(unsigned int flags)
{
	return (ActiveCompatFlags & flags) == flags;
}

const char* HCDE_ModCompat_ResolveStartupMapOverride(const char* requestedMap)
{
	if (ActiveStartupMapOverride == nullptr || requestedMap == nullptr || requestedMap[0] == '\0')
	{
		return nullptr;
	}

	// External launchers commonly pass MAP01/E1M1 as a default map value when
	// the user has not picked one. For mods with a custom first map, treat
	// those two values as a launcher default and remap them to the known
	// compatibility entrypoint.
	if (stricmp(requestedMap, "MAP01") != 0 && stricmp(requestedMap, "E1M1") != 0)
	{
		return nullptr;
	}

	return ActiveStartupMapOverride;
}
