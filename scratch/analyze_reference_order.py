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
    row_sequence.append(row_num)

# Let's print the first 120 elements of row_sequence to see the pattern
print("First 120 Excel row numbers in reference TUDF:")
print(row_sequence[:120])

# Let's see if we can find the offsets
# For example, what is the value modulo 24 of these rows?
modulos = [r % 24 for r in row_sequence[:120] if r != -1]
print("\nRow numbers modulo 24:")
print(modulos)

# Let's count how many rows fall into each modulo 24 bucket
buckets = {}
for r in row_sequence:
    if r != -1:
        m = r % 24
        buckets[m] = buckets.get(m, 0) + 1
print("\nCounts of rows by modulo 24:")
print(buckets)

# Let's inspect the first 10 rows of modulo 16 (or whatever modulo matches the first bucket)
print("\nFirst 10 rows matching modulo 16:")
mod_16_rows = [r for r in row_sequence if r % 24 == 16][:10]
print(mod_16_rows)

print("\nFirst 10 rows matching modulo 17:")
mod_17_rows = [r for r in row_sequence if r % 24 == 17][:10]
print(mod_17_rows)

print("\nFirst 10 rows matching modulo 18:")
mod_18_rows = [r for r in row_sequence if r % 24 == 18][:10]
print(mod_18_rows)
