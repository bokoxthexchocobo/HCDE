import zipfile
from pathlib import Path

pk3_path = Path("tests/id24_validation/dist/HCDE-ID24-validation.pk3")
output_path = Path("temp_id24_validation.zs")

with zipfile.ZipFile(pk3_path, "r") as zf:
    with zf.open("zscript/id24_validation.zs") as zscript_file:
        output_path.write_bytes(zscript_file.read())
