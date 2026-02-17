namespace ProductMonitoring.API.DTO
{
    public class PartMasterDTO
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string? Location { get; set; }
        public string? Section { get; set; }
        public string? BitAddress { get; set; }
    }
}
