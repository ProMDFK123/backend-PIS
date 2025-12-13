using bolsafeucn_back.src.Application.DTOs.AuthDTOs;
using bolsafeucn_back.src.Domain.Models;
using Mapster;

namespace bolsafeucn_back.src.Application.Mappers
{
    public class CompanyMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureAuthMapping();
        }

        public void ConfigureAuthMapping()
        {
            TypeAdapterConfig<RegisterCompanyDTO, GeneralUser>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.Email != null && src.Email.Contains("@") ? src.Email.Substring(0, src.Email.IndexOf("@")) : src.Email)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.UserType, src => UserType.Empresa)
                .Map(dest => dest.Banned, src => false)
                .Map(dest => dest.EmailConfirmed, src => false);

            TypeAdapterConfig<RegisterCompanyDTO, Company>
                .NewConfig()
                .Map(dest => dest.CompanyName, src => src.CompanyName)
                .Map(dest => dest.LegalName, src => src.LegalName);
        }
    }
}
