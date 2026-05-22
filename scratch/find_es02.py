import os
import re

def find_es02():
    filepath = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"
    if not os.path.exists(filepath):
        return

    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    # Find all start indices of ES02
    indices = [m.start() for m in re.finditer("ES02", content)]
    print(f"Total ES02 found: {len(indices)}")
    
    # Print what is around them if they don't end with "**" or if they are unusual.
    for idx in indices:
        around = content[max(0, idx - 10):min(len(content), idx + 20)]
        if not around.startswith("ES02**", 10):
            print(f"Unusual ES02 at {idx}: '{around}'")

if __name__ == '__main__':
    find_es02()
