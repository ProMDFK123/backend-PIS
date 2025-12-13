namespace bolsafeucn_back.src.Domain.Models
{
    /// <summary>
    /// Tipos de códigos de verificación.
    /// </summary>
    public enum CodeType
    {
        EmailConfirmation,
        PasswordReset,
    }

    /// <summary>
    /// Clase que representa un código de verificación para acciones como confirmación de correo o restablecimiento de contraseña.
    /// </summary>
    public class VerificationCode
    {
        public int Id { get; set; }
        public required string Code { get; set; }
        public required CodeType CodeType { get; set; }
        public required int GeneralUserId { get; set; }
        public int Attempts { get; set; } = 0;
        public required DateTime Expiration { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
