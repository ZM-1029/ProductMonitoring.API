using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OfficeOpenXml;
using ProductMonitoring.API.Models;
using ProductMonitoring.API.Repository;
using ProductMonitoring.API.SignalRsetup;
using System.Text.Json;


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

ExcelPackage.License.SetNonCommercialPersonal("Narendra"); 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ProductMonitoringDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine, LogLevel.Error);

});

builder.Services.AddSignalR();

/*builder.Services.AddCors(options =>
{
options.AddPolicy("AllowSpecificOrigin",
    builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
               //.AllowCredentials();
    }); 
});*/
//http://localhost:4200/
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCors", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                 "http://150.241.246.64:525",
                 "https://dev.snapsend.co:525"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // ✅ Now allowed because origins are specific
    });
});

builder.Services.AddScoped<IMasterRepo,MasterRepo>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapHub<SolutionNotificationHub>("/hubs/solution-notifications");

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var ex = error.Error;
            var result = JsonSerializer.Serialize(new
            {
                Status = false,
                Message = ex.Message,
                Detail = ex.InnerException?.Message,
                StackTrace = app.Environment.IsDevelopment() ? ex.StackTrace : null
            });
            await context.Response.WriteAsync(result);
        }
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("SignalRCors");
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "ErrorManual")),
    RequestPath = "/errorManual"
});

app.MapControllers();

app.Run();
