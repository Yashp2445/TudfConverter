import os

def parse():
    filepath = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"
    if not os.path.exists(filepath):
        print(f"File not found: {filepath}")
        return

    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    body = content[146:]
    if body.endswith("TRLR"):
        body = body[:-4]

    records = body.split("ES02**")
    
    # We want to count segments using actual segment tag boundaries, not simple substrings
    # A segment starts with a tag like PN03, ID03, PT03, PA03, TL04.
    # Let's count them properly by looking at how they start.
    
    id_counts = {}
    pt_counts = {}
    pa_counts = {}
    tl_counts = {}
    
    for i, r in enumerate(records):
        if not r.strip():
            continue
            
        # To count properly, let's count occurrences of the segment markers.
        # But we know PN03 could be inside a value. But segment markers always appear at segment boundaries.
        # Let's count them by split or by pattern:
        # A segment in a record always starts with a tag: PN03, ID03, PT03, PA03, TL04.
        # Let's count occurrences of "ID03" in each record block.
        # Since ID03 tag prefix is "ID03", is "ID03" ever in an ID value? No, ID03 is unlikely to be in an ID value.
        # Let's count them.
        id_c = r.count("ID03")
        pt_c = r.count("PT03")
        pa_c = r.count("PA03")
        tl_c = r.count("TL04")
        
        id_counts[id_c] = id_counts.get(id_c, 0) + 1
        pt_counts[pt_c] = pt_counts.get(pt_c, 0) + 1
        pa_counts[pa_c] = pa_counts.get(pa_c, 0) + 1
        tl_counts[tl_c] = tl_counts.get(tl_c, 0) + 1

    print("ID03 counts distribution (number of ID03 segments per record -> record count):")
    print(sorted(id_counts.items()))
    
    print("\nPT03 counts distribution:")
    print(sorted(pt_counts.items()))
    
    print("\nPA03 counts distribution:")
    print(sorted(pa_counts.items()))
    
    print("\nTL04 counts distribution:")
    print(sorted(tl_counts.items()))

if __name__ == '__main__':
    parse()
