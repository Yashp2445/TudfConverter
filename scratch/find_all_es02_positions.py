tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

with open(tudf_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Let's find all indices of 'ES02'
pos = 0
matches = []
while True:
    idx = content.find("ES02", pos)
    if idx == -1:
        break
    matches.append(idx)
    pos = idx + 4

print(f"Total 'ES02' occurrences: {len(matches)}")

# Print the context (first 40 and last 40 chars around it) for all, but if too many, print the ones that are NOT followed by '**'
not_standard = []
for idx in matches:
    context = content[max(0, idx-20):idx+30]
    # Standard format is ES02**
    if not content[idx:idx+6] == "ES02**":
        not_standard.append((idx, context))

print(f"Occurrences NOT matching 'ES02**': {len(not_standard)}")
for idx, ctx in not_standard:
    print(f"  Position {idx}: '{ctx}'")

# Let's also print the last 5 standard ES02 segments
print("\nLast 5 occurrences in the file:")
for idx in matches[-5:]:
    context = content[idx:idx+40]
    print(f"  Position {idx}: '{context}'")
