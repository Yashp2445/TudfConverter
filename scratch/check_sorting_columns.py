import zipfile
import xml.etree.ElementTree as ET

xlsx_path = r"d:\TudfConverter\Docs\CU11880001_30042026__04052026_1131_F2_1.xlsx"
tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

# 1. Read all Excel rows
excel_rows = []
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
            row_data['_row_num'] = row_num
            excel_rows.append(row_data)

# 2. Read TUDF records
with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

body = content[146:]
if body.endswith("TRLR"):
    body = body[:-4]
records = body.split("ES02**")

print("TUDF Index: Acc, ExcelRow, MemberCode, ShortName, DateOpened, Name")
count = 0
for idx, r in enumerate(records):
    if not r.strip():
        continue
    
    # Parse Account
    acc = ""
    tl_start = r.find("TL04")
    member_code = ""
    date_opened = ""
    if tl_start != -1:
        # Account Number (Tag 03)
        t03_idx = r.find("T03", tl_start)
        if t03_idx != -1:
            len_acc = int(r[t03_idx+3:t03_idx+5])
            acc = r[t03_idx+5:t03_idx+5+len_acc]
            
        # Member Code (Tag 01)
        t01_idx = r.find("T01", tl_start)
        if t01_idx != -1:
            len_mc = int(r[t01_idx+3:t01_idx+5])
            member_code = r[t01_idx+5:t01_idx+5+len_mc]
            
        # Date Opened (Tag 05)
        t05_idx = r.find("T05", tl_start)
        if t05_idx != -1:
            date_opened = r[t05_idx+5:t05_idx+13]

    # Parse Name
    name = ""
    pn_start = r.find("PN03N0101")
    if pn_start != -1:
        len_name = int(r[pn_start+9:pn_start+11])
        name = r[pn_start+11:pn_start+11+len_name]
        
    # Find matching excel row (match by Acc AND Name first 5 chars to avoid duplicates)
    match_row = "NOT FOUND"
    excel_name = ""
    excel_mc = ""
    excel_do = ""
    for er in excel_rows:
        if er.get('AJ') == acc and er.get('A', '').startswith(name[:5]):
            match_row = er['_row_num']
            excel_name = er.get('A')
            excel_mc = er.get('AH')
            excel_do = er.get('AK')
            break
            
    print(f"Record {idx}: Acc='{acc}', Row={match_row}, MC='{member_code}'/'{excel_mc}', DO='{date_opened}'/'{excel_do}', Name='{name}'/'{excel_name}'")
    count += 1
    if count >= 30:
        break
