using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// Dto que se usa al momento en que el admin administra las compra y venta ya publicadas y se le muestran estas. 
    /// </summary>
    public class BuySellBasicAdminDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string NameOwner { get; set; }
        public DateTime PublicationDate { get; set; }
        public Types Type { get; set; }
        public bool Activa { get; set; }  
    }
}