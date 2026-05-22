import glob
import os

ref_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

# Find the latest generated file
gen_files = glob.glob(r"D:\TudfConverter\Output\GeneratedFiles\CU11880001_DATASUBMISSION_*.tudf")
gen_path = max(gen_files, key=os.path.getmtime) if gen_files else None

if not gen_path:
    print("ERROR: No generated file found!")
    exit(1)

print(f"Reference: {ref_path}")
print(f"Generated: {gen_path}")

with open(ref_path, 'r', encoding='utf-8', errors='ignore') as f:
    ref = f.read()
with open(gen_path, 'r', encoding='utf-8', errors='ignore') as f:
    gen = f.read()

# Header comparison
print(f"\n--- HEADER (first 146 bytes) ---")
ref_hdr = ref[:146]
gen_hdr = gen[:146]
print(f"Match: {ref_hdr == gen_hdr}")
if ref_hdr != gen_hdr:
    print(f"  Ref: {repr(ref_hdr)}")
    print(f"  Gen: {repr(gen_hdr)}")

# Segment counts
segments = ["PN03", "ID03", "PT03", "PA03", "TL04", "ES02"]
print(f"\n--- SEGMENT COUNTS ---")
print(f"{'Segment':<10} {'Reference':>12} {'Generated':>12} {'Match':>8}")
print("-" * 44)
for seg in segments:
    ref_count = ref.count(seg)
    gen_count = gen.count(seg)
    match = "✓" if ref_count == gen_count else "✗"
    print(f"{seg:<10} {ref_count:>12} {gen_count:>12} {match:>8}")

# ID type breakdown
print(f"\n--- ID TYPE BREAKDOWN ---")
for id_type in ["0102", "0106", "0109", "0110"]:
    # Count occurrences of the ID type tag pattern in ID segments
    # Pattern: ID03Ixx0102XX where XX is the type code
    ref_count = ref.count(f"01{id_type[2:]}")  # This won't work well
    
# Better approach: count ID segments by type
import re
for label, type_code in [("Type 01 (PAN)", "0102010201"), ("Type 06 (Aadhaar)", "010202"), ("Type 09 (CKYC)", "010209")]:
    pass

# Simpler: just count "0102010201" pattern etc
# Actually let's count by splitting on ID03 and checking the type tag
def count_id_types(content):
    counts = {}
    pos = 0
    while True:
        idx = content.find("ID03I", pos)
        if idx == -1:
            break
        # After ID03Ixx comes tag 01 with the type code
        tag_start = idx + 7  # skip "ID03Ixx"
        if tag_start + 6 <= len(content):
            tag = content[tag_start:tag_start+2]  # should be "01"
            if tag == "01":
                type_len = int(content[tag_start+2:tag_start+4])
                type_val = content[tag_start+4:tag_start+4+type_len]
                counts[type_val] = counts.get(type_val, 0) + 1
        pos = idx + 1
    return counts

ref_id_types = count_id_types(ref)
gen_id_types = count_id_types(gen)

all_types = sorted(set(list(ref_id_types.keys()) + list(gen_id_types.keys())))
print(f"{'ID Type':<15} {'Reference':>12} {'Generated':>12} {'Match':>8}")
print("-" * 49)
for t in all_types:
    rc = ref_id_types.get(t, 0)
    gc = gen_id_types.get(t, 0)
    match = "✓" if rc == gc else "✗"
    print(f"Type {t:<10} {rc:>12} {gc:>12} {match:>8}")

# Total IDs
ref_total = sum(ref_id_types.values())
gen_total = sum(gen_id_types.values())
print(f"{'TOTAL':<15} {ref_total:>12} {gen_total:>12} {'✓' if ref_total == gen_total else '✗':>8}")

# Trailer
print(f"\n--- TRAILER ---")
print(f"Ref ends with TRLR: {ref.endswith('TRLR')}")
print(f"Gen ends with TRLR: {gen.endswith('TRLR')}")

# File size comparison
print(f"\n--- FILE SIZE ---")
print(f"Reference: {len(ref):,} chars")
print(f"Generated: {len(gen):,} chars")
print(f"Difference: {len(gen) - len(ref):,} chars ({(len(gen) - len(ref)) / len(ref) * 100:.2f}%)")
