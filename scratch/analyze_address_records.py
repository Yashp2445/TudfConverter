import zipfile
import xml.etree.ElementTree as ET
import os
import re

xlsx_path = r"d:\TudfConverter\Docs\CU11880001_30042026__04052026_1131_F2_1.xlsx"
tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

if not os.path.exists(tudf_path):
    print("tudf not found")
    exit(1)

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
            if acc_num:
                excel_rows[acc_num] = row_data

# 2. Read TUDF records
with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

body = content[146:]
if body.endswith("TRLR"):
    body = body[:-4]
records = body.split("ES02**")

for idx, r in enumerate(records):
    if not r.strip():
        continue
    
    # Let's count actual PA03 segments properly. A PA03 segment starts with PA03 and has segment index like A01, A02.
    # e.g., PA03A01 or PA03A02
    pa_segs = re.findall(r"PA03A\d\d", r)
    if len(pa_segs) > 1:
        # Extract Name from PN03
        pn_start = r.find("PN03N0101")
        name = ""
        if pn_start != -1:
            len_val = int(r[pn_start+9:pn_start+11])
            name = r[pn_start+11:pn_start+11+len_val]
            
        # Extract Account Number from TL04
        tl_start = r.find("TL04")
        acc = ""
        if tl_start != -1:
            # Format: TL04T01{len}{val}T02{len}{val}T03...
            # Account number is tag 03 in TL04.
            # Let's search for "T03" within TL04.
            t03_idx = r.find("T03", tl_start)
            if t03_idx != -1:
                len_acc = int(r[t03_idx+3:t03_idx+5])
                acc = r[t03_idx+5:t03_idx+5+len_acc]
        
        print(f"\nTUDF record {idx}: Name='{name}' Acc='{acc}' has {len(pa_segs)} PA03 segments:")
        # Print the segments
        for seg_tag in pa_segs:
            seg_start = r.find(seg_tag)
            # find end of segment (another tag or segment tag)
            # Simple extraction: let's extract 100 chars from seg_start
            print(f"  {r[seg_start:seg_start+120]}")
            
        # Find in Excel
        if acc in excel_rows:
            ex = excel_rows[acc]
            print(f"  Excel Name: '{ex.get('A')}'")
            print(f"  Address 1 (X-AB): '{ex.get('X')}', '{ex.get('Y')}', '{ex.get('Z')}', '{ex.get('AA')}', '{ex.get('AB')}'")
            print(f"  Address 2 (AC-AG): '{ex.get('AC')}', '{ex.get('AD')}', '{ex.get('AE')}', '{ex.get('AF')}', '{ex.get('AG')}'")
        else:
            print(f"  Account '{acc}' not found in Excel by AJ!")
