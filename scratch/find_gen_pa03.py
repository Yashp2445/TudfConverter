import os
import re

def find_pa03():
    filepath = r"d:\TudfConverter\Output\GeneratedFiles\CU11880001_30042026_161746.tudf"
    if not os.path.exists(filepath):
        return

    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    indices = [m.start() for m in re.finditer("PA03", content)]
    print(f"Total PA03 found in generated: {len(indices)}")
    
    unusual = 0
    for idx in indices:
        around = content[max(0, idx - 10):min(len(content), idx + 20)]
        if not around.startswith("PA03A01", 10):
            unusual += 1
            if unusual <= 10:
                print(f"Unusual PA03 at {idx}: '{around}'")
    print(f"Total unusual PA03: {unusual}")

if __name__ == '__main__':
    find_pa03()
