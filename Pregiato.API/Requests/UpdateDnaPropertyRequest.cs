namespace Pregiato.API.Requests
{
    public class UpdateDnaPropertyRequest
    {
        public string PropertyName { get; set; } // ex.: "physicalCharacteristics"
        public Dictionary<string, object> Values { get; set; } // Valores a atualizar
    }
}
