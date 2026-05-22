import os

def analyze():
    ref_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"
    gen_path = r"d:\TudfConverter\Output\GeneratedFiles\CU11880001_30042026_161746.tudf"
    
    for label, filepath in [("REFERENCE", ref_path), ("GENERATED", gen_path)]:
        if not os.path.exists(filepath):
            print(f"File not found: {filepath}")
            continue
            
        with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()
            
        print(f"\n--- {label} ({os.path.basename(filepath)}) ---")
        print(f"Length: {len(content)} characters")
        print(f"PN03 count: {content.count('PN03')}")
        print(f"  PN03N01 (real name): {content.count('PN03N01')}")
        print(f"ID03 count: {content.count('ID03')}")
        print(f"  ID03I01 (ID 1): {content.count('ID03I01')}")
        print(f"  ID03I02 (ID 2): {content.count('ID03I02')}")
        print(f"  ID03I03 (ID 3): {content.count('ID03I03')}")
        print(f"  ID03I04 (ID 4): {content.count('ID03I04')}")
        print(f"PT03 count: {content.count('PT03')}")
        print(f"  PT03T01 (Phone 1): {content.count('PT03T01')}")
        print(f"PA03 count: {content.count('PA03')}")
        print(f"  PA03A01 (Addr 1): {content.count('PA03A01')}")
        print(f"  PA03A02 (Addr 2): {content.count('PA03A02')}")
        print(f"TL04 count: {content.count('TL04')}")
        print(f"ES02 count: {content.count('ES02')}")
        print(f"  ES02** (Real ES): {content.count('ES02**')}")

if __name__ == '__main__':
    analyze()
