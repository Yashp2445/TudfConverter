tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

body = content[146:]
if body.endswith("TRLR"):
    body = body[:-4]
records = body.split("ES02**")

def parse_all_ids(r):
    ids = []
    start = 0
    while True:
        id_idx = r.find("ID03", start)
        if id_idx == -1:
            break
        # Tag is at id_idx
        # Let's read field by field
        # Tag 01 should start at id_idx + 7
        pos = id_idx + 7
        id_type = ""
        id_num = ""
        
        while pos < len(r) and r[pos:pos+2] in ["01", "02", "03", "04"]:
            field_tag = r[pos:pos+2]
            field_len = int(r[pos+2:pos+4])
            field_val = r[pos+4:pos+4+field_len]
            if field_tag == "01":
                id_type = field_val
            elif field_tag == "02":
                id_num = field_val
            pos = pos + 4 + field_len
            
        ids.append((id_type, id_num))
        start = id_idx + 4
    return ids

for idx in range(min(15, len(records))):
    r = records[idx]
    if not r.strip():
        continue
    # Get name
    pn_start = r.find("PN03N0101")
    name = ""
    if pn_start != -1:
        len_val = int(r[pn_start+9:pn_start+11])
        name = r[pn_start+11:pn_start+11+len_val]
        
    tl_start = r.find("TL04")
    acc = ""
    if tl_start != -1:
        t03_idx = r.find("T03", tl_start)
        if t03_idx != -1:
            len_acc = int(r[t03_idx+3:t03_idx+5])
            acc = r[t03_idx+5:t03_idx+5+len_acc]

    parsed_ids = parse_all_ids(r)
    print(f"Record {idx}: Acc='{acc}', Name='{name}'")
    for t, n in parsed_ids:
        print(f"  ID Type {t} = '{n}'")
