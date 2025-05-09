namespace Pregiato.API.Helper
{
    public static class PhoneNumberUtils
    {
        public static string NormalizeToE164(string rawNumber)
        {
            if (string.IsNullOrWhiteSpace(rawNumber))
            {
                throw new ArgumentException("Número de telefone vazio.");
            }

            var digitsOnly = new string(rawNumber.Where(char.IsDigit).ToArray());

            if (digitsOnly.Length is < 10 or > 11)
            {
                throw new FormatException($"Número de telefone inválido: {rawNumber}");
            }

            return $"55{digitsOnly}";
        }
    }
}
