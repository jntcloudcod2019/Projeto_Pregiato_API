using Pregiato.API.Interfaces;

namespace Pregiato.API.Services
{
    public class EnvironmentVariableProviderEmail : IEnvironmentVariableProviderEmail
    {
        public string GetVariable(string key, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
        {
            return Environment.GetEnvironmentVariable(key, target);
        }
    }
}
