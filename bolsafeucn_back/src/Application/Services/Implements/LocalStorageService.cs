using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Application.Services.Interfaces;
using Serilog;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;

namespace bolsafeucn_back.src.Application.Services.Implements
{
    /// <summary>
    /// Servicio de almacenamiento local de documentos.
    /// </summary>
    public class LocalStorageService : IDocumentStorageProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IFileRepository _fileRepository;
        private readonly IUserRepository _userRepository;
        private readonly string _basePath;
        private readonly string _baseUrl;

        public LocalStorageService(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IFileRepository fileRepository,
            IUserRepository userRepository)
        {
            _environment = environment;
            _configuration = configuration;
            _fileRepository = fileRepository;
            _userRepository = userRepository;
            _basePath = _configuration["Storage:LocalPath"]!;
            _baseUrl = _configuration["Storage:BaseUrl"] ?? "/uploads";
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
                Log.Information("Directorio de almacenamiento local creado en: {BasePath}", _basePath);
            }
        }
        public async Task<bool> UploadCVAsync(IFormFile cvFile, GeneralUser user)
        {
            if (user.CV != null)
            {
                Log.Warning("El usuario {UserId} ya tiene un CV asociado. Se sobrescribirá el existente.", user.Id);
                user.CV.IsActive = false;
            }   
            if (cvFile == null || cvFile.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            const long maxSize = 10 * 1024 * 1024; // 10MB
            if (cvFile.Length > maxSize)
            {
                throw new ArgumentException($"File size exceeds maximum allowed ({maxSize / 1024 / 1024}MB)");
            }
            var now = DateTime.UtcNow;
            var folder = $"{now:yyyy/MM/dd}";
            var folderPath = Path.Combine(_basePath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var extension = Path.GetExtension(cvFile.FileName).ToLowerInvariant();
            var uniqueFileName = Guid.NewGuid().ToString("N") + extension;

            var filePath = Path.Combine(folderPath, uniqueFileName);

            // Save file
            await using var stream = new FileStream(filePath, FileMode.Create);
            await cvFile.CopyToAsync(stream);

            var relativeUrl = $"{_baseUrl}/{folder}/{uniqueFileName}";

            var newFile = new Curriculum()
            {
                OriginalFileName = cvFile.FileName,
                Url = relativeUrl,
                PublicId = uniqueFileName,
                FileSizeBytes = cvFile.Length
            };

            var result = await _fileRepository.CreateCVAsync(newFile);
            if (result == false) 
            {
                throw new Exception("Error al guardar el CV en la base de datos.");
            }

            user.CV = newFile;

            var updateResult = await _userRepository.UpdateAsync(user);
            if (!updateResult) 
            {
                Log.Error("Error al asociar el CV al usuario {UserId}", user.Id);
                throw new Exception("Error al asociar el CV al usuario.");
            }     
            return true;
        }
        public async Task<bool> DeleteCVAsync(GeneralUser user)
        {
            if (user.CV == null)
            {
                Log.Warning("El usuario {UserId} no tiene un CV asociado para eliminar.", user.Id);
                return true;
            }

            var cvPath = Path.Combine(_basePath, user.CV.Url.Replace(_baseUrl + "/", ""));
            if (File.Exists(cvPath))
            {
                File.Delete(cvPath);
                Log.Information("CV eliminado del almacenamiento local: {CVPath}", cvPath);
            }
            else
            {
                Log.Warning("El CV no se encontró en la ruta esperada: {CVPath}", cvPath);
            }

            await _fileRepository.DeleteCVAsync(user.CV.PublicId);
            /*var updateResult = await _userRepository.UpdateAsync(user);
            if (!updateResult)
            {
                Log.Error("Error al desasociar el CV del usuario {UserId}", user.Id);
                throw new Exception("Error al desasociar el CV del usuario.");
            }*/
            return true;
        }
        public async Task<Curriculum?> DownloadCVAsync(GeneralUser user)
        {
            if (user.CV == null)
            {
                Log.Warning("El usuario {UserId} no tiene un CV asociado para descargar.", user.Id);
                return null;
            }
            return user.CV;
        }
        public async Task<bool> CVExistsAsync(GeneralUser user)
        {
            // Implementación para verificar si el CV existe en el almacenamiento local
            throw new NotImplementedException();
        }
    }
}