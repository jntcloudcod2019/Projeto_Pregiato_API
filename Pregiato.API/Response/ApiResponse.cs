namespace Pregiato.API.Response
{
    public class ApiResponse<T>
    {
        public bool StatusSuccess { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }

        public ApiResponse()
        {
            Errors = [];
        }
        public static ApiResponse<T> Success(T data, string message = "Operação realizada com sucesso")
        {
            return new ApiResponse<T>
            {
                StatusSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> Fail(string message, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                StatusSuccess = false,
                Message = message,
                Errors = errors ?? []
            };
        }
    }
}


