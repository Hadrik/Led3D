using Led3D_2.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NLog;

namespace Led3D_2;

class Program
{
    private static void Main(string[] args)
    {
        LogManager.Setup().LoadConfiguration(builder =>
        {
            builder.ForLogger().FilterMinLevel(LogLevel.Trace).WriteToConsole();
        });

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton(Core.Core.Instance);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo{Title = "Led3D API", Version = "v1"});
        });
        
        var app = builder.Build();
        
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        
        app.Run();
    }
}