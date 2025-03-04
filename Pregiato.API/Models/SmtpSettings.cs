namespace Pregiato.API.Models
{
    public class SmtpSettings
    {
        public string Server { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSsl { get; set; }
        public bool UseTls { get; set; } 
        public int MaxRetryAttempts { get; set; } = 3;  
        public int RetryDelaySeconds { get; set; } = 5;  
    }
}
