using Microsoft.Build.Framework;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class CreateRequestContractImageRights
    {
        [SwaggerSchema("Data de Agendamento")]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DataAgendamento { get; set; }

        [SwaggerSchema("Hora de Agendamento")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public string horaAgendamento { get; set; }

        [SwaggerSchema("Valor do Cache")]
        public decimal valorChache { get; set; }
    }
}
