using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using TudfConverter.Application.Interfaces;
using TudfConverter.Application.Mapping;
using TudfConverter.Application.Pipeline;

namespace TudfConverter.Infrastructure.Excel
{
    public class ClosedXmlExcelReaderService : IExcelReaderService
    {
        private readonly ILogger<ClosedXmlExcelReaderService> _logger;

        public ClosedXmlExcelReaderService(ILogger<ClosedXmlExcelReaderService> logger)
        {
            _logger = logger;
        }

        public Task<ExcelReadResult> ReadAsync(string filePath, CancellationToken ct = default)
        {
            _logger.LogInformation("Starting to read Excel file: {FilePath}", filePath);
            var result = new ExcelReadResult();

            try
            {
                using var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                using var workbook = new XLWorkbook(fileStream);
                var worksheet = workbook.Worksheet(1);
                IXLRow? headerRow = null;
                var knownHeaderKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Reporting Member ID", "Member User ID", "Member ID", "MemberUserId",
                    "Short Name", "Member Short Name", "ShortName",
                    "Cycle Identification", "Reporting Cycle", "Cycle",
                    "Date Reported", "Date Reported and Certified", "DateReported"
                };
                int skipNextRowsCount = 0;
                
                // Scan the first 50 rows to find the header row containing "Consumer Name" or "Current/New Member Code"
                for (int r = 1; r <= 50; r++)
                {
                    if (skipNextRowsCount > 0)
                    {
                        skipNextRowsCount--;
                        continue;
                    }

                    var row = worksheet.Row(r);
                    bool found = false;
                    foreach (var cell in row.CellsUsed())
                    {
                        var text = cell.Value.ToString().Trim();
                        if (text.Equals(ExcelColumnMap.ConsumerName, StringComparison.OrdinalIgnoreCase) ||
                            text.Equals(ExcelColumnMap.CurrentNewMemberCode, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        headerRow = row;
                        break;
                    }

                    // Look for cells in this row that match known header keys
                    var matchedCells = new List<(IXLCell cell, string matchedKey)>();
                    foreach (var cell in row.CellsUsed())
                    {
                        var text = cell.Value.ToString().Trim();
                        if (knownHeaderKeys.Contains(text))
                        {
                            matchedCells.Add((cell, text));
                        }
                    }

                    if (matchedCells.Count >= 2)
                    {
                        // Treat as Horizontal Header Row: headers are in row `r`, values in row `r + 1`
                        var nextRow = worksheet.Row(r + 1);
                        foreach (var (headerCell, key) in matchedCells)
                        {
                            int colNum = headerCell.Address.ColumnNumber;
                            var valCell = nextRow.Cell(colNum);
                            var val = valCell.Value.ToString().Trim();
                            if (valCell.DataType == XLDataType.DateTime && valCell.Value.IsDateTime)
                            {
                                val = valCell.Value.GetDateTime().ToString("ddMMyyyy");
                            }
                            if (!string.IsNullOrEmpty(val))
                            {
                                result.HeaderData[key] = val;
                            }
                        }
                        skipNextRowsCount = 1; // skip row r+1 since we consumed it as values
                    }
                    else if (matchedCells.Count == 1)
                    {
                        // Treat as Vertical Key-Value: header cell contains key, next cell (same row) contains value
                        var (keyCell, key) = matchedCells[0];
                        int colNum = keyCell.Address.ColumnNumber;
                        var valCell = row.Cell(colNum + 1);
                        var val = valCell.Value.ToString().Trim();
                        if (valCell.DataType == XLDataType.DateTime && valCell.Value.IsDateTime)
                        {
                            val = valCell.Value.GetDateTime().ToString("ddMMyyyy");
                        }
                        if (!string.IsNullOrEmpty(val))
                        {
                            result.HeaderData[key] = val;
                        }
                    }
                    else
                    {
                        // Fallback: If no known keys matched, keep the old generic 2-column key-value pair extraction
                        var cellsUsed = row.CellsUsed().ToList();
                        if (cellsUsed.Count >= 2)
                        {
                            var key = cellsUsed[0].Value.ToString().Trim();
                            var val = cellsUsed[1].Value.ToString().Trim();
                            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val))
                            {
                                if (cellsUsed[1].DataType == XLDataType.DateTime && cellsUsed[1].Value.IsDateTime)
                                {
                                    val = cellsUsed[1].Value.GetDateTime().ToString("ddMMyyyy");
                                }
                                result.HeaderData[key] = val;
                            }
                        }
                    }
                }

                if (headerRow == null)
                {
                    var error = "Could not locate the header row in the template. Expected to find 'Consumer Name' or 'Current/New Member Code'.";
                    _logger.LogError(error);
                    result.Errors.Add(error);
                    return Task.FromResult(result);
                }

                var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var cell in headerRow.CellsUsed())
                {
                    var header = cell.Value.ToString().Trim();
                    if (!string.IsNullOrEmpty(header))
                    {
                        columnMap[header] = cell.Address.ColumnNumber;
                    }
                }

                var expectedColumns = ExcelColumnMap.GetAllExpectedColumns();
                var missingColumns = expectedColumns.Where(c => !columnMap.ContainsKey(c)).ToList();

                if (missingColumns.Any())
                {
                    var error = $"Missing required columns in template: {string.Join(", ", missingColumns)}";
                    _logger.LogError(error);
                    result.Errors.Add(error);
                    return Task.FromResult(result);
                }

                var rowsUsed = worksheet.RowsUsed().Where(r => r.RowNumber() > headerRow.RowNumber());
                int rowCount = 0;

                foreach (var row in rowsUsed)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    bool isEmpty = true;
                    foreach (var cell in row.CellsUsed())
                    {
                        if (!string.IsNullOrWhiteSpace(cell.Value.ToString()))
                        {
                            isEmpty = false;
                            break;
                        }
                    }

                    if (isEmpty)
                    {
                        continue;
                    }

                    var rawRow = new RawExcelRow
                    {
                        RowNumber = row.RowNumber()
                    };

                    foreach (var kvp in columnMap)
                    {
                        var cell = row.Cell(kvp.Value);
                        string cellValue = string.Empty;

                        if (cell.DataType == XLDataType.DateTime && cell.Value.IsDateTime)
                        {
                            cellValue = cell.Value.GetDateTime().ToString("dd/MM/yyyy");
                        }
                        else if (cell.DataType == XLDataType.Number && cell.Value.IsNumber)
                        {
                            // Convert to string without scientific notation
                            cellValue = cell.Value.GetNumber().ToString("0.################");
                        }
                        else
                        {
                            cellValue = cell.Value.ToString().Trim();
                        }

                        rawRow.Columns[kvp.Key] = cellValue;
                    }

                    result.Rows.Add(rawRow);
                    rowCount++;
                }

                _logger.LogInformation("Successfully read {RowCount} data rows from {FilePath}", rowCount, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Excel file: {FilePath}", filePath);
                result.Errors.Add($"Error reading Excel file: {ex.Message}");
            }

            return Task.FromResult(result);
        }
    }
}
