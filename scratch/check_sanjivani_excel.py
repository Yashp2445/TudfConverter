import zipfile
import xml.etree.ElementTree as ET
import os

xlsx_path = r"d:\TudfConverter\Docs\CU11880001_30042026__04052026_1131_F2_1.xlsx"
tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

# Find row details in Excel
target_name = "SANJIVANI NAGA SAH PAT MAR CHIT BR KARAD"
pan = None

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
            
            if row_data.get('A', '').strip() == target_name:
                pan = row_data.get('D', '').strip()
                print(f"Excel row {row_num}: Name='{target_name}', PAN='{pan}'")
                break

if pan and os.path.exists(tudf_path):
    with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()
    
    idx = content.find(pan)
    if idx != -1:
        # Find start of record (PN03)
        pn_idx = content.rfind("PN03", 0, idx)
        # Find end of Name Segment (ID03)
        id_idx = content.find("ID03", pn_idx)
        name_seg = content[pn_idx:id_idx]
        print(f"Reference Name Segment for PAN {pan}: '{name_seg}'")
    else:
        print(f"PAN {pan} not found in reference TUDF")
