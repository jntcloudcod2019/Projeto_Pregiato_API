using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;

public class SampleMessageStore : MessageStore
{
    public override async Task<SmtpResponse> SaveAsync(
        ISessionContext context,
        IMessageTransaction transaction,
        ReadOnlySequence<byte> buffer,
        CancellationToken cancellationToken)
    {
        // Converte os bytes do e-mail para uma string legível
        string textMessage = Encoding.UTF8.GetString(buffer.ToArray());

        // Log para depuração (pode ser substituído por gravação em banco de dados ou arquivo)
        Console.WriteLine($"[SMTP Interno] E-mail recebido:\n{textMessage}");

        return SmtpResponse.Ok;
    }
}
