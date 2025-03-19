namespace Pregiato.API.Interfaces
{
    public interface IEnvironmentVariableProviderEmail
    {
        string GetVariable(string key, EnvironmentVariableTarget target = EnvironmentVariableTarget.Process);
    }
}

