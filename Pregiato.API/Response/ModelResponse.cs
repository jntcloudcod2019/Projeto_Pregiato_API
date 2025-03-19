namespace Pregiato.API.Response
{
    public class ModelResponse 
    {
        public ModelInfo Model { get; set; }
        public string Mensage { get; set; }
    }

    public class ModelInfo
    {
        public string Name { get; set; }
        
    }
}
