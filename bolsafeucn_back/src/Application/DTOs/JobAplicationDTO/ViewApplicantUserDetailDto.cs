using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bolsafeucn_back.src.Domain.Models;

namespace bolsafeucn_back.src.Application.DTOs.JobAplicationDTO
{
    public class ViewApplicantUserDetailDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Rut { get; set; }
        public required float? Rating { get; set; }
        public required string MotivationLetter { get; set; }
        public required string Disability { get; set; }
        public required string CurriculumVitae { get; set; }
        //public string? ProfileImageUrl { get; set; }
    }
}
