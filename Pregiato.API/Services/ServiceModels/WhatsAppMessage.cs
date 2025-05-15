namespace Pregiato.API.Services.ServiceModels
{
    public class WhatsAppMessage
    {
        public string? Phone { get; set; }
        public string? UserName { get; set; }
        public string? NickName { get; set; }
        public string? Password { get; set; }
        public string? VerificationCode { get; set; }

        public string GetFormattedMessage()
        {
            return $"Olá {UserName}, aqui estão suas credenciais de acesso:\n\n" +
                   $"Usuário: {NickName}\n" +
                   $"Senha: {Password}\n\n" +
                   $"Acesse: https://my.pregiato.com.br/auth";
        }

        public string SendMessageVerificationCode()
        {
            return $"Olá {UserName}, o seu código de verificação My Pregiato é: {VerificationCode}.\n\n" +
                   $"Para validar a sua conta, digite o código que você recebeu. \n" +
                   $"Este código é válido por 15 minutos.\n\n";
        }
    }
}
