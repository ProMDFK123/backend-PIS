namespace bolsafeucn_back.src.Domain.Models
{
    /// <summary>
    /// Clase que representa un administrador.
    /// </summary>
    public class Admin : ModelBase
    {
        public required GeneralUser GeneralUser { get; set; }
        public required int GeneralUserId { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public bool SuperAdmin { get; set; } = false;
    }
}
