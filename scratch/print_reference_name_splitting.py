import zipfile
import xml.etree.ElementTree as ET

xlsx_path = r"d:\TudfConverter\Docs\CU11880001_30042026__04052026_1131_F2_1.xlsx"
tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

body = content[146:]
if body.endswith("TRLR"):
    body = body[:-4]
records = body.split("ES02**")

def parse_name_tags(r):
    pn_start = r.find("PN03N01")
    if pn_start == -1:
        return []
    
    pos = pn_start + 7
    tags = []
    # Read tags
    while pos < len(r):
        tag_id = r[pos:pos+2]
        if tag_id not in ["01", "02", "03", "04", "05", "07", "08"]:
            break
        tag_len = int(r[pos+2:pos+4])
        tag_val = r[pos+4:pos+4+tag_len]
        if tag_id in ["01", "02", "03", "04", "05"]:
            tags.append((tag_id, tag_val))
        pos = pos + 4 + tag_len
    return tags

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
    
    count = 0
    # Let's map Excel Name -> Excel Row
    excel_rows = []
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

# Print a few records that have long names
long_names_count = 0
for idx, r in enumerate(records):
    if not r.strip():
        continue
    tags = parse_name_tags(r)
    # Find original name by searching the excel rows
    # Because they might be sorted, let's find the matching record by account number
    tl_start = r.find("TL04")
    acc = ""
    if tl_start != -1:
        t03_idx = r.find("T03", tl_start)
        if t03_idx != -1:
            len_acc = int(r[t03_idx+3:t03_idx+5])
            acc = r[t03_idx+5:t03_idx+5+len_acc]
            
    # Find Excel name
    excel_name = ""
    for er in excel_rows:
        if er.get('AJ') == acc:
            excel_name = er.get('A', '')
            break
            
    if len(excel_name) > 25:
        print(f"Record {idx}: Acc='{acc}', Excel Name='{excel_name}'")
        for tid, tval in tags:
            print(f"  Tag {tid}: '{tval}' (len {len(tval)})")
        long_names_count += 1
        if long_names_count >= 15:
            break
