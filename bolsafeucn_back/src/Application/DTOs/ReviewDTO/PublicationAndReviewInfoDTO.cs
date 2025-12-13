using bolsafeucn_back.src.Application.DTOs.PublicationDTO;

namespace bolsafeucn_back.src.Application.DTOs.ReviewDTO
{
    public class PublicationAndReviewInfoDTO
    {
        public required PublicationsDTO Publication { get; set; }
        public required ShowReviewDTO Review { get; set; }
    }
}