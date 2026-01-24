namespace ProductMonitoring.API.DTO
{
    public class TicketRequest
    {
        public string key { get; set; }

        public bool IsExistingSolution { get; set; }

        public string? Remedy { get; set; }

    }
}
