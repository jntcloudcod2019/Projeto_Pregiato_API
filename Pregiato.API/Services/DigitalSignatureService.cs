using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System.Drawing;
using static iText.Signatures.PdfSigner;

namespace Pregiato.API.Services
{
    public class DigitalSignatureService
    {
        private readonly string _certificatePath;
        private readonly string _certificatePassword;

        public DigitalSignatureService(string certificatePath, string certificatePassword)
        {
            _certificatePath = certificatePath;
            _certificatePassword = certificatePassword;
        }

        public byte[] ApplyDigitalSignature(byte[] pdfBytes)
        {
            // Carregar o certificado PKCS12
            Pkcs12Store pkcs12 = new Pkcs12Store(new FileStream(_certificatePath, FileMode.Open, FileAccess.Read), _certificatePassword.ToCharArray());
            string alias = pkcs12.Aliases.Cast<string>().FirstOrDefault(pkcs12.IsKeyEntry);

            if (alias == null)
            {
                throw new Exception("Nenhum alias encontrado no certificado.");
            }

            // Obter a chave privada
            var privateKey = pkcs12.GetKey(alias).Key;

            // Obter a cadeia de certificados
            var chain = pkcs12.GetCertificateChain(alias)
                .Select(c => DotNetUtilities.ToX509Certificate(c.Certificate))
                .ToArray();

            // Criar o PdfReader e o PdfSigner
            using var pdfReader = new PdfReader(new MemoryStream(pdfBytes));
            using var pdfOutputStream = new MemoryStream();
            var pdfSigner = new PdfSigner(pdfReader, pdfOutputStream, new StampingProperties());

            // Configuração da assinatura
            IExternalSignature externalSignature = new PrivateKeySignature((iText.Commons.Bouncycastle.Crypto.IPrivateKey)privateKey, "SHA-256");
            IExternalDigest digest = new BouncyCastleDigest();

            // Campo de assinatura padrão
            pdfSigner.SignDetached(digest, externalSignature, (iText.Commons.Bouncycastle.Cert.IX509Certificate[])chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);

            return pdfOutputStream.ToArray();
        }
    }

}
