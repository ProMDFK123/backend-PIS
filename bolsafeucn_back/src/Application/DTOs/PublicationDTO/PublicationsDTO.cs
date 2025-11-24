using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.DTOs.PublicationDTO
{
    public class PublicationsDTO
    {
        public int IdPublication { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public Types types { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }
        public ICollection<Image> Images { get; set; } = new List<Image>();
        public bool IsActive { get; set; }
        public StatusValidation statusValidation { get; set; }
    }
}
