using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SddDemo.Functions.Models;
using SddDemo.Functions.Services;
using SddDemo.Functions.Validation;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ILegalEntityMap, StaticLegalEntityMap>();
builder.Services.AddSingleton<ISupplierRegistry, StaticSupplierRegistry>();

builder.Services
    .AddOptions<MaxOrderValueOptions>()
    .Bind(builder.Configuration.GetSection(MaxOrderValueOptions.SectionName))
    .Validate(o => o.Threshold > 0, "MaxOrderValue:Threshold must be greater than zero.")
    .ValidateOnStart();

builder.Services.AddSingleton<IValidator<PurchaseOrder>, PurchaseOrderValidator>();

// Demo adapters — swap these two lines for Service Bus + Azure SQL in production.
builder.Services.AddSingleton<InMemoryMessagePublisher>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<InMemoryMessagePublisher>());
builder.Services.AddSingleton<ISubmissionStatusStore, InMemorySubmissionStatusStore>();

builder.Services.AddSingleton<ISubmissionService, SubmissionService>();

builder.Build().Run();
