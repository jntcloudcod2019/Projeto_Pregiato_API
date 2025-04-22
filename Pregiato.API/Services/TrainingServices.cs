using Pregiato.API.Interfaces;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using System.Text;

namespace Pregiato.API.Services
{
    public class TrainingServices : ITrainingServices
    {
      

        private static readonly string DefaulTemplateCertificate = "CertificateTemplate.html";
        private readonly IBrowserService _browserService;


        public TrainingServices(IBrowserService browserService)
        {
            _browserService = browserService;
        }



        public async  Task<Dictionary<string, string>> AddLogoToParameters(Dictionary<string, string> parameters)
        {
            Dictionary<string, string> updatedParameters = new(parameters);

            string logoFileName = "logo-pregiato.png";
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", logoFileName);

            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("Logo da Pregiato não encontrada.", imagePath);
            }

            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath).ConfigureAwait(true);
            string imageBase64 = Convert.ToBase64String(imageBytes);

            updatedParameters["LogoBase64"] = $"data:image/png;base64,{imageBase64}";

            return await Task.FromResult(updatedParameters);
        }

        public async Task<string> PopulateTemplateCertificateAsync(string template , Dictionary<string, string> parameters)
        {
            if(string.IsNullOrWhiteSpace(template))
            {
                return ("O template está vazio ou inválido.");
            }

            if (parameters == null || !parameters.Any())
            {
                return ("Os parâmetros para preenchimento do template estão vazios ou nulos.");
            }

            StringBuilder stringBuilder = new StringBuilder(template);
            foreach (KeyValuePair<string, string> param in parameters)
            {
                stringBuilder.Replace($"<span class=\"highlight\">{{{param.Key}}}</span>", param.Value);
                stringBuilder.Replace($"{{{param.Key}}}", param.Value);
            }
            return await Task.FromResult(stringBuilder.ToString());
        }

        public async Task<byte[]> ConvertHtmlToPdf(string populatedHtml, Dictionary<string, string> parameters)
        {

            try
            {

                IBrowser browser = await _browserService.GetBrowserAsync().ConfigureAwait(true);

                await using IPage? page = await browser.NewPageAsync().ConfigureAwait(true);

                await page.SetContentAsync(populatedHtml, new NavigationOptions
                {
                    WaitUntil = [WaitUntilNavigation.Networkidle0]
                }).ConfigureAwait(true);

                PdfOptions pdfOptions = new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "20px",
                        Bottom = "40px",
                        Left = "20px",
                        Right = "20px"
                    }
                };

                return await page.PdfDataAsync(pdfOptions).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Erro ao converter HTML para PDF: {ex.Message}").ConfigureAwait(true);
                throw;
            }
        }
    }
}
