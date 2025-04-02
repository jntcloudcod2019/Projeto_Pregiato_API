namespace Pregiato.API.Response
{
    public class ProducersResponse
    {

        public bool SUCESS { get; set; }
        public string MESSAGE { get; set; }
        public List<ResulProducersResponse> DATA { get; set; }
    }

    public class ResulProducersResponse
    {
        public string ID { get; set; }
        public string NAME { get; set; }
        public string CODPRODUCER { get; set; }
     
    }
}
