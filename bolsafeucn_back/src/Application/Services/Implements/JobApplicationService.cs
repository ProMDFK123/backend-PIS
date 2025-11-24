using bolsafeucn_back.src.Application.DTOs.JobAplicationDTO;
using bolsafeucn_back.src.Application.Services.Interfaces;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using CloudinaryDotNet.Actions;

namespace bolsafeucn_back.src.Application.Services.Implements
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _jobApplicationRepository;
        private readonly IOfferRepository _offerRepository;
        private readonly IUserRepository _userRepository;

        public JobApplicationService(
            IJobApplicationRepository jobApplicationRepository,
            IOfferRepository offerRepository,
            IUserRepository userRepository
        )
        {
            _jobApplicationRepository = jobApplicationRepository;
            _offerRepository = offerRepository;
            _userRepository = userRepository;
        }

        public async Task<JobApplicationResponseDto> CreateApplicationAsync(
            int studentId,
            int offerId
        )
        {
            // Verificar que la oferta existe y está activa
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null || !offer.IsActive)
            {
                throw new KeyNotFoundException("La oferta no existe o no está activa");
            }

            // Validar elegibilidad del estudiante (incluye validación de CV si es obligatorio)
            if (!await ValidateStudentEligibilityAsync(studentId, offer.IsCvRequired))
            {
                if (offer.IsCvRequired)
                {
                    throw new UnauthorizedAccessException(
                        "Esta oferta requiere CV. Por favor, sube tu CV en tu perfil antes de postular"
                    );
                }
                else
                {
                    throw new UnauthorizedAccessException(
                        "El estudiante no es elegible para postular"
                    );
                }
            }

            // Validar que la fecha límite no haya expirado
            if (offer.DeadlineDate < DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "La fecha límite para postular a esta oferta ha expirado"
                );
            }

            // Validar que la oferta no haya finalizado
            if (offer.EndDate < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Esta oferta ha finalizado");
            }

            // Verificar que no haya postulado anteriormente
            var existingApplication = await _jobApplicationRepository.GetByStudentAndOfferAsync(
                studentId,
                offerId
            );
            if (existingApplication != null)
            {
                throw new InvalidOperationException("Ya has postulado a esta oferta");
            }

            // Obtener datos del estudiante con sus relaciones
            var student = await _userRepository.GetByIdWithRelationsAsync(studentId);
            if (student == null || student.Student == null)
            {
                throw new KeyNotFoundException("Estudiante no encontrado");
            }

            // Crear la postulación (CV obligatorio, carta de motivación opcional del perfil)
            var jobApplication = new JobApplication
            {
                StudentId = studentId,
                JobOfferId = offerId,
                Student = null!,
                JobOffer = null!,
                Status = "Pendiente",
                ApplicationDate = DateTime.UtcNow,
            };

            var createdApplication = await _jobApplicationRepository.AddAsync(jobApplication);

            return new JobApplicationResponseDto
            {
                Id = createdApplication.Id,
                StudentName = $"{student.Student.Name} {student.Student.LastName}",
                StudentEmail = student.Email!,
                OfferTitle = offer.Title,
                Status = createdApplication.Status,
                ApplicationDate = createdApplication.ApplicationDate,
                CurriculumVitae = student.Student.CurriculumVitae,
                MotivationLetter = student.Student.MotivationLetter, // Carta opcional del perfil
            };
        }

        public async Task<IEnumerable<JobApplicationResponseDto>> GetStudentApplicationsAsync(
            int studentId
        )
        {
            var applications = await _jobApplicationRepository.GetByStudentIdAsync(studentId);

            return applications.Select(app => new JobApplicationResponseDto
            {
                Id = app.Id,
                StudentName = $"{app.Student.Student?.Name} {app.Student.Student?.LastName}",
                StudentEmail = app.Student.Email!,
                OfferTitle = app.JobOffer.Title,
                Status = app.Status,
                ApplicationDate = app.ApplicationDate,
                CurriculumVitae = app.Student.Student?.CurriculumVitae,
                MotivationLetter = app.Student.Student?.MotivationLetter,
            });
        }
        public async Task<JobApplicationDetailDto?> GetApplicationDetailAsync(int applicationId)
        {
            // Obtener la postulación
            var application = await _jobApplicationRepository.GetByIdAsync(applicationId);
            
            if (application == null)
                return null;

            
            var offer = await _offerRepository.GetByIdAsync(application.JobOfferId);
            
            if (offer == null)
                return null;

           
            var user = await _userRepository.GetByIdWithRelationsAsync(offer.UserId);

            var authorName = user?.UserType == UserType.Empresa
                ? (user.Company?.CompanyName ?? "Empresa desconocida")
            : user?.UserType == UserType.Particular
                ? $"{(user.Individual?.Name ?? "").Trim()} {(user.Individual?.LastName ?? "").Trim()}".Trim()
            : (user?.UserName ?? "UCN");
            var statusMessage = application.Status switch
            {
                "Pendiente" => "Su solicitud fue enviada con éxito; será contactado a la brevedad.",
                "Seleccionado" => "¡Felicidades! Tu solicitud ha sido aceptada.",
                "No seleccionado" => "Lamentablemente, tu solicitud ha sido rechazado.",
                _ => ""
            };

            return new JobApplicationDetailDto
            {
                Id = application.Id,
                OfferTitle = offer.Title,
                CompanyName = authorName,
                ApplicationDate = application.ApplicationDate,
                PublicationDate = offer.PublicationDate,
                EndDate = offer.EndDate,
                Remuneration = offer.Remuneration,
                Description = offer.Description,
                Requirements = offer.Requirements,
                ContactInfo = offer.ContactInfo,
                Status = application.Status,
                StatusMessage = statusMessage
            };
        }

        public async Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByOfferIdAsync(
            int offerId
        )
        {
            var applications = await _jobApplicationRepository.GetByOfferIdAsync(offerId);

            return applications.Select(app => new JobApplicationResponseDto
            {
                Id = app.Id,
                StudentName = $"{app.Student.Student?.Name} {app.Student.Student?.LastName}",
                StudentEmail = app.Student.Email!,
                OfferTitle = app.JobOffer.Title,
                Status = app.Status,
                ApplicationDate = app.ApplicationDate,
                CurriculumVitae = app.Student.Student?.CurriculumVitae,
                MotivationLetter = app.Student.Student?.MotivationLetter,
            });
        }

        public async Task<IEnumerable<JobApplicationResponseDto>> GetApplicationsByCompanyIdAsync(
            int companyId
        )
        {
            // Obtener todas las ofertas de la empresa
            var companyOffers = await _offerRepository.GetOffersByUserIdAsync(companyId);
            var offerIds = companyOffers.Select(o => o.Id).ToList();

            // Obtener todas las postulaciones de esas ofertas
            var allApplications = new List<JobApplicationResponseDto>();

            foreach (var offerId in offerIds)
            {
                var applications = await GetApplicationsByOfferIdAsync(offerId);
                allApplications.AddRange(applications);
            }

            return allApplications.OrderByDescending(a => a.ApplicationDate);
        }

        public async Task<bool> UpdateApplicationStatusAsync(
            int applicationId,
            string newStatus,
            int companyId
        )
        {
            // Validar que el estado sea válido
            var validStatuses = new[] { "Pendiente", "Aceptada", "Rechazada" };
            if (!validStatuses.Contains(newStatus))
            {
                throw new ArgumentException(
                    $"Estado inválido. Debe ser uno de: {string.Join(", ", validStatuses)}"
                );
            }

            // Obtener la postulación
            var application = await _jobApplicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                throw new KeyNotFoundException("Postulación no encontrada");
            }

            // Verificar que la oferta pertenece a la empresa
            var offer = await _offerRepository.GetByIdAsync(application.JobOfferId);
            if (offer == null || offer.UserId != companyId)
            {
                throw new UnauthorizedAccessException(
                    "No tienes permiso para modificar esta postulación"
                );
            }

            // Actualizar el estado
            application.Status = newStatus;
            await _jobApplicationRepository.UpdateAsync(application);

            return true;
        }

        public async Task<bool> ValidateStudentEligibilityAsync(
            int studentId,
            bool isCvRequired = true
        )
        {
            var student = await _userRepository.GetByIdWithRelationsAsync(studentId);

            if (student == null || student.UserType != UserType.Estudiante)
                return false;

            // Verificar que tenga correo institucional
            if (!student.Email!.EndsWith("@alumnos.ucn.cl"))
                return false;

            // Verificar que no esté bloqueado
            if (student.Banned)
                return false;

            // Verificar que tenga CV SOLO si es obligatorio
            if (isCvRequired)
            {
                if (
                    student.Student == null
                    || string.IsNullOrEmpty(student.Student.CurriculumVitae)
                )
                    return false;
            }

            return true;
        }

        public async Task<IEnumerable<ViewApplicantsDto>> GetApplicantsForAdminManagement(
            int offerId
        )
        {
            var applicant = await _jobApplicationRepository.GetByOfferIdAsync(offerId);
            return applicant
                .Select(app => new ViewApplicantsDto
                {
                    Applicant = $"{app.Student.Student?.Name} {app.Student.Student?.LastName}",
                    Status = app.Status,
                })
                .ToList();
        }

        public async Task<ViewApplicantDetailAdminDto> GetApplicantDetailForAdmin(int studentId)
        {
            var applicant = await _jobApplicationRepository.GetByIdAsync(studentId);
            return new ViewApplicantDetailAdminDto
            {
                StudentName =
                    $"{applicant.Student.Student?.Name} {applicant.Student.Student?.LastName}",
                Email = applicant.Student.Email,
                PhoneNumber = applicant.Student.PhoneNumber,
                Status = applicant.Status,
                // TODO: falta descripcion
            };
        }

        public async Task<IEnumerable<OffererApplicantViewDto>> GetApplicantsForOffererAsync(
            int offerId,
            int offererUserId
        )
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
            {
                throw new KeyNotFoundException($"La oferta con id {offerId} no fue encontrada.");
            }

            // Comprueba que el ID del usuario de la oferta (offer.UserId)
            // sea el mismo que el ID del usuario logueado (offererUserId).
            if (offer.UserId != offererUserId)
            {
                throw new UnauthorizedAccessException(
                    "No tienes permiso para ver los postulantes de esta oferta, ya que no eres el propietario."
                );
            }

            // Tu repositorio GetByOfferIdAsync ya incluye Student y Student.Student (según tu código)
            var applications = await _jobApplicationRepository.GetByOfferIdAsync(offerId);

            // 4. Mapear al nuevo DTO que creamos
            var applicantDtos = applications
                .Select(app => new OffererApplicantViewDto
                {
                    ApplicationId = app.Id,
                    StudentId = app.StudentId,
                    ApplicantName = $"{app.Student.Student?.Name} {app.Student.Student?.LastName}",
                    Status = app.Status,
                    ApplicationDate = app.ApplicationDate,
                    // Enviamos el link del CV directamente
                    CurriculumVitaeUrl = app.Student.Student?.CurriculumVitae,
                })
                .ToList();

            return applicantDtos;
        }

        

        public async Task<ViewApplicantUserDetailDto> GetApplicantDetailForOfferer(
            int studentId,
            int offerId,
            int offererUserId
        )
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
            {
                throw new KeyNotFoundException($"La oferta con id {offerId} no fue encontrada.");
            }
        


            // Comprueba que el ID del usuario de la oferta (offer.UserId)
            // sea el mismo que el ID del usuario logueado (offererUserId).
            if (offer.UserId != offererUserId)
            {
                throw new UnauthorizedAccessException(
                    "No tienes permiso para ver los postulantes de esta oferta, ya que no eres el propietario."
                );
            }

            var applicant = await _jobApplicationRepository.GetByStudentIdAsync(studentId);
            var applicantDto = new ViewApplicantUserDetailDto
            {
                Name =
                    $"{applicant.First().Student.Student.Name} {applicant.First().Student.Student.LastName}",
                Email = applicant.First().Student.Email,
                Phone = applicant.First().Student.PhoneNumber,
                Rut = applicant.First().Student.Rut,
                Rating = applicant.First().Student.Student?.Rating,
                MotivationLetter = applicant.First().Student.Student?.MotivationLetter,
                Disability = applicant.First().Student.Student?.Disability.ToString(),
                CurriculumVitae = applicant.First().Student.Student?.CurriculumVitae,
            };

            return applicantDto;
        }
    }
}
