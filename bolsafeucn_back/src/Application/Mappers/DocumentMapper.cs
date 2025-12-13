using Mapster;
using bolsafeucn_back.src.Application.DTOs.UserDTOs;
using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.Mappers
{
    public class DocumentMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureCVMappings();
        }
        public void ConfigureCVMappings()
        {
            TypeAdapterConfig<UploadCVDTO, Curriculum>
                .NewConfig()
                .Map(dest => dest.OriginalFileName, src => src.CVFile.FileName)
                .Map(dest => dest.FileSizeBytes, src => src.CVFile.Length);
        }
    }
}