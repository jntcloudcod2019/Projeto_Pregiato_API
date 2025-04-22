namespace Pregiato.API.Interfaces
{
    public interface ITrainingServices
    {
        Task<Dictionary<string, string>> AddLogoToParameters(Dictionary<string, string> parameters);

        Task<string> PopulateTemplateCertificateAsync(string template, Dictionary<string, string> parameters);

        Task<byte[]> ConvertHtmlToPdf(string populatedHtml, Dictionary<string, string> parameters);
    }
}
