import os

def analyze():
    filepath = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"
    if not os.path.exists(filepath):
        return

    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    body = content[146:]
    if body.endswith("TRLR"):
        body = body[:-4]

    records = body.split("ES02**")

    # Let's parse the first 10 records and look at their Name and IDs.
    for i in range(10):
        r = records[i]
        if not r.strip():
            continue
        
        # Name segment starts with PN03N0101
        # Let's extract Name: find PN03, then extract until next tag (which is usually ID03)
        pn_idx = r.find("PN03")
        id_indices = []
        start = 0
        while True:
            id_idx = r.find("ID03", start)
            if id_idx == -1:
                break
            id_indices.append(id_idx)
            start = id_idx + 4
            
        # Get Name
        next_tag_idx = id_indices[0] if id_indices else len(r)
        name_seg = r[pn_idx:next_tag_idx]
        
        print(f"\nRecord {i+1}:")
        print(f"  Name Segment: {name_seg}")
        
        # Get IDs
        for j, id_idx in enumerate(id_indices):
            end_idx = id_indices[j+1] if j+1 < len(id_indices) else r.find("PT03", id_idx)
            if end_idx == -1:
                end_idx = r.find("PA03", id_idx)
            if end_idx == -1:
                end_idx = r.find("TL04", id_idx)
            if end_idx == -1:
                end_idx = len(r)
            id_seg = r[id_idx:end_idx]
            print(f"  ID Segment {j+1}: {id_seg}")

if __name__ == '__main__':
    analyze()
