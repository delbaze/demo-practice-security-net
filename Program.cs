using Microsoft.EntityFrameworkCore;
using DemoPracticeSecurityNet.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Middleware qui expose les exceptions
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync($"Error: {ex.Message}\nStack trace: {ex.StackTrace}");
    }
});

app.MapControllers();
app.Run();