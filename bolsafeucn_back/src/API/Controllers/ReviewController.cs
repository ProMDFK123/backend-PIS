using bolsafeucn_back.src.Application.DTOs.ReviewDTO;
using bolsafeucn_back.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace bolsafeucn_back.src.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar las reseñas entre oferentes y estudiantes.
    /// </summary>
    public class ReviewController : BaseController
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        /// <summary>
        /// Agrega una nueva reseña.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("AddReview")]
        public async Task<IActionResult> AddReview([FromBody] ReviewDTO dto)
        {
            await _reviewService.AddReviewAsync(dto);
            return Ok("Review added successfully");
        }
        /// <summary>
        /// Obtiene todas las reseñas para un oferente específico.
        /// </summary>
        /// <param name="offerorId"></param>
        /// <returns></returns>
        [HttpGet("offeror/{offerorId}")]
        public async Task<IActionResult> GetReviewsByOfferorId(int offerorId)
        {
            var reviews = await _reviewService.GetReviewsByOfferorAsync(offerorId);
            return Ok(reviews);
        }
        
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetReviewsByStudentId(int studentId)
        {
            var reviews = await _reviewService.GetReviewsByStudentAsync(studentId);
            return Ok(reviews);
        }
        /// <summary>
        /// Obtiene todas las reseñas del usuario autenticado.
        /// Funciona tanto para estudiantes (Applicant) como para oferentes (Offerent).
        /// El usuario solo puede ver sus propias reseñas.
        /// </summary>
        /// <returns>Lista de reseñas del usuario autenticado</returns>
        [HttpGet("my-reviews")]
        [Authorize(Roles = "Applicant,Offerent")]
        public async Task<IActionResult> GetMyReviews()
        {
            // Obtener el ID y el rol del usuario autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized("No se pudo identificar al usuario autenticado.");
            }
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            // Si es estudiante, buscar sus reseñas como estudiante
            if (userRole == "Applicant")
            {
                var reviews = await _reviewService.GetReviewsByStudentAsync(currentUserId);
                return Ok(reviews);
            }
            // Si es oferente, buscar sus reseñas como oferente
            else if (userRole == "Offerent")
            {
                var reviews = await _reviewService.GetReviewsByOfferorAsync(currentUserId);
                return Ok(reviews);
            }
            return Unauthorized("El usuario no tiene un rol válido para ver reseñas.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(int id)
        {
            var review = await _reviewService.GetReviewAsync(id);
            return Ok(review);
        }
        /// <summary>
        /// Obtiene la calificación promedio de un oferente específico.
        /// </summary>
        /// <param name="offerorId"></param>
        /// <returns></returns>
        [HttpGet("average/{offerorId}")]
        public async Task<IActionResult> GetAverage(int offerorId)
        {
            var avg = await _reviewService.GetAverageRatingAsync(offerorId);
            return Ok(avg);
        }
        /// <summary>
        /// Agrega la reseña del oferente hacia el estudiante.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("AddStudentReview")]
        [Authorize(Roles = "Offerent")]
        public async Task<IActionResult> AddStudentReview([FromBody] ReviewForStudentDTO dto)
        {
            // Obtener el ID del usuario autenticado (OFERENTE calificando al estudiante)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized("No se pudo identificar al usuario autenticado.");
            }
            await _reviewService.AddStudentReviewAsync(dto, currentUserId);
            return Ok("Student review added successfully");
        }
        /// <summary>
        /// Agrega la reseña del estudiante hacia el oferente.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("AddOfferorReview")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> AddOfferorReview([FromBody] ReviewForOfferorDTO dto)
        {
            // Obtener el ID del usuario autenticado (ESTUDIANTE calificando al oferente)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized("No se pudo identificar al usuario autenticado.");
            }
            await _reviewService.AddOfferorReviewAsync(dto, currentUserId);
            return Ok("Offeror review added successfully");
        }
        /// <summary>
        /// Agrega una nueva reseña inicial.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>El ID de la reseña creada.</returns>
        [HttpPost("AddInitialReview")]
        public async Task<IActionResult> AddInitialReview([FromBody] InitialReviewDTO dto)
        {
            var review = await _reviewService.CreateInitialReviewAsync(dto);
            return Ok(new { reviewId = review.Id, message = "Initial review added successfully" });
        }
        /// <summary>
        /// Elimina una parte de la reseña.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpDelete("Admin/DeleteReviewPart")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReviewPart([FromBody] DeleteReviewPartDTO dto)
        {
            await _reviewService.DeleteReviewPartAsync(dto);
            return Ok("Review part(s) deleted successfully");
        }
        /// <summary>
        /// Obtiene todas las reseñas del sistema. Solamente el administrador puede
        /// acceder a este endpoint.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Admin/GetAllReviews")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return Ok(reviews);
        }
        
        /// <summary>
        /// Obtiene la información de las publicaciones asociadas a las reseñas del usuario autenticado.
        /// Identifica automáticamente si el usuario es estudiante u oferente.
        /// </summary>
        /// <returns>Una lista de publicaciones asociadas a las reseñas del usuario.</returns>
        [HttpGet("publications")]
        [Authorize(Roles = "Applicant,Offerent")]
        public async Task<IActionResult> GetMyPublicationInformation()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }
            var publicationInfo = await _reviewService.GetPublicationInformationAsync(userId);
            return Ok(publicationInfo);
        }
    }
}