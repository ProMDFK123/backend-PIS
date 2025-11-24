using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.DTOs.UserDTOs.UserProfileDTOs
{
    public interface IUpdateParamsDTO
    {
        /// <summary>
        /// Aplica los parámetros de actualización al usuario dado.
        /// </summary>
        /// <param name="user">Usuario al que se le aplicarán los parámetros de actualización.</param>
        public void ApplyTo(GeneralUser user);

        /// <summary>
        /// Aplica las imágenes al DTO de imágenes de usuario.
        /// </summary>
        /// <param name="images">DTO de imágenes de usuario al que se le aplicarán las imágenes.</param>
        public void ApplyTo(UserImagesDTO images);
    }
}