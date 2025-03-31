using System.Text.Json;

namespace Pregiato.API.System.Text.Json
{
    public class UpperCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name.ToUpperInvariant(); 
        }
    }
}
