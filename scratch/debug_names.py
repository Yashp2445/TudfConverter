import os

tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"
if not os.path.exists(tudf_path):
    print("Reference file not found.")
    exit(1)

with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

body = content[146:]
if body.endswith("TRLR"):
    body = body[:-4]

records = body.split("ES02**")
found = 0
for idx, r in enumerate(records):
    pn_start = r.find("PN03N01")
    if pn_start == -1:
        continue
    
    # Parse fields of the PN03 segment
    fields = {}
    curr = pn_start + 7
    while curr < len(r):
        tag = r[curr:curr+2]
        if tag in ["07", "08"]:
            break
        # If we see ID03 or other tags, stop
        if not tag.isdigit():
            break
        try:
            val_len = int(r[curr+2:curr+4])
            val = r[curr+4:curr+4+val_len]
            fields[tag] = val
            curr = curr + 4 + val_len
        except ValueError:
            break
            
    if "02" in fields:
        parts = [fields[t] for t in sorted(fields.keys())]
        print(f"Rec {idx:5d}: Fields={fields} -> Joined: '{' '.join(parts)}'")
        found += 1
        if found >= 20:
            break
