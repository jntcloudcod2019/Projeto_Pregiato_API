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

        public string cpfModelo { get; set; }

        [SwaggerSchema("Valor do Cache")]
        [Range(0, 10000, ErrorMessage = "O valor do cache deve estar entre 0 e 10.000,00.")]
        public decimal valorChache { get; set; }

        private decimal _valorCache;
        public decimal ValorCache
        {
            get => _valorCache;
            set
            {
                if (value < 0 || value > 10000)
                    throw new ArgumentException("O valor do cache deve estar entre 0 e 10.000,00. Caso o cache seja maior realizar contrato de cache especial.");

                _valorCache = value;
            }
        }
    }
}
