namespace ProductMonitoring.API.Models
{
    public class PartMaster
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string? Description { get; set; }
        public int? Quantity { get; set; }

    }
}
