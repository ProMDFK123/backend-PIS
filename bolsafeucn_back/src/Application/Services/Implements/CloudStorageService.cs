using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Application.Services.Interfaces;

namespace bolsafeucn_back.src.Application.Services.Implements
{
    /// <summary>
    /// Servicio de almacenamiento local de documentos.
    /// </summary>
    public class CloudStorageService : IDocumentStorageProvider
    {
        public async Task<bool> UploadCVAsync(IFormFile cvFile, GeneralUser generalUser)
        {
            // Implementación para subir el CV al almacenamiento local
            throw new NotImplementedException();
        }
        public async Task<bool> DeleteCVAsync(GeneralUser generalUser)
        {
            // Implementación para eliminar el CV del almacenamiento local
            throw new NotImplementedException();
        }
        public async Task<Curriculum?> DownloadCVAsync(GeneralUser generalUser)
        {
            // Implementación para descargar el CV del almacenamiento local
            throw new NotImplementedException();
        }
        public async Task<bool> CVExistsAsync(GeneralUser generalUser)
        {
            // Implementación para verificar si el CV existe en el almacenamiento local
            throw new NotImplementedException();
        }
    }
}