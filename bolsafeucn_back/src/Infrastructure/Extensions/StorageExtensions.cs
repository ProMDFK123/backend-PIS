using bolsafeucn_back.src.Application.Services.Implements;
using bolsafeucn_back.src.Application.Services.Interfaces;

namespace bolsafeucn_back.src.Infrastructure.Extensions
{
    public static class StorageExtensions
    {
        public static IServiceCollection AddDocumentStorageProvider(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var providerRaw = configuration["Storage:Provider"];
            if (string.IsNullOrWhiteSpace(providerRaw)) throw new InvalidOperationException("Storage provider no esta configurado en appsettings.json.");
            var provider = providerRaw.ToLowerInvariant();
            _ = provider switch
            {
                "local" => services.AddScoped<IDocumentStorageProvider, LocalStorageService>(),
                //"proveedor" => services.AddScoped<IDocumentStorageProvider, {Proveedor}StorageService>(),
                _ => throw new InvalidOperationException(
                    $"Storage:Provider '{providerRaw}' no es válido. Use 'Local'.") //Si agrega un nuevo proveedor agreguelo al error en esta linea.
            };
            return services;
        }
    }
}