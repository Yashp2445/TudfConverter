import zipfile
import xml.etree.ElementTree as ET
import os

xlsx_path = r"d:\TudfConverter\Docs\CU11880001_30042026__04052026_1131_F2_1.xlsx"
tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

if not os.path.exists(tudf_path):
    print("tudf not found")
    exit(1)

# 1. Read Excel rows
excel_rows = []
with zipfile.ZipFile(xlsx_path, 'r') as zip_ref:
    shared_strings = []
    try:
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
    except Exception as e:
        print(e)

    sheet_xml = zip_ref.read('xl/worksheets/sheet1.xml')
    root = ET.fromstring(sheet_xml)
    ns = {'ns': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
    
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
            excel_rows.append(row_data)

# 2. Read TUDF records
with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

body = content[146:]
if body.endswith("TRLR"):
    body = body[:-4]
records = body.split("ES02**")

# Match first 50
for idx in range(min(50, len(records))):
    r = records[idx]
    if not r.strip():
        continue
    
    # Extract Name from PN03
    pn_start = r.find("PN03N0101")
    if pn_start == -1:
        continue
    len_val = int(r[pn_start+9:pn_start+11])
    name = r[pn_start+11:pn_start+11+len_val]
    
    # Extract all ID03 segments
    id_segments = []
    start = 0
    while True:
        id_idx = r.find("ID03", start)
        if id_idx == -1:
            break
        # Parse it
        type_code = r[id_idx+10:id_idx+12]
        len_val_id = int(r[id_idx+14:id_idx+16])
        id_val = r[id_idx+16:id_idx+16+len_val_id]
        id_segments.append((type_code, id_val))
        start = id_idx + 4
        
    # Excel row
    ex_row = excel_rows[idx]
    ex_name = (ex_row.get('A') or '').strip()
    ex_add1 = (ex_row.get('N') or '').strip()
    ex_add2 = (ex_row.get('O') or '').strip()
    ex_ckyc = (ex_row.get('BS') or '').strip()
    ex_nrega = (ex_row.get('BT') or '').strip()
    
    print(f"Index {idx}: Name='{name}' vs Excel Name='{ex_name}'")
    print(f"  Excel Add1='{ex_add1}', Add2='{ex_add2}', CKYC='{ex_ckyc}', NREGA='{ex_nrega}'")
    for tc, val in id_segments:
        if tc == '09':
            print(f"  TUDF Type 09 = '{val}'")
