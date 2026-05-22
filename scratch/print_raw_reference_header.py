tudf_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"

with open(tudf_path, 'rb') as f:
    header_bytes = f.read(146)

print("Raw reference header (len 146):")
print(header_bytes)
print("Decoded:")
print(header_bytes.decode('ascii', errors='ignore'))

# Print field by field
print(f"Segment Tag (1-4): '{header_bytes[0:4].decode()}'")
print(f"Version (5-6): '{header_bytes[4:6].decode()}'")
print(f"Reporting Member ID (7-36): '{header_bytes[6:36].decode()}'")
print(f"Reporting Member Short Name (37-52): '{header_bytes[36:52].decode()}'")
print(f"Reporting Cycle (53-54): '{header_bytes[52:54].decode()}'")
print(f"Date Reported (55-62): '{header_bytes[54:62].decode()}'")
print(f"Future Use (63-92): '{header_bytes[62:92].decode()}'")
print(f"Future Use A (93): '{header_bytes[92:93].decode()}'")
print(f"Future Use Zeros (94-98): '{header_bytes[93:98].decode()}'")
print(f"Member Data (99-146): '{header_bytes[98:146].decode()}'")
