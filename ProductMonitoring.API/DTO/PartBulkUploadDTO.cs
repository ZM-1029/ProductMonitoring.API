namespace ProductMonitoring.API.DTO
{
    public class PartBulkUploadDTO
    {
        public string Number { get; set; } = null!;
        public string? Description { get; set; }
        public int? Quantity { get; set; }
    }

    public class BulkUploadResultDTO
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
