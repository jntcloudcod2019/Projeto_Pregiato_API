namespace Pregiato.API.Response
{
    public class CustomResponse
    {
        public int? StatusCode { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }

    }
}
