namespace TechTestBackend.Models
{
    public class ErrorResponseModel
    {
        public string? Response { get; set; }
        public string? Error { get; set; }
        public int StatusCode { get; internal set; }
    }
}
