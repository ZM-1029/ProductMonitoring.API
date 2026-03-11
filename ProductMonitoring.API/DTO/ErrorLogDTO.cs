namespace ProductMonitoring.API.DTO
{
    public class ErrorLogDTO
    {
        public string? BitAddress { get; set; }
        public int? CategoryId { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool IsOpen { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? File { get; set; }
    }
}
