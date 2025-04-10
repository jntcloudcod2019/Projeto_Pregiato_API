using Pregiato.API.Models;
using System.Text.Json;

namespace Pregiato.API.Response
{
    public class ModelsResponse
    {
        public bool SUCESS { get; set; }
        public string MESSAGE { get; set; }
        public List<ResulModelsResponse>? DATA { get; set; }
    }

    public class ResulModelsResponse
    {
        public string ID { get; set; }
        public string IDUSER { get; set; }
        public string NAME { get; set; }
        public string CPF { get; set; }
        public string RG { get; set; }
        public DateTime? DATEOFBIRTH { get; set; }
        public string EMAIL { get; set; }
        public int AGE { get; set; }
        public JsonDocument MODELATTRIBUTES { get; set; }
        public string? TELEFONEPRINCIPAL { get; set; }
        public string STATUS { get; set; }
        public string RESPONSIBLEPRODUCER { get; set; }
        public AdressInfo? ADRESSINFO { get; set; }
    }

    public class AdressInfo
    {
        public string? ADDRESS { get; set; }
        public string NUMBERADDRESS { get; set; }
        public string? POSTALCODE { get; set; }
        public string? CITY { get; set; }
        public string? UF { get; set; }
    }
}
