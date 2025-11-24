using bolsafeucn_back.src.Application.DTOs.PublicationDTO;
using bolsafeucn_back.src.Application.DTOs.ReviewDTO;
using bolsafeucn_back.src.Application.Mappers;
using bolsafeucn_back.src.Application.Services.Interfaces;
using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace bolsafeucn_back.src.Application.Services.Implements
{
    /// <summary>
    /// Implementación del servicio de reseñas.
    /// Gestiona la lógica de negocio para las operaciones de reseñas bidireccionales
    /// entre oferentes y estudiantes, incluyendo validaciones y permisos.
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _repository;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de reseñas.
        /// </summary>
        /// <param name="repository">El repositorio de reseñas para acceso a datos.</param>
        /// <param name="userRepository">El repositorio de usuarios para validación.</param>
        public ReviewService(IReviewRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Agrega una nueva reseña completa (obsoleto - no implementado).
        /// </summary>
        /// <param name="dto">DTO con la información de la reseña completa.</param>
        /// <exception cref="NotImplementedException">Este método no está implementado.</exception>
        public async Task AddReviewAsync(ReviewDTO dto)
        {
            // var review = ReviewMapper.ToEntity(dto);
            // await _repository.AddAsync(review);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtiene todas las reseñas asociadas a un oferente específico.
        /// </summary>
        /// <param name="offerorId">El identificador del oferente.</param>
        /// <returns>Una colección de DTOs de reseñas del oferente.</returns>
        public async Task<IEnumerable<ReviewDTO>> GetReviewsByOfferorAsync(int offerorId)
        {
            var reviews = await _repository.GetByOfferorIdAsync(offerorId);
            return reviews.Select(ReviewMapper.ToDTO);
        }

        /// <summary>
        /// Calcula el promedio de calificaciones recibidas por un oferente.
        /// </summary>
        /// <param name="offerorId">El identificador del oferente.</param>
        /// <returns>El promedio de calificaciones, o null si no hay reseñas.</returns>
        public async Task<double?> GetAverageRatingAsync(int offerorId)
        {
            return await _repository.GetAverageRatingAsync(offerorId);
        }

        /// <summary>
        /// Agrega la evaluación del oferente hacia el estudiante.
        /// Valida que el usuario autenticado sea el oferente y que no haya completado previamente su evaluación.
        /// Si el estudiante ya completó su parte, marca la reseña como completada.
        /// </summary>
        /// <param name="dto">DTO con la calificación y comentarios para el estudiante.</param>
        /// <param name="currentUserId">ID del usuario autenticado (debe ser el oferente).</param>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra una reseña para el ID de publicación.</exception>
        /// <exception cref="UnauthorizedAccessException">Lanzada si el usuario no es el oferente de esta publicación.</exception>
        /// <exception cref="InvalidOperationException">Lanzada si el oferente ya completó su evaluación.</exception>
        public async Task AddStudentReviewAsync(ReviewForStudentDTO dto, int currentUserId)
        {
            var review = await _repository.GetByPublicationIdAsync(dto.PublicationId);
            if(review == null)
                throw new KeyNotFoundException("No se ha encontrado una reseña para el ID de publicación dado.");
            
            // Validar que el usuario actual sea el OFERENTE (quien califica al estudiante)
            if(review.OfferorId != currentUserId)
            {
                throw new UnauthorizedAccessException("Solo el oferente de esta publicación puede dejar una review hacia el estudiante.");
            }
            // Validar que el oferente no haya completado ya su review hacia el estudiante
            if (review.StudentReviewCompleted)
            {
                throw new InvalidOperationException("Ya has completado tu review para este estudiante.");
            }
            if(review.HasReviewForStudentBeenDeleted)
            {
                throw new InvalidOperationException("No se puede agregar la Review hacia el estudiante, ya que esta ya ha sido eliminada.");
            }
            ReviewMapper.studentUpdateReview(dto, review);
            if(review.OfferorReviewCompleted) {
                review.IsCompleted = true;
            }
            await _repository.UpdateAsync(review);
            Log.Information("Offeror {OfferorId} added review for student in publication {PublicationId}", currentUserId, dto.PublicationId);
        }

        /// <summary>
        /// Agrega la evaluación del estudiante hacia el oferente.
        /// Valida que el usuario autenticado sea el estudiante y que no haya completado previamente su evaluación.
        /// Si el oferente ya completó su parte, marca la reseña como completada.
        /// </summary>
        /// <param name="dto">DTO con la calificación y comentarios para el oferente.</param>
        /// <param name="currentUserId">ID del usuario autenticado (debe ser el estudiante).</param>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra una reseña para el ID de publicación.</exception>
        /// <exception cref="UnauthorizedAccessException">Lanzada si el usuario no es el estudiante de esta publicación.</exception>
        /// <exception cref="InvalidOperationException">Lanzada si el estudiante ya completó su evaluación.</exception>
        public async Task AddOfferorReviewAsync(ReviewForOfferorDTO dto, int currentUserId)
        {
            var review = await _repository.GetByPublicationIdAsync(dto.PublicationId);
            if(review == null)
            {
                throw new KeyNotFoundException("No se ha encontrado una reseña para el ID de publicación dado.");
            }
            // Validar que el usuario actual sea el ESTUDIANTE (quien califica al oferente)
            if(review.StudentId != currentUserId)
            {
                throw new UnauthorizedAccessException("Solo el estudiante de esta publicación puede dejar una review hacia el oferente.");
            }
            // Validar que el estudiante no haya completado ya su review hacia el oferente
            if (review.OfferorReviewCompleted)
            {
                throw new InvalidOperationException("Ya has completado tu review para este oferente.");
            }
            if(review.HasReviewForOfferorBeenDeleted)
            {
                throw new InvalidOperationException("No se puede agregar la Review hacia el oferente, ya que esta ya ha sido eliminada.");
            }
            ReviewMapper.offerorUpdateReview(dto, review);
            if(review.OfferorReviewCompleted) {
                review.IsCompleted = true;
                //await BothReviewsCompletedAsync();
            }
            await _repository.UpdateAsync(review);
            Log.Information("Student {StudentId} added review for offeror in publication {PublicationId}", currentUserId, dto.PublicationId);
        }

        /// <summary>
        /// Método para ejecutar acciones cuando ambas partes completan sus reseñas.
        /// Actualmente no implementado - reservado para lógica futura.
        /// </summary>
        /// <exception cref="NotImplementedException">Este método no está implementado.</exception>
        public Task BothReviewsCompletedAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Crea una reseña inicial en estado pendiente para una publicación.
        /// La ventana de revisión se establece automáticamente en 14 días.
        /// Ambas partes deben completar sus evaluaciones posteriormente.
        /// </summary>
        /// <param name="dto">DTO con los identificadores del estudiante, oferente y publicación.</param>
        /// <returns>La reseña inicial creada con estado pendiente.</returns>
        /// <exception cref="InvalidOperationException">Lanzada si ya existe una reseña para la publicación especificada.</exception>
        /// <exception cref="ArgumentException">Lanzada si el estudiante u oferente no existen.</exception>
        public async Task<Review> CreateInitialReviewAsync(InitialReviewDTO dto)
        {
            // Validar que existan el estudiante y el oferente
            var student = await _userRepository.GetByIdAsync(dto.StudentId);
            if (student == null)
            {
                throw new ArgumentException($"El estudiante con ID {dto.StudentId} no existe.");
            }
            var offeror = await _userRepository.GetByIdAsync(dto.OfferorId);
            if (offeror == null)
            {
                throw new ArgumentException($"El oferente con ID {dto.OfferorId} no existe.");
            }
            // Validar que no exista ya una review para esta publicación
            var existingReview = await _repository.GetByPublicationIdAsync(dto.PublicationId);
            if (existingReview != null)
            {
                throw new InvalidOperationException($"Ya existe una review para la publicación con ID {dto.PublicationId}.");
            }
            var review = ReviewMapper.CreateInitialReviewAsync(dto);
            await _repository.AddAsync(review);
            return review;
        }

        /// <summary>
        /// Elimina parcial o completamente una reseña.
        /// Permite eliminar la parte del estudiante, del oferente, o ambas.
        /// Si se eliminan ambas partes, la reseña se marca como no completada.
        /// </summary>
        /// <param name="dto">DTO especificando qué partes eliminar (estudiante y/o oferente).</param>
        /// <exception cref="InvalidOperationException">Lanzada si no se especifica ninguna parte para eliminar.</exception>
        /// <exception cref="KeyNotFoundException">Lanzada si no se encuentra la reseña con el ID especificado.</exception>
        public async Task DeleteReviewPartAsync(DeleteReviewPartDTO dto)
        {
            // Validar que se solicite eliminar al menos una parte
            if (!dto.DeleteReviewForOfferor && !dto.DeleteReviewForStudent)
            {
                throw new InvalidOperationException("Debe especificar al menos una parte de la review para eliminar.");
            }
            // Obtener la review
            var review = await _repository.GetByIdAsync(dto.ReviewId);
            if (review == null)
            {
                throw new KeyNotFoundException($"No se encontró una review con ID {dto.ReviewId}.");
            }
            // Eliminar la parte del estudiante si se solicita
            if (dto.DeleteReviewForStudent)
            {
                review.RatingForStudent = null;
                review.CommentForStudent = null;
                review.AtTime = false;
                review.GoodPresentation = false;
                review.StudentReviewCompleted = false;
                review.IsCompleted = false;
                review.HasReviewForStudentBeenDeleted = true;
                Log.Information("Deleted student part of review ID {ReviewId}", dto.ReviewId);
            }
            // Eliminar la parte del oferente si se solicita
            if (dto.DeleteReviewForOfferor)
            {
                review.RatingForOfferor = null;
                review.CommentForOfferor = null;
                review.OfferorReviewCompleted = false;
                review.IsCompleted = false;
                review.HasReviewForOfferorBeenDeleted = true;
                Log.Information("Deleted offeror part of review ID {ReviewId}", dto.ReviewId);
            }
            await _repository.UpdateAsync(review);
        }

        public async Task<ReviewDTO> GetReviewAsync(int id)
        {
            var review = await _repository.GetByIdAsync(id);
            if(review == null)
                throw new KeyNotFoundException($"No se encontró una review con ID {id}.");
            return ReviewMapper.ToDTO(review);
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByStudentAsync(int studentId)
        {
            var reviews = await _repository.GetByStudentIdAsync(studentId);
            if(reviews == null || !reviews.Any())
                throw new KeyNotFoundException($"No se encontraron reseñas para el estudiante con ID {studentId}.");
            return reviews.Select(ReviewMapper.ToDTO);
        }
        public async Task<IEnumerable<ReviewDTO>> GetAllReviewsAsync()
        {
            var reviews = await _repository.GetAllAsync();
            return reviews.Select(ReviewMapper.ToDTO);
        }

        public async Task<IEnumerable<PublicationsDTO>> GetPublicationInformationAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if(user == null)
                throw new KeyNotFoundException($"No se encontró el usuario con ID {userId}.");
            IEnumerable<Review> reviews;
            if(user.UserType == UserType.Estudiante)
            {
                reviews = await _repository.GetByStudentIdAsync(userId);
                if(reviews == null || !reviews.Any())
                    throw new KeyNotFoundException($"No se encontraron reseñas para el estudiante con ID {userId}.");
            }
            else if(user.UserType == UserType.Empresa || user.UserType == UserType.Particular)
            {
                reviews = await _repository.GetByOfferorIdAsync(userId);
                if(reviews == null || !reviews.Any())
                    throw new KeyNotFoundException($"No se encontraron reseñas para el oferente con ID {userId}.");
            }
            else
            {
                throw new InvalidOperationException($"El tipo de usuario {user.UserType} no puede tener reseñas.");
            }
            var publicationsList = new List<Publication>();
            foreach(var review in reviews)
            {
                var publications = await _repository.GetPublicationInformationAsync(review.Id);
                if(publications != null)
                {
                    publicationsList.AddRange(publications);
                }
            }
            if(!publicationsList.Any())
                throw new KeyNotFoundException($"No se encontró información de publicación para el usuario con ID {userId}.");
            return publicationsList.Select(PublicationMapper.ToDTO);
        }
    }
}
