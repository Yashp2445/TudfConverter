import os

def check_sanjivani():
    filepath = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"
    if not os.path.exists(filepath):
        return

    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    # Find SANJIVANI
    idx = content.find("SANJIVANI")
    if idx != -1:
        around = content[idx-20:idx+60]
        print(f"Found SANJIVANI in reference TUDF: '{around}'")
    else:
        print("SANJIVANI not found in reference TUDF")

if __name__ == '__main__':
    check_sanjivani()
