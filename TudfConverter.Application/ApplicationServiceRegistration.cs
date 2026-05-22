using Microsoft.Extensions.DependencyInjection;
using TudfConverter.Application.Interfaces;
using TudfConverter.Application.Mapping;
using TudfConverter.Application.Services;

namespace TudfConverter.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<ExcelToConsumerRecordMapper>();
        services.AddScoped<IFileProcessingService, FileProcessingService>();
        
        return services;
    }
}
