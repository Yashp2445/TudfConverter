import zipfile
import xml.etree.ElementTree as ET

xlsx_path = r"d:\TudfConverter\Docs\CU11880001_30042026__04052026_1131_F2_1.xlsx"

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
        print(f"Error reading shared strings: {e}")

    sheet_xml = zip_ref.read('xl/worksheets/sheet1.xml')
    root = ET.fromstring(sheet_xml)
    ns = {'ns': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
    
    headers = {}
    
    for row in root.findall('.//ns:row', ns):
        row_num = int(row.attrib['r'])
        if row_num == 10:
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
                headers[col_letter] = val
        elif 11 <= row_num <= 20:
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
            
            # Print row name and specific columns
            name = row_data.get('A', '')
            pan = row_data.get('D', '')
            passport = row_data.get('E', '')
            voter = row_data.get('H', '')
            dl = row_data.get('I', '')
            ration = row_data.get('L', '')
            uid = row_data.get('M', '')
            add1 = row_data.get('N', '')
            add2 = row_data.get('O', '')
            ckyc = row_data.get('BS', '')
            nrega = row_data.get('BT', '')
            print(f"Row {row_num}: Name='{name}'")
            print(f"  PAN (Col D)='{pan}', UID (Col M)='{uid}'")
            print(f"  Add1 (Col N)='{add1}', Add2 (Col O)='{add2}'")
            print(f"  CKYC (Col BS)='{ckyc}', NREGA (Col BT)='{nrega}'")
            
if __name__ == '__main__':
    pass
