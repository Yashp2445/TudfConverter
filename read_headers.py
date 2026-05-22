import openpyxl
import sys

def main():
    wb = openpyxl.load_workbook('d:/TudfConverter/docs/CU11880001_30042026__04052026_1131_F2_1.xlsx', data_only=True)
    sheet = wb.active
    
    headers = []
    for cell in sheet[1]:
        headers.append(cell.value)
    
    for i, h in enumerate(headers):
        print(f"{i}: {h}")

if __name__ == '__main__':
    main()
