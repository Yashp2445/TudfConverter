tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

body = content[146:]
if body.endswith("TRLR"):
    body = body[:-4]
records = body.split("ES02**")

indices_to_inspect = [0, 1, 2, 3, 4, 5, 148, 3161, 3465]

def parse_record(r):
    # Extract Account Number
    acc = ""
    tl_start = r.find("TL04")
    if tl_start != -1:
        t03_idx = r.find("T03", tl_start)
        if t03_idx != -1:
            len_acc = int(r[t03_idx+3:t03_idx+5])
            acc = r[t03_idx+5:t03_idx+5+len_acc]
            
    # Extract Name from PN03
    name = ""
    pn_start = r.find("PN03N0101")
    if pn_start != -1:
        len_val = int(r[pn_start+9:pn_start+11])
        name = r[pn_start+11:pn_start+11+len_val]
        
    # Extract all ID03 segments
    id_segments = []
    start = 0
    while True:
        id_idx = r.find("ID03", start)
        if id_idx == -1:
            break
        type_code = r[id_idx+10:id_idx+12]
        len_val_id = int(r[id_idx+14:id_idx+16])
        id_val = r[id_idx+16:id_idx+16+len_val_id]
        id_segments.append(f"{type_code}:{id_val}")
        start = id_idx + 4
        
    return acc, name, id_segments, r[:80] # return first 80 chars of record for raw view

for idx in indices_to_inspect:
    if idx < len(records):
        acc, name, ids, raw = parse_record(records[idx])
        print(f"Index {idx}: Acc='{acc}', Name='{name}', IDs={ids}")
        print(f"  Raw head: {raw}")
