namespace bolsafeucn_back.src.Domain.Models
{
    /// <summary>
    /// Clase que representa una empresa.
    /// </summary>
    public class Company
    {
        public int Id { get; set; }
        public required GeneralUser GeneralUser { get; set; }
        public required int GeneralUserId { get; set; }
        public required string CompanyName { get; set; }
        public required string LegalName { get; set; }
    }
}
