# Script archive fixtures

`scripts-solid.7z` was generated with 7-Zip 26.01 using LZMA2 and
`-ms=on`. Its five files share one solid block. Test execution reads the
embedded fixture and does not require 7-Zip to be installed.

Contents (UTF-8, no BOM):

- `scripts/a.zs`: `a`
- `scripts/b.zs`: `b`
- `scripts/c.zs`: `c`
- `zscript.a`: `version 4.13`, followed by includes for `scripts/c.zs`,
  `scripts/a.zs`, and `scripts/b.zs`, each on a separate LF-delimited line.
- `zscript.b`: `version 4.12`, followed by an include for `scripts/a.zs`.

Include directives use `#include "path"`. The last line of each file has
no trailing newline. The archive also contains the `scripts` directory.

To regenerate, create those files in a staging folder, then run 7-Zip from
that folder with `7z a -t7z -m0=lzma2 -ms=on scripts-solid.7z *`.
Confirm `Solid = +` and `Blocks = 1` with `7z l -slt scripts-solid.7z`.

`scripts-encrypted.7z` contains only `zscript` with text `version 4.13`
(UTF-8 without BOM or trailing newline). It was generated with 7-Zip 26.01
using `7z a -t7z -mhe=on -pfixture scripts-encrypted.7z *` from its staging
folder. The public test password is `fixture`. The loader provides no
password and must reject the encrypted header.

`scripts-bzip2.zip` contains `zscript` with `version 4.13` and an include
for `scripts/child.zs`; that child contains `bzip2 child`. Text is UTF-8
without BOM, uses LF, and has no trailing newline. The fixture includes
the `scripts` directory. It was generated with 7-Zip 26.01 using
`7z a -tzip -mm=BZip2 scripts-bzip2.zip *` from its staging folder.

`scripts-lzma.zip` and `scripts-encrypted.zip` contain the same root/include
layout, with child text `lzma child`. They were generated with 7-Zip 26.01
using `7z a -tzip -mm=LZMA scripts-lzma.zip *` and
`7z a -tzip -mem=ZipCrypto -pfixture scripts-encrypted.zip *`.
The public test password is `fixture`; the loader skips encrypted files.
