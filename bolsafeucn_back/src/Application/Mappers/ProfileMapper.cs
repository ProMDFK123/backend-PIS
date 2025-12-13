using Mapster;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Application.DTOs.UserDTOs.UserProfileDTOs;
using System.IO.Compression;

namespace bolsafeucn_back.src.Application.Mappers
{
    /// <summary>
    /// Clase encargada de mapear entidades relacionadas con el perfil de usuario.
    /// </summary>
    public class ProfileMapper
    {
        /// <summary>
        /// Configura todos los mapeos relacionados con el perfil de usuario.
        /// </summary>
        public void ConfigureAllMappings()
        {
            ConfigureViewProfileMapping();
            ConfigureUpdateMappings();
        }
        /// <summary>
        /// Configura el mapeo de GeneralUser a GetUserProfileDTO para ver el perfil de usuario.
        /// </summary>
        public void ConfigureViewProfileMapping()
        {
            TypeAdapterConfig<GeneralUser, GetStudentProfileDTO>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.Name, src => src.Student!.Name)
                .Map(dest => dest.LastName, src => src.Student!.LastName)
                //.Map(dest => dest.Rating, src => src.Student!.Rating)
                .Map(dest => dest.CurriculumVitae, src => src.Student!.CurriculumVitae)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.AboutMe, src => src.AboutMe)
                .Map(dest => dest.ProfilePhoto, src => src.ProfilePhoto!.Url)
                .Map(dest => dest.ProfileBanner, src => src.ProfileBanner!.Url);

            TypeAdapterConfig<GeneralUser, GetIndividualProfileDTO>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.Name, src => src.Individual!.Name)
                .Map(dest => dest.LastName, src => src.Individual!.LastName)
                //.Map(dest => dest.Rating, src => src.Individual!.Rating)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.AboutMe, src => src.AboutMe)
                .Map(dest => dest.ProfilePhoto, src => src.ProfilePhoto!.Url)
                .Map(dest => dest.ProfileBanner, src => src.ProfileBanner!.Url);

            TypeAdapterConfig<GeneralUser, GetCompanyProfileDTO>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.CompanyName, src => src.Company!.CompanyName)
                .Map(dest => dest.LegalName, src => src.Company!.LegalName)
                //.Map(dest => dest.Rating, src => src.Company!.Rating)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.AboutMe, src => src.AboutMe)
                .Map(dest => dest.ProfilePhoto, src => src.ProfilePhoto!.Url)
                .Map(dest => dest.ProfileBanner, src => src.ProfileBanner!.Url);

            TypeAdapterConfig<GeneralUser, GetAdminProfileDTO>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.Name, src => src.Admin!.Name)
                .Map(dest => dest.LastName, src => src.Admin!.LastName)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.AboutMe, src => src.AboutMe)
                .Map(dest => dest.ProfilePhoto, src => src.ProfilePhoto!.Url)
                .Map(dest => dest.ProfileBanner, src => src.ProfileBanner!.Url);

            /*TypeAdapterConfig<GeneralUser, GetUserProfileDTO>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.Name, src =>
                    src.Student != null ? src.Student.Name :
                    src.Individual != null ? src.Individual.Name :
                    src.Admin != null ? src.Admin.Name : 
                    null)
                .Map(dest => dest.LastName, src =>
                    src.Student != null ? src.Student.LastName :
                    src.Individual != null ? src.Individual.LastName :
                    src.Admin != null ? src.Admin.LastName :
                    null)
                .Map(dest => dest.CompanyName, src => src.Company != null ? src.Company.CompanyName : null)
                .Map(dest => dest.LegalName, src => src.Company != null ? src.Company.LegalName : null)
                .Map(dest => dest.Rating, src =>
                    src.Student != null ? src.Student.Rating :
                    src.Individual != null ? src.Individual.Rating :
                    src.Company != null ? src.Company.Rating :
                    (float?)null)
                .Map(dest => dest.CurriculumVitae, src => src.Student != null ? src.Student.CurriculumVitae : null)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.AboutMe, src => src.AboutMe);*/
        }

        /// <summary>
        /// Configura el mapeo de UpdateParamsDTO a GeneralUser para actualizar el perfil de usuario.
        /// </summary>
        public void ConfigureUpdateMappings()
        {
            TypeAdapterConfig<UpdateStudentParamsDTO, GeneralUser>
            .NewConfig()
            .IgnoreNullValues(true)
            .Ignore(src => src.ProfilePhoto!)
            .Ignore(src => src.ProfileBanner!)
            .Map(dest => dest.UserName, src => src.UserName)
            .Map(dest => dest.Student!.Name, src => src.Name)
            .Map(dest => dest.Student!.LastName, src => src.LastName)
            .Map(dest => dest.Rut, src => src.Rut)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.AboutMe, src => src.AboutMe)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber);

            TypeAdapterConfig<UpdateIndividualParamsDTO, GeneralUser>
            .NewConfig()
            .IgnoreNullValues(true)
            .Map(dest => dest.UserName, src => src.UserName)
            .Map(dest => dest.Individual!.Name, src => src.Name)
            .Map(dest => dest.Individual!.LastName, src => src.LastName)
            .Map(dest => dest.Rut, src => src.Rut)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.AboutMe, src => src.AboutMe)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber);

            TypeAdapterConfig<UpdateCompanyParamsDTO, GeneralUser>
            .NewConfig()
            .IgnoreNullValues(true)
            .Map(dest => dest.UserName, src => src.UserName)
            .Map(dest => dest.Company!.CompanyName, src => src.CompanyName)
            .Map(dest => dest.Company!.LegalName, src => src.LegalName)
            .Map(dest => dest.Rut, src => src.Rut)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.AboutMe, src => src.AboutMe)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber);

            TypeAdapterConfig<UpdateAdminParamsDTO, GeneralUser>
            .NewConfig()
            .IgnoreNullValues(true)
            .Map(dest => dest.UserName, src => src.UserName)
            .Map(dest => dest.Admin!.Name, src => src.Name)
            .Map(dest => dest.Admin!.LastName, src => src.LastName)
            .Map(dest => dest.Rut, src => src.Rut)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.AboutMe, src => src.AboutMe)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
            .Map(dest => dest.Admin!.SuperAdmin, src => src.IsSuperAdmin);
        }
    }
}