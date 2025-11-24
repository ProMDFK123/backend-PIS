namespace bolsafeucn_back.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de envío de correos electrónicos.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envia un correo electrónico para verificar la dirección de correo del usuario.
        /// </summary>
        Task<bool> SendVerificationEmailAsync(string email, string code);

        /// <summary>
        /// Envía un correo de bienvenida al nuevo usuario.
        /// </summary>
        Task<bool> SendWelcomeEmailAsync(string email);

        /// <summary>
        /// Envía un correo para restablecer la contraseña del usuario.
        /// </summary>
        Task<bool> SendResetPasswordVerificationEmailAsync(string email, string code);

        /// <summary>
        /// Carga una plantilla de correo electrónico.
        /// </summary>
        Task<string> LoadTemplateAsync(string templateName, string code);

        /// <summary>
        /// Envía un correo notificando que una postulación cambió de estado.
        /// </summary>
        /// <param name="email">Correo del estudiante.</param>
        /// <param name="offerName">Nombre de la oferta laboral.</param>
        /// <param name="companyName">Nombre de la empresa.</param>
        /// <param name="newStatus">Nuevo estado de la postulación.</param>
        Task<bool> SendPostulationStatusChangeEmailAsync(
            string email,
            string offerName,
            string companyName,
            string newStatus
        );
    }
}

