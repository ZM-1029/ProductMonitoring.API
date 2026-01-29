namespace ProductMonitoring.API.DTO
{
    public class BitAddressRemedyDTO
    {
        public long Id { get; set; }

        public long BitAddressId { get; set; }

        public string Remedy { get; set; } = null!;
        public bool IsAdditionRemedy { get; set; } = false;

        public string? Manual { get; set;}
    }
}
