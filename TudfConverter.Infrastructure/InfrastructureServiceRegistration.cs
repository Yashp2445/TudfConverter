using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TudfConverter.Application.Interfaces;
using TudfConverter.Domain.Validation;
using TudfConverter.Domain.Validation.IdValidators;
using TudfConverter.Domain.Validation.Validators;
using TudfConverter.Infrastructure.FileStorage;
using TudfConverter.Infrastructure.Reports;
using TudfConverter.Infrastructure.Tudf;
using TudfConverter.Infrastructure.Tudf.Builders;
using TudfConverter.Infrastructure.Validation;
using TudfConverter.Infrastructure.Excel;

namespace TudfConverter.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Readers and File Services
        services.AddSingleton<IExcelReaderService, ClosedXmlExcelReaderService>();
        services.AddSingleton<IFileExportService, FileExportService>();
        services.AddSingleton<IValidationReportWriter, ValidationReportWriter>();
        services.AddSingleton<FileCleanupService>();
        services.AddSingleton<HistoryRepository>();

        // TUDF Generation
        services.AddSingleton<HeaderSegmentBuilder>();
        services.AddSingleton<NameSegmentBuilder>();
        services.AddSingleton<IdentificationSegmentBuilder>();
        services.AddSingleton<TelephoneSegmentBuilder>();
        services.AddSingleton<EmailSegmentBuilder>();
        services.AddSingleton<AddressSegmentBuilder>();
        services.AddSingleton<AccountSegmentBuilder>();
        services.AddSingleton<AccountHistorySegmentBuilder>();
        services.AddSingleton<TudfRecordBuilder>();
        services.AddSingleton<TudfFileAssembler>();
        services.AddSingleton<ITudfGenerationService, TudfGenerationService>();

        // Validation Orchestrator
        services.AddScoped<IValidationOrchestrator, ValidationOrchestrator>();

        // FluentValidation Validators
        services.AddValidatorsFromAssembly(typeof(NameSegmentValidator).Assembly);
        services.AddSingleton<CrossSegmentValidator>();

        // ID Validators
        services.AddSingleton<IIdNumberValidator, PanValidator>();
        services.AddSingleton<IIdNumberValidator, AadhaarValidator>();
        services.AddSingleton<IIdNumberValidator, PassportValidator>();
        services.AddSingleton<IIdNumberValidator>(new GenericIdValidator(3)); // Voter ID
        services.AddSingleton<IIdNumberValidator>(new GenericIdValidator(4)); // Driving License
        services.AddSingleton<IIdNumberValidator>(new GenericIdValidator(5)); // Ration Card
        services.AddSingleton<IIdNumberValidator>(new GenericIdValidator(9)); // CKYC
        services.AddSingleton<IIdNumberValidator>(new GenericIdValidator(10)); // G RAM G

        return services;
    }
}
