import os

def compare():
    ref_path = r"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf"
    gen_path = r"d:\TudfConverter\Output\GeneratedFiles\CU11880001_30042026_161746.tudf"
    
    with open(ref_path, 'r', encoding='utf-8', errors='ignore') as f:
        ref_content = f.read()
    with open(gen_path, 'r', encoding='utf-8', errors='ignore') as f:
        gen_content = f.read()

    ref_body = ref_content[146:]
    ref_records = ref_body.split("ES02**")
    
    gen_body = gen_content[146:]
    gen_records = gen_body.split("ES02**")

    # Let's find records in gen that have SAYAJI, and find corresponding records in ref.
    # To do this, let's find the PAN or Account Number from the gen record and search for it in ref!
    # A generated record has a PAN in ID03 segment.
    # Let's loop over all gen records, if it has "SAYAJI", let's extract the PAN and print both gen and ref records.
    match_count = 0
    for idx, r_gen in enumerate(gen_records):
        if "SAYAJI" in r_gen:
            # Find PAN in r_gen: ID03I010102010210(PAN)
            pan_start = r_gen.find("ID03I010102010210")
            if pan_start != -1:
                pan = r_gen[pan_start+17:pan_start+27]
            else:
                pan = "NOTFOUND"
                
            # Search for this PAN in ref records
            r_ref = None
            for r in ref_records:
                if pan in r:
                    r_ref = r
                    break
            
            print(f"\nMatch {match_count+1} (PAN: {pan}):")
            print(f"  GEN Name Segment: {r_gen[:r_gen.find('ID03')]}")
            if r_ref:
                print(f"  REF Name Segment: {r_ref[:r_ref.find('ID03')]}")
            else:
                print(f"  REF: PAN not found in any record!")
            match_count += 1
            if match_count >= 5:
                break

if __name__ == '__main__':
    compare()
