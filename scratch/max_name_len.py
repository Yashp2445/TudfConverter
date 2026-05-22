import os

def max_name_len():
    filepath = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"
    if not os.path.exists(filepath):
        return

    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    body = content[146:]
    if body.endswith("TRLR"):
        body = body[:-4]

    records = body.split("ES02**")

    max_len = 0
    max_name_str = ""
    for r in records:
        if not r.strip():
            continue
        pn_start = r.find("PN03N0101")
        if pn_start != -1:
            len_val = int(r[pn_start+9:pn_start+11])
            name_val = r[pn_start+11:pn_start+11+len_val]
            if len_val > max_len:
                max_len = len_val
                max_name_str = name_val

    print(f"Max name length: {max_len} (Value: '{max_name_str}')")

if __name__ == '__main__':
    max_name_len()
