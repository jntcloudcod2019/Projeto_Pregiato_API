using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Pregiato.API.System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Requests
{
    public class CreateRequestCommitmentTerm
    {
        [JsonConverter(typeof(JsonDateTimeConverter))]
        [SwaggerSchema(Description = "Data do agendamento no formato dd-MM-yyyy.")]
        [DefaultValue("05-02-2025")]
        [NotMapped]
        public DateTime DataAgendamento { get; set; }

        [SwaggerSchema("Hora de Agendamento")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public string horaAgendamento { get; set; }

        public string cpfModelo { get; set; }

        [SwaggerSchema("Valor do Cache")]
        [Range(0, 10000, ErrorMessage = "O valor do cache deve estar entre 0 e 10.000,00.")]
        public decimal ValorCache { get; set; }
    }
}
