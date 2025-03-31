using System.ComponentModel.DataAnnotations.Schema;

namespace Pregiato.API.Services.ServiceModels
{
    [NotMapped]
    public class MetodoPagamento
    {
        public const string CartaoCredito = "CartaoCredito";
        public const string CartaoDebito = "CartaoDebito";
        public const string Pix = "Pix";
        public const string Dinheiro = "Dinheiro";
        public const string LinkPagamento = "LinkPagamento";

        private static readonly HashSet<string> ValidValues =
        [
            CartaoCredito,
            CartaoDebito,
            Pix,
            Dinheiro,
            LinkPagamento
        ];

        public string Value { get; private set; }

        private MetodoPagamento(string value)
        {
            if (!IsValid(value))
            {
                throw new ArgumentException($"O valor '{value}' não é válido para MetodoPagamento. Valores permitidos: {string.Join(", ", ValidValues)}");
            }

            Value = value;
        }
        public static MetodoPagamento Create(string value)
        {
            return new MetodoPagamento(value);
        }
        public static bool IsValid(string value)
        {
            return ValidValues.Contains(value);
        }

        public static implicit operator string(MetodoPagamento metodoPagamento)
        {
            return metodoPagamento.Value;
        }

        public static explicit operator MetodoPagamento(string value)
        {
            return new MetodoPagamento(value);
        }

        public override bool Equals(object obj) => obj is MetodoPagamento other && Value == other.Value;

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public override string ToString()
        {
            return Value;
        }
    }

}
