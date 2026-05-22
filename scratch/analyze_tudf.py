import os

def analyze():
    filepath = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"
    if not os.path.exists(filepath):
        print(f"File not found: {filepath}")
        return

    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    print(f"Total length of file: {len(content)} characters")
    
    # Print the first 500 characters
    print("\n--- FIRST 500 CHARS ---")
    print(content[:500])
    
    # Print the last 500 characters
    print("\n--- LAST 500 CHARS ---")
    print(content[-500:])

    # Segment tags typically start with two letters and two digits or similar (like PN03, ID03, PA03, PT03, TL04, ES02, TRLR, TUDF)
    # Let's count some tags we expect.
    tags = ["TUDF", "PN03", "ID03", "PT03", "PA03", "TL04", "ES02", "TRLR"]
    print("\n--- TAG COUNTS IN REF TUDF ---")
    for tag in tags:
        count = content.count(tag)
        print(f"{tag}: {count}")

if __name__ == '__main__':
    analyze()
