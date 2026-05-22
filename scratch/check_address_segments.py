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

# Find records with 2 PA03 segments in reference TUDF
for idx, r in enumerate(records):
    if not r.strip():
        continue
    
    pa_count = r.count("PA03")
    if pa_count == 2:
        # Extract Name
        pn_start = r.find("PN03N0101")
        name = ""
        if pn_start != -1:
            len_val = int(r[pn_start+9:pn_start+11])
            name = r[pn_start+11:pn_start+11+len_val]
            
        print(f"\nIndex {idx}: Name='{name}' has 2 PA03 segments")
        
        # Extract the two PA03 segments
        start = 0
        pa_segs = []
        while True:
            pa_idx = r.find("PA03", start)
            if pa_idx == -1:
                break
            # Find end of PA segment (next segment tag: TL04 or PA03)
            end_idx = r.find("PA03", pa_idx + 4)
            if end_idx == -1 or end_idx == pa_idx: # Wait, if it is the second PA03
                end_idx = r.find("TL04", pa_idx)
            if end_idx == -1:
                end_idx = len(r)
            pa_segs.append(r[pa_idx:end_idx])
            start = pa_idx + 4
            
        for i, pa in enumerate(pa_segs):
            print(f"  TUDF PA {i+1}: {pa}")
            
        # Match with Excel
        ex_row = excel_rows[idx]
        print(f"  Excel Name: '{ex_row.get('A')}'")
        print(f"  Excel Addr1 (Col X): '{ex_row.get('X')}', State1: '{ex_row.get('Y')}', PIN1: '{ex_row.get('Z')}'")
        print(f"  Excel Addr2 (Col AC): '{ex_row.get('AC')}', State2: '{ex_row.get('AD')}', PIN2: '{ex_row.get('AE')}'")
        
        # Let's break after first 3 matches
        if idx > 1000: # Just print some
            break
