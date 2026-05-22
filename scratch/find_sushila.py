import zipfile
import xml.etree.ElementTree as ET

xlsx_path = r"d:\TudfConverter\Docs\CU11880001_30042026__04052026_1131_F2_1.xlsx"

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
            for cell in row.findall('ns:c', ns):
                r_ref = cell.attrib['r']
                if r_ref.startswith('A'): # Column A is Name
                    v = cell.find('ns:v', ns)
                    val = ""
                    if v is not None:
                        t = cell.attrib.get('t', '')
                        if t == 's':
                            val = shared_strings[int(v.text)]
                        else:
                            val = v.text
                    val = (val or '').strip()
                    if 'SUSHILA' in val:
                        row_cells = {}
                        for c in row.findall('ns:c', ns):
                            c_ref = c.attrib['r']
                            col_letter = ''.join(ch for ch in c_ref if ch.isalpha())
                            cv = c.find('ns:v', ns)
                            cval = ""
                            if cv is not None:
                                ct = c.attrib.get('t', '')
                                if ct == 's':
                                    cval = shared_strings[int(cv.text)]
                                else:
                                    cval = cv.text
                            row_cells[col_letter] = cval
                        print(f"Row {row_num}: Name='{val}' AccNo='{row_cells.get('AJ')}'")
