using bolsafeucn_back.src.Domain.Models;
using bolsafeucn_back.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using bolsafeucn_back.src.Infrastructure.Data;

namespace bolsafeucn_back.src.Infrastructure.Repositories.Implements
{
    /// <summary>
    /// Implementación del repositorio de reseñas.
    /// Gestiona las operaciones de persistencia de datos para las reseñas usando Entity Framework Core.
    /// </summary>
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio de reseñas.
        /// </summary>
        /// <param name="context">El contexto de base de datos de la aplicación.</param>
        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Agrega una nueva reseña a la base de datos.
        /// </summary>
        /// <param name="review">La reseña a agregar.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task AddAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene todas las reseñas asociadas a un oferente específico.
        /// Incluye la información del estudiante relacionado.
        /// </summary>
        /// <param name="offerorId">El identificador del oferente.</param>
        /// <returns>Una colección de reseñas del oferente con información del estudiante.</returns>
        public async Task<IEnumerable<Review>> GetByOfferorIdAsync(int offerorId)
        {
            return await _context.Reviews
                .Where(r => r.OfferorId == offerorId)
                .Include(r => r.Student)
                .ToListAsync();
        }
        /// <summary>
        /// Obtiene todas las reseñas asociadas a un estudiante específico.
        /// </summary>
        /// <param name="studentId">El identificador del estudiante.</param>
        /// <returns>Una colección de reseñas del estudiante.</returns>
        public async Task<IEnumerable<Review>> GetByStudentIdAsync(int studentId)
        {
            return await _context.Reviews
                .Where(r => r.StudentId == studentId)
                .Include(r => r.Offeror)
                .ToListAsync();
        }

        /// <summary>
        /// Calcula el promedio de calificaciones de un oferente.
        /// Solo considera las calificaciones completadas (RatingForOfferor no null).
        /// </summary>
        /// <param name="offerorId">El identificador del oferente.</param>
        /// <returns>El promedio de calificaciones, o null si no hay reseñas.</returns>
        public async Task<double?> GetOfferorAverageRatingAsync(int offerorId)
        {
            return await _context.Reviews
                .Where(r => r.OfferorId == offerorId)
                .AverageAsync(r => (double?)r.RatingForOfferor);
        }
        /// <summary>
        /// Calcula el promedio de calificaciones de un estudiante.
        /// Solo considera las calificaciones completadas (RatingForStudent no null).
        /// </summary>
        /// <param name="studentId">El identificador del estudiante.</param>
        /// <returns>El promedio de calificaciones, o null si no hay reseñas.</returns>
        public async Task<double?> GetStudentAverageRatingAsync(int studentId)
        {
            return await _context.Reviews
                .Where(r => r.StudentId == studentId)
                .AverageAsync(r => (double?)r.RatingForStudent);
        }

        /// <summary>
        /// Obtiene una reseña asociada a una publicación específica.
        /// </summary>
        /// <param name="publicationId">El identificador de la publicación.</param>
        /// <returns>La reseña asociada a la publicación, o null si no existe.</returns>
        public async Task<Review?> GetByPublicationIdAsync(int publicationId)
        {
            return await _context.Reviews
                .Include(r => r.Student)
                .Include(r => r.Offeror)
                .Where(r => r.PublicationId == publicationId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtiene una reseña por su identificador único.
        /// </summary>
        /// <param name="reviewId">El identificador de la reseña.</param>
        /// <returns>La reseña solicitada, o null si no existe.</returns>
        public async Task<Review?> GetByIdAsync(int reviewId)
        {
            return await _context.Reviews
                .Include(r => r.Student)
                .Include(r => r.Offeror)
                .Where(r => r.Id == reviewId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Actualiza una reseña existente en la base de datos.
        /// </summary>
        /// <param name="review">La reseña con los datos actualizados.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Obtiene todas las reseñas del sistema.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _context.Reviews
                .Include(r => r.Student)
                .Include(r => r.Offeror)
                .ToListAsync();
        }

        public async Task<IEnumerable<Publication>> GetPublicationInformationAsync(int publicationId)
        {
            return await _context.Publications
                .Where(p => p.Id == publicationId)
                .ToListAsync();
        }
    }
}
