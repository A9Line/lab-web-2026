using System.Text.Json.Serialization;
using backend.Data;
using backend.Infrastructure;
using backend.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// строка подключения хранится в user-secrets или переменной окружения, не в Git
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Не задана строка подключения ConnectionStrings:Default. Как её задать — в README, раздел ЛР10.");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services
    .AddControllers(options =>
    {
        // в ошибках 400 имена полей как в JSON: title, laboratoryId
        options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<UnexpectedExceptionHandler>();
builder.Services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
builder.Services.AddScoped<IWasteRequestService, WasteRequestService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Text("ok"))
    .WithName("Health")
    .WithTags("Состояние приложения");
app.MapControllers();

app.Run();
