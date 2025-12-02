using CardDemo.Application.Features.BatchJobs.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CardDemo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Add AutoMapper
        services.AddAutoMapper(assembly);

        // Add Batch Job Services (COBOL batch program equivalents)
        services.AddScoped<TransactionPostingService>();      // CBTRN01C/CBTRN02C
        services.AddScoped<InterestCalculationService>();     // CBACT02C
        services.AddScoped<StatementGenerationService>();     // CBSTM03A/CBSTM03B
        services.AddScoped<DataExportImportService>();        // CBEXPORT/CBIMPORT

        return services;
    }
}
