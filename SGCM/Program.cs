using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using SGCM.Data.Context;
using SGCM.Data;
using SGCM.Application;
using SGCM.Domain.Settings;

namespace SGCM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddIdentityService(builder.Configuration);
            builder.Services.AddDataLayerIoc(builder.Configuration);
            builder.Services.AddApplicationIoc();

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // El proxy de Azure App Service no está en la lista de redes/proxies
                // confiables por defecto; hay que vaciarla para que los headers
                // reenviados (X-Forwarded-Proto, etc.) se respeten.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var app = builder.Build();

            // Debe ir antes que cualquier middleware que dependa del esquema/host
            // real de la petición (p. ej. UseHttpsRedirection), ya que App Service
            // termina TLS en su borde y reenvía la petición como HTTP puro.
            app.UseForwardedHeaders();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.ApplyMigrations();
            app.SeedTestDataAsync().GetAwaiter().GetResult();

            if (!app.Environment.IsDevelopment())
            {
                var frontendSettings = app.Services.GetRequiredService<IOptions<FrontendSettings>>().Value;
                if (string.IsNullOrWhiteSpace(frontendSettings.BaseUrl) || frontendSettings.BaseUrl.Contains("localhost"))
                {
                    app.Logger.LogWarning(
                        "FrontendSettings:BaseUrl no está configurado para este ambiente (valor actual: '{BaseUrl}'). " +
                        "Los enlaces de confirmación de cuenta y restablecimiento de contraseña quedarán rotos. " +
                        "Configúralo en la App Service Configuration como FrontendSettings__BaseUrl.",
                        frontendSettings.BaseUrl);
                }
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
