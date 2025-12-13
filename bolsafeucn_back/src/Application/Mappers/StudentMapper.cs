using bolsafeucn_back.src.Application.DTOs.AuthDTOs;
using bolsafeucn_back.src.Domain.Models;
using Mapster;

namespace bolsafeucn_back.src.Application.Mappers
{
    public class StudentMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureAuthMapping();
        }

        public void ConfigureAuthMapping()
        {
            TypeAdapterConfig<RegisterStudentDTO, GeneralUser>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.Email != null && src.Email.Contains("@") ? src.Email.Substring(0, src.Email.IndexOf("@")) : src.Email)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.UserType, src => UserType.Estudiante)
                .Map(dest => dest.Banned, src => false)
                .Map(dest => dest.EmailConfirmed, src => false);

            TypeAdapterConfig<RegisterStudentDTO, Student>
                .NewConfig()
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Disability, src => src.Disability)
                .Map(dest => dest.CurriculumVitae, src => string.Empty)
                .Map(dest => dest.MotivationLetter, src => string.Empty);
        }
    }
}
