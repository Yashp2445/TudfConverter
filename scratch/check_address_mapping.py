import zipfile
import xml.etree.ElementTree as ET
import os
import re

xlsx_path = r"d:\TudfConverter\Docs\CU11880001_30042026__04052026_1131_F2_1.xlsx"
tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

if not os.path.exists(tudf_path):
    print("tudf not found")
    exit(1)

# 1. Read Excel rows where AC is not empty
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
            ac_val = (row_data.get('AC') or '').strip()
            if ac_val:
                excel_rows[acc_num] = row_data

print(f"Found {len(excel_rows)} rows in Excel with Column AC (Address Line 2) populated.")

# 2. Read TUDF records
with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

body = content[146:]
if body.endswith("TRLR"):
    body = body[:-4]
records = body.split("ES02**")

matched = 0
for idx, r in enumerate(records):
    if not r.strip():
        continue
        
    # Extract Account Number from TL04
    tl_start = r.find("TL04")
    acc = ""
    if tl_start != -1:
        t03_idx = r.find("T03", tl_start)
        if t03_idx != -1:
            len_acc = int(r[t03_idx+3:t03_idx+5])
            acc = r[t03_idx+5:t03_idx+5+len_acc]
            
    if acc in excel_rows:
        matched += 1
        ex = excel_rows[acc]
        # Extract name
        pn_start = r.find("PN03N0101")
        name = ""
        if pn_start != -1:
            len_val = int(r[pn_start+9:pn_start+11])
            name = r[pn_start+11:pn_start+11+len_val]
            
        print(f"\nMatch {matched}: Acc='{acc}' Name='{name}'")
        print(f"  Excel Address Line 1 (X): '{ex.get('X')}'")
        print(f"  Excel Address Line 2 (AC): '{ex.get('AC')}'")
        # Print the PA03 segments in the TUDF record for this account
        # Find all PA03 segments in the record
        pa_segs = []
        start = 0
        while True:
            # Let's search for "PA03" as segment start. Since we are inside the record block, we can look for "PA03A".
            pa_idx = r.find("PA03A", start)
            if pa_idx == -1:
                break
            # Find next segment in this record
            next_seg = len(r)
            for marker in ["PN03", "ID03", "PT03", "TL04", "ES02"]:
                m_idx = r.find(marker, pa_idx + 5)
                if m_idx != -1 and m_idx < next_seg:
                    next_seg = m_idx
            pa_segs.append(r[pa_idx:next_seg])
            start = pa_idx + 5
            
        for i, pa in enumerate(pa_segs):
            print(f"  TUDF PA segment {i+1}: '{pa}'")
            
        if matched >= 5:
            break
