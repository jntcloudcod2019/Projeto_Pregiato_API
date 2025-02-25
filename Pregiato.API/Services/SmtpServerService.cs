using Pregiato.API.Services;
using SmtpServer;
using SmtpServer.ComponentModel;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class SmtpServerService
{
    public static void StartSmtpServer()
    {
        var options = new SmtpServerOptionsBuilder()
            .ServerName("localhost") // Nome do servidor
            .Port(1025, true) // Porta SMTP
            .Build();

        // Registrando o serviço de armazenamento no ServiceProvider
        var serviceProvider = new SampleServiceProvider(new SampleMessageStore());

        // Inicializando o servidor SMTP com um IServiceProvider
        var server = new SmtpServer.SmtpServer(options, serviceProvider);
        Task.Run(() => server.StartAsync(CancellationToken.None)); // Iniciar o servidor em uma nova thread
    }
}

public class SampleMessageStore : MessageStore
{
    public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        var message = Encoding.UTF8.GetString(buffer.ToArray());

        Console.WriteLine(" E-mail recebido:");
        Console.WriteLine($"De: {transaction.From.AsAddress()}");
        Console.WriteLine($"Para: {string.Join(", ", transaction.To)}");
        Console.WriteLine($" Conteúdo:\n{message}");

        return Task.FromResult(SmtpResponse.Ok);
    }
}
