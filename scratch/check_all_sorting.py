import os

tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

body = content[146:]
if body.endswith("TRLR"):
    body = body[:-4]
records = body.split("ES02**")

accs_with_info = []
for idx, r in enumerate(records):
    if not r.strip():
        continue
    tl_start = r.find("TL04")
    if tl_start != -1:
        t03_idx = r.find("T03", tl_start)
        if t03_idx != -1:
            len_acc = int(r[t03_idx+3:t03_idx+5])
            acc = r[t03_idx+5:t03_idx+5+len_acc]
            accs_with_info.append((acc, idx))

# 1. Print first 10 actual
print("Actual First 10:")
for acc, idx in accs_with_info[:10]:
    print(f"  {acc} (index {idx})")

# 2. Check if sorted
is_sorted = True
for i in range(len(accs_with_info) - 1):
    if accs_with_info[i][0] > accs_with_info[i+1][0]:
        print(f"Out of order at {i}: {accs_with_info[i][0]} > {accs_with_info[i+1][0]}")
        is_sorted = False
        break

if is_sorted:
    print("The entire list of account numbers in the reference TUDF is in strict alphabetical sorted order!")
else:
    print("The list of account numbers is NOT in strict alphabetical sorted order.")
