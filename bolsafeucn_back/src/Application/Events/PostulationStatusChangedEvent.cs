public class PostulationStatusChangedEvent
{
    public int PostulationId { get; set; }
    public string NewStatus { get; set; }
    public string OfferName { get; set; }
    public string CompanyName { get; set; }
    public string StudentEmail { get; set; }
}
