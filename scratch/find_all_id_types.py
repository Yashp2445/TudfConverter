import os
import re

def find_all_id_types():
    filepath = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"
    if not os.path.exists(filepath):
        print("Reference file not found.")
        return

    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    body = content[146:]
    if body.endswith("TRLR"):
        body = body[:-4]

    # Find all occurrences of "ID03"
    # An ID segment has format ID03I{index}0102{type_code}02{len}{value}
    # Let's use regex to find all ID03 segments and parse the type code.
    pattern = r"ID03I\d\d0102(\d\d)"
    types = re.findall(pattern, body)
    
    type_counts = {}
    for t in types:
        type_counts[t] = type_counts.get(t, 0) + 1
        
    print("Unique ID types in reference TUDF and their counts:")
    print(type_counts)

if __name__ == '__main__':
    find_all_id_types()
