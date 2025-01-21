using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using iText.Signatures;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System.IO;

namespace Pregiato.API.Services
{
    public class DigitalSignatureService
    {
        private readonly string _certificatesDirectory;

        public DigitalSignatureService()
        {
            _certificatesDirectory = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Certificates");

            if (!Directory.Exists(_certificatesDirectory))
            {
                Directory.CreateDirectory(_certificatesDirectory);
            }
        }

        public byte[] SignPdfWithCertificate(byte[] pdfBytes, string certificateName, string password)
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
                throw new ArgumentException("PDF inválido.", nameof(pdfBytes));

            if (string.IsNullOrWhiteSpace(certificateName))
                throw new ArgumentException("O nome do certificado não pode ser nulo ou vazio.", nameof(certificateName));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("A senha do certificado não pode ser nula ou vazia.", nameof(password));

            var certificatePath = System.IO.Path.Combine(_certificatesDirectory, certificateName);
            if (!File.Exists(certificatePath))
                throw new FileNotFoundException("Certificado não encontrado no servidor.", certificatePath);

            var certificateBytes = File.ReadAllBytes(certificatePath);
            var pkcs12Store = new Pkcs12Store(new MemoryStream(certificateBytes), password.ToCharArray());
            var alias = pkcs12Store.Aliases.Cast<string>().FirstOrDefault(a => pkcs12Store.IsKeyEntry(a));

            if (alias == null)
                throw new InvalidOperationException("Nenhum alias encontrado no certificado.");

            using (var pdfReader = new PdfReader(new MemoryStream(pdfBytes)))
            using (var pdfOutputStream = new MemoryStream())
            {
                var pdfSigner = new PdfSigner(pdfReader, pdfOutputStream, new StampingProperties());

                // Define o retângulo onde o campo de assinatura será criado
                Rectangle signatureRect = new Rectangle(100, 100, 200, 50);

                // Cria o campo de assinatura
                PdfFormField signatureField = PdfFormField.CreateSignature(pdfSigner.GetDocument(), signatureRect);
                signatureField.SetFieldName("SignatureField");

                // Obtém o formulário e adiciona o campo de assinatura
                PdfAcroForm acroForm = PdfAcroForm.GetAcroForm(pdfSigner.GetDocument(), createIfNotExist: true);
                acroForm.AddField(signatureField);

                // Configurações da aparência
                var appearance = pdfSigner.GetSignatureAppearance();
                appearance.SetReason("Assinado Digitalmente");
                appearance.SetLocation("Localização Padrão");

                // Configurações da assinatura
                var privateKey = pkcs12Store.GetKey(alias).Key;
                var certificateChain = pkcs12Store.GetCertificateChain(alias)
                    .Select(c => c.Certificate)
                    .Cast<Org.BouncyCastle.X509.X509Certificate>()
                    .ToArray();

                var pks = new PrivateKeySignature(privateKey, DigestAlgorithms.SHA256);
                pdfSigner.SignDetached(
                    pks,
                    certificateChain,
                    null,
                    null,
                    null,
                    0,
                    PdfSigner.CryptoStandard.CMS
                );

                return pdfOutputStream.ToArray();
            }
        }
    }
}
