using bolsafeucn_back.src.Application.DTOs.JobAplicationDTO;
using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de postulaciones a ofertas laborales
    /// </summary>
    public interface IJobApplicationService
    {
        /// <summary>
        /// Crea una nueva postulación de un estudiante a una oferta (postulación directa)
        /// </summary>
        Task<JobApplicationResponseDto> CreateApplicationAsync(int studentId, int offerId);

        /// <summary>
        /// Obtiene todas las postulaciones de un estudiante
        /// </summary>
        Task<IEnumerable<JobApplicationResponseDto>> GetStudentApplicationsAsync(int studentId);

        /// <summary>
        /// Obtiene todas las postulaciones recibidas para una oferta específica
        /// </summary>
        Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByOfferIdAsync(int offerId);

        Task<JobApplicationDetailDto?> GetApplicationDetailAsync(int applicationId);

        /// <summary>
        /// Obtiene todas las postulaciones de todas las ofertas de una empresa
        /// </summary>
        Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByCompanyIdAsync(int companyId);

        /// <summary>
        /// Actualiza el estado de una postulación (Pendiente, Aceptada, Rechazada)
        /// </summary>
        Task<bool> UpdateApplicationStatusAsync(int applicationId, ApplicationStatus newStatus, int companyId);

        /// <summary>
        /// Valida si un estudiante es elegible para postular
        /// </summary>
        Task<bool> ValidateStudentEligibilityAsync(int studentId, bool isCvRequired = true);

        /// <summary>
        /// Muestra una lista de los postulantes a una cierta oferta
        /// </summary>
        Task<IEnumerable<ViewApplicantsDto>> GetApplicantsForAdminManagement(int offerId);

        /// <summary>
        /// Obtiene los detalles de un postulante que postula a una oferta de trabajo
        /// </summary>
        Task<ViewApplicantDetailAdminDto> GetApplicantDetailForAdmin(int studentId);


        /// <summary>
        /// Obtiene los postulantes para una oferta específica, validando que el oferente sea el dueño.
        /// </summary>
        Task<IEnumerable<OffererApplicantViewDto>> GetApplicantsForOffererAsync(int offerId, int offererUserId);

        /// <summary>
        /// Obtiene los detalles de un postulante para una oferta específica, validando que el oferente sea el dueño.
        /// </summary>
        Task<ViewApplicantUserDetailDto> GetApplicantDetailForOfferer(int studentId, int offerId, int offererUserId);






    }
}
