using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using TudfConverter.Domain.Models;
using TudfConverter.Domain.Validation;
using TudfConverter.Domain.Validation.IdValidators;
using TudfConverter.Domain.Validation.Validators;
using TudfConverter.Infrastructure.Validation;
using TudfConverter.Infrastructure.Tudf.Builders;
using TudfConverter.Infrastructure.Tudf;
using Microsoft.Extensions.DependencyInjection;
using TudfConverter.Application;
using TudfConverter.Infrastructure;
using TudfConverter.Application.Pipeline;

namespace TudfConverter.ValidationTests;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("--- TUDF Validator Engine Tests ---");

        var idValidators = new IIdNumberValidator[]
        {
            new PanValidator(),
            new AadhaarValidator(),
            new PassportValidator(),
            new GenericIdValidator(1), // Voter ID
            new GenericIdValidator(4)  // Driving License
        };

        var nameValidator = new NameSegmentValidator();
        var addressValidator = new AddressSegmentValidator();
        var identValidator = new IdentificationSegmentValidator(idValidators);
        var telephoneValidator = new TelephoneSegmentValidator();
        var emailValidator = new EmailSegmentValidator();
        var accountValidator = new AccountSegmentValidator();
        var crossValidator = new CrossSegmentValidator(NullLogger<CrossSegmentValidator>.Instance);

        var orchestrator = new ValidationOrchestrator(
            nameValidator,
            accountValidator,
            addressValidator,
            identValidator,
            telephoneValidator,
            emailValidator,
            crossValidator,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ValidationOrchestrator>.Instance
        );

        var headerDate = new DateOnly(2026, 4, 30);


        // --- VALIDATION TESTS ---

        // Test case 3: DateClosed provided with positive CurrentBalance
        var record3 = new ConsumerRecord
        {
            RowNumber = 3,
            Name = new NameSegmentModel { FullName = "Valid Name", DateOfBirth = new DateOnly(1980, 1, 1) },
            Account = new AccountSegmentModel
            {
                CurrentMemberCode = "MEM01",
                AccountNumber = "ACC01",
                AccountType = 5,
                OwnershipIndicator = 1,
                DateOpenedDisbursed = new DateOnly(2020, 1, 1),
                HighCreditSanctionedAmount = 50000,
                CurrentBalance = 50000,
                IsCurrentBalanceNegative = false,
                DateClosed = new DateOnly(2022, 1, 1),
                DateReportedAndCertified = new DateOnly(2023, 10, 1)
            }
        };
        record3.Addresses.Add(new AddressModel { SegmentIndex = 1, AddressLine1 = "123", PinCode = "123456", StateCode = "99" });
        record3.Identifications.Add(new IdentificationModel { SegmentIndex = 1, IdType = 1, IdNumber = "ABCDE1234F" });
        
        // Test case 4: DPD greater than zero with AmountOverdue zero for personal loan.
        var record4 = new ConsumerRecord
        {
            RowNumber = 4,
            Name = new NameSegmentModel { FullName = "Valid Name", DateOfBirth = new DateOnly(1980, 1, 1) },
            Account = new AccountSegmentModel
            {
                CurrentMemberCode = "MEM01",
                AccountNumber = "ACC01",
                AccountType = 5, // Personal Loan
                OwnershipIndicator = 1,
                DateOpenedDisbursed = new DateOnly(2020, 1, 1),
                HighCreditSanctionedAmount = 50000,
                CurrentBalance = 10000,
                IsCurrentBalanceNegative = false,
                NumberOfDaysPastDue = 45,
                AmountOverdue = null,
                DateReportedAndCertified = new DateOnly(2023, 10, 1)
            }
        };
        record4.Addresses.Add(new AddressModel { SegmentIndex = 1, AddressLine1 = "123", PinCode = "123456", StateCode = "99" });
        record4.Identifications.Add(new IdentificationModel { SegmentIndex = 1, IdType = 1, IdNumber = "ABCDE1234F" });

        // Test case 5: Invalid PAN format.
        var record5 = new ConsumerRecord
        {
            RowNumber = 5,
            Name = new NameSegmentModel { FullName = "Valid Name", DateOfBirth = new DateOnly(1980, 1, 1) },
            Account = new AccountSegmentModel
            {
                CurrentMemberCode = "MEM01",
                AccountNumber = "ACC01",
                AccountType = 5,
                OwnershipIndicator = 1,
                DateOpenedDisbursed = new DateOnly(2020, 1, 1),
                HighCreditSanctionedAmount = 50000,
                CurrentBalance = 10000,
                IsCurrentBalanceNegative = false,
                DateReportedAndCertified = new DateOnly(2023, 10, 1)
            }
        };
        record5.Addresses.Add(new AddressModel { SegmentIndex = 1, AddressLine1 = "123", PinCode = "123456", StateCode = "99" });
        record5.Identifications.Add(new IdentificationModel { SegmentIndex = 1, IdType = 1, IdNumber = "ABCXD1234E" }); // Invalid fourth char

        // Test case 6: PIN prefix does not match state code.
        var record6 = new ConsumerRecord
        {
            RowNumber = 6,
            Name = new NameSegmentModel { FullName = "Valid Name", DateOfBirth = new DateOnly(1980, 1, 1) },
            Account = new AccountSegmentModel
            {
                CurrentMemberCode = "MEM01",
                AccountNumber = "ACC01",
                AccountType = 5,
                OwnershipIndicator = 1,
                DateOpenedDisbursed = new DateOnly(2020, 1, 1),
                HighCreditSanctionedAmount = 50000,
                CurrentBalance = 10000,
                IsCurrentBalanceNegative = false,
                DateReportedAndCertified = new DateOnly(2023, 10, 1)
            }
        };
        record6.Addresses.Add(new AddressModel { SegmentIndex = 1, AddressLine1 = "123", StateCode = "27", PinCode = "110001" });
        record6.Identifications.Add(new IdentificationModel { SegmentIndex = 1, IdType = 1, IdNumber = "ABCDE1234F" });

        // Test case 7: No ID and no telephone for account opened after June 2007.
        var record7 = new ConsumerRecord
        {
            RowNumber = 7,
            Name = new NameSegmentModel { FullName = "Valid Name", DateOfBirth = new DateOnly(1980, 1, 1) },
            Account = new AccountSegmentModel
            {
                CurrentMemberCode = "MEM01",
                AccountNumber = "ACC01",
                AccountType = 5,
                OwnershipIndicator = 1,
                DateOpenedDisbursed = new DateOnly(2010, 7, 15),
                HighCreditSanctionedAmount = 50000,
                CurrentBalance = 10000,
                IsCurrentBalanceNegative = false,
                DateReportedAndCertified = new DateOnly(2023, 10, 1)
            }
        };
        record7.Addresses.Add(new AddressModel { SegmentIndex = 1, AddressLine1 = "123", PinCode = "123456", StateCode = "99" });

        // Test case 8: Single name with no ID.
        var record8 = new ConsumerRecord
        {
            RowNumber = 8,
            Name = new NameSegmentModel { FullName = "RAKESH", DateOfBirth = new DateOnly(1980, 1, 1) },
            Account = new AccountSegmentModel
            {
                CurrentMemberCode = "MEM01",
                AccountNumber = "ACC01",
                AccountType = 5,
                OwnershipIndicator = 1,
                DateOpenedDisbursed = new DateOnly(2005, 1, 1), // Before June 2007 to isolate CROSS-09
                HighCreditSanctionedAmount = 50000,
                CurrentBalance = 10000,
                IsCurrentBalanceNegative = false,
                DateReportedAndCertified = new DateOnly(2023, 10, 1)
            }
        };
        record8.Addresses.Add(new AddressModel { SegmentIndex = 1, AddressLine1 = "123", PinCode = "123456", StateCode = "99" });

        var results = await orchestrator.ValidateAllAsync(new List<ConsumerRecord> { record3, record4, record5, record6, record7, record8 }, headerDate);

        var record3Result = results.First(r => r.RowNumber == 3);
        Console.WriteLine($"Test case 3: {(record3Result.Errors.Any(e => e.ErrorCode == "TL-10C" && e.Outcome == TudfConverter.Domain.Validation.Results.FailureOutcome.RejectRecord) ? "PASS" : "FAIL")}");
        foreach (var err in record3Result.Errors) Console.WriteLine($"  [{err.ErrorCode}] {err.ErrorMessage}");

        var record4Result = results.First(r => r.RowNumber == 4);
        Console.WriteLine($"Test case 4: {(record4Result.Errors.Any(e => e.ErrorCode == "TL-14A") ? "PASS" : "FAIL")}");
        foreach (var err in record4Result.Errors) Console.WriteLine($"  [{err.ErrorCode}] {err.ErrorMessage}");

        var record5Result = results.First(r => r.RowNumber == 5);
        Console.WriteLine($"Test case 5: {(record5Result.Errors.Any(e => e.ErrorCode == "ID-02-FMT") ? "PASS" : "FAIL")}");
        foreach (var err in record5Result.Errors) Console.WriteLine($"  [{err.ErrorCode}] {err.ErrorMessage}");

        var record6Result = results.First(r => r.RowNumber == 6);
        Console.WriteLine($"Test case 6: {(record6Result.Errors.Any(e => e.ErrorCode == "PA-07") ? "PASS" : "FAIL")}");
        foreach (var err in record6Result.Errors) Console.WriteLine($"  [{err.ErrorCode}] {err.ErrorMessage}");

        var record7Result = results.First(r => r.RowNumber == 7);
        Console.WriteLine($"Test case 7: {(record7Result.Errors.Any(e => e.ErrorCode == "CROSS-01") ? "PASS" : "FAIL")}");
        foreach (var err in record7Result.Errors) Console.WriteLine($"  [{err.ErrorCode}] {err.ErrorMessage}");

        var record8Result = results.First(r => r.RowNumber == 8);
        Console.WriteLine($"Test case 8: {(record8Result.Errors.Any(e => e.ErrorCode == "CROSS-09") ? "PASS" : "FAIL")}");
        foreach (var err in record8Result.Errors) Console.WriteLine($"  [{err.ErrorCode}] {err.ErrorMessage}");


        Console.WriteLine("\n--- TUDF Generation Tests ---");

        // Infrastructure for generation
        var nameSegBuilder = new NameSegmentBuilder();
        var idSegBuilder = new IdentificationSegmentBuilder();
        var phoneSegBuilder = new TelephoneSegmentBuilder();
        var emailSegBuilder = new EmailSegmentBuilder();
        var addrSegBuilder = new AddressSegmentBuilder();
        var accSegBuilder = new AccountSegmentBuilder();
        var histSegBuilder = new AccountHistorySegmentBuilder();
        var headerSegBuilder = new HeaderSegmentBuilder();

        var recordBuilder = new TudfRecordBuilder(
            nameSegBuilder, idSegBuilder, phoneSegBuilder, emailSegBuilder,
            addrSegBuilder, accSegBuilder, histSegBuilder);

        var fileAssembler = new TudfFileAssembler(headerSegBuilder, recordBuilder);
        var genService = new TudfGenerationService(fileAssembler, NullLogger<TudfGenerationService>.Instance);

        // Test 1: Header segment byte count
        var testHeader = new HeaderSegmentModel
        {
            MemberUserId = "USER123",
            ShortName = "BANK ABC",
            ReportingCycle = "01",
            DateReportedAndCertified = new DateOnly(2023, 10, 31),
            MemberData = "TEST DATA"
        };
        var testHeaderOutput = headerSegBuilder.Build(testHeader);
        Console.WriteLine($"Test 1: {(testHeaderOutput.Length == 146 ? "PASS" : "FAIL")}");
        if (testHeaderOutput.Length != 146) Console.WriteLine($"  Length was {testHeaderOutput.Length}");

        // Test 2: End of subject segment
        var minRecord = new ConsumerRecord
        {
            RowNumber = 999,
            Name = new NameSegmentModel { FullName = "TEST NAME" },
            Account = new AccountSegmentModel
            {
                CurrentMemberCode = "MEM01",
                AccountNumber = "ACC01",
                AccountType = 5,
                OwnershipIndicator = 1,
                DateOpenedDisbursed = new DateOnly(2020, 1, 1),
                HighCreditSanctionedAmount = 50000,
                CurrentBalance = 10000,
                IsCurrentBalanceNegative = false,
                DateReportedAndCertified = new DateOnly(2023, 10, 1)
            }
        };
        var recordOutput = recordBuilder.BuildRecord(minRecord);
        Console.WriteLine($"Test 2: {(recordOutput.EndsWith("ES02**") ? "PASS" : "FAIL")}");

        // Test 3: File starts with TUDF and ends with TRLR
        var fileOutput = fileAssembler.Assemble(new List<ConsumerRecord> { minRecord }, testHeader);
        Console.WriteLine($"Test 3: {(fileOutput.StartsWith("TUDF") && fileOutput.EndsWith("TRLR") ? "PASS" : "FAIL")}");

        // Test 4: Variable field encoding
        var varField = TudfFieldFormatter.FormatVariableField("01", "HAREN PATEL");
        Console.WriteLine($"Test 4: {(varField == "0111HAREN PATEL" ? "PASS" : "FAIL")}");

        // Test 5: Signed negative balance
        var negBalance = TudfFieldFormatter.FormatSignedAmountField("13", 50000, true);
        Console.WriteLine($"Test 5: {(negBalance.EndsWith("-") ? "PASS" : "FAIL")}");

        // Test 6: No newlines in output
        Console.WriteLine($"Test 6: {(!fileOutput.Contains("\n") && !fileOutput.Contains("\r") ? "PASS" : "FAIL")}");

        Console.WriteLine("\n--- Pipeline Integration Test ---");

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddLogging();
        services.AddInfrastructureServices();
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Integration Test 1
        var pipelineService = serviceProvider.GetService<TudfConverter.Application.Interfaces.IFileProcessingService>();
        Console.WriteLine($"Integration Test 1 (DI Resolves Service): {(pipelineService != null ? "PASS" : "FAIL")}");

        // Integration Test 2
        var minValidRecord = new ConsumerRecord
        {
            RowNumber = 1,
            Name = new NameSegmentModel { FullName = "VALID NAME", DateOfBirth = new DateOnly(1980, 1, 1) },
            Account = new AccountSegmentModel
            {
                CurrentMemberCode = "MEM01",
                AccountNumber = "ACC01",
                AccountType = 5,
                OwnershipIndicator = 1,
                DateOpenedDisbursed = new DateOnly(2020, 1, 1),
                HighCreditSanctionedAmount = 50000,
                CurrentBalance = 10000,
                IsCurrentBalanceNegative = false,
                DateReportedAndCertified = new DateOnly(2023, 10, 1)
            }
        };
        minValidRecord.Addresses.Add(new AddressModel { SegmentIndex = 1, AddressLine1 = "123 ST", PinCode = "123456", StateCode = "99" });
        minValidRecord.Identifications.Add(new IdentificationModel { SegmentIndex = 1, IdType = 1, IdNumber = "ABCDE1234F" });

        var intAssembler = new TudfFileAssembler(new HeaderSegmentBuilder(), new TudfRecordBuilder(
            new NameSegmentBuilder(), new IdentificationSegmentBuilder(), new TelephoneSegmentBuilder(),
            new EmailSegmentBuilder(), new AddressSegmentBuilder(), new AccountSegmentBuilder(),
            new AccountHistorySegmentBuilder()));
        
        var intHeader = new HeaderSegmentModel { MemberUserId = "MEM", ShortName = "NAME", ReportingCycle = "W1", DateReportedAndCertified = new DateOnly(2023,10,1), MemberData = "" };
        var intFileOutput = intAssembler.Assemble(new List<ConsumerRecord> { minValidRecord }, intHeader);
        
        bool startsTu = intFileOutput.StartsWith("TUDF");
        bool endsTrlr = intFileOutput.EndsWith("TRLR");
        bool hasEs02 = intFileOutput.Contains("ES02**");
        bool hasNoLines = !intFileOutput.Contains("\n") && !intFileOutput.Contains("\r");
        
        Console.WriteLine($"Integration Test 2 (Valid Assembly): {(startsTu && endsTrlr && hasEs02 && hasNoLines ? "PASS" : "FAIL")}");
        if (!startsTu) Console.WriteLine("  Failed: Starts with TUDF");
        if (!endsTrlr) Console.WriteLine("  Failed: Ends with TRLR");
        if (!hasEs02) Console.WriteLine("  Failed: Contains ES02**");
        if (!hasNoLines) Console.WriteLine("  Failed: Has no newlines");

        // Integration Test 3
        if (pipelineService != null)
        {
            var options = new ProcessingOptions 
            { 
                OutputFolder = "Output", 
                MemberUserId = "MEM1",
                MemberShortName = "NAME",
                ReportFolder = "Output",
                ReportingCycle = "W1",
                DateReportedAndCertified = new DateOnly(2023, 10, 1)
            };
            var processResult = await pipelineService.ProcessFileAsync("NonExistentFile.xlsx", options);
            bool isFailed = !processResult.IsSuccess;
            bool hasMsg = processResult.ErrorMessage?.Contains("not found") == true;
            Console.WriteLine($"Integration Test 3 (Missing File Graceful Fail): {(isFailed && hasMsg ? "PASS" : "FAIL")}");
        }

        Console.WriteLine("\n--- Real Data Test ---");

        var excelFile = @"d:\TudfConverter\Docs\CU11880001_30042026__04052026_1131_F2_1.xlsx";
        var expectedTudf = @"d:\TudfConverter\Docs\ODUU_CU11880001_30042026__04052026_1131_F2_1-04-May-2026.tudf";

        var readerService = serviceProvider.GetService<TudfConverter.Application.Interfaces.IExcelReaderService>();
        
        // Test 1
        var readResult = await readerService!.ReadAsync(excelFile, CancellationToken.None);
        Console.WriteLine($"Test 1 - Read Excel file: {(readResult.IsSuccess && readResult.Rows.Count > 0 ? "PASS" : "FAIL")}");
        if (!readResult.IsSuccess) Console.WriteLine("  Error: " + string.Join(", ", readResult.Errors));
        if (readResult.Rows.Count > 0)
        {
            Console.WriteLine("First row mapping preview:");
            foreach (var kvp in readResult.Rows.First().Columns.Take(5))
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
            }
        }

        // Test 2
        var mapper = new TudfConverter.Application.Mapping.ExcelToConsumerRecordMapper(NullLogger<TudfConverter.Application.Mapping.ExcelToConsumerRecordMapper>.Instance, idValidators);
        var mappedRecords = new List<ConsumerRecord>();
        bool mapError = false;
        try
        {
            foreach (var row in readResult.Rows)
            {
                mappedRecords.Add(mapper.Map(row));
            }
            Console.WriteLine($"Test 2 - Map to ConsumerRecord: PASS ({mappedRecords.Count} records mapped)");
            var first = mappedRecords.FirstOrDefault();
            if (first != null)
            {
                Console.WriteLine($"First Record: Name={first.Name?.FullName}, AccType={first.Account?.AccountType}, DateOpened={first.Account?.DateOpenedDisbursed}");
            }
        }
        catch (Exception ex)
        {
            mapError = true;
            Console.WriteLine($"Test 2 - Map to ConsumerRecord: FAIL. {ex.Message}");
        }

        // Test 3
        foreach (var r in mappedRecords)
        {
            if (r.Account != null) r.Account.DateReportedAndCertified = headerDate;
        }

        var valOrch = serviceProvider.GetService<TudfConverter.Application.Interfaces.IValidationOrchestrator>();
        var valResults = await valOrch!.ValidateAllAsync(mappedRecords, headerDate);
        int accepted = valResults.Count(r => !r.IsRecordRejected);
        int rejected = valResults.Count(r => r.IsRecordRejected);
        Console.WriteLine($"Test 3 - Validate all records: {(accepted > 0 ? "PASS" : "FAIL")} (Accepted: {accepted}, Rejected: {rejected})");
        if (rejected > 0)
        {
            foreach (var r in valResults.Where(x => x.IsRecordRejected).Take(5))
            {
                Console.WriteLine($"  Row {r.RowNumber} Rejected: {r.Errors.First().ErrorMessage}");
            }
        }

        // Test 4
        var tudfGenService = serviceProvider.GetService<TudfConverter.Application.Interfaces.ITudfGenerationService>();
        var realHeader = new HeaderSegmentModel
        {
            MemberUserId = "CU11880001_DATASUBMISSION",
            ShortName = "RAYAT",
            ReportingCycle = "CU",
            DateReportedAndCertified = new DateOnly(2026, 4, 30)
        };
        
        var generatedTudf = tudfGenService!.Generate(mappedRecords.Where(r => !valResults.First(v => v.RowNumber == r.RowNumber).IsRecordRejected).ToList(), realHeader);
        var tempFile = Path.Combine(Path.GetTempPath(), "test_output.tudf");
        File.WriteAllText(tempFile, generatedTudf);
        
        var expectedContent = File.ReadAllText(expectedTudf);
        var actHeader = generatedTudf.Length >= 146 ? generatedTudf.Substring(0, 146) : "";
        var expHeader = expectedContent.Length >= 146 ? expectedContent.Substring(0, 146) : "";
        bool headerMatch = actHeader == expHeader;
        bool trailerMatch = generatedTudf.EndsWith("TRLR");
        Console.WriteLine($"Test 4 - Generate TUDF: {(headerMatch && trailerMatch ? "PASS" : "FAIL")} (Header Match: {headerMatch}, Trailer Match: {trailerMatch})");

        // Test 5
        var expectedEs02Count = expectedContent.Split(new[] { "ES02**" }, StringSplitOptions.None).Length - 1;
        var actualEs02Count = generatedTudf.Split(new[] { "ES02**" }, StringSplitOptions.None).Length - 1;
        var expMemberId = expHeader.Substring(4, 30).Trim();
        var actMemberId = actHeader.Length >= 34 ? actHeader.Substring(4, 30).Trim() : "";
        bool countsMatch = expectedEs02Count == actualEs02Count;
        bool membersMatch = expMemberId == actMemberId;
        Console.WriteLine($"Test 5 - Compare output structure: {(countsMatch && membersMatch ? "PASS" : "FAIL")} (ES02 Count: {actualEs02Count} vs Expected: {expectedEs02Count}, Member Match: {membersMatch})");

        // Test 6 - Full End-to-End Pipeline Service execution (matching WPF flow)
        Console.WriteLine("\n--- Full End-to-End Service Test ---");
        Console.WriteLine("Parsed Header Data keys and values:");
        foreach (var kvp in readResult.HeaderData)
        {
            Console.WriteLine($"  [{kvp.Key}] = '{kvp.Value}'");
        }
        var runOptions = new ProcessingOptions 
        { 
            OutputFolder = "Output/GeneratedFiles", 
            MemberUserId = "BNK1234567",
            MemberShortName = "BANK ABC",
            ReportFolder = "Output/ValidationReports",
            ReportingCycle = "W1",
            DateReportedAndCertified = default
        };
        var e2eResult = await pipelineService!.ProcessFileAsync(excelFile, runOptions);
        
        bool e2eSuccess = e2eResult.IsSuccess;
        bool e2eCountMatch = e2eResult.AcceptedRows == 28882 && e2eResult.RejectedRows == 0;
        bool e2eFileGenerated = !string.IsNullOrEmpty(e2eResult.GeneratedFilePath) && File.Exists(e2eResult.GeneratedFilePath);
        bool e2eReportGenerated = !string.IsNullOrEmpty(e2eResult.ReportFilePath) && File.Exists(e2eResult.ReportFilePath);
        
        Console.WriteLine($"Test 6 - Pipeline run success: {(e2eSuccess ? "PASS" : "FAIL")}");
        Console.WriteLine($"Test 7 - Pipeline count check: {(e2eCountMatch ? "PASS" : "FAIL")} (Accepted: {e2eResult.AcceptedRows}, Rejected: {e2eResult.RejectedRows})");
        Console.WriteLine($"Test 8 - Pipeline output path resolved & exists: {(e2eFileGenerated ? "PASS" : "FAIL")} (Path: {e2eResult.GeneratedFilePath})");
        Console.WriteLine($"Test 9 - Pipeline report path resolved & exists: {(e2eReportGenerated ? "PASS" : "FAIL")} (Path: {e2eResult.ReportFilePath})");
        
        if (e2eFileGenerated)
        {
            var e2eFileContent = File.ReadAllText(e2eResult.GeneratedFilePath!);
            var rawMemberId = e2eFileContent.Substring(6, 30);
            var trimmedMemberId = rawMemberId.Trim();
            var hasRealMemberId = trimmedMemberId == "CU11880001_DATASUBMISSION";
            var fileName = Path.GetFileName(e2eResult.GeneratedFilePath!);
            var startsWithRealId = fileName.StartsWith("CU11880001");
            Console.WriteLine($"DEBUG TEST 10: rawMemberId='{rawMemberId}', trimmedMemberId='{trimmedMemberId}', hasRealMemberId={hasRealMemberId}, fileName='{fileName}', startsWithRealId={startsWithRealId}");
            Console.WriteLine($"Test 10 - Filename and content use parsed Member ID: {((hasRealMemberId && startsWithRealId) ? "PASS" : "FAIL")}");
        }
    }
}
