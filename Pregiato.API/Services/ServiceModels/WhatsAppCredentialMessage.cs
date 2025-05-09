namespace Pregiato.API.Services.ServiceModels
{
    public class WhatsAppCredentialMessage
    {
        public string Phone { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string Password { get; set; }

        public string GetFormattedMessage()
        {
            return $"Olá {UserName}, aqui estão suas credenciais de acesso:\n\n" +
                   $"Usuário: {NickName}\n" +
                   $"Senha: {Password}\n\n" +
                   $"Acesse: https://my.pregiato.com.br/auth";
        }
    }
}
