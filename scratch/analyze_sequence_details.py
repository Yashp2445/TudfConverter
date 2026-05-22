import zipfile
import xml.etree.ElementTree as ET

xlsx_path = r"d:\TudfConverter\Docs\CU11880001_30042026__04052026_1131_F2_1.xlsx"
tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

# 1. Read Excel rows
excel_rows = {}
with zipfile.ZipFile(xlsx_path, 'r') as zip_ref:
    shared_strings = []
    sst_xml = zip_ref.read('xl/sharedStrings.xml')
    root = ET.fromstring(sst_xml)
    ns = {'ns': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
    for si in root.findall('ns:si', ns):
        t = si.find('ns:t', ns)
        if t is not None:
            shared_strings.append(t.text)
        else:
            text_parts = []
            for r in si.findall('ns:r', ns):
                r_t = r.find('ns:t', ns)
                if r_t is not None and r_t.text:
                    text_parts.append(r_t.text)
            shared_strings.append("".join(text_parts))

    sheet_xml = zip_ref.read('xl/worksheets/sheet1.xml')
    root = ET.fromstring(sheet_xml)
    
    for row in root.findall('.//ns:row', ns):
        row_num = int(row.attrib['r'])
        if row_num >= 11:
            row_data = {}
            for cell in row.findall('ns:c', ns):
                r_ref = cell.attrib['r']
                col_letter = ''.join(c for c in r_ref if c.isalpha())
                v = cell.find('ns:v', ns)
                val = ""
                if v is not None:
                    t = cell.attrib.get('t', '')
                    if t == 's':
                        val = shared_strings[int(v.text)]
                    else:
                        val = v.text
                row_data[col_letter] = val
            acc_num = (row_data.get('AJ') or '').strip()
            name = (row_data.get('A') or '').strip()
            if acc_num:
                key = (acc_num, name[:5])
                excel_rows[key] = row_num

# 2. Read TUDF records
with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

body = content[146:]
if body.endswith("TRLR"):
    body = body[:-4]
records = body.split("ES02**")

row_sequence = []
for idx, r in enumerate(records):
    if not r.strip():
        continue
    tl_start = r.find("TL04")
    acc = ""
    if tl_start != -1:
        t03_idx = r.find("T03", tl_start)
        if t03_idx != -1:
            len_acc = int(r[t03_idx+3:t03_idx+5])
            acc = r[t03_idx+5:t03_idx+5+len_acc]
            
    pn_start = r.find("PN03N0101")
    name = ""
    if pn_start != -1:
        len_name = int(r[pn_start+9:pn_start+11])
        name = r[pn_start+11:pn_start+11+len_name]
        
    key = (acc, name[:5])
    row_num = excel_rows.get(key, -1)
    row_sequence.append((idx, row_num))

# Detect transition and ordering behavior
print(f"Total row sequence elements: {len(row_sequence)}")
# Let's group sequence elements by modulo 24
mod_groups = {}
for idx, r_num in row_sequence:
    if r_num != -1:
        m = r_num % 24
        if m not in mod_groups:
            mod_groups[m] = []
        mod_groups[m].append((idx, r_num))

# Print info about each modulo group's index range in the TUDF file
for m in sorted(mod_groups.keys()):
    indices = [x[0] for x in mod_groups[m]]
    r_nums = [x[1] for x in mod_groups[m]]
    print(f"Modulo {m}: count={len(indices)}, min_idx={min(indices)}, max_idx={max(indices)}")
    # Are the row numbers within this modulo group strictly sorted?
    is_sorted = r_nums == sorted(r_nums)
    print(f"  Row numbers sorted? {is_sorted}")
    if not is_sorted:
        # Check if they are sorted except for a few anomalies
        unsorted_count = sum(1 for i in range(len(r_nums)-1) if r_nums[i] > r_nums[i+1])
        print(f"  Unsorted transitions count: {unsorted_count}")
        # Print first few unsorted elements
        unsorted_examples = []
        for i in range(len(r_nums)-1):
            if r_nums[i] > r_nums[i+1]:
                unsorted_examples.append((r_nums[i], r_nums[i+1]))
        print(f"  First 5 unsorted pairs: {unsorted_examples[:5]}")
