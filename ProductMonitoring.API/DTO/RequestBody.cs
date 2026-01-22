namespace ProductMonitoring.API.DTO
{
    public class RequestBody
    {
        public string Key { get; set; } = null!;

        public int CategoryId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
