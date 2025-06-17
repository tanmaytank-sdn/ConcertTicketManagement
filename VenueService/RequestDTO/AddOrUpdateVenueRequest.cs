namespace VenueService.RequestDTO
{
    public class AddOrUpdateVenueRequest
    {
        public int VenueId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int MaxCapacity { get; set; }
    }
}
