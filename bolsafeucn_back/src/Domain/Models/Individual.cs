namespace bolsafeucn_back.src.Domain.Models
{
    /// <summary>
    /// Clase que representa un oferente particular.
    /// </summary>
    public class Individual
    {
        public int Id { get; set; }
        public required GeneralUser GeneralUser { get; set; }
        public required int GeneralUserId { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
    }
}
