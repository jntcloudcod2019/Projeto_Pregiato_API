using Microsoft.AspNetCore.Mvc;

namespace Pregiato.API.Services.ServiceModels
{
    public class ActionResultIndex : IActionResult
    {
        public bool IsCompletedSuccessfully { get; set; } = true;
        public bool IsSpeakOnOperation { get; set; } = false;
        public string Mensage { get; set; }
        public object? Data { get; set; }

        public ActionResultIndex(bool isCompletedSuccessfully, string message, object? data = null, bool isSpeakOnOperation = false)
        {
            IsCompletedSuccessfully = isCompletedSuccessfully;
            Mensage = message;
            Data = data;
            IsSpeakOnOperation = isSpeakOnOperation;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            HttpResponse response = context.HttpContext.Response;
            response.ContentType = "application/json";
            response.StatusCode = IsCompletedSuccessfully ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;

            var result = new
            {
                success = IsCompletedSuccessfully,
                message = Mensage,
                data = Data,
                isSpeakOnOperation = IsSpeakOnOperation
            };

            await response.WriteAsJsonAsync(result);
        }

        public static ActionResultIndex Success(object? data = null, string message = "Operação concluída com sucesso")
        {
            return new ActionResultIndex(true, message, data);
        }

        public static ActionResultIndex Failure(string messageErro, bool isSpeakOnOperation = false)
        {
            return new ActionResultIndex(false, messageErro, null, isSpeakOnOperation);
        }
    }
}