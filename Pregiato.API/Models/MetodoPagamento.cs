namespace Pregiato.API.Models
{
    public class MetodoPagamento
    {
        public const string CartaoCredito = "CartaoCredito";
        public const string CartaoDebito = "CartaoDebito";
        public const string Pix = "Pix";
        public const string Dinheiro = "Dinheiro";
        public const string LinkPagamento = "LinkPagamento";

        private static readonly HashSet<string> ValidValues = new()
        {
            CartaoCredito,
            CartaoDebito,
            Pix,
            Dinheiro,
            LinkPagamento
        };

        public string Value { get; private set; }

        // Construtor privado para controle centralizado
        private MetodoPagamento(string value)
        {
            if (!IsValid(value))
            {
                throw new ArgumentException($"O valor '{value}' não é válido para MetodoPagamento. Valores permitidos: {string.Join(", ", ValidValues)}");
            }

            Value = value;
        }

        // Método para criar um MetodoPagamento
        public static MetodoPagamento Create(string value)
        {
            return new MetodoPagamento(value);
        }

        // Verifica se o valor é válido
        public static bool IsValid(string value)
        {
            return ValidValues.Contains(value);
        }

        // Converte implicitamente MetodoPagamento para string
        public static implicit operator string(MetodoPagamento metodoPagamento)
        {
            return metodoPagamento.Value;
        }

        // Permite criar MetodoPagamento a partir de string
        public static explicit operator MetodoPagamento(string value)
        {
            return new MetodoPagamento(value);
        }

        // Override de Equals e GetHashCode para comparação
        public override bool Equals(object obj)
        {
            return obj is MetodoPagamento other && Value == other.Value;
        }

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
