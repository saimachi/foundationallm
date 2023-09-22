using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.ConfigurationOptions;
using FoundationaLLM.Core.Services;

namespace FoundationaLLM.GatekeeperAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplicationInsightsTelemetry();

            builder.Services.AddOptions<GatekeeperServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:GatekeeperService"));
            builder.Services.AddSingleton<IGatekeeperService, GatekeeperService>();

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            //app.UseExceptionHandler(exceptionHandlerApp
            //    => exceptionHandlerApp.Run(async context
            //        => await Results.Problem().ExecuteAsync(context)));

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}