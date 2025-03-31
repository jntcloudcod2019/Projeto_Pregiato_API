namespace Pregiato.API.Services.ServiceModels
{
   public class StatusPagamento
{
    public const string Paid = "Paid";
    public const string Pending = "Pending";
    public const string NotDone = "NotDone";

    private static readonly HashSet<string> ValidValues =
    [
        Paid,
        Pending,
        NotDone
    ];

    public string Value { get; private set; }


    private StatusPagamento(string value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException($"O valor '{value}' não é válido para StatusPagamento. Valores permitidos: {string.Join(", ", ValidValues)}");
        }

        Value = value;
    }

    // Método para criar um StatusPagamento
    public static StatusPagamento Create(string value)
    {
        return new StatusPagamento(value);
    }

    // Verifica se o valor é válido
    public static bool IsValid(string value)
    {
        return ValidValues.Contains(value);
    }

    // Converte implicitamente StatusPagamento para string
    public static implicit operator string(StatusPagamento statusPagamento)
    {
        return statusPagamento.Value;
    }

    // Permite criar StatusPagamento a partir de string
    public static explicit operator StatusPagamento(string value)
    {
        return new StatusPagamento(value);
    }

    // Override de Equals e GetHashCode para comparação
    public override bool Equals(object obj)
    {
        return obj is StatusPagamento other && Value == other.Value;
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
